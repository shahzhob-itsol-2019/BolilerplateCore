using System;
using System.Collections.Generic;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Models
{
    public class NotificationTemplateModel
    {
        public NotificationTemplates Id { get; set; }

        public NotificationTypes NotificationTypeId { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
    }
}
