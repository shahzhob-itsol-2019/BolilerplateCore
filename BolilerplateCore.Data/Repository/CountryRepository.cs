using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Repository
{
    public class CountryRepository : BaseRepository<Country, int>, ICountryRepository
    {
        public CountryRepository(ISqlServerDbContext context) : base(context)
        {
        }
    }
}
