using AutoMapper;
using BoilerplateCore.Common.Models;
using BoilerplateCore.Common.Utility.Constants;
using BoilerplateCore.Data.Entities;
using BoilerplateCore.Data.IRepository;
using BoilerplateCore.Services.IService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Services
{
    public class NotificationTemplateService : BaseService<NotificationTemplateModel, NotificationTemplate, int>, INotificationTemplateService
    {
        private readonly INotificationTemplateRepository _notificationTemplateRepository;

        public NotificationTemplateService(IMapper mapper, INotificationTemplateRepository notificationTemplateRepository, IUnitOfWork unitOfWork) : base(mapper, notificationTemplateRepository, unitOfWork)
        {
            _notificationTemplateRepository = notificationTemplateRepository;
        }

        public async Task<NotificationTemplateModel> GetNotificationTemplate(NotificationTemplates notificationTemplates, NotificationTypes notificationTypes)
        {
            var template = await _notificationTemplateRepository.FirstOrDefaultAsync(x => x.Id == notificationTemplates && x.NotificationTypeId == notificationTypes);
            return mapper.Map<NotificationTemplate, NotificationTemplateModel>(template); ;
        }
    }
}
