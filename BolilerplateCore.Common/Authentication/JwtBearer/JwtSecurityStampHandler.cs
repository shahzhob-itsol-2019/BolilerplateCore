//// -----------------------------------------------------------------------
//// <copyright file="JwtSecurityStampHandler.cs" company="Playtertainment">
//// Copyright (c) Playtertainment. All rights reserved.
//// </copyright>
//// -----------------------------------------------------------------------

//namespace BoilerplateCore.Common.Authentication.JwtBearer
//{
//    using System.Linq;
//    using System.Security.Claims;
//    using System.Threading.Tasks;
//    using Abp;
//    using Abp.Dependency;
//    using Abp.Runtime.Caching;

//    public class JwtSecurityStampHandler : IJwtSecurityStampHandler, ITransientDependency
//    {
//        private readonly ICacheManager cacheManager;

//        public JwtSecurityStampHandler(
//            ICacheManager cacheManager)
//        {
//            this.cacheManager = cacheManager;
//        }

//        public async Task<bool> Validate(ClaimsPrincipal claimsPrincipal)
//        {
//            if (claimsPrincipal?.Claims == null || !claimsPrincipal.Claims.Any())
//            {
//                return false;
//            }

//            var securityStampKey = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == Constants.SecurityStampKey);
//            if (securityStampKey == null)
//            {
//                return false;
//            }

//            var userIdentifierString = claimsPrincipal.Claims.First(c => c.Type == Constants.UserIdentifier);
//            var userIdentifier = UserIdentifier.Parse(userIdentifierString.Value);

//            var isValid = await ValidateSecurityStampFromCache(userIdentifier, securityStampKey.Value);
//            /*
//             if (!isValid)
//            {
//                isValid = await ValidateSecurityStampFromDb(userIdentifier, securityStampKey.Value);
//            }
//            */

//            return isValid;
//        }

//        public async Task SetSecurityStampCacheItem(long userId, string securityStamp)
//        {
//            await cacheManager.GetCache(Constants.SecurityStampKey)
//                .SetAsync(GenerateCacheKey(userId), securityStamp);
//        }

//        public async Task RemoveSecurityStampCacheItem(long userId)
//        {
//            await cacheManager.GetCache(Constants.SecurityStampKey).RemoveAsync(GenerateCacheKey(userId));
//        }

//        private string GenerateCacheKey(long userId) => $"{userId}";

//        private async Task<bool> ValidateSecurityStampFromCache(UserIdentifier userIdentifier, string securityStamp)
//        {
//            var securityStampKey = await cacheManager
//                .GetCache(Constants.SecurityStampKey)
//                .GetOrDefaultAsync(GenerateCacheKey(userIdentifier.UserId));

//            return securityStampKey != null && (string)securityStampKey == securityStamp;
//        }
//    }
//}
