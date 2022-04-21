using BoilerplateCore.Common.Utility.Constants;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Options
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
   

    public static class BoilerplateClaims
    {
        public const string Id = "id";
        public const string Name = "name";
        public const string Email = "email";
        public const string Role = "role";
        public const string ClientId = "client_id";
        public const string Address = "address";
        public const string Gender = "gender";
        public const string BirthDate = "birthdate";
        public const string Picture = "picture";
    }
}
