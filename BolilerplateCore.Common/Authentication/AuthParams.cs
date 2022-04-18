// -----------------------------------------------------------------------
// <copyright file="AuthParams.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    using Newtonsoft.Json;

    public class AuthParams
    {
        /// <summary>
        ///  Gets or sets static client identifier; generated fro the app when provisioned with auth0.
        /// </summary>
        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets static client secret; generated for the app when provisioned with auth0.
        /// </summary>
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets grant types; for app 2 app communication should be client_credentials.
        /// </summary>
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        /// <summary>
        /// Gets or sets iD of the device initiating the call; it must be unique to prevent abuse.
        /// (client can't use the same device to play as an anonymous user multiple times).
        /// </summary>
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        // TODO: we may need it in the future public string scope { get; set; }
    }
}
