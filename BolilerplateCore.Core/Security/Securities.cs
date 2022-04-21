using BoilerplateCore.Common.Encryption;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.ISecurity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.Security
{
    public static class Securities
    {
        public static void RegisterServices(IServiceCollection services)
        {
            var componentOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<ComponentOptions>>();
            var securityOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<SecurityOptions>>();

            switch (componentOptions.Value.Security.SecurityService)
            {
                case "AspnetIdentity":
                    services.AddTransient<ISecurityService, SecurityAspnetIdentity>();
                    break;
                case "SingleSignOn":
                    services.AddTransient<ISecurityService, SecuritySingleSignOn>();
                    break;
                default:
                    break;
            }

            switch (componentOptions.Value.Security.EncryptionService)
            {
                case "AES":
                    services.AddTransient<IEncryptionService, EncryptionAES>();
                    break;
                default:
                    break;
            }

            AddAuthentication(services);

            if (securityOptions.Value.MicrosoftAuthenticationAdded)
                AddMicrosoftAuthentication(services);

            if (securityOptions.Value.GoogleAuthenticationAdded)
                AddGoogleAuthentication(services);

            if (securityOptions.Value.TwitterAuthenticationAdded)
                AddTwitterAuthentication(services);

            if (securityOptions.Value.FacebookAuthenticationAdded)
                AddFacebookAuthentication(services);
        }

        private static void AddAuthentication(IServiceCollection services)
        {
            var boilerplateOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<BoilerplateOptions>>();

            var key = Encoding.ASCII.GetBytes("Core.Secret@Boilerplate");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    //ValidateIssuer = false,
                    //ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = boilerplateOptions.Value.ApiUrl,
                    ValidAudience = boilerplateOptions.Value.ApiUrl,
                };
            });

            /// Todo:
            //// Configuring Identity Server
            //var authority = Configuration["Security:Authority"];
            //var requiredScope = Configuration["Security:RequiredScope"];
            //services.AddAuthentication().AddIdentityServerAuthentication(option => 
            //{
            //    option.Authority = authority;
            //    //AllowedScopes = new[] { requiredScope },
            //    option.RequireHttpsMetadata = false;
            //    option.InboundJwtClaimTypeMap = new Dictionary<string, string>();
            //    option.JwtBearerEvents = new JwtBearerEvents()
            //    {
            //        OnAuthenticationFailed = f =>
            //        {
            //            f.Response.ContentType = "application/json";
            //            var response = new
            //            {
            //                success = false,
            //                message = "Un-Authorized Access"
            //            };
            //            //c.HandleResponse();
            //            f.Response.WriteAsync(JsonSerializer.Serialize(response));
            //            return Task.FromResult(0);
            //        },
            //        OnChallenge = c =>
            //        {
            //            var response = new
            //            {
            //                success = false,
            //                message = "Un-Authorized Access"
            //            };
            //            c.HandleResponse();
            //            c.Response.WriteAsync(JsonSerializer.Serialize(response));
            //            return Task.FromResult(0);
            //        }
            //    };
            //});


            // Todo:
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("TrainedStaffOnly",
            //        policy => policy.RequireClaim("CompletedBasicTraining"));
            //});
        }

        private static void AddMicrosoftAuthentication(IServiceCollection services)
        {
            var outlookOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<OutlookOptions>>();
            services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = outlookOptions.Value.ApplicationId;
                microsoftOptions.ClientSecret = outlookOptions.Value.ApplicationSecret;
            });
        }

        private static void AddGoogleAuthentication(IServiceCollection services)
        {
            var googleOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<GoogleOptions>>();
            services.AddAuthentication().AddGoogle(googleAuthOptions =>
            {
                googleAuthOptions.ClientId = googleOptions.Value.ClientId;
                googleAuthOptions.ClientSecret = googleOptions.Value.ClientSecret;
            });
        }

        private static void AddTwitterAuthentication(IServiceCollection services)
        {
            var twiitterOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<TwitterOptions>>();
            services.AddAuthentication().AddTwitter(twiitterAuthOptions =>
            {
                twiitterAuthOptions.ConsumerKey = twiitterOptions.Value.ConsumerKey;
                twiitterAuthOptions.ConsumerSecret = twiitterOptions.Value.ConsumerSecret;
            });
        }

        private static void AddFacebookAuthentication(IServiceCollection services)
        {
            var facebookkOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<FacebookOptions>>();
            services.AddAuthentication().AddFacebook(facebookAuthOptions =>
            {
                facebookAuthOptions.ClientId = facebookkOptions.Value.AppId;
                facebookAuthOptions.ClientSecret = facebookkOptions.Value.AppSecret;
            });
        }
    }
}
