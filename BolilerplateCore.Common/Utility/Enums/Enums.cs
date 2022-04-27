using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoilerplateCore.Common.Utility
{
    public class Enums
    {
        public enum ApplicationType
        {
            CoreApi = 1,
            Web = 2
        }

        public enum StatusTypes
        {
            UserStatus = 1,
            NotificationStatus = 2
        }

        public enum UserStatus
        {
            Preactive,
            Active,
            Inactive,
            Canceled,
            Frozen,
            Blocked
        }

        public enum NotificationStatus
        {
            Created,
            Queued,
            Succeeded,
            Failed
        }
        public enum TwoFactorTypes
        {
            None = 1,
            Email = 2,
            Phone = 3,
            Authenticator = 4
        }

        public enum LoginStatus
        {
            Locked = 0,
            AccountLocked,
            InvalidCredential,
            Succeded,
            TimeoutLocked,
            Failed,
            RequiresTwoFactor
        }

        public enum UserRoles
        {
            SuperAdmin,
            Admin,
            User,
            Customer,
            Merchant,
            Guest
        }

        public enum UserStatusType
        {
            Preactive,
            Active,
            Inactive,
            Cancel,
            Freez,
            Block
        }

        public enum CustomClaimTypes
        {
            User,
            FirstName,
            LastName,
            ValidationCallTime,
            SecurityStamp
        }

        public enum ResponseType
        {
            Success,
            Error
        }
        public enum NotificationTypes
        {
            Email = 1,
            Sms = 2,
            Site = 3,
            Push = 4
        }

        public enum NotificationTemplates
        {
            EmailUserRegisteration = 1,
            SmsUserRegisteration = 2,
            EmailForgotPassword = 3,
            SmsForgotPassword = 4,
            EmailSetPassword = 5,
            SmsSetPassword = 6,
            EmailChangePassword = 7,
            SmsChangePassword = 8,
            EmailTwoFactorToken = 9,
            SmsTwoFactorToken = 10,
            EmailUserStatusChange = 11,
            SmsUserStatusChange = 12
        }

        public enum Module
        {
            Clientes,
            Profile,
            Home,
            Consulta_Tarjeta_Debito,
            Seguridad,
            Débito_por_voluntad_del_cliente
        }

        public enum LogResult
        {
            Success,
            Failed
        }
        public enum Option
        {
            Clientes,
            Security,
            Permisos,
            Celular
        }

        public enum Action
        {
            Actualizar_Celular
        }
        public enum StatusType
        {
            ApplicionStatus = 1,
            OperationStatus = 2
        }
        public enum Gender
        {
            Male,
            Female
        }

        public enum TransferOrigin
        {
            Api,
            Web,
            File
        }
        public struct Severity
        {
            public const string INFO = "INFO";
            public const string WARN = "WARN";
            public const string ERROR = "ERROR";
        }

        public enum EmailTemplateType
        {
            PaymentStatusTemplate,
        }

        public enum UploadQuality
        {
            High,
            Medium,
            Low
        }

        public enum RequestLogTypes
        {
            SmsApi = 17,
            UserApi_GetUser = 27,
            UpdatePhoneNumber = 67,
        }

        public enum Type
        {
            All,
            Aval,
            NonAval
        }

        public enum VerificationCodeType
        {
            UserVerification,
            ResetPasswordEmail,
            ResetPasswordPhone,
            ConfirmationEmail,
            EasyPaymentVerification,
            ChangePhoneNumberVerification,
            ChangeEmailVerification
        }
        public enum Platform
        {
            Api,
            Web,
        }

        public enum ApplicationModes
        {
            Development,
            Production
        }

        public enum TransactionKind
        {
            Credit,
            Debit
        }

        public enum ResponseTypes
        {
            SUCCESS,
            FAILURE,
        }

        public enum UserSearchType
        {
            EMAIL,
            PHONE,
            USERID,
            CEDULA,
            ACCOUNTNO,
            ALTERNATIVEACCOUNTID
        }

        public enum ApplicationUserStatus
        {
            Preactive,
            Active,
            Inactive,
            Canceled,
            Frozen,
            Blocked,
        }

        public enum ApplicationUserTokenStatus
        {
            ACTIVE,
            DEACTIVATED,
            DELETE,
            SUSPEND,
            RESUME
        }

        public enum ApplicationUserNameStatus
        {
            PREACTIVO,
            ACTIVO,
            INACTIVO,
            CANCELADO,
            BLOQUEADO,
            NOVINCULADO
        }

        public enum APIResponseTypes
        {
            SUCCESS,
            FAILURE
        }

        public struct NotificationTypeCodes
        {
            public const string Email = "EMAIL";
            public const string Sms = "SMS";
            public const string Site = "SITE";
            public const string Push = "PUSH";
            public const string Voice = "VOICE";
        }




        public enum ProjectNames
        {
            CoreApi,
            InternalApi,
            MerchantApi,
            MobileApi,
            WebClient,
            PublicApi,
            BackgroundJobs
        }


        public enum SystemSettingKeys
        {

        }

        public enum TransactionsTypes
        {

        }

        public enum LoginResponseTypes
        {
            REDIRECT_WEB_SITE,
            REDIRECT_CUSTOMER_SITE
        }



        public enum Framework
        {
            Angular,
            MVC
        }


        public struct LogTypes
        {
            public const string Information = "Information";
            public const string Error = "Error";
            public const string Warning = "Warning";
            public const string SuccessAudit = "SuccessAudit";
            public const string FailureAudit = "FailureAudit";
        }

        public struct LogCategories
        {
            public const string General = "General";
            public const string System = "System";
            public const string Services = "Services";
            public const string Integration = "Integration";
        }


        public enum ErrorTypes
        {
            Application = 1,
            Connection = 2,
            Service = 3
        }


        public struct RoleTypes
        {
            public const string natural = "natural";
            public const string person = "person";
            public const string MerchantExternal = "MerchantExternal";
        }


        public struct RepresentativeType
        {
            public const string LegalRepresentative = "Legal Representative";
            public const string BusinessOwner = "Business Owner";
        }


        public struct AuthorizationServiceSeverity
        {
            public const string Success = "Success";
            public const string Failure = "Failure";
            public const string Warning = "Warning";
        }


        public enum OpetationType
        {
            Authorization,
            Rollback,
            CostQuery
        }


        public enum AccountType
        {
            Corriente,
            Ahorros
        }



        public class EnumWithDefination<T>
        {
            public T Value { get; set; }
            public string Defination { get; set; }
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }



        #region IdServer
        public static class PersistedGrantTypes
        {
            public const string AuthorizationCode = "authorization_code";
            public const string ReferenceToken = "reference_token";
            public const string RefreshToken = "refresh_token";
            public const string UserConsent = "user_consent";
        }
        #endregion IdServer
    }
}
