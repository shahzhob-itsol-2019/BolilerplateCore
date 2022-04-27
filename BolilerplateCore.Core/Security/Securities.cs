﻿using BoilerplateCore.Common.Encryption;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.ISecurity;
using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        public static void RegisterServices(IServiceCollection services, ApplicationType applicationType)
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

            if (securityOptions.Value.MicrosoftAuthenticationAdded)
                //AddMicrosoftAuthentication(services);

                if (securityOptions.Value.GoogleAuthenticationAdded)
                    //AddGoogleAuthentication(services);

                    if (securityOptions.Value.TwitterAuthenticationAdded)
                        AddTwitterAuthentication(services);

            if (securityOptions.Value.FacebookAuthenticationAdded)
                AddFacebookAuthentication(services);
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
        private static void AddAuthorization(IServiceCollection services, ApplicationType applicationType)
        {
        }
        private static void AddAuthentication(IServiceCollection services, ApplicationType applicationType)
        {
            if (applicationType == ApplicationType.CoreApi)
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
            } else if (applicationType == ApplicationType.Web)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.LoginPath = new PathString("/Account/Login");
                    options.AccessDeniedPath = new PathString("/Account/Login/");
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnValidatePrincipal = ctx =>
                        {
                            var ret = Task.Run(async () =>
                            {
                                var accessToken = ctx.Principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
                                var userName = ctx.Principal.FindFirst(ClaimTypes.Name)?.Value;
                                //var result = (await HttpClientHelper.GetAsync<UserClaim>("Account/GetUser?userName=" + userName));
                                //if (result == null || result.Data == null)
                                //{
                                //    ctx.RejectPrincipal();
                                //}
                            });
                            return ret;
                        },
                        OnSigningIn = async (context) =>
                        {
                            ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
                            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, ""));
                            identity.AddClaim(new Claim(ClaimTypes.Sid, ""));
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ""));
                            identity.AddClaim(new Claim(ClaimTypes.Name, ""));
                            identity.AddClaim(new Claim(ClaimTypes.Email, ""));
                            identity.AddClaim(new Claim(ClaimTypes.GivenName, ""));
                            identity.AddClaim(new Claim(ClaimTypes.Surname, ""));
                            identity.AddClaim(new Claim(ClaimTypes.Role, ""));
                        }
                    };
                })
                .AddCookie(IdentityConstants.ExternalScheme, options =>
                {
                    options.Cookie.Name = IdentityConstants.ExternalScheme;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login/");
                });
                //.AddMicrosoftAccount(microsoftOptions =>
                //{
                //    microsoftOptions.ClientId = "68669dee-ad51-4ab0-8a8f-16f456a05917";
                //    microsoftOptions.ClientSecret = "xwaxyXEPRO726#@}icBG05@";
                //})
                //.AddGoogle(googleOptions =>
                //{
                //    googleOptions.ClientId = "434467402013-4ehq09dvqp7qu57jucr1rra56fs0glcv.apps.googleusercontent.com";
                //    googleOptions.ClientSecret = "k4kvo8ckstA6u1Da5Skkiqaj";
                //});
            }
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
