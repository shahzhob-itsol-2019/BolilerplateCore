using Boilerplate.IServices;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.Models;
using BoilerplateCore.Data.Options;
using BoilerplateCore.Data.Templates;
using BoilerplateCore.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using static BoilerplateCore.Data.Constants.Enums;

namespace ARRMM.Services
{
    public class SecurityAspnetIdentity : ISecurityService
    {
        //private readonly SecurityOptions securityOptions;
        private readonly BoilerplateOptions _arrmmOptions;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        protected readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICommunicationService _communicationService;
        protected readonly IDbContext _dbContext;
        private readonly UrlEncoder _urlEncoder;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string clientId = string.Empty;
        private string clientSecret = string.Empty;
        private string authenticatorUriFormat = string.Empty;
        private int numberOfRecoveryCodes;
        private string scopes = string.Empty;
        private string apiUrl = string.Empty;

        public SecurityAspnetIdentity(
            //IOptionsSnapshot<SecurityOptions> securityOptions,
            IOptionsSnapshot<BoilerplateOptions> arrmmOptions,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ICommunicationService communicationService,
            IDbContext dbContext,
            UrlEncoder urlEncoder,
            IHttpContextAccessor httpContextAccessor)
        {
            //this.securityOptions = securityOptions.Value;
            this._arrmmOptions = arrmmOptions.Value;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _communicationService = communicationService;
            _dbContext = dbContext;
            _urlEncoder = urlEncoder;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseModel> CreateUser(string firstName, string lastName, string userName, string email, string nicNumber, string password, string confirmpassword, bool createActivated)
        {
            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
                Email = email,
                //PhoneNumber = phoneNumber,
                NicNumber = nicNumber,
                EmailConfirmed = createActivated
            };

            var existingUser = _userManager.Users.FirstOrDefault(x => x.NicNumber.Equals(nicNumber));
            if(existingUser != null)
                return new BaseModel { Success = false, Message = "User already exists with given NIC Number." };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                //await AddPreviousPassword(user, password);
                await _userManager.AddToRoleAsync(user, UserRoles.Guest.ToString());

                if (!string.IsNullOrWhiteSpace(firstName))
                    await AddUserClaim(user.Id, ClaimTypes.GivenName.ToString(), firstName);
                if (!string.IsNullOrWhiteSpace(lastName))
                    await AddUserClaim(user.Id, ClaimTypes.Surname.ToString(), lastName);
                await AddUserClaim(user.Id, ClaimTypes.Sid.ToString(), user.Id);
                await AddUserClaim(user.Id, ClaimTypes.NameIdentifier, user.UserName);
                await AddUserClaim(user.Id, ClaimTypes.Name, user.UserName);
                await AddUserClaim(user.Id, ClaimTypes.Email, user.Email);
                await AddUserClaim(user.Id, ClaimTypes.Role, UserRoles.Guest.ToString());

                if (!createActivated)
                {
                    var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var link = _arrmmOptions.WebUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(emailConfirmationToken);
                    //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailUserRegisteration, NotificationTypes.Email);
                    var emailMessage = EmailTemplateUserRegisteration.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
                                                                                 .Replace("#Link", $"{link}");

                    var sent = await _communicationService.SendEmail(EmailTemplateUserRegisteration.Subject, emailMessage, user.Email);
                    return new BaseModel { Success = true, Data = emailConfirmationToken, Message = "Account created successfully. A confirmation link has been sent to your specified email , click the link to confirm your email and proceed to login." };
                }
                return new BaseModel { Success = true, Message = "Registeration successfull." };

            }
            var errorMessaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new BaseModel { Success = false, Message = errorMessaeg };
        }

        public async Task<BaseModel> GenerateEmailVerificationToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BaseModel { Success = false, Message = "No user exists with the specified email address." };

            var varficationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            if (string.IsNullOrWhiteSpace(varficationCode))
                return new BaseModel { Success = false, Message = "Email varification could not be generated." };

            return new BaseModel { Success = true, Data = varficationCode };
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

        private async Task<ClaimsPrincipal> Token(ApplicationUser user)
        {
            List<Claim> claims = new List<Claim>();
            claims.AddRange(new List<Claim>
            {
                new Claim(ClaimTypes.GivenName.ToString(), user.FirstName),
                new Claim(ClaimTypes.Surname.ToString(), user.LastName),
                new Claim(ClaimTypes.Sid.ToString(), user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email.ToString(), user.Email),
            });

            //if (!string.IsNullOrWhiteSpace(user.FirstName))
            //    claims.Add(new Claim(JwtClaimTypes.GivenName.ToString(), user.FirstName));
            //if (!string.IsNullOrWhiteSpace(user.LastName))
            //    claims.Add(new Claim(JwtClaimTypes.FamilyName.ToString(), user.LastName));

            var roles = (await _userManager.GetRolesAsync(user));

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role.ToString(), role.ToString()));

            //var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("Core.Secret@ARRMM"));
            //var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);
            //var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            //                issuer: apiUrl,
            //                audience: apiUrl,
            //                claims: claims,
            //                expires: DateTime.Now.AddDays(1),
            //                signingCredentials: credentials);
            //var accessToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
            //return accessToken;

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimPrincipal;
        }

        public async Task<LoginResponseModel> Login(string userName, string password, bool persistCookie = false)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new LoginResponseModel { Success = false, Message = "Invalid user name or password." };
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return new LoginResponseModel { Success = false, Message = "Invalid user name or password." };
            }

            if (!user.EmailConfirmed)
                return new LoginResponseModel { Success = false, Message = "Email has not yet been confirmed , please confirm your email and login again." };

            var result = await _signInManager.PasswordSignInAsync(userName, password, persistCookie, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                // Todo:
                //var map = new TMap();
                //var data = map.Transform<T, TKey>(user, roles);

                //var accessToken = await Token(user);
                var claims = await Token(user);

                //var tokenClient = new TokenClient("http://localhost:54866" + "/connect/token", userName, password);
                //var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(userName, password);

                return new LoginResponseModel { Success = true, Data = claims };
            }
            //else if (result.RequiresTwoFactor)
            //{
            //    //// ToDo: Check how SendTwoFactorToken works in SingleSignOn

            //    //var authenticationResult = await GetAuthenticationDetail(user.UserName);
            //    //if (authenticationResult.IsSuccess)
            //    //{
            //    //    UserAuthenticationInfo authenticationDetail = (UserAuthenticationInfo)authenticationResult.Data;

            //    //    if (authenticationDetail.TwoFactorType == Infrastructure.Utility.Constants.TwoFactorTypes.Email)
            //    //        await SendTwoFactorToken(user.UserName, Infrastructure.Utility.Constants.TwoFactorTypes.Email);
            //    //    else if (authenticationDetail.TwoFactorType == Infrastructure.Utility.Constants.TwoFactorTypes.Phone)
            //    //        await SendTwoFactorToken(user.UserName, Infrastructure.Utility.Constants.TwoFactorTypes.Phone);

            //    //    return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = authenticationDetail };
            //    //}
            //    //return new LoginResponse { Status = LoginStatus.Failed, Message = authenticationResult.Message };

            //    return new LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = user.UserName };
            //}
            else
            {
                return new LoginResponseModel { Success = false, Message = result.IsLockedOut ? "Locked Out" : "Invalid login attempt." };
            }
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

            var userClaims = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                NicNumber = user.NicNumber,
                Roles = roles
            };
            return new BaseModel { Success = true, Data = userClaims };
        }

        public async Task<BaseModel> GetUserDetail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not exists." };

            var userInfo = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                NicNumber = user.NicNumber
            };
            return new BaseModel { Success = true, Data = userInfo };
        }

        public async Task<BaseModel> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null)
                return new BaseModel { Success = false, Message = "No users exist." };

            var userDetails = new List<UserDetailModel>();
            foreach (var user in users)
            {
                var userDetail = new UserDetailModel();
                userDetail.Id = user.Id;
                userDetail.FirstName = user.FirstName;
                userDetail.LastName = user.LastName;
                userDetail.UserName = user.UserName;
                userDetail.Email = user.Email;
                userDetail.NicNumber = user.NicNumber;
                userDetail.PhoneNumber = user.PhoneNumber;
                userDetail.IsEmailConfirmed = user.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(user.PasswordHash);
                userDetail.TwoFactorEnabled = user.TwoFactorEnabled;
                userDetail.LockoutEnabled = user.LockoutEnabled;
                userDetail.LockoutEnd = user.LockoutEnd;
                userDetail.AccessFailedCount = user.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(user);
                var userLogins = userLoginProvders.Select(userLogin => new BoilerplateCore.Data.Models.UserLoginInfo
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

            var userDetails = new List<UserDetailModel>();
            foreach (var user in users)
            {
                var applicationUser = (ApplicationUser)user;
                var userDetail = new UserDetailModel();
                userDetail.Id = applicationUser.Id;
                userDetail.FirstName = applicationUser.FirstName;
                userDetail.LastName = applicationUser.LastName;
                userDetail.UserName = applicationUser.UserName;
                userDetail.Email = applicationUser.Email;
                userDetail.NicNumber = applicationUser.NicNumber;
                userDetail.PhoneNumber = applicationUser.PhoneNumber;
                userDetail.IsEmailConfirmed = applicationUser.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(applicationUser.PasswordHash);
                userDetail.TwoFactorEnabled = applicationUser.TwoFactorEnabled;
                userDetail.LockoutEnabled = applicationUser.LockoutEnabled;
                userDetail.LockoutEnd = applicationUser.LockoutEnd;
                userDetail.AccessFailedCount = applicationUser.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(applicationUser);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(applicationUser);
                var userLogins = userLoginProvders.Select(userLogin => new BoilerplateCore.Data.Models.UserLoginInfo
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

            if (where != null)
                query = query.Where(where);
            if (includeProperties != null)
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            if (orderBy != null)
                query = orderBy(query);

            var users = await query.ToListAsync();
            if (users == null)
                return new BaseModel { Success = false, Message = "No users exist." };

            var userDetails = new List<UserDetailModel>();
            foreach (var user in users)
            {
                var applicationUser = (ApplicationUser)user;
                var userDetail = new UserDetailModel();
                userDetail.Id = applicationUser.Id;
                userDetail.FirstName = applicationUser.FirstName;
                userDetail.LastName = applicationUser.LastName;
                userDetail.UserName = applicationUser.UserName;
                userDetail.Email = applicationUser.Email;
                userDetail.NicNumber = applicationUser.NicNumber;
                userDetail.PhoneNumber = applicationUser.PhoneNumber;
                userDetail.IsEmailConfirmed = applicationUser.EmailConfirmed;
                userDetail.HasPassword = string.IsNullOrWhiteSpace(applicationUser.PasswordHash);
                userDetail.TwoFactorEnabled = applicationUser.TwoFactorEnabled;
                userDetail.LockoutEnabled = applicationUser.LockoutEnabled;
                userDetail.LockoutEnd = applicationUser.LockoutEnd;
                userDetail.AccessFailedCount = applicationUser.AccessFailedCount;

                var roles = await _userManager.GetRolesAsync(applicationUser);
                foreach (var role in roles)
                {
                    userDetail.Roles.Add(role);
                }

                var userLoginProvders = await _userManager.GetLoginsAsync(applicationUser);
                var userLogins = userLoginProvders.Select(userLogin => new BoilerplateCore.Data.Models.UserLoginInfo
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

            var lockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!lockoutResult.Succeeded)
            {
                var message = lockoutResult.Errors.FirstOrDefault() != null
                                   ? lockoutResult.Errors.FirstOrDefault().Description
                                   : "User could not be block.";
                return new BaseModel { Success = false, Message = message };
            }
            return new BaseModel { Success = false, Message = "User has been successfully blocked." };
        }

        public async Task<BaseModel> AddUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower());

                if (givenClaim != null)
                    return new BaseModel { Success = false, Message = "The specified claim already assigned to user, try different value." };
            }
            var result = await _userManager.AddClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
                return new BaseModel { Success = true, Message = "Claim added." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : "Failed to add claim.";

            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> GetUserClaim(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Data = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);
            return new BaseModel { Success = false, Data = userClaims };
        }

        public async Task<BaseModel> RemoveUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

            var userClaims = await _userManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower() && x.Value.ToLower() == claimValue.ToLower());

                if (givenClaim == null)
                    return new BaseModel { Success = false, Message = "User doesn't have the specified claim." };
            }

            var result = await _userManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
                return new BaseModel { Success = true, Message = "Claim removed successfully." };

            return new BaseModel
            {
                Success = false,
                Message = result.Errors.FirstOrDefault() != null ?
                result.Errors.FirstOrDefault().Description : "Failed to remove claim."
            };
        }

        public async Task<BaseModel> CreateRole(string role)
        {
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (roleExist)
                return new BaseModel { Success = false, Message = $"{role} role already exists." };

            var identityRole = new IdentityRole
            {
                Name = role
            };
            var result = await _roleManager.CreateAsync(identityRole);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = $"{role} role successfully added." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to add {role} role.";

            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> UpdateRole(string id, string role)
        {
            var existingRole = await _roleManager.FindByIdAsync(id);
            if (existingRole == null)
                return new BaseModel { Success = false, Message = $"Role not found with specified Id." };

            existingRole.Name = role;
            var result = await _roleManager.UpdateAsync(existingRole);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = $"Role successfully updated." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to update role.";

            return new BaseModel { Success = false, Message = message };
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

        public async Task<BaseModel> RemoveRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return new BaseModel { Success = false, Message = $"{role} role not found." };

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = $"{role} role removed successfully." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to remove {role} role.";

            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> AddUserRole(string userId, IEnumerable<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

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
                return new BaseModel
                {
                    Success = false,
                    Message = $"{notFoundRoles.Remove(notFoundRoles.LastIndexOf(','))} {(notExistingRoles.Count() > 1 ? "roles" : "role")} not found in the system."
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
                return new BaseModel
                {
                    Success = false,
                    Message = $"User is already in {alreadyFoundUserRoles.Remove(alreadyFoundUserRoles.LastIndexOf(','))} {(alreadyFoundUserRoles.Count() > 1 ? "roles" : "role")}."
                };

            var result = await _userManager.AddToRolesAsync(user, roles);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = $"{(roles.Count() > 1 ? "Roles" : "Role")} successfully added for user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to add {(roles.Count() > 1 ? "roles" : "role")} for user.";

            return new BaseModel { Success = false, Message = message };
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

        public async Task<BaseModel> RemoveUserRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

            var role = await _roleManager.FindByIdAsync(roleName);
            if (role == null)
                return new BaseModel { Success = false, Message = $"{roleName} role not found in the system." };

            var userInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!userInRole)
                return new BaseModel { Success = false, Message = $"User do not have {roleName} role." };

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = $"{roleName} role removed successfully from user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : $"Failed to remove {roleName} role form user.";

            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> RemoveUserRoles(string userId, IEnumerable<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new BaseModel { Success = false, Message = "User not found with specified Id." };

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
                return new BaseModel
                {
                    Success = false,
                    Message = $"{notFoundRoles.Remove(notFoundRoles.LastIndexOf(','))} {(notExistingRoles.Count() > 1 ? "roles" : "role")} not found in the system."
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
                return new BaseModel
                {
                    Success = false,
                    Message = $"User is not in {notFoundUserRoles.Remove(notFoundUserRoles.LastIndexOf(','))} {(notFoundUserRoles.Count() > 1 ? "roles" : "role")}."
                };

            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded)
                return new BaseModel { Success = true, Message = "Roles removed successfully from user." };

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : "Failed to remove roles form user.";

            return new BaseModel { Success = false, Message = message };
        }

        public async Task<BaseModel> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BaseModel { Success = false, Message = "No user exists with the specified email." };

            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = _arrmmOptions.WebUrl + "Account/ResetPassword?code=" + HttpUtility.UrlEncode(resetCode);
            //var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailForgotPassword, NotificationTypes.Email);
            var emailMessage = EmailTemplateForgotPassword.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
                                                                 .Replace("#Link", $"{link}");

            var sent = await _communicationService.SendEmail(EmailTemplateForgotPassword.Subject, emailMessage, user.Email);

            return new BaseModel { Success = true, Message = "Your password reset code has been sent to your specified email address, follow the link to reset your password." };
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

        public async Task<BaseModel> ValidatePasswordResetToken(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new BaseModel { Success = false, Message = "Please provide a valid token to validate." };
            }

            var result = await _userManager.VerifyUserTokenAsync(user, "Default", "ResetPassword", token);
            return result
                    ? new BaseModel { Success = true, Message = "Token is valid." }
                    : new BaseModel { Success = false, Message = "Token is in-valid." };
        }

        public async Task<BaseModel> ResetPassword(string email, string code, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BaseModel { Success = false, Message = "No user exists with the specified user Id." };

            //var previousPasswordValidation = await ValidatePreviousPassword(user, password);
            //if (previousPasswordValidation.ResponseType.Equals(ResponseType.Error))
            //    return previousPasswordValidation;

            var result = await _userManager.ResetPasswordAsync(user, code, password);
            if (result.Succeeded)
            {
                //await AddPreviousPassword(user, password);
                return new BaseModel { Success = true, Message = "Password was reset successfully." };
            }

            return new BaseModel
            {
                Success = false,
                Message = result.Errors.FirstOrDefault() != null
                        ? result.Errors.FirstOrDefault().Description
                        : "Password reset failed."
            };
        }

        public async Task<BaseModel> ChangePassword(string userName, string currentPassword, string newPassword)
        {
            //if (currentPassword == newPassword)
            //    return new BaseModel { Success = false, Message = "New password must not be same as cureent password." };

            var user = await _userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                    return new BaseModel { Success = false, Message = "No user exists with the specified email/username." };
            }

            //var previousPasswordValidation = await ValidatePreviousPassword(user, newPassword);
            //if (previousPasswordValidation.ResponseType.Equals(ResponseType.Error))
            //    return previousPasswordValidation;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                //await AddPreviousPassword(user, newPassword);
                return new BaseModel { Success = true, Message = "Password changed successfully." };
            }

            string messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return new BaseModel { Success = false, Message = messaeg ?? "Failed to change password." };
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
                var userUpdateResult = await _userManager.UpdateAsync(user);
                //await AddPreviousPassword(user, newPassword);
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

        public async Task<BaseModel> GetPasswordFailuresSinceLastSuccess(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new BaseModel { Success = false, Message = "User doesn't exist with the specified email." };

            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            return new BaseModel
            {
                Success = false,
                Message = "Total failed access count:" + failedAttempts.ToString()
            };
        }

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

        public async Task<BaseModel> Logout()
        {
            await _signInManager.SignOutAsync();
            return new BaseModel { Success = true, Message = "Signed out successfully." };
        }
    }
}