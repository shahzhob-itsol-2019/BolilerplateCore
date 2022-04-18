using BoilerplateCore.Data.DependencyResolutions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class Extensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            ServiceModule.Configure(services);
            RepositoryModule.Configure(services, configuration);
            ConfigurationModule.Configure(services, configuration);
            InfrastructureModule.Configure(services, configuration);
            ComponentModule.Configure(services);
        }


        public static void RegisterApps(this IApplicationBuilder apps, IWebHostEnvironment env)
        {
            ConfigurationModule.Configure(apps, env);
            InfrastructureModule.Configure(apps, env);
            ComponentModule.Configure(apps);
            RepositoryModule.Configure(apps, env);
        }
    }
}
