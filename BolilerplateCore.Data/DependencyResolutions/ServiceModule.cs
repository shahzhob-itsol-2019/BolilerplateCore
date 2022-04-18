using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class ServiceModule
    {
        public static void Configure(IServiceCollection services)
        {
            //services.AddScoped<ISecurityService, SecurityAspnetIdentity>();
            //services.AddTransient<ICommunicationService, CommunicationService>();
            //services.AddTransient<IEmailService, EmailServiceGoogle>();
            //services.AddTransient<ISmsService, SmsServiceTest>();

            //services.AddTransient<IStatusService, StatusService>();
            //services.AddTransient<ICountryService, CountryService>();
            //services.AddTransient<ICompanyService, CompanyService>();
            //services.AddTransient<ILocationService, LocationService>();
            //services.AddTransient<IMineralService, MineralService>();
            //services.AddTransient<IApplicationService, ApplicationService>();
        }
    }
}
