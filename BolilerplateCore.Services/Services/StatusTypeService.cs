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
    public class StatusTypeService : BaseService<StatusTypeModel, StatusType, int>, IStatusTypeService
    {
        private readonly IStatusTypeRepository statusTypeRepository;

        public StatusTypeService(IMapper mapper, IStatusTypeRepository statusTypeRepository, IUnitOfWork unitOfWork) : base(mapper, statusTypeRepository, unitOfWork)
        {
            this.statusTypeRepository = statusTypeRepository;
        }
    }
}
