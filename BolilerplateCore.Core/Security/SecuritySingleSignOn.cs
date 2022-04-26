using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Helpers.Interfaces;
using BoilerplateCore.Common.Models;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Common.Utility.Constants;
using BoilerplateCore.Core.ISecurity;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;
using AuthenticationResponse = BoilerplateCore.Common.Models.AuthenticationResponse;
using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;

namespace BoilerplateCore.Core.Security
{
    public class SecuritySingleSignOn : ISecurityService
    {
        //from config
        private string authority = string.Empty;
        private string baseUri = string.Empty;
        private string clientId = string.Empty;
        private string clientSecret = string.Empty;
        private string scopes = string.Empty;
        private string adminUsername = string.Empty;
        private string adminPassword = string.Empty;

        private IHttpContextAccessor contextAccessor;
        private readonly IHttpClient _httpClient;
        private readonly SecurityOptions securityOptions;
        private readonly BoilerplateOptions boilerplateOptions;

        public SecuritySingleSignOn(
            IOptionsSnapshot<BoilerplateOptions> boilerplateOptions,
            IOptionsSnapshot<SecurityOptions> securityOptions,
            IHttpContextAccessor contextAccessor, IHttpClient httpClient)
        {
            this.securityOptions = securityOptions.Value;
            authority = this.securityOptions.Authority;
            baseUri = authority + "api/";
            clientId = this.securityOptions.ClientId;
            clientSecret = this.securityOptions.ClientSecret;
            scopes = this.securityOptions.Scopes;
            adminUsername = this.securityOptions.AdminUsername;
            adminPassword = this.securityOptions.AdminPassword;

            _httpClient = httpClient;
        }

        public async Task<BaseModel> CreateUser(RegisterUserModel model)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserName", model.UserName),
                new KeyValuePair<string, string>("Email", model.Email),
                new KeyValuePair<string, string>("Password", model.Password),
                new KeyValuePair<string, string>("ClientID", clientId),
                new KeyValuePair<string, string>("ConfirmPassword", model.ConfirmPassword),
                new KeyValuePair<string, string>("CreateActivated", model.CreateActivated.ToString()),
            });

            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "Account/Register", content);
            return new BaseModel { Data = response.Data };
        }
        public async Task<AuthenticationResponse> CreateUser(object user, string password)
        {
            throw new NotImplementedException();
        }
        public async Task<LoginResponse> CreateExternalUser(RegisterExternalModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> GenerateEmailVerificationToken(string email)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("email",email)
            });

            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/GenerateEmailVerificationToken", content);
            return response.Data;
        }

        public async Task<BaseModel> ConfirmEmail(string userId, string code)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("userId",userId),
                new KeyValuePair<string, string>("verificationCode",code)
            });

            var response = await _httpClient.PostAsync<BaseModel>(baseUri + "account/VerifyEmail", content);
            return response.Data;
        }

        //public async Task<BaseModel> SendEmailCode(string userId, string email)
        //{
        //    var userResult = await GetUserDetail(userId);
        //    if (!userResult.Success)
        //        return new BaseModel { Success = false, Message = userResult.Message };
        //    var user = (Common.Models.UserInfo)userResult.Data;

        //    var tokenResult = await GenerateChangeEmailToken(userId, email);
        //    if (!tokenResult.Success)
        //        return new BaseModel { Success = false, Message = tokenResult.Message };
        //    var token = (string)tokenResult.Data;

        //    var link = boilerplateOptions.WebUrl + "Account/ChangeEmail?userId=" + user.Id + "&email=" + email + "&code=" + HttpUtility.UrlEncode(token);
        //    var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailChangePassword, NotificationTypes.Email);
        //    var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
        //                                           .Replace("#Link", $"{link}");

        //    var sent = await _communicationService.SendEmail(template.Subject, emailMessage, email);
        //    if (!sent)
        //        return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

        //    return new BaseModel { Success = true, Message = $"A confirmation link has been sent to {email}, please verify your email to change it." };
        //}

        public async Task<LoginResponse> Login(string userName, string password, bool persistCookie = false)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("UserName",userName),
                new KeyValuePair<string, string>("Password",password),
                new KeyValuePair<string, string>("ClientId",clientId),
                new KeyValuePair<string, string>("ClientSecret",clientSecret),
                new KeyValuePair<string, string>("Scopes",scopes)
            });

            var response = await _httpClient.PostAsync<Common.Models.LoginResponse>(baseUri + "account/login", content);
            return response.Data;
        }

        public async Task<LoginResponse> ExternalLogin(string loginProvider, string providerKey, bool isPersistent = false, bool bypassTwoFactor = false)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponse> TwoFactorLogin(string provider, string code, bool persistCookie = false, bool rememberMachine = false)
        {
            throw new NotImplementedException();
        }

        public async Task<LoginResponse> RecoveryCodeLogin(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetLoginProviders()
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetLogins(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetLoginProperties(string provider, string redirectUrl, string userId = null)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> AddLogin(string userId, string loginProvider, string providerKey, string displayName)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> RemoveLogin(string userId, string provider, string providerKey)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetAuthenticationDetail(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUser(string userName)
        {
            throw new NotImplementedException();
        }
        public async Task<BaseModel> GetExternalUser(string loginProvider, string providerKey)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUserDetail(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUsers()
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUsers(Expression<Func<object, bool>> where)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUsers(Expression<Func<object, bool>> where = null, Func<IQueryable<object>, IOrderedQueryable<object>> orderBy = null, params Expression<Func<object, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> BlockUser(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> AddUserClaim(string userId, string claimType, string claimValue)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("userId",userId),
                new KeyValuePair<string, string>("claimType",claimType),
                new KeyValuePair<string, string>("claimValue",claimValue)
            });

            var token = await GetAdminToken(adminUsername, adminPassword);
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Success,
                    Data = JsonSerializer.Serialize(new BaseModel
                    {
                        Success = false,
                        Message = "you are not authorized to perform this action"
                    })
                };

            await _httpClient.SetBearerToken(token);
            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "Account/adduserclaim", content);
            return response.Data;
        }

        public async Task<BaseModel> GetUserClaim(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> RemoveUserClaim(string userId, string claimType, string claimValue)
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("userId",userId),
                new KeyValuePair<string,string>("claimType",claimType),
                new KeyValuePair<string, string>("claimValue",claimValue)
            });
            var token = await GetAdminToken(adminUsername, adminPassword);
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationResponse
                {
                    ResponseType = ResponseType.Success,
                    Data = JsonSerializer.Serialize(new BaseModel
                    {
                        Success = false,
                        Message = "you are not authorized to perform this action"
                    })
                };

            await _httpClient.SetBearerToken(token);
            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/removeuserclaim", content);
            return response.Data;
        }

        public async Task<AuthenticationResponse> CreateRole(string role)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> UpdateRole(string id, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetRoles()
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> RemoveRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> AddUserRole(string userId, IEnumerable<string> roles)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetUserRoles(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> RemoveUserRole(string userId, string roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> RemoveUserRoles(string userId, IEnumerable<string> roles)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> ForgotPassword(string email)
        {
            throw new NotImplementedException();
            //var content = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string,string>("email",email)
            //});

            //var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/generatepasswordresetToken", content);
            //return response.Data;
        }

        public async Task<BaseModel> GeneratePasswordResetToken(string email)
        {
            throw new NotImplementedException();
            //var content = new FormUrlEncodedContent(new[]
            //{
            //    new KeyValuePair<string,string>("email",email)
            //});

            //var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/GeneratePasswordResetToken", content);
            //return response.Data;
        }

        public async Task<AuthenticationResponse> ValidatePasswordResetToken(string userId, string token)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("userId",userId),
                    new KeyValuePair<string, string>("clientId",clientId),
                    new KeyValuePair<string, string>("token",token)
                });

                var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/validatepasswordresettoken", content);
                return response.Data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<AuthenticationResponse> ResetPassword(string email, string code, string password)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("email",email), // Todo: It was userId, i changed it..
                new KeyValuePair<string, string>("token",code),
                new KeyValuePair<string, string>("newPassword",password)
            });

            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "/account/resetpassword", content);
            return response.Data;
        }

        public async Task<AuthenticationResponse> ChangePassword(string userName, string currentPassword, string newPassword)
        {
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("email",userName),
                new KeyValuePair<string, string>("currentPassword",currentPassword),
                new KeyValuePair<string, string>("newPassword",newPassword)
            });

            var token = contextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Substring(6);
            await _httpClient.SetBearerToken(token);
            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/changePassword", content);
            return response.Data;
        }

        public async Task<BaseModel> SetPassword(string userId, string newPassword)
        {
            throw new NotImplementedException();
        }

        public async Task<AuthenticationResponse> UserExists(string email)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("email",email)
            });

            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "Account/UserExists", content);
            return response.Data;
        }

        public async Task<AuthenticationResponse> GetPasswordFailuresSinceLastSuccess(string email)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("email",email)
            });

            var response = await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/GetPasswordFailuresSinceLastSuccess", content);
            return response.Data;
        }

        private async Task<string> GetAdminToken(string adminUsername, string adminPassword)
        {
            var tokenClient = new TokenClient(authority + "connect/token", clientId, clientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(adminUsername, adminPassword, scopes);

            if (tokenResponse.IsError)
                return null;

            return tokenResponse.AccessToken;
        }

        public async Task<BaseModel> GenerateChangeEmailToken(string userId, string email)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> ChangeEmail(string userId, string email, string code)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GenerateChangePhoneNumberToken(string userId, string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> ValidateChangePhoneNumberToken(string userId, string phoneNumber, string code)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> ChangePhoneNumber(string userId, string phoneNumber, string code)
        {
            throw new NotImplementedException();
        }

        public Task<BaseModel> RemovePhoneNumber(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GetSharedKeyAndQrCodeUri(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> GenerateTwoFactorRecoveryCodes(string userId, int numberOfCodes = 0)
        {
            throw new NotImplementedException();
        }

        public Task<BaseModel> SendTwoFactorToken(string userName, string provider)
        {
            throw new NotImplementedException();
        }

        public Task<BaseModel> GenerateTwoFactorToken(string userName, string provider)
        {
            throw new NotImplementedException();
        }

        public Task<BaseModel> VerifyTwoFactorToken(string userName, string provider, string code)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseModel> EnableTwoFactorAuthentication(string userId, string tokenProvider, string code)
        {
            throw new NotImplementedException();
        }
        public async Task<BaseModel> ResetAuthenticator(string userId)
        {
            throw new NotImplementedException();
        }
        public async Task<BaseModel> DisableTwoFactorAuthentication(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task InitializeUsers(string email, string password, string role, bool isClaimedBasedRole = true)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Email",email),
                new KeyValuePair<string,string>("Password",password),
                new KeyValuePair<string,string>("ClientID",clientId),
                new KeyValuePair<string,string>("Role",role)
            });

            var authToken = await GetAdminToken(adminUsername, adminPassword);
            if (!string.IsNullOrWhiteSpace(authToken))
            {
                await _httpClient.SetBearerToken(authToken);
                await _httpClient.PostAsync<AuthenticationResponse>(baseUri + "account/inituser", content);
            }
        }

        public async Task<BaseModel> Logout()
        {
            //await _signInManager.SignOutAsync();
            return null;
        }

        public Task<BaseModel> UpdateUserDetail(UserModel userInfo)
        {
            throw new NotImplementedException();
        }
    }
}