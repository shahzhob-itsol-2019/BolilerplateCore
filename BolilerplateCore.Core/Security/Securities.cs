using BoilerplateCore.Common.Encryption;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.Authorization;
using BoilerplateCore.Core.ISecurity;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Core.Security
{
    public static class Securities
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration, ApplicationType applicationType)
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


            AddIdentity(services, applicationType);
            AddAuthentication(services, applicationType);
            AddAuthorization(services, configuration, applicationType);

            if (securityOptions.Value.MicrosoftAuthenticationAdded)
                AddMicrosoftAuthentication(services);

            if (securityOptions.Value.GoogleAuthenticationAdded)
                AddGoogleAuthentication(services);

            if (securityOptions.Value.TwitterAuthenticationAdded)
                        AddTwitterAuthentication(services);

            if (securityOptions.Value.FacebookAuthenticationAdded)
                AddFacebookAuthentication(services);
        }
        private static void AddAuthentication(IServiceCollection services, ApplicationType applicationType)
        {
            var boilerplateOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<BoilerplateOptions>>();
            if (applicationType == ApplicationType.CoreApi)
            {

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
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        //ValidateIssuer = false,
                        //ValidateAudience = false,
                        //ValidateIssuer = true,
                        //ValidateAudience = true,
                        //ValidateLifetime = true,
                        ValidIssuer = boilerplateOptions.Value.ApiUrl,
                        ValidAudience = boilerplateOptions.Value.ApiUrl,
                    };
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    OnMessageReceived = context =>
                    //    {
                    //        if (context.Request.Query.ContainsKey("Bearer"))
                    //        {
                    //            context.Token = context.Request.Query["Bearer"];
                    //        }
                    //        return Task.CompletedTask;
                    //    }
                    //};
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
            else if (applicationType == ApplicationType.Web)
            {
                
            }
        }

        private static void AddAuthorization(IServiceCollection services, IConfiguration configuration, ApplicationType applicationType)
        {
            services.AddAuthorization(config =>
            {
                //work under the hood
                //var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                //var defaultAuthPolicy = defaultAuthBuilder
                //    .RequireAuthenticatedUser()
                //    .RequireClaim(ClaimTypes.DateOfBirth)
                //    .Build();
                //config.DefaultPolicy = defaultAuthPolicy;

                // working same as above
                //config.AddPolicy("Claim.DoB", policyBuilder =>
                //{
                //    policyBuilder.RequireClaim(ClaimTypes.DateOfBirth);
                //});

                // config.AddPolicy("Admin", policyBuilder => policyBuilder.RequireClaim(ClaimTypes.Role, "Admin"));


                config.AddPolicy("Claim.DoB", policyBuilder =>
                {
                    ////policyBuilder.AddRequirements(new CustomRequireClaim(ClaimTypes.DateOfBirth));
                    
                    ///same as above using extension method
                    policyBuilder.RequireCustomClaim(JwtClaimTypes.BirthDate);
                });
            });
                
        }

        private static void AddIdentity(IServiceCollection services, ApplicationType applicationType)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<SqlServerDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Identity.Cookie";
                config.LoginPath = "/Home/Login";
            });
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

        private static void AddExternalSchemeAuthentication(IServiceCollection services)
        {
            var outlookOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<OutlookOptions>>();
            services.AddAuthentication().AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = IdentityConstants.ExternalScheme;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login/");
            });
        }
    }
}
