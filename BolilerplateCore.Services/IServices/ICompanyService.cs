using BoilerplateCore.Common.Models;
using BoilerplateCore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Services.IService
{
    public interface ICompanyService : IBaseService<CompanyModel, Company, int>
    {
    }
}
