using BoilerplateCore.Common.Models;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Core.Security
{
    public abstract class BaseSecurityService<T, TKey> where TKey : IEquatable<TKey>
        where T : IdentityUser<TKey>, new()
    {
        protected readonly UserManager<T> UserManager;
        protected readonly SignInManager<T> SignInManager;

        protected BaseSecurityService(UserManager<T> userManager, SignInManager<T> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        /// <summary>
        /// Change User Password
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<BaseModel> ChangePassword(string userName, string currentPassword, string newPassword)
        {
            var user = await UserManager.FindByEmailAsync(userName);

            if (user == null)
            {
                user = await UserManager.FindByNameAsync(userName);
                if (user == null)
                {
                    return Fail("No user exists with the specified email address");
                }
            }
            var result = await UserManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                return
                    Success(message: "Password changed successfully");
            }
            string messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return Fail(messaeg ?? "failed to change password");

        }

        /// <summary>
        /// Generate Password Reset Token.
        /// Success : UserId:guid  
        ///           restCode:string
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<BaseModel> GeneratePasswordResetToken(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Fail("No user exists with the specified email");
            }

            var resetCode = await UserManager.GeneratePasswordResetTokenAsync(user);
            return Success(new { userId = user.Id, resetCode });
        }

        /// <summary>
        /// Add Claim to the user.
        /// </summary>
        /// <param name="userId">Primary Key</param>
        /// <param name="claimType"></param>
        /// <param name="claimValue"></param>
        /// <returns></returns>
        public async Task<BaseModel> AddUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Fail("user not found with specified Id");
            }

            var userClaims = await UserManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower());

                if (givenClaim != null)
                {
                    return Success(message: "The specified claim already assigned to user, try different value.");
                }
            }
            var result = await UserManager.AddClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
            {
                return Success(message: "Claim added");
            }

            var message = result.Errors.FirstOrDefault() != null
                ? result.Errors.FirstOrDefault().Description
                : "failed to add claim";

            return Fail(message);
        }

        /// <summary>
        /// Removes Claim from the user
        /// </summary>
        /// <param name="userId">Primary Key</param>
        /// <param name="claimType"></param>
        /// <param name="claimValue"></param>
        /// <returns></returns>
        public async Task<BaseModel> RemoveUserClaim(string userId, string claimType, string claimValue)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Fail("user not found with specified Id");
            }

            var userClaims = await UserManager.GetClaimsAsync(user);

            if (userClaims.Any())
            {
                var givenClaim = userClaims.FirstOrDefault(x => x.Type.ToLower() == claimType.ToLower() && x.Value.ToLower() == claimValue.ToLower());

                if (givenClaim == null)
                {
                    return Success(message: "User doesn't have the specified claim.");

                }
            }

            var result = await UserManager.RemoveClaimAsync(user, new Claim(claimType, claimValue));

            if (result.Succeeded)
            {
                return Success(message: "Claim removed successfully.");

            }

            return Fail(result.Errors.FirstOrDefault() != null ?
                result.Errors.FirstOrDefault().Description : "failed to remove claim");
        }

        /// <summary>
        /// Get Password Failure count.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<BaseModel> GetPasswordFailuresSinceLastSuccess(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Fail("User doesn't exist with the specified email");
            }

            var failedAttempts = UserManager.GetAccessFailedCountAsync(user).Result;
            return Success(message: "Total failed access count:" + failedAttempts.ToString(),
                data: failedAttempts);

        }

        /// <summary>
        /// Generate Email verification token
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<BaseModel> GenerateEmailVerificationToken(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Fail("No user exists with the specified email address");
            }

            var varficationCode = UserManager.GenerateEmailConfirmationTokenAsync(user).Result;

            if (string.IsNullOrWhiteSpace(varficationCode))
            {
                return Fail("Email varification could not be generated.");
            }

            return Success(varficationCode, message: "Email verification token generated.");
        }

        /// <summary>
        /// Verify Email of the user.
        /// </summary>
        /// <param name="userId">Primary Key</param>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        public async Task<BaseModel> VerifyEmail(string userId, string verificationCode)
        {
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Fail("No user exists with the specified email address");
            }

            var response = await UserManager.ConfirmEmailAsync(user, verificationCode);

            if (response.Succeeded)
            {
                return Success(message: "Email confirmed.");
            }
            var message = response.Errors.FirstOrDefault() != null
                ? response.Errors.FirstOrDefault().Description
                : "Email confirmation failed.";
            return
                Fail(message);
        }

        /// <summary>
        /// Register user without additional required field validations.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="confirmpassword"></param>
        /// <returns></returns>
        //public async Task<BaseModel> Register(string name, string email, string password, string confirmpassword)
        //{
        //    var user = new T() { UserName = name, Email = email };
        //    return await Register<NovaDefaultRegistrationResult>(user, password);
        //}
        public async Task<BaseModel> Register(string name, string email, string password, string confirmpassword)
        {
            var user = new T() { UserName = name, Email = email };
            return await Register(user, password);
        }

        /// <summary>
        /// Register users and can cause aditional validation errors.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //public async Task<BaseModel> Register(T user, string password) where TMap : class, INovaIdentityResult, new()
        //{
        //    var result = await UserManager.CreateAsync(user, password);
        //    if (result.Succeeded)
        //    {
        //        var emailConfirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        //        await UserManager.AddClaimAsync(user, new Claim(JwtClaimTypes.Email, user.Email));
        //        await SignInManager.SignInAsync(user, false);

        //        var map = new TMap();
        //        var data = map.Transform<T, TKey>(user, emailConfirmationToken);
        //        //var data = new { userId = user.Id, emailConfirmationToken };
        //        return Success(data, message: "Registeration successfull");

        //    }
        //    var messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
        //    return Fail(messaeg);
        //}
        public async Task<BaseModel> Register(T user, string password)
        {
            var result = await UserManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                var emailConfirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                await UserManager.AddClaimAsync(user, new Claim(JwtClaimTypes.Email, user.Email));
                await SignInManager.SignInAsync(user, false);

                //var map = new TMap();
                //var data = map.Transform<T, TKey>(user, emailConfirmationToken);
                var data = new { UserId = user.Id, Email = user.Email, ConfirmationToken = emailConfirmationToken };
                return Success(data, message: "Registeration successfull");

            }
            var messaeg = result.Errors.Aggregate("", (current, error) => current + (error.Description + "\n")).TrimEnd('\n');
            return Fail(messaeg);
        }

        //public async Task<BaseModel> Register(T user, string password)
        //{
        //    return await Register<NovaDefaultRegistrationResult>(user, password);
        //}

        //public virtual async Task<BaseModel> Login(string userName, string password, bool persistCookie = false)
        //{
        //    return await Login<NovaDefaultLoginResult>(userName, password, persistCookie);
        //}
        public virtual async Task<BaseModel> Login(string userName, string password, bool persistCookie = false)
        {
            var user = await UserManager.FindByEmailAsync(userName);
            if (user == null)
            {
                return Fail("Invalid email address, not such user exists");
            }
            if (!user.EmailConfirmed)
            {
                return Fail("Email has not yet been confirmed , please confirm your email and login again");

            }
            var result = await SignInManager.PasswordSignInAsync(userName, password, persistCookie, lockoutOnFailure: false);

            if (result.Succeeded)
            {

                var roles = (await UserManager.GetRolesAsync(user))
                    .Aggregate("", (current, role) => current + (role + ",")).TrimEnd(',');
                //var map = new TMap();
                //var data = map.Transform<T, TKey>(user, roles);
                var data = new { UserId = user.Id, Email = user.Email };
                return Success(data);
            }

            return result.RequiresTwoFactor ? Fail("Requires two factor varification") : Fail(result.IsLockedOut ? "Locked Out" : "Invalid login attempt.");
        }

        //public virtual async Task<BaseModel> Login<TMap>(string userName, string password, bool persistCookie = false) where TMap : class, INovaIdentityResult, new()
        //{
        //    var user = await UserManager.FindByEmailAsync(userName);
        //    if (user == null)
        //    {
        //        return Fail("Invalid email address, not such user exists");
        //    }
        //    if (!user.EmailConfirmed)
        //    {
        //        return Fail("Email has not yet been confirmed , please confirm your email and login again");

        //    }
        //    var result = await SignInManager.PasswordSignInAsync(userName, password, persistCookie, lockoutOnFailure: false);

        //    if (result.Succeeded)
        //    {

        //        var roles = (await UserManager.GetRolesAsync(user))
        //            .Aggregate("", (current, role) => current + (role + ",")).TrimEnd(',');
        //        var map = new TMap();
        //        var data = map.Transform<T, TKey>(user, roles);
        //        return Success(data);
        //    }

        //    return result.RequiresTwoFactor ? Fail("Requires two factor varification") : Fail(result.IsLockedOut ? "Locked Out" : "Invalid login attempt.");
        //}
        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task<BaseModel> ResetPassword(string userId, string token, string newPassword)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Fail("No user exists with the specified user Id.");
            }


            var result = await UserManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                return Success(message: "Password was reset successfully.");
            }

            return BaseModel.Create(true,
                result.Errors.FirstOrDefault() != null
                    ? result.Errors.FirstOrDefault().Description
                    : "Password reset failed");

        }

        /// <summary>
        /// Used to validate Password Reset Token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<BaseModel> ValidatePasswordResetToken(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (string.IsNullOrWhiteSpace(token))
            {
                return Fail("Please provide a valid token to validate.");
            }

            var result = await UserManager.VerifyUserTokenAsync(user, "Default", "ResetPassword", token);
            return result ? Success("Token is valid.") : Fail("Token is in-valid.");
        }
        /// <summary>
        /// Initialize User with details.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<BaseModel> InitUser(string email, string password, string role, bool isClaimedBasedRole = true)
        {
            var appUser = new T { Email = email, UserName = email };

            var user = await UserManager.FindByEmailAsync(email);
            if (user != null) return Success(message: "Default user already initialized");

            var result = await UserManager.CreateAsync(appUser, password);
            if (!result.Succeeded) return Fail(result.Errors.FirstOrDefault()?.Description);

            if (isClaimedBasedRole)
                await UserManager.AddClaimAsync(appUser, new Claim(JwtClaimTypes.Role, role));
            else
                await UserManager.AddToRoleAsync(appUser, role);

            return Success();
        }

        public async Task<BaseModel> InitUser(T appUser, string password, string role, bool isClaimedBasedRole = true)
        {
            var user = await UserManager.FindByEmailAsync(appUser.Email);
            if (user != null) return Success(message: "Default user already initialized");

            var result = await UserManager.CreateAsync(appUser, password);
            if (!result.Succeeded)
                return Fail(result.Errors.FirstOrDefault()?.Description);

            if (isClaimedBasedRole)
                await UserManager.AddClaimAsync(appUser, new Claim(JwtClaimTypes.Role, role));
            else
                await UserManager.AddToRoleAsync(appUser, role);

            return Success();
        }

        public BaseModel Success(object data = null, int total = 0, string message = "")
        {
            return BaseModel.Succeed(data, total, message);
        }

        public BaseModel Fail(string message, object data = null, int total = 0)
        {
            return BaseModel.Failed(message, data, total);
        }

        public virtual async Task<BaseModel> GetTokenIdentityService(string userName, string password, string authority, string clientId, string clientSecret, string scope)
        {
            BaseModel data = null;
            try
            {
                var tokenClient = new TokenClient(authority + "/connect/token", clientId, clientSecret);
                var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(userName, password, scope);

                if (tokenResponse.IsError)
                {
                    if (tokenResponse.Json == null && !string.IsNullOrEmpty(tokenResponse.Error))
                    {
                        data = Fail(tokenResponse.Error);
                    }
                    if (tokenResponse.Json != null)
                    {
                        var error = (string)tokenResponse.Json["error"];
                        var hasDescription = tokenResponse.Json.TryGetValue("error_description", out JToken errorDescription);
                        data = hasDescription ? Fail((string)errorDescription) : Fail(error);
                    }
                }
                else
                {
                    data = BaseModel.Succeed(tokenResponse.Json);
                }
            }
            catch (Exception e)
            {
                data = BaseModel.Failed(e.Message);
            }
            return await Task.FromResult(data);
        }

        public virtual async Task<BaseModel> Logout()
        {
            await SignInManager.SignOutAsync();
            return await Task.FromResult(BaseModel.Succeed("Logout"));
        }
    }
}
