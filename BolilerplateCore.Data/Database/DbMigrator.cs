using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.Database
{
    public static class DbMigrator
    {
        public static Task Migrate(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<SqlServerDbContext>().Database.Migrate();
                return Task.FromResult(0);
            }
        }
    }
}
