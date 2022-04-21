using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContext dbContext;

        public UnitOfWork(IDbContext context)
        {
            dbContext = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await dbContext.SaveChangesAsync();
        }
    }
}
