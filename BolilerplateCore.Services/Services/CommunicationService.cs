using BoilerplateCore.Data.Options;
using BoilerplateCore.Services.IServices;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BoilerplateCore.Services
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public CommunicationService(IEmailService emailService, ISmsService smsService)
        {
            _emailService = emailService;
            _smsService = smsService;
        }

        public async Task<bool> SendEmail(string subject, string content, string toEmail, string fromEmail = null, string fromName = null, string attachment = null)
        {
            return await _emailService.SendEmail(subject, content, toEmail, fromEmail, fromName, attachment);
        }

        public async Task<bool> SendSms()
        {
            return await _smsService.SendSms();
        }
    }

    public class EmailServiceGoogle : IEmailService
    {
        private readonly GoogleOptions googleOptions;
        private string FromName = string.Empty;
        private string FromEmail = string.Empty;
        private string Username = string.Empty;
        private string Password = string.Empty;
        private string Host = string.Empty;
        private int Port;
        private bool EnableSsl;
        private bool UseDefaultCredentials;

        public EmailServiceGoogle(IOptionsSnapshot<GoogleOptions> googleOptions)
        {
            this.googleOptions = googleOptions.Value;
            FromName = this.googleOptions.FromName;
            FromEmail = this.googleOptions.FromEmail;
            Username = this.googleOptions.Username;
            Password = this.googleOptions.Password;
            Host = this.googleOptions.Host;
            Port = this.googleOptions.Port;
            EnableSsl = this.googleOptions.EnableSsl;
            UseDefaultCredentials = this.googleOptions.UseDefaultCredentials;
        }

        public async Task<bool> SendEmail(string subject, string content, string toEmail, string fromEmail = null, string fromName = null, string attachment = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subject))
                    subject = FromName;
                if (string.IsNullOrWhiteSpace(fromName))
                    fromName = FromName;
                if (string.IsNullOrWhiteSpace(fromEmail))
                    fromEmail = FromEmail;

                var mailMsg = new MailMessage()
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = true
                };

                if (!string.IsNullOrEmpty(attachment))
                    mailMsg.Attachments.Add(new Attachment(attachment));

                mailMsg.To.Add(toEmail);

                SmtpClient smtpClient = new SmtpClient
                {
                    Host = Host,
                    Port = Port,
                    EnableSsl = EnableSsl,
                    UseDefaultCredentials = UseDefaultCredentials,
                    Credentials = new System.Net.NetworkCredential(Username, Password)
                };
                smtpClient.Send(mailMsg);
                mailMsg.Dispose();
            }
            catch (Exception ex)
            {
                var test = ex;
                return false;
            }
            return true;
        }
    }

    public class SmsServiceTest : ISmsService
    {
        public async Task<bool> SendSms()
        {
            throw new NotImplementedException();
        }
    }
}
