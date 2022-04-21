using BoilerplateCore.Common.Models;
using BoilerplateCore.Common.Utility.Constants;
using BoilerplateCore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Services.IService
{
    public interface INotificationTemplateService : IBaseService<NotificationTemplateModel, NotificationTemplate, int>
    {
        Task<NotificationTemplateModel> GetNotificationTemplate(NotificationTemplates notificationTemplates, NotificationTypes notificationTypes);
    }
}
