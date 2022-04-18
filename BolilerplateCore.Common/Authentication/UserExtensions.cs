// -----------------------------------------------------------------------
// <copyright file="UserExtensions.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Extensions for ClaimsPrincipal user to help with Authentication.
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Retrieve a Claims by name and indicates if it succeeded.
        /// </summary>
        /// <param name="user">The current User.</param>
        /// <param name="claimsName">Queried claims name.</param>
        /// <param name="claimsValue">The value of the claims.</param>
        /// <returns>True if the Claims exist with a value.</returns>
        public static bool TryGetUserClaims(this ClaimsPrincipal user, string claimsName, out string claimsValue)
        {
            claimsValue = user?.Claims?.FirstOrDefault(c => c.Type == claimsName)?.Value;

            return !string.IsNullOrWhiteSpace(claimsValue);
        }

        /// <summary>
        /// Get user id from the claims principal.
        /// </summary>
        /// <param name="user">Claims pricipal representing the user.</param>
        /// <param name="userId">User Id obtained from the claims principal.</param>
        /// <returns>True if it could find the user id.</returns>
        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
        {
            // First, try to identify the user by player id
            if (user.TryGetUserClaims(CustomClaims.InternalId, out var playerIdString)
                && Guid.TryParse(playerIdString, out userId))
            {
                return true;
            }

            userId = Guid.Empty;
            return false;
        }

        /// <summary>
        /// Return the Access Token of the user.
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <param name="accessToken">The value of the Access Token.</param>
        /// <returns>True if the access token is not empty.</returns>
        public static bool TryGetAccessToken(this ClaimsPrincipal user, out string accessToken)
        {
            return user.TryGetUserClaims("access_token", out accessToken);
        }

        /// <summary>
        /// Return the Id Token of the user.
        /// </summary>
        /// <param name="user">The current user.</param>
        /// <param name="idToken">The value of the Id Token.</param>
        /// <returns>True if the id token is not empty.</returns>
        public static bool TryGetIdToken(this ClaimsPrincipal user, out string idToken)
        {
            return user.TryGetUserClaims("id_token", out idToken);
        }

        /// <summary>
        /// Return the Auth0Id of the user.
        /// </summary>
        /// <param name="user">Logged user.</param>
        /// <param name="externalId">External Id (Auth0).</param>
        /// <returns>True if the ExternalId is valid.</returns>
        public static bool TryGetExternalId(this ClaimsPrincipal user, out string externalId)
        {
            return user.TryGetUserClaims(ClaimTypes.NameIdentifier, out externalId);
        }

        /// <summary>
        /// Indicates whether this user is anonymous.
        /// </summary>
        /// <param name="user">Logged in user.</param>
        /// <returns>True if anonymous.</returns>
        public static bool IsAnonymous(this ClaimsPrincipal user)
        {
            return user.TryGetUserClaims(ClaimTypes.NameIdentifier, out string externalId) && AuthenticationHelper.IsAnonymous(externalId);
        }

        /// <summary>
        /// Indicates whether this user has any admin access.
        /// </summary>
        /// <param name="user">Logged user.</param>
        /// <returns>bool.</returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole(Roles.Admin) || user.IsInRole(Roles.MachineAdmin);

        /// <summary>
        /// Indicates whether this user is player's admin.
        /// </summary>
        /// <param name="user">Logged user.</param>
        /// <returns>bool.</returns>
        public static bool IsPlayerAdmin(this ClaimsPrincipal user)
            => user.IsInRole(Roles.Admin);

        /// <summary>
        /// Get Issued Token DateTime (UTC).
        /// </summary>
        /// <param name="user">ClaimsPrincipal.</param>
        /// <returns>IAT DateTime.</returns>
        public static DateTime? GetIssuedDateUtc(this ClaimsPrincipal user)
        {
            DateTime? iat = null;

            if (!user.TryGetUserClaims("iat", out string iatToken))
            {
                return iat;
            }

            if (long.TryParse(iatToken, NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out long seconds))
            {
                iat = DateTime.UnixEpoch.AddSeconds(seconds);
            }

            return iat;
        }
    }
}
