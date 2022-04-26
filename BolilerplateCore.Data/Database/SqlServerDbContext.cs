using BoilerplateCore.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.Database
{
    public class SqlServerDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, ISqlServerDbContext
    {
        private readonly IConfiguration configuration;
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options, IConfiguration configuration) 
            : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
            this.configuration = configuration;
        }

        public SqlServerDbContext()
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                   .HasIndex(u => u.NicNumber)
                   .IsUnique();

            // NOTE: This piece of code is used for changing the table creation scripts. If we want to make all of PG
            // tables/columns in lower case - this can be uncommented and migration deleted.
            // PostgreSQL is case sensitive and it's inconvenient to query if it's not done directly through EF
            // i.e. \d Accounts won't return anything although Accounts
            // table exists, while \d "Accounts" will work as opposed to \d accounts which works either way if table
            // name is accounts.

            // Lower case table names
            //modelBuilder.Model.GetEntityTypes()
            //    .ToList()
            //    .ForEach(e => e.SetTableName(e.GetTableName().ToLower()));

            // Lower column names
            //modelBuilder.Model.GetEntityTypes()
            //    .SelectMany(e => e.GetProperties())
            //    .ToList()
            //    .ForEach(p => p.SetColumnName(p.GetColumnName().ToLower()));

            // DeviceId is a unique field
            //modelBuilder.Entity<User>()
            //     .HasIndex(player => player.DeviceId)
            //     .IsUnique();


            //modelBuilder.Entity<Machine>()
            //    .HasOne(m => m.MachineLocation)
            //        .WithOne(ml => ml.Machine)
            //        .HasForeignKey<Machine>(m => m.MachineLocationId)
            //    .OnDelete(DeleteBehavior.SetNull);
        }

        public int SaveChanges(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public Task<int> SaveChangesWithoutStamp()
        {
            return base.SaveChangesAsync(true, default);
        }

        protected virtual void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            var now = DateTime.UtcNow;
            //var user = GetCurrentUser();

            foreach (var entry in entries)
            {
                if (entry.Entity is Entities.BaseEntity baseEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            entry.Property("CreatedAt").IsModified = false;
                            entry.Property("CreatedBy").IsModified = false;
                            baseEntity.UpdatedAt = now;
                            //baseEntity.UpdatedBy = user;
                            break;

                        case EntityState.Added:
                            baseEntity.CreatedAt = now;
                            //baseEntity.CreatedBy = user;
                            baseEntity.UpdatedAt = now;
                            //baseEntity.UpdatedBy = user;
                            break;
                    }
                }
            }
        }
        public virtual DbSet<ApplicationUser> User { get; set; }
        public virtual DbSet<TwoFactorType> TwoFactorTypes { get; set; }
        public virtual DbSet<Addresses> Addresses { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Company> Companies { get; set; }

        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<StatusType> StatusTypes { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public virtual DbSet<NotificationType> NotificationTypes { get; set; }
    }
}
