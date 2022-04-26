using BoilerplateCore.Common.Options;
using BoilerplateCore.Common.Utility;
using BoilerplateCore.Common.Utility.Constants;
using BoilerplateCore.Data.Entities;
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
using static BoilerplateCore.Common.Utility.Enums;
using StatusType = BoilerplateCore.Data.Entities.StatusType;

namespace BoilerplateCore.Data.Database
{
    public static class DataSeeder
    {
        public static async Task Seed(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SqlServerDbContext>();
                var componentOptions = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptionsSnapshot<ComponentOptions>>();


                #region Statuses StatusTypes

                var statusTypes = new List<StatusType>()
                {
                    new StatusType() { Id = StatusTypes.UserStatus, Name = "User statuses" },
                    new StatusType() { Id = StatusTypes.NotificationStatus, Name = "Notification statuses" }
                };
                var newStatusTypes = statusTypes.Where(s => !context.StatusTypes.Select(st => st.Id).Contains(s.Id));
                if (newStatusTypes.Any())
                {
                    context.StatusTypes.AddRange(newStatusTypes);
                    context.SaveChanges();
                }

                var statuses = new List<Status>()
                {
                    // User statuses
                    new Status() { Name = UserStatus.Preactive.ToString(), Code = UserStatusCode.Preactive, TypeId = StatusTypes.UserStatus },
                    new Status() { Name = UserStatus.Active.ToString(), Code = UserStatusCode.Active, TypeId = StatusTypes.UserStatus },
                    new Status() { Name = UserStatus.Inactive.ToString(), Code = UserStatusCode.Inactive, TypeId = StatusTypes.UserStatus },
                    new Status() { Name = UserStatus.Canceled.ToString(), Code = UserStatusCode.Canceled, TypeId = StatusTypes.UserStatus },
                    new Status() { Name = UserStatus.Frozen.ToString(), Code = UserStatusCode.Frozen, TypeId = StatusTypes.UserStatus },
                    new Status() { Name = UserStatus.Blocked.ToString(), Code = UserStatusCode.Blocked, TypeId = StatusTypes.UserStatus },

                    // Notification statuses
                    new Status() { Name = NotificationStatus.Created.ToString(), Code = NotificationStatusCode.Created, TypeId = StatusTypes.NotificationStatus },
                    new Status() { Name = NotificationStatus.Queued.ToString(), Code = NotificationStatusCode.Queued, TypeId = StatusTypes.NotificationStatus },
                    new Status() { Name = NotificationStatus.Succeeded.ToString(), Code = NotificationStatusCode.Succeeded, TypeId = StatusTypes.NotificationStatus },
                    new Status() { Name = NotificationStatus.Failed.ToString(), Code = NotificationStatusCode.Failed, TypeId = StatusTypes.NotificationStatus },
                };
                var newStatuses = statuses.Where(s => !context.Statuses.Select(st => st.Name).Contains(s.Name));
                if (newStatuses.Any())
                {
                    context.Statuses.AddRange(newStatuses);
                    context.SaveChanges();
                }

                #endregion Statuses StatusTypes


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
            }
        }
    }
}
