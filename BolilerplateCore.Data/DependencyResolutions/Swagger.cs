using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.DependencyResolutions
{
    public static class Swagger
    {
        public static void ConfigureService(IServiceCollection services)
        {
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "ItSolution API",
                    Description = "ItSolution Web API"
                });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                });
                c.EnableAnnotations();
            });
        }

        public static void ConfigureApp(IApplicationBuilder apps)
        {
            apps.UseSwagger();
            apps.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ItSolution API V1");
            });
        }
    }
}
