// -----------------------------------------------------------------------
// <copyright file="IJwtSecurityStampHandler.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication.JwtBearer
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    public interface IJwtSecurityStampHandler
    {
        Task<bool> Validate(ClaimsPrincipal claimsPrincipal);

        Task SetSecurityStampCacheItem(long userId, string securityStamp);

        Task RemoveSecurityStampCacheItem(long userId);
    }
}
