// -----------------------------------------------------------------------
// <copyright file="Policies.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    /// <summary>
    /// Policies definitions.
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Player users only.
        /// </summary>
        public const string AllowAnonymousUsers = "AllowAnonymousUsers";

        /// <summary>
        /// Admin users only.
        /// </summary>
        public const string AdminOnly = "AdminOnlyPolicy";

        /// <summary>
        /// Machine admin users only.
        /// </summary>
        public const string MachineAdmin = "MachineAdminPolicy";

        /// <summary>
        /// Machine Admin or Admin users only.
        /// </summary>
        public const string AnyAdmin = "AnyAdmin";

        /// <summary>
        /// Machine Server only (used for machine to machine communication i.e. no user context).
        /// </summary>
        public const string MachineServer = "MachineServer";

        /// <summary>
        /// Webhooks only.
        /// </summary>
        public const string Webhook = "Webhook";

        /// <summary>
        /// Payment requests only.
        /// </summary>
        public const string Payment = "Payment";

        /// <summary>
        /// Physical Play (NEN) requests only.
        /// </summary>
        public const string PhysicalPlay = "PhysicalPlay";
    }
}
