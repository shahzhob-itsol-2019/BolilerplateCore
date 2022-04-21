using BoilerplateCore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoilerplateCore.Core.Security
{
    public interface ISecurityDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        DbSet<Data.Entities.ApplicationUser> AppUsers { get; set; }
        DbSet<TwoFactorType> TwoFactorTypes { get; set; }
        DbSet<PreviousPassword> PreviousPasswords { get; set; }
    }
}
