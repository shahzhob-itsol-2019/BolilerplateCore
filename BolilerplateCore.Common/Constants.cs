// -----------------------------------------------------------------------
// <copyright file="Constants.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common
{
    //using Microsoft.AspNetCore.Http;
    using System.Collections.Generic;

    /// <summary>
    /// Common Constants.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Security stamp key.
        /// </summary>
        public const string SecurityStampKey = "Identity.SecurityStamp";

        /// <summary>
        /// Token type identified.
        /// </summary>
        public const string TokenType = "token_type";

        /// <summary>
        /// User identifier.
        /// </summary>
        public const string UserIdentifier = "user_identifier";

        /// <summary>
        /// Only US is available for launch, and country is never filled on the UI.
        /// </summary>
        public const string USCountry = "US";

        /// <summary>
        /// Key value used to retrieve the body within a PATCH request's HttpContext.
        /// </summary>
        //public static readonly string BodyPatchKey = $"Body{HttpMethods.Patch}";

        /// <summary>
        /// List of IronSource IPAddress. Will be used for authorization.
        /// </summary>
        public static readonly List<string> IronSourceIPAddresses = new List<string>
        {
            "79.125.5.179",
            "79.125.26.193",
            "79.125.117.130",
            "176.34.224.39",
            "176.34.224.41",
            "176.34.224.49",
            "34.194.180.125",
            "34.196.56.165",
            "34.196.251.81",
            "34.196.253.23",
            "54.88.253.218",
            "54.209.185.78",
            "103.244.176.17",
        };
    }
}
