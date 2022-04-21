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
    public class PermissionService : BaseService<PermissionModel, Permission, int>, IPermissionService
    {
        private readonly IPermissionRepository permissionRepository;

        public PermissionService(IMapper mapper, IPermissionRepository permissionRepository, IUnitOfWork unitOfWork) : base(mapper, permissionRepository, unitOfWork)
        {
            this.permissionRepository = permissionRepository;
        }
    }
}
