﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Core.Authorization
{
    public class CustomRequireClaim : IAuthorizationRequirement
    {
        public CustomRequireClaim(string claimType)
        {
            ClaimType = claimType;
        }

        public string ClaimType { get; }
    }

    public class CustomRequireClaimHandler : AuthorizationHandler<CustomRequireClaim>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CustomRequireClaim requirement)
        {
            var hasClaim = context.User.Claims.Any(x => x.Type == requirement.ClaimType);
            if (hasClaim)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireCustomClaim(
            this AuthorizationPolicyBuilder builder,
            string claimType)
        {
            builder.AddRequirements(new CustomRequireClaim(claimType));
            return builder;
        }
    }
    //public static class AuthorizationPolicyBuilderExtensions
    //{
    //    public static AuthorizationPolicyBuilder RequireCustomClaim(
    //        this AuthorizationPolicyBuilder builder,
    //        string claimType)
    //    {
    //        builder.AddRequirements(new CustomRequireClaim(claimType));
    //        return builder;
    //    }
    //}
    //public class CustomRequireClaim : AuthorizationHandler<CustomRequireClaim>, IAuthorizationRequirement
    //{
    //    public string ClaimType { get; }

    //    public CustomRequireClaim(string claimType)
    //    {
    //        ClaimType = claimType;
    //    }

    //    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRequireClaim requirement)
    //    {
    //        //if (context.User.Identity.IsAuthenticated)
    //        //{
    //        //    context.Succeed(requirement);
    //        //    return Task.CompletedTask;
    //        //}

    //        var hasClaim = context.User.Claims.Any(x => x.Type == requirement.ClaimType);
    //        if (hasClaim)
    //        {
    //            context.Succeed(requirement);
    //        }

    //        return Task.CompletedTask;
    //    }
    //}
}
