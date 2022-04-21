using BoilerplateCore.Common.Utility.Constants;
using System;
using System.Collections.Generic;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Common.Models
{
    public class NotificationTypeModel
    {
        public NotificationTypes Id { get; set; }
        public string Name { get; set; }
    }
}
