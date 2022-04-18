// -----------------------------------------------------------------------
// <copyright file="TokenAuthConfiguration.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication.JwtBearer
{
    using System;
    using Microsoft.IdentityModel.Tokens;

    public class TokenAuthConfiguration
    {
        public SymmetricSecurityKey SecurityKey { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public SigningCredentials SigningCredentials { get; set; }

        public TimeSpan AccessTokenExpiration { get; set; }

        public TimeSpan RefreshTokenExpiration { get; set; }
    }
}
