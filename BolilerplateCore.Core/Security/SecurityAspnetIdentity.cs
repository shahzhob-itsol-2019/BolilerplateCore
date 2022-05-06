using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Models;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.Entities;
using BoilerplateCore.Core.ISecurity;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using static BoilerplateCore.Common.Utility.Enums;
//using static BoilerplateCore.Common.Utility.Enums;
using AuthenticationResponse = BoilerplateCore.Common.Models.AuthenticationResponse;
using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;
using UserClaims = BoilerplateCore.Common.Models.UserClaims;

namespace BoilerplateCore.Core.Security
{
    public class SecurityAspnetIdentity : ISecurityService
    {
        private readonly SecurityOptions securityOptions;
        private readonly BoilerplateOptions boilerplateOptions;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly ISqlServerDbContext _dbContext;
        private readonly UrlEncoder _urlEncoder;

        private string clientId = string.Empty;
        private string clientSecret = string.Empty;
        private string authenticatorUriFormat = string.Empty;
        private int numberOfRecoveryCodes;
        private string scopes = string.Empty;
        private string apiUrl = string.Empty;

        public SecurityAspnetIdentity(
            IOptionsSnapshot<SecurityOptions> securityOptions,
            IOptionsSnapshot<BoilerplateOptions> boilerplateOptions,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder,
            ISqlServerDbContext dbContext
            )
        {
            this.securityOptions = securityOptions.Value;
            this.boilerplateOptions = boilerplateOptions.Value;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _dbContext = dbContext;

            clientId = this.securityOptions.ClientId;
            clientSecret = this.securityOptions.ClientSecret;
            authenticatorUriFormat = this.securityOptions.AuthenticatorUriFormat;
            numberOfRecoveryCodes = this.securityOptions.NumberOfRecoveryCodes;
            scopes = this.securityOptions.Scopes;
            apiUrl = this.boilerplateOptions.ApiUrl;
        }

        public async Task<BaseModel> CreateUser(RegisterUserModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.CreateActivated,
                TwoFactorTypeId = TwoFactorTypes.None,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await AddPreviousPassword(user, model.Password);
                await _userManager.AddToRoleAsync(user, UserRoles.User.ToString());

                if (!string.IsNullOrWhiteSpace(model.FirstName))
                    await AddUserClaim(user.Id, JwtClaimTypes.GivenName.ToString(), model.FirstName);
                if (!string.IsNullOrWhiteSpace(model.LastName))
                    await AddUserClaim(user.Id, JwtClaimTypes.FamilyName.ToString(), model.LastName);
                await AddUserClaim(user.Id, JwtClaimTypes.Name, user.UserName);
                await AddUserClaim(user.Id, JwtClaimTypes.Email, user.Email);
                await AddUserClaim(user.Id, JwtClaimTypes.Role, UserRoles.User.ToString());

                if (!model.CreateActivated)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    return new BaseModel { Success = true, Data = emailConfirmationToken, Message = "Account created successfully." };

                    //// ToDo: Check how it works in SingleSignOn
                    //var link = webUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(emailConfirmationToken);
                    //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailUserRegisteration, NotificationTypes.Email);
                    //var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
                    //                                       .Replace("#Link", $"{link}");

                    //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
                    //return new AuthenticationResponse { ResponseType = ResponseType.Success, Message = "Account created successfully. A confirmation link has been sent to your specified email , click the link to confirm your email and proceed to login." };
                }
                return new BaseModel { Success = true, Message = "Registeration successfull." };

            }
            var errorMessaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new BaseModel { Success = false, Message = errorMessaeg };
        }
        public async Task<BaseModel> UpdateUserDetail(UserModel userInfo)
        {
            var user = await _userManager.FindByIdAsync(userInfo.Id);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            if (!string.IsNullOrWhiteSpace(userInfo.FirstName))
                user.FirstName = userInfo.FirstName;
            if (!string.IsNullOrWhiteSpace(userInfo.LastName))
                user.LastName = userInfo.LastName;
            //if (!string.IsNullOrWhiteSpace(userInfo.Address))
            //    user.Address = userInfo.Address;
            //if (!string.IsNullOrWhiteSpace(userInfo.Gender))
            //    user.Gender = userInfo.Gender;
            //if (userInfo.BirthDate.HasValue)
            //    user.BirthDate = userInfo.BirthDate.Value;
            //if (!string.IsNullOrWhiteSpace(userInfo.Picture))
            //    user.Picture = userInfo.Picture;

            await _userManager.UpdateAsync(user);
            //var userUpdateResult = await Update(user);
            //if (!userUpdateResult.Succeeded)
            //{
            //    var message = userUpdateResult.Errors.FirstOrDefault() != null
            //                       ? userUpdateResult.Errors.FirstOrDefault().Description
            //                       : "Faild to update user detail.";
            //    return new BaseModel { Success = false, Message = message };
            //}
            return new BaseModel { Success = true, Message = "User info has been successfully updated." };
        }
        public async Task<AuthenticationResponse> CreateUser(object user, string password)
        {
            var appUser = (ApplicationUser)user;
            appUser.TwoFactorTypeId = TwoFactorTypes.None;

            var result = await _userManager.CreateAsync(appUser, password);
            if (result.Succeeded)
            {
                await AddPreviousPassword(appUser, password);
                await _userManager.AddToRoleAsync(appUser, UserRoles.User.ToString());

                //if (!string.IsNullOrWhiteSpace(appUser.FirstName))
                //    await AddUserClaim(appUser.Id, JwtClaimTypes.GivenName.ToString(), appUser.FirstName);
                //if (!string.IsNullOrWhiteSpace(appUser.LastName))
                //    await AddUserClaim(appUser.Id, JwtClaimTypes.FamilyName.ToString(), appUser.LastName);
                await AddUserClaim(appUser.Id, JwtClaimTypes.Name, appUser.UserName);
                await AddUserClaim(appUser.Id, JwtClaimTypes.Email, appUser.Email);
                await AddUserClaim(appUser.Id, JwtClaimTypes.Role, UserRoles.User.ToString());

                if (!appUser.EmailConfirmed)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = emailConfirmationToken };

                    //// ToDo: Check how it works in SingleSignOn
                    //var link = webUrl + "Account/ConfirmEmail?userId=" + appUser.Id + "&code=" + HttpUtility.UrlEncode(emailConfirmationToken);
                    //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailUserRegisteration, NotificationTypes.Email);
                    //var emailMessage = template.MessageBody.Replace("#Name", $"{ appUser.FirstName} { appUser.LastName}")
                    //                                       .Replace("#Link", $"{link}");

                    //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, appUser.Email);
                    //return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Account created successfully. A confirmation link has been sent to your specified email , click the link to confirm your email and proceed to login." };
                }

                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Registeration successfull." };

            }
            var errorMessaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = errorMessaeg };
        }

        public async Task<LoginResponse> CreateExternalUser(RegisterExternalModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                TwoFactorTypeId = TwoFactorTypes.None
            };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User.ToString());

                if (!string.IsNullOrWhiteSpace(model.FirstName))
                    await AddUserClaim(user.Id, JwtClaimTypes.GivenName.ToString(), model.FirstName);
                if (!string.IsNullOrWhiteSpace(model.LastName))
                    await AddUserClaim(user.Id, JwtClaimTypes.FamilyName.ToString(), model.LastName);
                await AddUserClaim(user.Id, JwtClaimTypes.Name, user.UserName);
                await AddUserClaim(user.Id, JwtClaimTypes.Email, user.Email);
                await AddUserClaim(user.Id, JwtClaimTypes.Role, UserRoles.User.ToString());

                var loginInfo = await AddLogin(user.Id, model.Provider, model.ProviderKey, model.ProviderDisplayName);
                if (!loginInfo.Success)
                {
                    result = await _userManager.DeleteAsync(user);
                    return new LoginResponse { Status = LoginStatus.Failed, Message = loginInfo.Message };
                }
                var login = await ExternalLogin(model.Provider, model.ProviderKey);
                return login;
            }
            var errorMessaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new LoginResponse { Status = LoginStatus.Failed, Message = errorMessaeg };
        }

        public async Task<AuthenticationResponse> GenerateEmailVerificationToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "No user exists with the specified email address." };

            var varficationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            if (string.IsNullOrWhiteSpace(varficationCode))
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "Email varification could not be generated." };

            return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = varficationCode };
        }

        public async Task<BaseModel> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "No user exists with the specified email address." };

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (emailConfirmed)
                return BaseModel.Failed(message: "Email has already been confirmed.");

            var response = await _userManager.ConfirmEmailAsync(user, code);
            if (response.Succeeded)
                return new BaseModel { Success = true, Message = "Email confirmed successfully." };

            var message = response.Errors.FirstOrDefault() != null
                ? response.Errors.FirstOrDefault().Description
                : "Email confirmation failed.";
            return new BaseModel { Success = false, Message = message };
        }

        private async Task<string> Token(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>();
            claims.AddRange(new List<Claim>
            {
                new Claim(JwtClaimTypes.Id, user.Id),
                new Claim(JwtClaimTypes.Name, user.UserName),
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.BirthDate, "2022-11-11"),
            });

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                claims.Add(new Claim(JwtClaimTypes.GivenName.ToString(), user.FirstName));
            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim(JwtClaimTypes.FamilyName.ToString(), user.LastName));

            var roles = (await _userManager.GetRolesAsync(user));

            foreach (var role in roles)
                claims.Add(new Claim(JwtClaimTypes.Role, role.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Core.Secret@Boilerplate"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                            issuer: apiUrl,
                            audience: apiUrl,
                            claims: claims,
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: credentials);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            return accessToken;
        }

        public async Task<LoginResponse> Login(string Email, string password, bool persistCookie = false)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(Email);
                if (user == null)
                    return new LoginResponse { Status = LoginStatus.Failed, Message = "Invalid user name or password." };
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return new LoginResponse { Status = LoginStatus.Failed, Message = "Invalid user name or password." };
            }

            if (!user.EmailConfirmed)
                return new LoginResponse { Status = LoginStatus.Failed, Message = "Email has not yet been confirmed , please confirm your email and login again." };

            var result = await _signInManager.PasswordSignInAsync(Email, password, persistCookie, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Todo:
                //var map = new TMap();
                //var data = map.Transform<T, TKey>(user, roles);

                var accessToken = await Token(user);

                return new LoginResponse { Status = LoginStatus.Succeded, Data = accessToken };
            }
            else if (result.RequiresTwoFactor)
            {
                //// ToDo: Check how SendTwoFactorToken works in SingleSignOn

                var authenticationResult = await GetAuthenticationDetail(user.UserName);
                if (authenticationResult.Success)
                {
                    UserAuthenticationInfo authenticationDetail = (UserAuthenticationInfo)authenticationResult.Data;

                    if (authenticationDetail.TwoFactorType == Common.Utility.Constants.TwoFactorTypes.Email)
                        await SendTwoFactorToken(user.UserName, Common.Utility.Constants.TwoFactorTypes.Email);
                    else if (authenticationDetail.TwoFactorType == Common.Utility.Constants.TwoFactorTypes.Phone)
                        await SendTwoFactorToken(user.UserName, Common.Utility.Constants.TwoFactorTypes.Phone);

                    return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = authenticationDetail };
                }
                //return new LoginResponse { Status = LoginStatus.Failed, Message = authenticationResult.Message };

                return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = user.UserName };
            }
            else
            {
               return new LoginResponse { Status = LoginStatus.Failed, Message = result.IsLockedOut ? "Locked Out" : "Invalid login attempt." };
            }
        }

        public async Task<LoginResponse> ExternalLogin(string loginProvider, string providerKey, bool isPersistent = false, bool bypassTwoFactor = false)
        {
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user == null)
                return new LoginResponse { Status = LoginStatus.Failed, Message = "Invalid user name or password." };

            var result = await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
            if (result.Succeeded)
            {
                var accessToken = await Token(user);
                return new LoginResponse { Status = LoginStatus.Succeded, Data = accessToken };
            }
            else if (result.RequiresTwoFactor)
            {
                //// ToDo: Check how SendTwoFactorToken works in SingleSignOn

                //var authenticationResult = await GetAuthenticationDetail(user.UserName);
                //if (authenticationResult.IsSuccess)
                //{
                //    UserAuthenticationInfo authenticationDetail = (UserAuthenticationInfo)authenticationResult.Data;

                //    if (authenticationDetail.TwoFactorType == Infrastructure.Utility.Constants.TwoFactorTypes.Email)
                //        await SendTwoFactorToken(user.UserName, Infrastructure.Utility.Constants.TwoFactorTypes.Email);
                //    else if (authenticationDetail.TwoFactorType == Infrastructure.Utility.Constants.TwoFactorTypes.Phone)
                //        await SendTwoFactorToken(user.UserName, Infrastructure.Utility.Constants.TwoFactorTypes.Phone);

                //    return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = authenticationDetail };
                //}
                //return new LoginResponse { Status = LoginStatus.Failed, Message = authenticationResult.Message };

                return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = user.UserName };
            }
            else
            {
                return new LoginResponse { Status = LoginStatus.Failed, Message = result.IsLockedOut ? "Locked Out" : "Invalid login attempt." };
            }
        }

        public async Task<LoginResponse> TwoFactorLogin(string provider, string code, bool persistCookie = false, bool rememberMachine = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return new LoginResponse { Status = LoginStatus.Failed, Message = "Invalid email address, not such user exists." };

            var result = await _signInManager.TwoFactorSignInAsync(provider, code, persistCookie, rememberMachine);
            if (result.Succeeded)
            {
                var accessToken = await Token(user);
                return new LoginResponse { Status = LoginStatus.Succeded, Data = accessToken };
            }
            return result.RequiresTwoFactor
                ? new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification." }
                : new LoginResponse { Status = LoginStatus.Failed, Message = result.IsLockedOut ? "Locked Out" : "Invalid authenticator code." };
        }

        public async Task<LoginResponse> RecoveryCodeLogin(string code)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return new LoginResponse { Status = LoginStatus.Failed, Message = "User not exist." };

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(code);

            if (result.Succeeded)
            {
                var accessToken = await Token(user);
                return new LoginResponse { Status = LoginStatus.Succeded, Data = accessToken };
            }
            return result.RequiresTwoFactor
                ? new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification." }
                : new LoginResponse { Status = LoginStatus.Failed, Message = result.IsLockedOut ? "Locked Out" : "Invalid recovery code." };
        }

        public async Task<BaseModel> GetLoginProviders()
        {
            var externalAuthenticationProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
            if (externalAuthenticationProviders == null)
                return new BaseModel { Success = false, Message = "No login provider is available." };

            return new BaseModel { Success = true, Data = externalAuthenticationProviders.ToList() };
        }

        public async Task<BaseModel> GetLogins(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var userLogins = await _userManager.GetLoginsAsync(user);
            return new BaseModel { Success = true, Data = userLogins };
        }

        public async Task<BaseModel> GetLoginProperties(string provider, string redirectUrl, string userId = null)
        {
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new BaseModel { Success = false, Message = "User not exists." };
            }
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
            if (properties == null)
                return new BaseModel { Success = false, Message = "External authentication not found in the system." };

            return new BaseModel { Success = true, Data = properties };
        }

        public async Task<BaseModel> AddLogin(string userId, string loginProvider, string providerKey, string displayName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };
            
            var loginInfo = new ExternalLoginInfo(null, loginProvider, providerKey, displayName);

            var result = await _userManager.AddLoginAsync(user, loginInfo);
            if (!result.Succeeded)
                return new BaseModel { Success = false, Message = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "The external login could not be added, please try again latter." };

            return new BaseModel { Success = true, Message = "The external login is added." };
        }

        public async Task<BaseModel> RemoveLogin(string userId, string provider, string providerKey)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.RemoveLoginAsync(user, provider, providerKey);
            if (!result.Succeeded)
                return new BaseModel { Success = false, Message = result.Errors.FirstOrDefault() != null ? result.Errors.FirstOrDefault().Description : "The external login could not be removed, please try again latter." };

            return new BaseModel { Success = true, Message = "The external login is removed." };
        }

        public async Task<BaseModel> GetAuthenticationDetail(string userName)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new BaseModel { Success = false, Message = "User not exists." };
            }

            var twoFactorType = await _dbContext.TwoFactorTypes.FirstOrDefaultAsync(t => t.Id == user.TwoFactorTypeId);
            if (twoFactorType == null)
                throw new Exception($"{nameof(twoFactorType)} is not found in the system.");

            var userLoginProvders = await _userManager.GetLoginsAsync(user);
            var otherLoginProvders = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                                                          .Where(auth => userLoginProvders.All(ul => auth.Name != ul.LoginProvider))
                                                          .ToList();

            var userLogins = userLoginProvders.Select(userLogin => new Common.Models.UserLoginInfo
            {
                Provider = userLogin.LoginProvider,
                ProviderKey = userLogin.ProviderKey,
                ProviderDisplayName = userLogin.ProviderDisplayName
            }).ToList();

            var authenticationInfo = new UserAuthenticationInfo
            {
                UserId = user.Id,
                IsEmailConfirmed = user.EmailConfirmed,
                HasPassword = await _userManager.HasPasswordAsync(user),
                TwoFactorEnabled = user.TwoFactorEnabled,
                TwoFactorType = twoFactorType.Name,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
                Logins = userLogins,
                OtherLogins = otherLoginProvders,
                //BrowserRemembered = await _userManager.TwoFactorBrowserRememberedAsync(user) Todo
            };
            return new BaseModel { Success = true, Data = authenticationInfo };
        }

        public async Task<BaseModel> GetUser(string userName)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new BaseModel { Success = false, Message = "User not exists." };
            }

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
            //if (appUser == null)
            //    return new BaseModel { Success = false, Message = "User not exists." };

            var userClaims = new UserClaims
            {
                Id = user.Id,
                //FirstName = appUser.FirstName,
                //LastName = appUser.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };
            return new BaseModel { Success = true, Data = userClaims };
        }

        public async Task<BaseModel> GetExternalUser(string loginProvider, string providerKey)
        {
            var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var roles = (await _userManager.GetRolesAsync(user)).ToList();

            //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
            //if (appUser == null)
            //    return new BaseModel { Success = false, Message = "User not exists." };

            var userClaims = new UserClaims
            {
                Id = user.Id,
                //FirstName = appUser.FirstName,
                //LastName = appUser.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles
            };
            return new BaseModel { Success = true, Data = userClaims };
        }

        public async Task<BaseModel> GetUserDetail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
            //if (appUser == null)
            //    return new BaseModel { Success = false, Message = "User not exists." };

            var userInfo = new UserInfo
            {
                Id = user.Id,
                //FirstName = appUser.FirstName,
                //LastName = appUser.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
            return new BaseModel { Success = true, Data = userInfo };
        }

        public async Task<BaseModel> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null)
                return new BaseModel { Success = false, Message = "No users exist." };

            var userDetails = new List<UserDetail>();
            foreach (var user in users)
            {
                //var twoFactorType = await _securityDbContext.TwoFactorTypes.FirstOrDefaultAsync(t => t.Id == user.TwoFactorTypeId);
                //if (twoFactorType == null)
                //    throw new Exception($"{nameof(twoFactorType)} is not found in the system.");

                //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(user.Id));
                //if (appUser == null)
                //    return new BaseModel { Success = false, Message = "User not exists." };

                var userDetail = new UserDetail();
                userDetail.Id = user.Id;
                //userDetail.FirstName = appUser.FirstName;
                //userDetail.LastName = appUser.LastName;
                userDetail.UserName = user.UserName;
                userDetail.Email = user.Email;
                userDetail.PhoneNumber = user.PhoneNumber;
                userDetail.IsEmailConfirmed = user.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(user.PasswordHash);
                userDetail.TwoFactorEnabled = user.TwoFactorEnabled;
                //userDetail.TwoFactorType = twoFactorType.Name;
                userDetail.LockoutEnabled = user.LockoutEnabled;
                userDetail.LockoutEnd = user.LockoutEnd;
                userDetail.AccessFailedCount = user.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(user);
                var userLogins = userLoginProvders.Select(userLogin => new Common.Models.UserLoginInfo
                {
                    Provider = userLogin.LoginProvider,
                    ProviderKey = userLogin.ProviderKey,
                    ProviderDisplayName = userLogin.ProviderDisplayName
                }).ToList();
                userDetail.Logins = userLogins;

                userDetails.Add(userDetail);
            }

            return new BaseModel { Success = true, Data = userDetails };
        }

        public async Task<BaseModel> GetUsers(Expression<Func<object, bool>> where)
        {
            var users = await _userManager.Users.Where(where).ToListAsync();
            if (users == null)
                return new BaseModel { Success = false, Message = "No users exist." };

            var userDetails = new List<UserDetail>();
            foreach (var user in users)
            {
                var applicationUser = (ApplicationUser)user;

                //var twoFactorType = await _securityDbContext.TwoFactorTypes.FirstOrDefaultAsync(t => t.Id == applicationUser.TwoFactorTypeId);
                //if (twoFactorType == null)
                //    throw new Exception($"{nameof(twoFactorType)} is not found in the system.");

                //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(applicationUser.Id));
                //if (appUser == null)
                //    return new BaseModel { Success = false, Message = "User not exists." };

                var userDetail = new UserDetail();
                userDetail.Id = applicationUser.Id;
                //userDetail.FirstName = appUser.FirstName;
                //userDetail.LastName = appUser.LastName;
                userDetail.UserName = applicationUser.UserName;
                userDetail.Email = applicationUser.Email;
                userDetail.PhoneNumber = applicationUser.PhoneNumber;
                userDetail.IsEmailConfirmed = applicationUser.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(applicationUser.PasswordHash);
                userDetail.TwoFactorEnabled = applicationUser.TwoFactorEnabled;
                //userDetail.TwoFactorType = twoFactorType.Name;
                userDetail.LockoutEnabled = applicationUser.LockoutEnabled;
                userDetail.LockoutEnd = applicationUser.LockoutEnd;
                userDetail.AccessFailedCount = applicationUser.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(applicationUser);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(applicationUser);
                var userLogins = userLoginProvders.Select(userLogin => new Common.Models.UserLoginInfo
                {
                    Provider = userLogin.LoginProvider,
                    ProviderKey = userLogin.ProviderKey,
                    ProviderDisplayName = userLogin.ProviderDisplayName
                }).ToList();
                userDetail.Logins = userLogins;

                userDetails.Add(userDetail);
            }

            return new BaseModel { Success = true, Data = userDetails };
        }

        public async Task<BaseModel> GetUsers(Expression<Func<object, bool>> where = null, Func<IQueryable<object>, IOrderedQueryable<object>> orderBy = null, params Expression<Func<object, object>>[] includeProperties)
        {
            IQueryable<object> query = _userManager.Users;

            if (Check.NotNull(where))
                query = query.Where(where);
            if (Check.NotNull(includeProperties))
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            if (Check.NotNull(orderBy))
                query = orderBy(query);

            var users = await query.ToListAsync();
            if (users == null)
                return new BaseModel { Success = false, Message = "No users exist." };

            var userDetails = new List<UserDetail>();
            foreach (var user in users)
            {
                var applicationUser = (ApplicationUser)user;

                //var twoFactorType = await _securityDbContext.TwoFactorTypes.FirstOrDefaultAsync(t => t.Id == applicationUser.TwoFactorTypeId);
                //if (twoFactorType == null)
                //    throw new Exception($"{nameof(twoFactorType)} is not found in the system.");

                //var appUser = await _securityDbContext.AppUsers.FirstOrDefaultAsync(u => u.Id.Equals(applicationUser.Id));
                //if (appUser == null)
                //    return new BaseModel { Success = false, Message = "User not exists." };

                var userDetail = new UserDetail();
                userDetail.Id = applicationUser.Id;
                //userDetail.FirstName = appUser.FirstName;
                //userDetail.LastName = appUser.LastName;
                userDetail.UserName = applicationUser.UserName;
                userDetail.Email = applicationUser.Email;
                userDetail.PhoneNumber = applicationUser.PhoneNumber;
                userDetail.IsEmailConfirmed = applicationUser.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(applicationUser.PasswordHash);
                userDetail.TwoFactorEnabled = applicationUser.TwoFactorEnabled;
                userDetail.TwoFactorType = applicationUser.TwoFactorType.Name;
                //userDetail.TwoFactorType = twoFactorType.Name;
                userDetail.LockoutEnabled = applicationUser.LockoutEnabled;
                userDetail.LockoutEnd = applicationUser.LockoutEnd;
                userDetail.AccessFailedCount = applicationUser.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(applicationUser);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(applicationUser);
                var userLogins = userLoginProvders.Select(userLogin => new Common.Models.UserLoginInfo
                {
                    Provider = userLogin.LoginProvider,
                    ProviderKey = userLogin.ProviderKey,
                    ProviderDisplayName = userLogin.ProviderDisplayName
                }).ToList();
                userDetail.Logins = userLogins;

                userDetails.Add(userDetail);
            }

            return new BaseModel { Success = true, Data = userDetails };
        }

        public async Task<BaseModel> BlockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

            var lockoutResult =  await _userManager.SetLockoutEnabledAsync(user, true);
            if (!lockoutResult.Succeeded)
            {
                var message = lockoutResult.Errors.FirstOrDefault() != null
                                   ? lockoutResult.Errors.FirstOrDefault().Description
                                   : "User could not be block.";
                return new BaseModel { Success = false, Message = message };
            }
            return new BaseModel { Success = false, Message = "User has been successfully blocked." };
        }

        public async Task<AuthenticationResponse> AddUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower());

                if (givenClaim != null)
                    return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "The specified claim already assigned to user, try different value." };
            }
            var result = await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Claim added." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : "Failed to add claim.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<BaseModel> GetUserClaim(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Data = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);
            return new BaseModel { Success = false, Data = userClaims };
        }

        public async Task<AuthenticationResponse> RemoveUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower() && x.Value.ToLower() == claimValue.ToLower());

                if (givenClaim == null)
                    return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User doesn't have the specified claim." };
            }

            var result = await _userManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Claim removed successfully." };

            return new AuthenticationResponse
            {
                ResponseType = ResponseType.Success,
                Data = result.Errors.FirstOrDefault() != null ?
                result.Errors.FirstOrDefault().Description : "Failed to remove claim."
            };
        }

        public async Task<AuthenticationResponse> CreateRole(string role)
        {
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (roleExist)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = $"{role} role already exists." };

            var identityRole = new IdentityRole
            {
                Name = role
            };
            var result = await _roleManager.CreateAsync(identityRole);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = $"{role} role successfully added." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to add {role} role.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<AuthenticationResponse> UpdateRole(string id, string role)
        {
            var existingRole = await _roleManager.FindByIdAsync(id);
            if (existingRole == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = $"Role not found with specified Id." };

            existingRole.Name = role;
            var result = await _roleManager.UpdateAsync(existingRole);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = $"Role successfully updated." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to update role.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<BaseModel> GetRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return new BaseModel { Success = false, Data = $"{role} role not found." };

            return new BaseModel { Success = true, Data = role };
        }

        public async Task<BaseModel> GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return new BaseModel { Success = true, Data = roles };
        }

        public async Task<AuthenticationResponse> RemoveRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = $"{role} role not found." };

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = $"{role} role removed successfully." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to remove {role} role.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<AuthenticationResponse> AddUserRole(string userId, IEnumerable<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User not found with specified Id." };

            string notFoundRoles = string.Empty;
            //var notExistingRoles = _roleManager.Roles.ToList().Select(role => { role.Name = role.Name.ToLower(); return role.Name; }).Where(roleName => !roles.Contains(roleName));
            var notExistingRoles = roles
                                    .ToList()
                                    .Select(roleName => { roleName = roleName.ToLower(); return roleName; })
                                    .Where(roleName => !_roleManager.Roles
                                                                        .ToList()
                                                                        .Select(role => { role.Name = role.Name.ToLower(); return role.Name; })
                                                                        .Contains(roleName));
            foreach (var roleName in notExistingRoles)
            {
                notFoundRoles += roleName + ",";
            }
            if (!string.IsNullOrWhiteSpace(notFoundRoles))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Error,
                    Data = $"{notFoundRoles.Remove(notFoundRoles.LastIndexOf(','))} {(notExistingRoles.Count() > 1 ? "roles" : "role")} not found in the system."
                };

            var alreadyFoundUserRoles = string.Empty;
            var userRoles = await _userManager.GetRolesAsync(user);
            var alreadyExistingUserRoles = userRoles
                                                .Select(roleName => { roleName = roleName.ToLower(); return roleName; })
                                                .Where(roleName => roles.Contains(roleName));
            foreach (var roleName in alreadyExistingUserRoles)
            {
                alreadyFoundUserRoles += roleName + ",";
            }
            if (!string.IsNullOrWhiteSpace(alreadyFoundUserRoles))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Error,
                    Data = $"User is already in {alreadyFoundUserRoles.Remove(alreadyFoundUserRoles.LastIndexOf(','))} {(alreadyFoundUserRoles.Count() > 1 ? "roles" : "role")}."
                };

            var result = await _userManager.AddToRolesAsync(user, roles);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = $"{(roles.Count() > 1 ? "Roles" : "Role")} successfully added for user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to add {(roles.Count() > 1 ? "roles" : "role")} for user.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<BaseModel> GetUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || !userRoles.Any())
                return new BaseModel { Success = false, Message = "User do not have any role." };

            return new BaseModel { Success = true, Data = userRoles };
        }

        public async Task<AuthenticationResponse> RemoveUserRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User not found with specified Id." };

            var role = await _roleManager.FindByIdAsync(roleName);
            if (role == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = $"{roleName} role not found in the system." };

            var userInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!userInRole)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = $"User do not have {roleName} role." };

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = $"{roleName} role removed successfully from user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to remove {roleName} role form user.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<AuthenticationResponse> RemoveUserRoles(string userId, IEnumerable<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User not found with specified Id." };

            string notFoundRoles = string.Empty;
            //var notExistingRoles = _roleManager.Roles.ToList().Select(role => { role.Name = role.Name.ToLower(); return role.Name; }).Where(roleName => !roles.Contains(roleName));
            var notExistingRoles = roles
                                    .ToList()
                                    .Select(roleName => { roleName = roleName.ToLower(); return roleName; })
                                    .Where(roleName => !_roleManager.Roles
                                                                        .ToList()
                                                                        .Select(role => { role.Name = role.Name.ToLower(); return role.Name; })
                                                                        .Contains(roleName));
            foreach (var roleName in notExistingRoles)
            {
                notFoundRoles += roleName + ",";
            }
            if (!string.IsNullOrWhiteSpace(notFoundRoles))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Error,
                    Data = $"{notFoundRoles.Remove(notFoundRoles.LastIndexOf(','))} {(notExistingRoles.Count() > 1 ? "roles" : "role")} not found in the system."
                };

            var notFoundUserRoles = string.Empty;
            var userRoles = await _userManager.GetRolesAsync(user);
            var notExistingUserRoles = roles
                                        .Select(roleName => { roleName = roleName.ToLower(); return roleName; })
                                        .Where(roleName => !userRoles
                                                                .Select(roleNam => { roleNam = roleNam.ToLower(); return roleNam; })
                                                                .Contains(roleName));
            foreach (var roleName in notExistingUserRoles)
            {
                notFoundUserRoles += roleName + ",";
            }
            if (!string.IsNullOrWhiteSpace(notFoundUserRoles))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Error,
                    Data = $"User is not in {notFoundUserRoles.Remove(notFoundUserRoles.LastIndexOf(','))} {(notFoundUserRoles.Count() > 1 ? "roles" : "role")}."
                };

            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded)
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Roles removed successfully from user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : "Failed to remove roles form user.";

            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = message };
        }

        public async Task<BaseModel> ForgotPassword(string email)
        {
            throw new NotImplementedException();

            //// ToDo: Check how it works in SingleSignOn
            //var user = await _userManager.FindByEmailAsync(email);
            //if (user == null)
            //    return new BaseModel { IsSuccess = false, Message = "No user exists with the specified email." };

            //var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            //var link = webUrl + "Account/ResetPassword?code=" + HttpUtility.UrlEncode(resetCode);
            //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailForgotPassword, NotificationTypes.Email);
            //var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
            //                                       .Replace("#Link", $"{link}");

            //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);

            //return new BaseModel { IsSuccess = true, Message = "Your password reset code has been sent to your specified email address, follow the link to reset your password." };
        }

        public async Task<BaseModel> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new BaseModel { Success = false, Message = "No user exists with the specified email." };
            }

            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            return new BaseModel { Success = true, Data = resetCode };
        }

        public async Task<AuthenticationResponse> ValidatePasswordResetToken(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "Please provide a valid token to validate." };
            }

            var result = await _userManager.VerifyUserTokenAsync(user, "Default", "ResetPassword", token);
            return result
                    ? new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Token is valid." }
                    : new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "Token is in-valid." };
        }

        public async Task<AuthenticationResponse> ResetPassword(string email, string code, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "No user exists with the specified user Id." };

            //var previousPasswordValidation = await ValidatePreviousPassword(user, password);
            //if (previousPasswordValidation.ResponseType.Equals(ResponseType.Error))
            //    return previousPasswordValidation;

            var result = await _userManager.ResetPasswordAsync(user, code, password);
            if (result.Succeeded)
            {
               await AddPreviousPassword(user, password);
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Password was reset successfully." };
            }

            return new AuthenticationResponse
            {
                ResponseType = ResponseType.Error,
                Data = result.Errors.FirstOrDefault() != null
                        ? result.Errors.FirstOrDefault().Description
                        : "Password reset failed."
            };
        }

        public async Task<AuthenticationResponse> ChangePassword(string userName, string currentPassword, string newPassword)
        {
            if (currentPassword == newPassword)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "New password must not be same as cureent password." };

            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "No user exists with the specified email/username." };
            }

            //var previousPasswordValidation = await ValidatePreviousPassword(user, newPassword);
            //if (previousPasswordValidation.ResponseType.Equals(ResponseType.Error))
            //    return previousPasswordValidation;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await AddPreviousPassword(user, newPassword);
                return new AuthenticationResponse { ResponseType = ResponseType.Success, Data = "Password changed successfully." };
            }

            string messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = messaeg ?? "Failed to change password." };
        }

        public async Task<BaseModel> SetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "No user exists." };

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
                return new BaseModel { Success = false, Message = "You already have a password. You can only change your password." };

            var result = await _userManager.AddPasswordAsync(user, newPassword);
            if (result.Succeeded)
            {
                user.TwoFactorTypeId =  TwoFactorTypes.None;
                var userUpdateResult = await _userManager.UpdateAsync(user);
                await AddPreviousPassword(user, newPassword);
                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                return new BaseModel { Success = true, Data = emailConfirmationToken, Message = $"Password has been set successfully." };

                //// ToDo: Check how it works in SingleSignOn
                //var link = webUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(emailConfirmationToken);
                //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailSetPassword, NotificationTypes.Email);
                //var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
                //                                       .Replace("#Link", $"{link}");

                //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);

                //return new BaseModel { success = true, message = $"Password has been set successfully. But to confirm your email address, a confirmation link has been sent to {user.Email}, please verify your email." };
            }
            string messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new BaseModel { Success = false, Message = messaeg ?? "Failed to set password." };
        }

        //public async Task<AuthenticationResponse> UserExists(string email)
        //{
        //    var content = new FormUrlEncodedContent(new[]
        //    {
        //        new KeyValuePair<string,string>("email",email)
        //    });

        //    var response = await httpClient.PostAsync(baseUri + "Account/UserExists", content).ConfigureAwait(false);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var result = response.Content.ReadAsStringAsync().Result;
        //        return JsonSerializer.Deserialize<AuthenticationResponse>(result);
        //    }

        //    return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = response.ReasonPhrase };
        //}

        public async Task<AuthenticationResponse> GetPasswordFailuresSinceLastSuccess(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "User doesn't exist with the specified email." };

            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            return new AuthenticationResponse
            {
                ResponseType = ResponseType.Success,
                Data = "Total failed access count:" + failedAttempts.ToString()
            };
        }

        //private async Task<string> GetAdminToken(string adminUsername, string adminPassword)
        //{
        //    var tokenClient = new TokenClient(authority + "connect/token", clientId, clientSecret);
        //    var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(adminUsername, adminPassword, scopes);

        //    if (tokenResponse.IsError)
        //        return null;

        //    return tokenResponse.AccessToken;
        //}

        public async Task<BaseModel> GenerateChangeEmailToken(string userId, string email)
        {
            // Generate the token and send it
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
            return new BaseModel { Success = true, Data = token, Message = "Change email token has been successfully created." };

            //// ToDo: Check how it works in SingleSignOn
            //var link = webUrl + "Account/ChangeEmail?userId=" + user.Id + "&email=" + email + "&code=" + HttpUtility.UrlEncode(token);
            //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailChangePassword, NotificationTypes.Email);
            //var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
            //                                       .Replace("#Link", $"{link}");

            //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, email);
            //return new BaseModel { success = true, message = $"A confirmation link has been sent to {email}, please verify your email to change it." };
        }

        public async Task<BaseModel> ChangeEmail(string userId, string email, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.ChangeEmailAsync(user, email, code);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = "Your email has been changed successfully." };

            var message = result.Errors.FirstOrDefault() != null
               ? result.Errors.FirstOrDefault().Description
               : "Faild to change email.";
            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> GenerateChangePhoneNumberToken(string userId, string phoneNumber)
        {
            // ToDo: Check how it works in SingleSignOn

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            return new BaseModel { Success = true, Data = code, Message = "Change phone token has been successfully created." };

            //// ToDo: Check how it works in SingleSignOn
            //if (!await _communicationService.SendSms()) // Todo: Phone notification is not done yet.
            //    return new BaseModel { success = false, message = "Sms could not be sent." };

            //return new BaseModel { success = true, message = $"Sms has been sent to {phoneNumber}." };
        }

        public async Task<BaseModel> ValidateChangePhoneNumberToken(string userId, string phoneNumber, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user, phoneNumber, code);
            if (!result)
                return new BaseModel { Success = false, Message = "Code is not correct." }; 

            return new BaseModel { Success = true, Message = "Phone number verified successfully." };
        }

        public async Task<BaseModel> ChangePhoneNumber(string userId, string phoneNumber, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, code);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = "Your phone number has been changed successfully." };

            string messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new BaseModel { Success = false, Message = messaeg };
        }

        public async Task<BaseModel> RemovePhoneNumber(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.SetPhoneNumberAsync(user, null);
            if (!result.Succeeded)
                return new BaseModel { Success = false, Message = "Phone number could not be deleted." };

            return new BaseModel { Success = true, Message = "Your phone number has been deleted successfully." };
        }

        public async Task<BaseModel> GetSharedKeyAndQrCodeUri(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var model = new AuthenticatorModel();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);
            if (string.IsNullOrWhiteSpace(model.SharedKey))
                return new BaseModel { Success = false, Message = "Shared key could not be generated." };

            return new BaseModel { Success = true, Data = model };
        }

        public async Task<BaseModel> GenerateTwoFactorRecoveryCodes(string userId, int numberOfCodes = 0)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            if (!user.TwoFactorEnabled)
                return new BaseModel { Success = false, Message = "Cannot generate recovery codes for user, as they do not have two factor authentication enabled." };

            if (numberOfCodes <= 0)
                numberOfCodes = numberOfRecoveryCodes;

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, numberOfCodes);

            if (!recoveryCodes.Any())
                return new BaseModel { Success = false, Message = "Recovery codes could not be generated." };

            return new BaseModel { Success = true, Message = "Recovery codes generated successfully.", Data = recoveryCodes };
        }

        public async Task<BaseModel> SendTwoFactorToken(string userName, string provider)
        {
            throw new NotImplementedException();

            //// ToDo: Check how it works in SingleSignOn
            //var user = await _userManager.FindByEmailAsync(userName);
            //if (user == null)
            //{
            //    user = await _userManager.FindByNameAsync(userName);
            //    if (user == null)
            //        return new BaseModel { IsSuccess = false, Message = "User not exists." };
            //}

            //var token = await _userManager.GenerateTwoFactorTokenAsync(user, provider);

            //// ToDo: If provder = Email, send code to email Else send code to phone.

            //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailTwoFactorToken, NotificationTypes.Email);
            //var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
            //                                       .Replace("#Token", $"{token}");

            //var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);

            //return new BaseModel { IsSuccess = true, Message = $"A code has been sent to {user.Email}, please verify the code." };
        }

        public async Task<BaseModel> GenerateTwoFactorToken(string userName, string provider)
        {
            // ToDo: Check how it works in SingleSignOn

            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new BaseModel { Success = false, Message = "User not exists." };
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            return new BaseModel { Success = true, Data = token, Message = $"Two factor code has been created successfully." };
        }

        public async Task<BaseModel> VerifyTwoFactorToken(string userName, string provider, string code)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new BaseModel { Success = false, Message = "User not exists." };
            }

            var isEmailTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, provider, code);

            if (!isEmailTokenValid)
                return new BaseModel { Success = false, Message = "Invalid token." };

            return new BaseModel { Success = true, Message = "Token is valid." };
        }

        public async Task<BaseModel> EnableTwoFactorAuthentication(string userId, string tokenProvider, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var tokenVerificationResult = await VerifyTwoFactorToken(user.UserName, tokenProvider, code);
            if (!tokenVerificationResult.Success)
                return new BaseModel { Success = false, Message = tokenVerificationResult.Message };

            var twoFactorResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (!twoFactorResult.Succeeded)
            {
                var message = twoFactorResult.Errors.FirstOrDefault() != null
                                   ? twoFactorResult.Errors.FirstOrDefault().Description
                                   : "Faild to enable two factor authentication.";
                return new BaseModel { Success = false, Message = message };
            }

            //var twoFactorType = await _securityDbContext.TwoFactorTypes.FirstOrDefaultAsync(t => t.Name.Equals(tokenProvider));
            //if (twoFactorType == null)
            //    throw new Exception($"{nameof(twoFactorType)} is not found in the system.");

            //user.TwoFactorTypeId = twoFactorType.Id;
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                var message = userUpdateResult.Errors.FirstOrDefault() != null
                                   ? userUpdateResult.Errors.FirstOrDefault().Description
                                   : "Faild to enable two factor authentication.";
                return new BaseModel { Success = false, Message = message };
            }

            var recoveryCodesResult = await GenerateTwoFactorRecoveryCodes(userId, numberOfRecoveryCodes);
            if(!recoveryCodesResult.Success)
                return new BaseModel { Success = false, Message = recoveryCodesResult.Message };

            return new BaseModel { Success = true, Message = "Two factor authentication enabled successfully.", Data = recoveryCodesResult.Data };
        }

        public async Task<BaseModel> ResetAuthenticator(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var disableAuthenticatorResult =  await DisableTwoFactorAuthentication(userId);
            if (!disableAuthenticatorResult.Success)
                return new BaseModel { Success = false, Message = disableAuthenticatorResult.Message };

            var resetAuthenticatorResult = await _userManager.ResetAuthenticatorKeyAsync(user);
            if (!resetAuthenticatorResult.Succeeded)
            {
                var message = resetAuthenticatorResult.Errors.FirstOrDefault() != null
                                   ? resetAuthenticatorResult.Errors.FirstOrDefault().Description
                                   : "Faild to rest authenticator.";
                return new BaseModel { Success = false, Message = message };
            }
            return new BaseModel { Success = true, Message = "Authenticator has been reset successfully." };
        }

        public async Task<BaseModel> DisableTwoFactorAuthentication(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!result.Succeeded)
            {
                var message = result.Errors.FirstOrDefault() != null
                                                       ? result.Errors.FirstOrDefault().Description
                                                       : "Faild to disable two factor authentication.";
                return new BaseModel { Success = false, Message = message };
            }

            user.TwoFactorTypeId = TwoFactorTypes.None;
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                var message = userUpdateResult.Errors.FirstOrDefault() != null
                                   ? userUpdateResult.Errors.FirstOrDefault().Description
                                   : "Faild to disable two factor authentication.";
                return new BaseModel { Success = false, Message = message };
            }
            return new BaseModel { Success = true, Message = "Two factor authentication disabled successfully." };
        }

        public async Task InitializeUsers(string email, string password, string role, bool isClaimedBasedRole = true)
        {
            var appUser = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var result = await _userManager.CreateAsync(appUser, password);
                if (result.Succeeded)
                {
                    if (isClaimedBasedRole)
                        await _userManager.AddClaimAsync(appUser, new Claim(JwtClaimTypes.Role, role));
                    else
                        await _userManager.AddToRoleAsync(appUser, role);
                }
            }
        }

        public async Task<BaseModel> Logout()
        {
            await _signInManager.SignOutAsync();
            return new BaseModel { Success = true, Message = "Signed out successfully." };
        }


        #region Private Methods



        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
                result.Append(unformattedKey.Substring(currentPosition));

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                authenticatorUriFormat,
                _urlEncoder.Encode("Boilerplate"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user, AuthenticatorModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey);
            model.AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey);
        }

        private async Task AddPreviousPassword(ApplicationUser user, string newPassword)
        {
            var previousPassword = new PreviousPassword
            {
                UserId = user.Id,
                PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword),
                CreateDate = DateTime.Now
            };
            //await _securityDbContext.PreviousPasswords.AddAsync(previousPassword);
            //await _securityDbContext.SaveChangesAsync();
        }

        //private async Task<AuthenticationResponse> ValidatePreviousPassword(ApplicationUser user, string newPassword)
        //{
        //    var isPreviousPassword = 
        //        await _securityDbContext.PreviousPasswords.Where(x => x.UserId.Equals(user.Id))
        //                                                  .OrderByDescending(x => x.CreateDate)
        //                                                  .Take(securityOptions.PreviousPasswordValidationLimit)
        //                                                  .AnyAsync(x => _userManager.PasswordHasher.VerifyHashedPassword(user, x.PasswordHash, newPassword) != PasswordVerificationResult.Failed);

        //    return isPreviousPassword
        //            ? new AuthenticationResponse { ResponseType = ResponseType.Error, Data = "You cannot use your previous passwords." }
        //            : new AuthenticationResponse { ResponseType = ResponseType.Success };
        //}

        #endregion Private Methods
    }
}