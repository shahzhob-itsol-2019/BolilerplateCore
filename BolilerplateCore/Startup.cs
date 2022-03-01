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
using BoilerplateCore.Core.Entities;
using Microsoft.EntityFrameworkCore;
//using BoilerplateCore.Common.Authentication;

namespace BoilerplateCore
{
    public class Startup
    {
        public static IConfiguration StaticConfiguration { get; private set; }
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfiguration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // AddCors must be before AddMvc
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "CorsPolicy",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("x-pagination");
                    });
            });

            services.AddMvc(option => option.EnableEndpointRouting = false)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddHealthChecks();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.MigrationsAssembly("BoilerplateCore.Data")));
            //var sp = services.BuildServiceProvider();
            //var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            //var logger = sp.GetService<ILogger<AnonymousRequirement>>();
            // Add authorization policies
            //services.AddAuthorization(auth =>
            //{
            //    auth.AddPolicy(Policies.AllowAnonymousUsers, policy =>
            //    {
            //        policy.AddRequirements(new AnonymousRequirement(Configuration, httpContextAccessor, logger));
            //    });

            //    auth.AddPolicy(Policies.AdminOnly, policy =>
            //    {
            //        policy.RequireRole(Roles.Admin);
            //    });

            //    auth.AddPolicy(Policies.MachineAdmin, policy =>
            //    {
            //        policy.RequireRole(Roles.MachineAdmin);
            //    });

            //    auth.AddPolicy(Policies.AnyAdmin, policy =>
            //    {
            //        policy.RequireRole(Roles.Admin, Roles.MachineAdmin);
            //    });

            //    auth.AddPolicy(Policies.MachineServer, policy =>
            //    {
            //        policy.AddRequirements(new MachineServerRequirement(Configuration, httpContextAccessor));
            //    });

            //    auth.AddPolicy(Policies.Webhook, policy =>
            //    {
            //        policy.AddRequirements(new QueryParameterRequirement(Configuration, httpContextAccessor));
            //    });

            //    auth.AddPolicy(Policies.Payment, policy =>
            //    {
            //        policy.AddRequirements(new PaymentRequirement(Configuration, httpContextAccessor));
            //    });

            //    auth.AddPolicy(Policies.PhysicalPlay, policy =>
            //    {
            //        policy.AddRequirements(new PhysicalPlayRequirement(Configuration, httpContextAccessor));
            //    });
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

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
