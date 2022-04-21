using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class InfrastructureModule
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            //Utilities.RegisterServices(services);


            // Todo: we have to change to Interfaces in some of following:

            //var config = Helpers.ModelMapper.Configure();
            //AutoMapper.IMapper mapper = config.CreateMapper();
            //services.AddSingleton(mapper);

            //services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            var httpContextAccessor = services.BuildServiceProvider().GetService<IHttpContextAccessor>();

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
            });

            // way to register Cookie
            //services.ConfigureApplicationCookie(config =>
            //{
            //    config.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
            //    config.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //    config.LoginPath = new PathString("/Account/Login");
            //    config.AccessDeniedPath = new PathString("/Account/Login/");
            //});
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            //Utilities.RegisterApps(apps);
        }
    }
}
