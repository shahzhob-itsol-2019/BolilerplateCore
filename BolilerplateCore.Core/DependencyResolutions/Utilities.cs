using AutoMapper;
using BoilerplateCore.Common.Filters;
using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Helpers.Interfaces;
using BoilerplateCore.Data.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.DependencyResolutions
{
    public static class Utilities
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Todo: we have to change to Interfaces in some of following:

            var config = ModelMapper.Configure();
            IMapper mapper = config.CreateMapper();

            services.AddTransient<IHttpClient, HttpClientHelper>();

            services.AddScoped<ValidateModelState>();

            services.AddSingleton(mapper);
        }

        public static void RegisterApps(IApplicationBuilder apps)
        {

        }
    }
}
