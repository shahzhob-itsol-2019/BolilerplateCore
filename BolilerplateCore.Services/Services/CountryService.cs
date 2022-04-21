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
    public class CountryService : BaseService<CountryModel, Country, int>, ICountryService
    {
        private readonly ICountryRepository countryRepository;

        public CountryService(IMapper mapper, ICountryRepository countryRepository, IUnitOfWork unitOfWork) : base(mapper, countryRepository, unitOfWork)
        {
            this.countryRepository = countryRepository;
        }
    }
}
