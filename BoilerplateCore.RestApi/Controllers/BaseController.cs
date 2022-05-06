using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;

namespace BoilerplateCore.RestApi.Controllers
{
    public class BaseController : Controller
    {

        public string GetUserId()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == JwtClaimTypes.Id)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == JwtClaimTypes.Name)
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
            var identity = User;
            var firstName = identity.Claims
                           .Where(c => c.Type == ClaimTypes.GivenName)
                           .Select(c => c.Value)
                           .SingleOrDefault();

            var lastName = identity.Claims
                           .Where(c => c.Type == ClaimTypes.Surname)
                           .Select(c => c.Value)
                           .SingleOrDefault();

            return  $"{firstName} {lastName}";
        }

        public string GetUserFistName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.GivenName)
                           .Select(c => c.Value)
                           .SingleOrDefault();
        }

        public string GetUserLastName()
        {
            var identity = User;
            return identity.Claims
                           .Where(c => c.Type == ClaimTypes.Surname)
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
    }
}
