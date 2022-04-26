using BoilerplateCore.Data.Database;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.Repository
{
    public class CityRepository : BaseRepository<City, int>, ICityRepository
    {
        public CityRepository(ISqlServerDbContext context) : base(context)
        {
        }
    }
}
