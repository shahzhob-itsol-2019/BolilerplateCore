using BoilerplateCore.Common.Models;
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

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class InfrastructureModule
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationModule.Configure(services, configuration);
            Documentations.RegisterServices(services);
            Utilities.RegisterServices(services);
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Documentations.RegisterApps(app);
            Utilities.RegisterApps(app);
        }
    }
}
