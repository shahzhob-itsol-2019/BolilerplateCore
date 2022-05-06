using BoilerplateCore.Common.Models;
using BoilerplateCore.Core.Security;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;

namespace BoilerplateCore.RestApi.Models
{
    public class CurrentUser
    {
        private UserClaims ClaimsContext;
        private ClaimsPrincipal Principal;
        public CurrentUser(IHttpContextAccessor context)
        {
            ClaimsContext = new UserClaims();

            var claims = context.HttpContext.User.Claims.ToList();
            ClaimsContext.Id = claims.GetClaimValue(BoilerplateClaims.Id);
            ClaimsContext.FirstName = claims.GetClaimValue(BoilerplateClaims.FirstName);
            ClaimsContext.LastName = claims.GetClaimValue(BoilerplateClaims.LastName);
            ClaimsContext.UserName = claims.GetClaimValue(BoilerplateClaims.Name);
            ClaimsContext.Email = claims.GetClaimValue(BoilerplateClaims.Email);
            ClaimsContext.Roles = claims.GetClaimValues(BoilerplateClaims.Role).ToList();
        }

        public UserClaims Claims
        {
            get
            {
                return ClaimsContext;
            }
        }
    }
}
