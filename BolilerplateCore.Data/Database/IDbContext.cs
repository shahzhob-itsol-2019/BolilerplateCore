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
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        int SaveChanges(CancellationToken cancellationToken = default(CancellationToken));

        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        DbSet<Status> Statuses { get; set; }
    }
}

