using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoilerplateCore.Core.Security
{
    public static class ExtensionsClaims
    {
        /// <summary>
        /// Get FirstOrDefault Claim Value
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetClaimValue(this IEnumerable<System.Security.Claims.Claim> claims, string key)
        {
            return claims.FirstOrDefault(p => p.Type.ToLower() == key)?.Value;
        }

        /// <summary>
        /// Get All Claims Values.
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetClaimValues(this IEnumerable<System.Security.Claims.Claim> claims, string key)
        {
            return claims.Where(p => p.Type.ToLower() == key).Select(s => s.Value);
        }
    }
}
