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
    public class StatusService : BaseService<StatusModel, Status, int>, IStatusService
    {
        private readonly IStatusRepository statusRepository;

        public StatusService(IMapper mapper, IStatusRepository statusRepository, IUnitOfWork unitOfWork) : base(mapper, statusRepository, unitOfWork)
        {
            this.statusRepository = statusRepository;
        }
    }
}
