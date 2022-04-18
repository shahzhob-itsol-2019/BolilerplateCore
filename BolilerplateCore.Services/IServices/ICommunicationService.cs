using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerplateCore.Services.IServices
{
    public interface ICommunicationService
    {
        Task<bool> SendEmail(string subject, string content, string toEmail, string fromEmail = null, string fromName = null, string attachment = null);
        Task<bool> SendSms();
    }

    public interface IEmailService
    {
        Task<bool> SendEmail(string subject, string content, string toEmail, string fromEmail = null, string fromName = null, string attachment = null);
    }

    public interface ISmsService
    {
        Task<bool> SendSms();
    }
}
