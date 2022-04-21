using BoilerplateCore.Common.Utility.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Data.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public NotificationTypes NotificationTypeId { get; set; }
        public string Recipient { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public int StatusId { get; set; }
        public int Attempts { get; set; }       
        public DateTime ProcessedAt { get; set; }
        public string Result { get; set; }


        [ForeignKey("NotificationTypeId")]
        public virtual NotificationType NotificationType { get; set; }

        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }
    }
}
