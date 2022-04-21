using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BoilerplateCore.Common.Extensions
{
    public static class UserExtensions
    {
        public static string GetAccessToken(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        }

        public static string GetName(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        public static string GetUserFullName(this ClaimsPrincipal principal)
        {
            var firstName = principal.Claims
                                     .FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;

            var lastName = principal.Claims
                                    .FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;

            return $"{firstName} {lastName}";
        }

        public static string GetUserFistName(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        }

        public static string GetUserLastName(this ClaimsPrincipal principal)
        {
            return principal.Claims
                            .FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        }

        public static bool UserIsInRole(this ClaimsPrincipal principal, string role)
        {
            var actualRole = principal.Claims
                                      .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            return actualRole == role;
        }
    }
}
