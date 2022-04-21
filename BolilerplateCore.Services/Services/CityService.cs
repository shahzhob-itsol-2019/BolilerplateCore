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
    public class CityService : BaseService<CityModel, City, int>, ICityService
    {
        private readonly ICityRepository cityRepository;

        public CityService(IMapper mapper, ICityRepository cityRepository, IUnitOfWork unitOfWork) : base(mapper, cityRepository, unitOfWork)
        {
            this.cityRepository = cityRepository;
        }
    }
}
