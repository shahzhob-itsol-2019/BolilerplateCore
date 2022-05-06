using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Models;
using Microsoft.AspNetCore.Mvc;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Web.Controllers
{
    public class BaseController : Controller
    {
        public string GetAccessToken()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.PrimarySid)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserId()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.Sid)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.Name)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserEmail()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.Email)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserFullName()
        {
            var user = GetUser();
            return user == null ? string.Empty : $"{user.FirstName} {user.LastName}";
        }

        public string GetUserFistName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == CustomClaimTypes.FirstName.ToString())
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserLastName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == CustomClaimTypes.LastName.ToString())
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public bool UserIsInRole(string role)
        {
            var identity = User;
            var actualRole = identity.Claims
                                     .Where(c => c.Type == ClaimTypes.Role)
                                     .Select(c => c.Value)
                                     .SingleOrDefault();
            return actualRole == role;
        }

        public UserClaim GetUser()
        {
            var identity = User;
            var user = identity.Claims
                               .Where(c => c.Type == CustomClaimTypes.User.ToString())
                               .Select(c => c.Value)
                               .SingleOrDefault();
            return JsonSerializer.Deserialize<UserClaim>(user ?? "");
        }
    }
}