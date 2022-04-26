using BoilerplateCore.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AuthenticationResponse = BoilerplateCore.Common.Models.AuthenticationResponse;
using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;

namespace BoilerplateCore.Core.ISecurity
{
    public interface ISecurityService
    {
        Task<BaseModel> CreateUser(RegisterUserModel model);

        //Reasoning: Since this project does't have knowlege about IdentityUser,IdentityUser<Tkey> hence object datatype is selected
        Task<AuthenticationResponse> CreateUser(object user, string password);

        Task<LoginResponse> CreateExternalUser(RegisterExternalModel model);

        Task<AuthenticationResponse> GenerateEmailVerificationToken(string email);
        Task<BaseModel> UpdateUserDetail(UserModel userInfo);

        Task<BaseModel> ConfirmEmail(string userId, string code);

        Task<LoginResponse> Login(string Email, string password, bool persistCookie = false);

        Task<LoginResponse> ExternalLogin(string loginProvider, string providerKey, bool isPersistent = false, bool bypassTwoFactor = false);

        Task<LoginResponse> TwoFactorLogin(string provider, string code, bool persistCookie = false, bool rememberMachine = false);

        Task<LoginResponse> RecoveryCodeLogin(string code);

        Task<BaseModel> GetLoginProviders();

        Task<BaseModel> GetLogins(string userId);

        Task<BaseModel> GetLoginProperties(string provider, string redirectUrl, string userId = null);

        Task<BaseModel> AddLogin(string userId, string loginProvider, string providerKey, string displayName);

        Task<BaseModel> RemoveLogin(string userId, string provider, string providerKey);

        Task<BaseModel> GetAuthenticationDetail(string userName);

        Task<BaseModel> GetUser(string userName);

        Task<BaseModel> GetExternalUser(string loginProvider, string providerKey);

        Task<BaseModel> GetUserDetail(string userId);

        Task<BaseModel> GetUsers();

        Task<BaseModel> GetUsers(Expression<Func<object, bool>> where);

        Task<BaseModel> GetUsers(Expression<Func<object, bool>> where = null, Func<IQueryable<object>, IOrderedQueryable<object>> orderBy = null, params Expression<Func<object, object>>[] includeProperties);

        Task<BaseModel> BlockUser(string userId);

        Task<AuthenticationResponse> AddUserClaim(string userId, string claimType, string claimValue);

        Task<BaseModel> GetUserClaim(string userId);

        Task<AuthenticationResponse> RemoveUserClaim(string userId, string claimType, string claimValue);

        Task<AuthenticationResponse> CreateRole(string role);

        Task<AuthenticationResponse> UpdateRole(string id, string role);

        Task<BaseModel> GetRole(string roleName);

        Task<BaseModel> GetRoles();

        Task<AuthenticationResponse> RemoveRole(string roleName);

        Task<AuthenticationResponse> AddUserRole(string userId, IEnumerable<string> roles);

        Task<BaseModel> GetUserRoles(string userId);

        Task<AuthenticationResponse> RemoveUserRole(string userId, string roleName);

        Task<AuthenticationResponse> RemoveUserRoles(string userId, IEnumerable<string> roles);

        Task<BaseModel> ForgotPassword(string email);

        Task<BaseModel> GeneratePasswordResetToken(string email);
        Task<AuthenticationResponse> ValidatePasswordResetToken(string userId, string token);

        Task<AuthenticationResponse> ResetPassword(string email, string code, string password);

        Task<AuthenticationResponse> ChangePassword(string userName, string currentPassword, string newPassword);

        Task<BaseModel> SetPassword(string userId, string newPassword);

        Task<AuthenticationResponse> GetPasswordFailuresSinceLastSuccess(string email);

        Task<BaseModel> GenerateChangeEmailToken(string userId, string email);

        Task<BaseModel> ChangeEmail(string userId, string email, string code);

        Task<BaseModel> GenerateChangePhoneNumberToken(string userId, string phoneNumber);

        Task<BaseModel> ValidateChangePhoneNumberToken(string userId, string phoneNumber, string code);

        Task<BaseModel> ChangePhoneNumber(string userId, string phoneNumber, string code);

        Task<BaseModel> RemovePhoneNumber(string userId);

        Task<BaseModel> GetSharedKeyAndQrCodeUri(string userId);

        Task<BaseModel> GenerateTwoFactorRecoveryCodes(string userId, int numberOfCodes = 0);

        Task<BaseModel> SendTwoFactorToken(string userName, string provider);

        Task<BaseModel> GenerateTwoFactorToken(string userName, string provider);

        Task<BaseModel> VerifyTwoFactorToken(string userName, string provider, string code);

        Task<BaseModel> EnableTwoFactorAuthentication(string userId, string tokenProvider, string code);

        Task<BaseModel> ResetAuthenticator(string userId);

        Task<BaseModel> DisableTwoFactorAuthentication(string userId);

        Task<BaseModel> Logout();
    }
}
