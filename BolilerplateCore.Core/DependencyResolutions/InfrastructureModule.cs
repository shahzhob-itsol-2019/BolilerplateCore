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
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class InfrastructureModule
    {
        public static void Configure(IServiceCollection services, IConfiguration configuration, ApplicationType applicationType)
        {
            ConfigurationModule.Configure(services, configuration, applicationType);
            RepositoryModule.ConfigureDbContext(services, configuration);
            Documentations.AddSwagger(services);
            Utilities.RegisterLifetime(services);
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigurationModule.Configure(app, env);
            RepositoryModule.Configure(app, env);
            Documentations.RegisterApps(app);
            Utilities.LifetimeApps(app);
        }
    }
}
