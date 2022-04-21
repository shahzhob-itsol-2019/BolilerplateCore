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
    public class NotificationTypeService : BaseService<NotificationTypeModel, NotificationType, int>, INotificationTypeService
    {
        private readonly INotificationTypeRepository notificationTypeRepository;

        public NotificationTypeService(IMapper mapper, INotificationTypeRepository notificationTypeRepository, IUnitOfWork unitOfWork) : base(mapper, notificationTypeRepository, unitOfWork)
        {
            this.notificationTypeRepository = notificationTypeRepository;
        }
    }
}
