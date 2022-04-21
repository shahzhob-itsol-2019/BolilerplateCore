using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Core.ICommunication
{
    public interface ICommunicationService
    {
        Task<bool> SendEmail(string subject, string content, string toEmail, string fromEmail = null, string fromName = null, string attachment = null);
        Task<bool> SendSms();
    }
}
