// -----------------------------------------------------------------------
// <copyright file="AuthenticationHelper.cs" company="Playtertainment">
// Copyright (c) Playtertainment. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BoilerplateCore.Common.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Authentication;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using JWT;
    using JWT.Algorithms;
    using JWT.Serializers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    //using WinnerWinner.Common.Services.Requests.Payment;

    /// <summary>
    /// Authentication related methods for the API.
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Extracts list of roles from the access token. The code assumes that roles are returned as part of the auth0
        /// token.
        /// </summary>
        /// <param name="accessToken">auth0 generate access token.</param>
        /// <returns>List of roles.</returns>
        public static IEnumerable<string> GetRoles(JwtSecurityToken accessToken)
        {
            var roles = Enumerable.FirstOrDefault<Claim>(accessToken?.Claims, c => c.Type == CustomClaims.Roles);
            return roles != null ? roles.Value.Split(',') : Array.Empty<string>();
        }

        /// <summary>
        /// Indicates whether this user is anonymous.
        /// </summary>
        /// <param name="externalId">External Id (Auth0).</param>
        /// <returns>True if anonymous.</returns>
        public static bool IsAnonymous(this string externalId)
        {
            return externalId?.StartsWith("anon_") == true;
        }

        /// <summary>
        /// Verify that client_id to the application correspond to what was configured by the admin.
        /// </summary>
        /// <param name="configuration">system configuration.</param>
        /// <param name="authParams">request parameters.</param>
        /// <param name="throwOnFailure">whether to throw if verification failed.</param>
        /// <returns>True if the client id can be verified.</returns>
        public static bool VerifyRequestForAnonToken(
            IConfiguration configuration,
            AuthParams authParams,
            bool throwOnFailure = false)
        {
            if (configuration["Auth0:ClientId"] == authParams.ClientId
                && authParams.GrantType == "client_credentials"
                && !string.IsNullOrEmpty(authParams.DeviceId))
            {
                return true;
            }

            if (throwOnFailure)
            {
                throw new AuthenticationException();
            }

            return false;
        }

        /// <summary>
        /// Determines if the token is an anonymous token, which has some specific properties
        /// in comparison to a normal token.
        /// </summary>
        /// <param name="token">Token to validate.</param>
        /// <returns>True if the token is valid.</returns>
        public static bool IsValidAnonymousToken(string token)
        {
            JwtSecurityToken jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Is an expired token (anonymous never expired as for now)
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                return false;
            }

            // Only anonymous tokens should go through this handler.
            bool isAnonymous = jwtToken.ReadTokenClaim(ClaimTypes.NameIdentifier).IsAnonymous();

            return isAnonymous;
        }

        /// <summary>
        /// Decodes a JWT token.
        /// </summary>
        /// <param name="token">Token to decode.</param>
        /// <param name="key">Encryption key for the token.</param>
        /// <returns>Returns the content of the token.</returns>
        public static string Decode(string token, string key)
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(serializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            var decoder = new JwtDecoder(serializer, validator, urlEncoder, new HMACSHA256Algorithm());

            return decoder.Decode(token, key, verify: true);
        }

        /// <summary>
        /// Encodes a JWT token.
        /// </summary>
        /// <param name="payload">Payload to encode into the token.</param>
        /// <param name="key">Signing key.</param>
        /// <returns>Returns an encoded token.</returns>
        public static string Encode(object payload, string key)
        {
            var algorithm = new HMACSHA256Algorithm();
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder.Encode(payload, key);
        }

        /// <summary>
        /// Will validate Auth0 token. Throws exceptions if not validated.
        /// Does not return anything as it only validates for now.
        /// </summary>
        /// <param name="token">token from Auth0.</param>
        /// <param name="domain">Auth0 domain.</param>
        /// <param name="audiences">Audience claim.</param>
        /// <param name="apiIssuer">Auth0 api issuer.</param>
        /// <param name="clientSecret">Client secret used to generate the token.</param>
        public static void ValidateApiToken(string token, string domain, IList<string> audiences, string apiIssuer, string clientSecret)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = domain,
                ValidAudiences = audiences,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) => GetSigningKey(apiIssuer, kid),
                ValidIssuers = new List<string>
                {
                    apiIssuer,
                },
            };

            SecurityToken validatedToken;
            try
            {
                jwtHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException ex)
            {
                // Check to see if this is an anonymous token, if it's not carry on with the throw
                // Else go into anonymous flow
                if (IsValidAnonymousToken(token))
                {
                    try
                    {
                        var json = Decode(token, clientSecret);
                        var principal = new ClaimsPrincipal(new ClaimsIdentity(GetUserClaims(json)));
                        if (principal == null || principal.Identity == null)
                        {
                            throw new SecurityTokenException("Anonymous user is invalid.");
                        }
                    }
                    catch
                    {
                        throw new SecurityTokenException("Anonymous token is invalid.");
                    }
                }
                else
                {
                    // Not anonymous, continue with the throw;
                    throw;
                }

            }
        }

        /// <summary>
        /// Derived from the unlisted nuget package: https://github.com/auth0/auth0-aspnet-owin/tree/master/src/Auth0.Owin.OpenIdConnectSigningKeyResolver
        /// This only retrieves the JWKS and does not pose a security issue.
        /// </summary>
        /// <param name="authority">Auth0 authority.</param>
        /// <param name="kid">Key Id.</param>
        /// <returns>The signing key.</returns>
        public static SecurityKey[] GetSigningKey(string authority, string kid)
        {
            var cm = new ConfigurationManager<OpenIdConnectConfiguration>($"{authority.TrimEnd('/')}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            var taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            var openIdConfig = taskFactory.StartNew(async () => await cm.GetConfigurationAsync()).Unwrap().GetAwaiter().GetResult();
            return new[] { openIdConfig.JsonWebKeySet.GetSigningKeys().FirstOrDefault(t => t.KeyId == kid) };
        }

        /// <summary>
        /// Converts the json into a collection of claims.
        /// </summary>
        /// <param name="json">Json data.</param>
        /// <returns>A collection of claims.</returns>
        public static IEnumerable<Claim> GetUserClaims(string json)
        {
            var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return items.Select(item => new Claim(item.Key, item.Value)).ToList();
        }

        /// <summary>
        /// Read the claim value from JwtSecurityToken.
        /// </summary>
        /// <param name="token">JwtSecurityToken.</param>
        /// <param name="claimName">ClaimName.</param>
        /// <returns>Claim value.</returns>
        public static string ReadTokenClaim(this JwtSecurityToken token, string claimName)
        {
            return token?.Claims?.FirstOrDefault(c => c.Type == claimName)?.Value;
        }

        /// <summary>
        /// Converts the json into a collection of claims.
        /// </summary>
        /// <param name="jsonToken">Json token.</param>
        /// <param name="clientSecret">Auth0 Client Secret.</param>
        /// <returns>A collection of claims.</returns>
        public static Guid GetGuid(string jsonToken, string clientSecret)
        {
            var json = AuthenticationHelper.Decode(jsonToken, clientSecret);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(GetUserClaims(json)));
            principal.TryGetUserId(out Guid value);
            return value;
        }
    }
}
