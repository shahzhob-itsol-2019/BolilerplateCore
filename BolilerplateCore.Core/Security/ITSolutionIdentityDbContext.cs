using BoilerplateCore.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Core.Security
{
    public abstract class BoilerplateIdentityDbContext : BoilerplateIdentityDbContext<ApplicationUser, IdentityRole, string> //, ISecurityDbContext
    {
        protected BoilerplateIdentityDbContext(DbContextOptions options) : base(options)
        {
        }

        protected BoilerplateIdentityDbContext()
        {
        }
    }

    public abstract class BoilerplateIdentityDbContext<TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>, new()
        where TRole : IdentityRole<TKey>
    {
        protected BoilerplateIdentityDbContext(DbContextOptions options) : base(options)
        {
        }

        protected BoilerplateIdentityDbContext()
        {
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PreviousPassword>(entity =>
            {
                entity.HasKey(e => new { e.PasswordHash, e.UserId });
            });
        }

        public virtual DbSet<TwoFactorType> TwoFactorTypes { get; set; }
        public virtual DbSet<PreviousPassword> PreviousPasswords { get; set; }
    }
}
