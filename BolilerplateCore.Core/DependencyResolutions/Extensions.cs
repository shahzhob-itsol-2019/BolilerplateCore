using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class Extensions
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            InfrastructureModule.Configure(services, configuration);
            RepositoryModule.Configure(services, configuration);
            ComponentModule.Configure(services);
            
        }


        public static void RegisterApps(this IApplicationBuilder apps, IWebHostEnvironment env)
        {
            InfrastructureModule.Configure(apps, env);
            RepositoryModule.Configure(apps, env);
            ConfigurationModule.Configure(apps, env);
            ComponentModule.Configure(apps);
        }
    }
}
