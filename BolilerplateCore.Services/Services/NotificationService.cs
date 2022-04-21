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
    public class NotificationService : BaseService<NotificationModel, Notification, int>, INotificationService
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationService(IMapper mapper, INotificationRepository notificationRepository, IUnitOfWork unitOfWork) : base(mapper, notificationRepository, unitOfWork)
        {
            this.notificationRepository = notificationRepository;
        }
    }
}
