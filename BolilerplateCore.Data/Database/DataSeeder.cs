using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BoilerplateCore.Data.Constants.Enums;

namespace BoilerplateCore.Data.Database
{
    public static class DataSeeder
    {
        public static async Task Seed(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SqlServerDbContext>();

                var roles = Enum.GetValues(typeof(UserRoles));
                var roleStore = new RoleStore<IdentityRole>(context);
                foreach (var role in roles)
                {
                    if (!await context.Roles.AnyAsync(r => r.Name.ToLower() == role.ToString()))
                    {
                        await context.Roles.AddAsync(new IdentityRole { Name = role.ToString(), NormalizedName = role.ToString().ToUpper() });
                        await context.SaveChangesAsync();
                    }
                }

                //var countries = new List<Country>()
                //{
                //    new Country() { Name = "Pakistan", Code = CountryCode.Pakistan, Nationality = "Pakistani" },
                //    new Country() { Name = "Canada", Code = CountryCode.Canada, Nationality = "Canadian" }
                //};
                //var newCountries = countries.Where(s => !context.Countries.Select(st => st.Name).Contains(s.Name));
                //if (newCountries.Any())
                //{
                //    await context.Countries.AddRangeAsync(newCountries);
                //    await context.SaveChangesAsync();
                //}

                //var statuses = new List<Status>()
                //{
                //    // Application statuses
                //    new Status() { Name = "None", Code = ApplicationStausCode.None, Type = StatusType.ApplicionStatus },
                //};
                //var newStatuses = statuses.Where(s => !context.Statuses.Select(st => st.Name).Contains(s.Name));
                //if (newStatuses.Any())
                //{
                //    await context.Statuses.AddRangeAsync(newStatuses);
                //    await context.SaveChangesAsync();
                //}

                //var minerals = new List<Mineral>()
                //{
                //    new Mineral() { Name = "Gold", Code = MineralCode.Gold, Cost = 3000 },
                //};
                //var newMinerals = minerals.Where(s => !context.Minerals.Select(st => st.Name).Contains(s.Name));
                //if (newMinerals.Any())
                //{
                //    await context.Minerals.AddRangeAsync(newMinerals);
                //    await context.SaveChangesAsync();
                //}
            }
        }
    }
}
