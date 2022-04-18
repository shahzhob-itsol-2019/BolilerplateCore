using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BoilerplateCore.Core.Entities;
using BoilerplateCore.Data.Entities;

namespace BoilerplateCore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //public const string Username = "https://winner-winner.com/username";
        //public const string Roles = "https://winner-winner.com/roles";

        private readonly IConfiguration configuration;
        //private readonly IHttpContextAccessor httpContextAccessor;
        //public ApplicationDbContext(IConfiguration configuration)
        //{
        //    this.configuration = configuration;
        //}
        //public ApplicationDbContext(IHttpContextAccessor httpContextAccessor)
        //{
        //    this.httpContextAccessor = httpContextAccessor;
        //}

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IConfiguration configuration)
            : base(options)
        {
            this.configuration = configuration;
            //this.httpContextAccessor = httpContextAccessor;
        }

        public virtual DbSet<ApplicationUser> User { get; set; }
        DbSet<Status> Statuses { get; set; }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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

        //private string GetCurrentUser() => $"{GetContextClaims(Roles) ?? string.Empty} : {GetContextClaims(Username) ?? "<unknown>"}";

        //private string GetContextClaims(string typeName) => httpContextAccessor?.HttpContext?.User?.Claims?.FirstOrDefault(c => c?.Type == typeName)?.Value;

    }
}
