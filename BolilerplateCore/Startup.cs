using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BoilerplateCore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BoilerplateCore.Common.Authentication;
using BoilerplateCore.Mobile.API.Middleware;
using BoilerplateCore.Data.DependencyResolutions;
//using BoilerplateCore.Common.Authentication;

namespace BoilerplateCore
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
            services.RegisterServices(Configuration);
            // use sql server db in production and sqlite db in development
            //if (_env.IsProduction())
            //    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            //    Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.MigrationsAssembly("BoilerplateCore.Data")));
            //else
            //    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
            //    Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.MigrationsAssembly("BoilerplateCore.Data")));
            
            // AddCors must be before AddMvc
            //services.AddCors(options =>
            //{
            //    options.AddPolicy(
            //        "CorsPolicy",
            //        builder =>
            //        {
            //            builder.AllowAnyOrigin()
            //                .AllowAnyMethod()
            //                .AllowAnyHeader()
            //                .WithExposedHeaders("x-pagination");
            //        });
            //});
            //services.AddControllers();
            //services.AddAuthentication(x =>
            //{
            //    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(x =>
            //{
            //    x.Events = new JwtBearerEvents
            //    {
            //        OnTokenValidated = context =>
            //        {
            //            //var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            //            //var userId = int.Parse(context.Principal.Identity.Name);
            //            //var user = userService.GetById(userId);
            //            //if (user == null)
            //            //{
            //            //    // return unauthorized if user no longer exists
            //            //    context.Fail("Unauthorized");
            //            //}
            //            return Task.CompletedTask;
            //        }
            //    };
            //    x.RequireHttpsMetadata = false;
            //    x.SaveToken = true;
            //    x.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuerSigningKey = true,
            //        //IssuerSigningKey = new SymmetricSecurityKey(key),
            //        ValidateIssuer = false,
            //        ValidateAudience = false
            //    };
            //});

            //services.AddMvc(option => option.EnableEndpointRouting = false)
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMvc(option => option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            //services.AddHealthChecks();
            //string domain = $"https://{Configuration["Auth0:Domain"]}/";
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    options.SaveToken = true;
            //    options.RequireHttpsMetadata = false;
            //    options.Authority = domain;

            //    // options.Audience = Configuration["Auth0:ApiIdentifier"];
            //    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidAudience = Configuration["JWT:ValidAudience"],
            //        ValidIssuer = Configuration["JWT:ValidIssuer"],
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"])),
            //        ValidateIssuerSigningKey = true,
            //        ValidAudiences = new List<string>
            //        {
            //            Configuration["Auth0:ApiIdentifier"],
            //            Configuration["Auth0:CustomAPIIdentifier"],
            //        },
            //        ValidateLifetime = true,
            //        ClockSkew = TimeSpan.Zero,
            //    };

            //    options.Events = new JwtBearerEvents
            //    {
            //        OnMessageReceived = context =>
            //        {
            //            return Task.CompletedTask;
            //        },
            //        OnTokenValidated = context =>
            //        {
            //            // Grab the raw value of the token, and store it as a claim so we can retrieve it again later in the request pipeline
            //            if (context.SecurityToken is JwtSecurityToken token)
            //            {
            //                if (context.Principal.Identity is ClaimsIdentity identity)
            //                {
            //                    identity.AddClaim(new Claim("access_token", token.RawData));
            //                    identity.AddClaim(new Claim("auth0_id", token.Subject ?? string.Empty));

            //                    // Extract roles from the token and add them (they're claims for further authorization)
            //                    //foreach (var role in AuthenticationHelper.GetRoles(token))
            //                    //{
            //                    //    identity.AddClaim(new Claim(ClaimTypes.Role, role));
            //                    //}
            //                }
            //            }

            //            return Task.FromResult(0);
            //        },
            //    };
            //}).AddCookie("MyCookieAuth", options => {
            //    options.Cookie.Name = "MyCookieAuth";
            //});
            //var sp = services.BuildServiceProvider();
            //var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            //var logger = sp.GetService<ILogger<AnonymousRequirement>>();
            // Add authorization policies
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(Policies.AllowAnonymousUsers, policy =>
                {
                    //policy.AddRequirements(new AnonymousRequirement(Configuration, httpContextAccessor, logger));
                });

                //auth.AddPolicy(Policies.AdminOnly, policy =>
                //{
                //    policy.RequireRole(Roles.Admin);
                //});

                //auth.AddPolicy(Policies.MachineAdmin, policy =>
                //{
                //    policy.RequireRole(Roles.MachineAdmin);
                //});

                //auth.AddPolicy(Policies.AnyAdmin, policy =>
                //{
                //    policy.RequireRole(Roles.Admin, Roles.MachineAdmin);
                //});

                //auth.AddPolicy(Policies.MachineServer, policy =>
                //{
                //    policy.AddRequirements(new MachineServerRequirement(Configuration, httpContextAccessor));
                //});

                //auth.AddPolicy(Policies.Webhook, policy =>
                //{
                //    policy.AddRequirements(new QueryParameterRequirement(Configuration, httpContextAccessor));
                //});

                //auth.AddPolicy(Policies.Payment, policy =>
                //{
                //    policy.AddRequirements(new PaymentRequirement(Configuration, httpContextAccessor));
                //});

                //auth.AddPolicy(Policies.PhysicalPlay, policy =>
                //{
                //    policy.AddRequirements(new PhysicalPlayRequirement(Configuration, httpContextAccessor));
                //});
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
