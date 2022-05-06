using AutoMapper;
using BoilerplateCore.Common.Filters;
using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Helpers.Interfaces;
using BoilerplateCore.Core.Authorization;
using BoilerplateCore.Core.Communication;
using BoilerplateCore.Data.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class Utilities
    {
        public static void RegisterLifetime(IServiceCollection services)
        {
            // Todo: we have to change to Interfaces in some of following:
            Communications.RegisterServices(services);
            var config = ModelMapper.Configure();
            IMapper mapper = config.CreateMapper();

            services.AddTransient<IHttpClient, HttpClientHelper>();

            services.AddScoped<ValidateModelState>();

            services.AddSingleton(mapper);
            services.AddScoped<IAuthorizationHandler, CustomRequireClaimHandler>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();
        }

        public static void LifetimeApps(IApplicationBuilder apps)
        {

        }
    }
}
