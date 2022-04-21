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
    public class AddressService : BaseService<AddressModel, Addresses, int>, IAddressService
    {
        private readonly IAddressRepository addressRepository;

        public AddressService(IMapper mapper, IAddressRepository addressRepository, IUnitOfWork unitOfWork) : base(mapper, addressRepository, unitOfWork)
        {
            this.addressRepository = addressRepository;
        }
    }
}
