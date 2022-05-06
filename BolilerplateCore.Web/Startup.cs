using BoilerplateCore.Common.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BoilerplateCore.Core.DependencyResolutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;
using BoilerplateCore.Common.Models;

namespace BoilerplateCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfiguration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfiguration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
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
                                var result = (await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserClaim>("Account/GetUser?userName=" + userName));
                                if (result == null || result.Data == null)
                                {
                                    ctx.RejectPrincipal();
                                }
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
                });
            services.Configure(Configuration, ApplicationType.Web);   
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.RegisterApps(env, ApplicationType.Web);
        }
    }
}
