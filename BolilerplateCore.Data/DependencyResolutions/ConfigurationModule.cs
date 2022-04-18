using BoilerplateCore.Data.Helpers;
using BoilerplateCore.Data.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class ConfigurationModule
    {

        public static IServiceCollection Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BoilerplateOptions>(configuration.GetSection("BoilerplateOptions"));
            services.Configure<GoogleOptions>(configuration.GetSection("Google"));
            AppServicesHelper.Configuration = configuration;
            return services;
        }


        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AppServicesHelper.Services = app.ApplicationServices;
        }
    }
}
