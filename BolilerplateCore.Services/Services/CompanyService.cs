using AutoMapper;
using BoilerplateCore.Common.Models;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using BoilerplateCore.Services.IService;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Services
{
    public class CompanyService : BaseService<CompanyModel, Company, int>, ICompanyService
    {
        private readonly ICompanyRepository companyRepository;

        public CompanyService(IMapper mapper, ICompanyRepository companyRepository, IUnitOfWork unitOfWork) : base(mapper, companyRepository, unitOfWork)
        {
            this.companyRepository = companyRepository;
        }
    }
}
