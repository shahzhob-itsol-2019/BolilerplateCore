using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BoilerplateCore.CoreApi.Models;
using BoilerplateCore.Services.ServicesDependencyResolutions;
using BoilerplateCore.Core.DependencyResolutions;
//using BoilerplateCore.Common.Authentication;

namespace BoilerplateCore.CoreApi
{
    public class Startup
    {
        public static IConfiguration StaticConfiguration { get; private set; }
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
            StaticConfiguration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure(Configuration);
            services.RegisterServices(Configuration);
            //services.AddScoped<IUserService, UserService>();
            //services.AddScoped<ICompanyService, CompanyService>();
            //services.AddScoped<IAddressService, AddressService>();
            //services.AddScoped<ICityService, CityService>();
            //services.AddScoped<ICountryService, CountryService>();

            //services.AddScoped<IStatusService, StatusService>();
            //services.AddScoped<IStatusTypeService, StatusTypeService>();

            //services.AddScoped<INotificationService, NotificationService>();
            //services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
            //services.AddScoped<INotificationTypeService, NotificationTypeService>();
            services.AddHttpContextAccessor();
            
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //var httpContextAccessor = services.BuildServiceProvider().GetService<IHttpContextAccessor>();
            
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            //})
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            //{
            //    options.Cookie.Name = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //    options.LoginPath = new PathString("/Account/Login");
            //    options.AccessDeniedPath = new PathString("/Account/Login/");
            //    options.Events = new CookieAuthenticationEvents()
            //    {
            //        OnValidatePrincipal = ctx =>
            //        {
            //            var ret = Task.Run(async () =>
            //            {
            //                var accessToken = ctx.Principal.FindFirst(ClaimTypes.PrimarySid)?.Value;
            //                var userName = ctx.Principal.FindFirst(ClaimTypes.Name)?.Value;
            //                var result = (await HttpClientHelper.GetAsync<UserClaim>("Account/GetUser?userName=" + userName));
            //                if (result == null || result.Data == null)
            //                {
            //                    ctx.RejectPrincipal();
            //                }
            //            });
            //            return ret;
            //        },
            //        OnSigningIn = async (context) =>
            //        {
            //            ClaimsIdentity identity = (ClaimsIdentity)context.Principal.Identity;
            //            identity.AddClaim(new Claim(ClaimTypes.PrimarySid, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.Sid, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.Name, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.Email, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.GivenName, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.Surname, ""));
            //            identity.AddClaim(new Claim(ClaimTypes.Role, ""));
            //        }
            //    };
            //})
            //.AddCookie(IdentityConstants.ExternalScheme, options =>
            //{
            //    options.Cookie.Name = IdentityConstants.ExternalScheme;
            //    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            //    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
            //    options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login/");
            //})
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

            //Local dependencies
            services.AddScoped<CurrentUser>();
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.RegisterApps(env);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapGet("/", async context =>
            //    {
            //        await context.Response.WriteAsync("Hello World!");
            //    });
            //});
        }
    }
}
