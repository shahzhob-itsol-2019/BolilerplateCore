//using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.Database
{
    public interface ISqlServerDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        int SaveChanges(CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        DbSet<ApplicationUser> User { get; set; }
        DbSet<TwoFactorType> TwoFactorTypes { get; set; }
        // DbSet<Permission> Permissions { get; set; }
        DbSet<Addresses> Addresses { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<City> Cities { get; set; }
        DbSet<Company> Companies { get; set; }

        DbSet<Status> Statuses { get; set; }
        DbSet<StatusType> StatusTypes { get; set; }

        DbSet<Notification> Notifications { get; set; }
        DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        DbSet<NotificationType> NotificationTypes { get; set; }
    }
}

