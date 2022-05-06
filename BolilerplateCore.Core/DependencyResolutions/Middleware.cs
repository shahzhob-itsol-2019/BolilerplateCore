using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using BoilerplateCore.Core.Security;
using BoilerplateCore.Core.Communication;
using AutoMapper.Configuration;
using static BoilerplateCore.Common.Utility.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class Middleware
    {
        internal static void RegisterServices(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration, ApplicationType applicationType)
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
            services.AddControllers();
            services.AddHealthChecks();
            if (applicationType == ApplicationType.Web)
            {
                services.AddControllersWithViews(config =>
                {
                    var defaultAuthBuilder = new AuthorizationPolicyBuilder();
                    var defaultAuthPolicy = defaultAuthBuilder
                        .RequireAuthenticatedUser()
                        .Build();

                    // global authorization filter which is applied on whole application if you want to bypass it on some endpoint use [AllowAnonymous]
                    //config.Filters.Add(new AuthorizeFilter(defaultAuthPolicy));
                });
                services.AddRazorPages();
            }

        }

        public static void RegisterApps(IApplicationBuilder app, IWebHostEnvironment env, ApplicationType applicationType)
        {
            if (applicationType == ApplicationType.CoreApi)
            {
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
            } else if(applicationType == ApplicationType.Web)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                    endpoints.MapRazorPages();
                });
            }
        }
    }
}
