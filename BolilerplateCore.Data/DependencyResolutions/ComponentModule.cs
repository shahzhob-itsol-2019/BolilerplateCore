using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using BoilerplateCore.Core.Security;
using BoilerplateCore.Core.Communication;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class ComponentModule
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            Securities.RegisterServices(services);
            Communications.RegisterServices(services);
        }

        public static void Configure(IApplicationBuilder apps)
        {
        }
    }
}
