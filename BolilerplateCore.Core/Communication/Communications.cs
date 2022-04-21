using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.ICommunication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.Communication
{
    public static class Communications
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<ICommunicationService, CommunicationService>();

            var componentOptions = services.BuildServiceProvider().GetService<IOptionsSnapshot<ComponentOptions>>();
            switch (componentOptions.Value.Communication.EmailService)
            {
                case "Google":
                    services.AddTransient<IEmailService, EmailServiceGoogle>();
                    break;
                case "Outlook":
                    services.AddTransient<IEmailService, EmailServiceOutlook>();
                    break;
                default:
                    break;
            }
            services.AddTransient<ISmsService, SmsServiceTest>();
        }
    }
}
