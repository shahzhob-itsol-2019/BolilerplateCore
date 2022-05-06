using System;
using System.Collections.Generic;
using System.Text;

namespace BoilerplateCore.Common.Options
{
    public class BoilerplateOptions
    {
        public BoilerplateOptions()
        {
        }
        public string ApiUrl { get; set; }
        public string WebUrl { get; set; }
    }

    public class Boilerplate
    {
        public Boilerplate()
        {
        }
        public string WebApiUrl { get; set; }
    }

    #region ComponentOptions
    public class ComponentOptions
    {
        public ComponentOptions()
        {
        }
        public Security Security { get; set; }
        //public string Communication { get; set; }
        public Communication Communication { get; set; }
    }

    public class Security
    {
        public string SecurityService { get; set; }
        public string EncryptionService { get; set; }
    }

    public class Communication
    {
        public Communication()
        {
        }

        public string EmailService { get; set; }
        public string SmsService { get; set; }
    }
    #endregion ComponentOptions


    public class InfrastructureOptions
    {
        public string Documentation { get; set; }
    }

    public class SecurityOptions
    {
        public SecurityOptions()
        {
        }
        public int PasswordLength { get; set; }
        public int AccountLockoutTimeSpan { get; set; }
        public int AccountLoginMaximumAttempts { get; set; }
        public int PreviousPasswordValidationLimit { get; set; }
        public string Authority { get; set; }
        public string RequiredScope { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthenticatorUriFormat { get; set; }
        public int NumberOfRecoveryCodes { get; set; }
        public string Scopes { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public int EncryptionIterationSize { get; set; }
        public string EncryptionPassword { get; set; }
        public string EncryptionSaltKey { get; set; }
        public string EncryptionVIKey { get; set; }
        public bool MicrosoftAuthenticationAdded { get; set; }
        public bool GoogleAuthenticationAdded { get; set; }
        public bool TwitterAuthenticationAdded { get; set; }
        public bool FacebookAuthenticationAdded { get; set; }
    }
    
    public class GoogleOptions
    {
        public GoogleOptions()
        {
        }

        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class OutlookOptions
    {
        public OutlookOptions()
        {
        }

        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationSecret { get; set; }
    }

    public class FacebookOptions
    {
        public FacebookOptions()
        {
        }

        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    public class TwitterOptions
    {
        public TwitterOptions()
        {
        }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
    }
}
