using BoilerplateCore.Data.Database;
using BoilerplateCore.Core.Entities;
using BoilerplateCore.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Text;
using BoilerplateCore.Data.Entities;

namespace BoilerplateCore.Data.Repository
{
    public class UserRepository : BaseRepository<ApplicationUser, string>, IUserRepository
    {
        public UserRepository(ISqlServerDbContext context) : base(context)
        {
        }
    }
}
