// -----------------------------------------------------------------------
// <copyright file="AnonymousRequirement.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Mobile.API.Middleware
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using BoilerplateCore.Common.Authentication;

    public class AnonymousRequirement : AuthorizationHandler<AnonymousRequirement>, IAuthorizationRequirement
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<AnonymousRequirement> logger;

        public AnonymousRequirement(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<AnonymousRequirement> logger)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AnonymousRequirement requirement)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Note : Any exception will send a 401 instead of 500 previously.
            // Only anonymous unexpired token should go through this handler (see git history for previous issue explanation & sources).
            try
            {
                string authHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];

                //if (TryGetToken(authHeader, out string token) && AuthenticationHelper.IsValidAnonymousToken(token))
                //{
                //    var key = configuration["Auth0:ClientSecret"];
                //    var json = AuthenticationHelper.Decode(token, key);

                //    var principal = new ClaimsPrincipal(new ClaimsIdentity(AuthenticationHelper.GetUserClaims(json)));
                //    httpContextAccessor.HttpContext.User = principal;
                //    context.Succeed(requirement);
                //}
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message + ex.StackTrace);
            }

            return Task.CompletedTask;
        }

        private bool TryGetToken(string authHeader, out string token)
        {
            token = string.Empty;

            if (authHeader != null && authHeader.Contains("Bearer"))
            {
                token = authHeader.Replace("Bearer ", string.Empty);
            }

            return !string.IsNullOrWhiteSpace(token);
        }
    }
}
