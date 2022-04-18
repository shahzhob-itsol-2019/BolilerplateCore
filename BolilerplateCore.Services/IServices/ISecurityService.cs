using BoilerplateCore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Boilerplate.IServices
{
    public interface ISecurityService
    {
        Task<BaseModel> CreateUser(string firstName, string lastName, string userName, string email, string nicNumber, string password, string confirmpassword, bool createActivated);

        Task<BaseModel> GenerateEmailVerificationToken(string email);

        Task<BaseModel> ConfirmEmail(string userId, string code);

        Task<LoginResponseModel> Login(string userName, string password, bool persistCookie = false);

        Task<BaseModel> GetUser(string userName);

        Task<BaseModel> GetUserDetail(string userId);

        Task<BaseModel> GetUsers();

        Task<BaseModel> GetUsers(Expression<Func<object, bool>> where);

        Task<BaseModel> GetUsers(Expression<Func<object, bool>> where = null, Func<IQueryable<object>, IOrderedQueryable<object>> orderBy = null, params Expression<Func<object, object>>[] includeProperties);

        Task<BaseModel> BlockUser(string userId);

        Task<BaseModel> AddUserClaim(string userId, string claimType, string claimValue);

        Task<BaseModel> GetUserClaim(string userId);

        Task<BaseModel> RemoveUserClaim(string userId, string claimType, string claimValue);

        Task<BaseModel> CreateRole(string role);

        Task<BaseModel> UpdateRole(string id, string role);

        Task<BaseModel> GetRole(string roleName);

        Task<BaseModel> GetRoles();

        Task<BaseModel> RemoveRole(string roleName);

        Task<BaseModel> AddUserRole(string userId, IEnumerable<string> roles);

        Task<BaseModel> GetUserRoles(string userId);

        Task<BaseModel> RemoveUserRole(string userId, string roleName);

        Task<BaseModel> RemoveUserRoles(string userId, IEnumerable<string> roles);

        Task<BaseModel> ForgotPassword(string email);

        Task<BaseModel> GeneratePasswordResetToken(string email);
        Task<BaseModel> ValidatePasswordResetToken(string userId, string token);

        Task<BaseModel> ResetPassword(string email, string code, string password);

        Task<BaseModel> ChangePassword(string userName, string currentPassword, string newPassword);

        Task<BaseModel> SetPassword(string userId, string newPassword);

        Task<BaseModel> GetPasswordFailuresSinceLastSuccess(string email);

        Task<BaseModel> GenerateChangeEmailToken(string userId, string email);

        Task<BaseModel> ChangeEmail(string userId, string email, string code);

        Task<BaseModel> GenerateChangePhoneNumberToken(string userId, string phoneNumber);

        Task<BaseModel> ValidateChangePhoneNumberToken(string userId, string phoneNumber, string code);

        Task<BaseModel> ChangePhoneNumber(string userId, string phoneNumber, string code);

        Task<BaseModel> RemovePhoneNumber(string userId);

        Task<BaseModel> Logout();
    }
}
