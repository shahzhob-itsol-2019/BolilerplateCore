using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Models
{
    public class AuthenticationResponse
    {
        public ResponseType ResponseType { get; set; }

        public string Data;

        public AuthenticationResponse()
        {

        }
        public AuthenticationResponse(ResponseType responseType, string data)
        {
            ResponseType = responseType;
            Data = data;
        }
        public static AuthenticationResponse Create(ResponseType responseType, string data)
        {
            return new AuthenticationResponse(responseType, data);
        }

        public static AuthenticationResponse Error(string data)
        {
            return new AuthenticationResponse(ResponseType.Error, data);
        }

        public static AuthenticationResponse Success(string data)
        {
            return new AuthenticationResponse(ResponseType.Success, data);
        }
    }

    public class LoginResponse
    {
        public LoginStatus Status { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
    }
    public class LoggedUser
    {
        public string userId { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ExternalLoginsModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<ExternalAuthenticationProvider> OtherLogins { get; set; }
    }

    public class UserAuthenticationInfo
    {
        public string UserId { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool HasPassword { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorType { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public IList<Microsoft.AspNetCore.Authentication.AuthenticationScheme> OtherLogins { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class UserClaims
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string ClientId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Picture { get; set; }
    }
    
    public class UserDetail
    {
        public UserDetail()
        {
            Roles = new List<string>();
        }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Picture { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool HasPassword { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorType { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public List<string> Roles { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
    }

    public static class BoilerplateClaims
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string FirstName = "firstname";
        public const string LastName = "lastname";
        public const string Email = "email";
        public const string Role = "role";
        public const string ClientId = "client_id";
        public const string Address = "address";
        public const string Gender = "gender";
        public const string BirthDate = "birthdate";
        public const string Picture = "picture";
    }
}
