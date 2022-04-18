// -----------------------------------------------------------------------
// <copyright file="Roles.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    public static class Roles
    {
        /// <summary>
        /// Administrative users (pretty much no restrictions at all).
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// Machine administrator (roles enabling machine administrative tasks).
        /// </summary>
        public const string MachineAdmin = "Machine Admin";

        /// <summary>
        /// A player role.
        /// </summary>
        public const string Player = "Player";
    }
}
