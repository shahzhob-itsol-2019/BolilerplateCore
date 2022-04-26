using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Repository
{
    public class CompanyRepository : BaseRepository<Company, int>, ICompanyRepository
    {
        public CompanyRepository(ISqlServerDbContext context) : base(context)
        {
        }
    }
}
