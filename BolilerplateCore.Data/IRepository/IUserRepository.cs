using BoilerplateCore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Data.IRepository
{
    public interface IUserRepository : IBaseRepository<ApplicationUser, string>
    {
    }
}
