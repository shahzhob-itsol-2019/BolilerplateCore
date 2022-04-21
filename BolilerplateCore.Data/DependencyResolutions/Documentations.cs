using BoilerplateCore.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class Documentations
    {
        private static IOptionsSnapshot<InfrastructureOptions> infrastructureOptions = null;

        public static void RegisterServices(IServiceCollection services)
        {
            infrastructureOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<InfrastructureOptions>>();
            switch (infrastructureOptions.Value.Documentation)
            {
                case "Swagger":
                    Swagger.ConfigureService(services);
                    break;
                default:
                    break;
            }
        }

        public static void RegisterApps(IApplicationBuilder apps)
        {
            switch (infrastructureOptions.Value.Documentation)
            {
                case "Swagger":
                    Swagger.ConfigureApp(apps);
                    break;
                default:
                    break;
            }
        }
    }
}
