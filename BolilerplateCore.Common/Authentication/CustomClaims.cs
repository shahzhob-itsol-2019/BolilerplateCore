// -----------------------------------------------------------------------
// <copyright file="CustomClaims.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    /// <summary>
    /// This class houses any custom claims stored and exposed by the Auth0 JWT access token that is received.
    /// Because Auth0 follows the OPENID protocol (which is a subset of oAuth), any claims that are not part of the standard claims must
    /// be namespaced. And Auth0 requires that we namespace custom claims in a URL format with either Http or Https.
    /// The url will not be called and is only for name spacing purposes.
    /// </summary>
    public static class CustomClaims
    {
        /// <summary>
        /// The name of the custom claims that stores the roles of the user (extracted from the incoming JWT token).
        /// </summary>
        public const string Roles = "https://winner-winner.com/roles";

        /// <summary>
        /// Represents surrogate id - immutable id of a user.
        /// </summary>
        public const string InternalId = "https://winner-winner.com/internal_id";

        /// <summary>
        /// The username stored in Auth0.
        /// </summary>
        public const string Username = "https://winner-winner.com/username";
    }
}
