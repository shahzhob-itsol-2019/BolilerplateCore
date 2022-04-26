//using AutoMapper;
//using BoilerplateCore.Common.Helpers.Interfaces;
//using BoilerplateCore.Common.Models;
//using BoilerplateCore.Common.Options;
//using BoilerplateCore.Core.ISecurity;
//using BoilerplateCore.Core.Entities;
//using BoilerplateCore.Data.IRepository;
//using BoilerplateCore.Core.ICommunication;
//using BoilerplateCore.Services.IService;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using static BoilerplateCore.Common.Utility.Enums;
//using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;
//using BoilerplateCore.Data.Entities;

//namespace BoilerplateCore.Services
//{
//    public class UserService : BaseService<UserModel, ApplicationUser, string>, IUserService
//    {
//        private readonly IUserRepository userRepository;

//        private readonly BoilerplateOptions boilerplateOptions;

//        private readonly ISecurityService _securityService;
//        private readonly IStatusService _statusService;
//        private readonly ICompanyService _companyService;
//        private readonly IAddressService _addressService;
//        private readonly INotificationTemplateService _notificationTemplateService;
//        private readonly ICommunicationService _communicationService;

//        public UserService(
//            IMapper mapper,
//            IUserRepository userRepository,
//            IUnitOfWork unitOfWork,

//            IOptionsSnapshot<BoilerplateOptions> boilerplateOptions,

//            ISecurityService securityService,
//            IStatusService statusService,
//            ICompanyService companyService,
//            IAddressService addressService,
//            INotificationTemplateService notificationTemplateService,
//            ICommunicationService communicationService
//            ) : base(mapper, userRepository, unitOfWork)
//        {
//            this.userRepository = userRepository;

//            this.boilerplateOptions = boilerplateOptions.Value;

//            _securityService = securityService;
//            _statusService = statusService;
//            _companyService = companyService;
//            _addressService = addressService;
//            _notificationTemplateService = notificationTemplateService;
//            _communicationService = communicationService;
//        }



//        public async Task<BaseModel> CreateUser(RegisterUserModel model)
//        {
//            var preactiveStats = await _statusService.FirstOrDefaultAsync(s => s.Name.Equals(UserStatusType.Preactive.ToString()));
//            if (preactiveStats == null)
//                throw new Exception($"{UserStatusType.Preactive.ToString()} is not found in the system.");

//            var result = await _securityService.CreateUser(model.FirstName,model.LastName, model.UserName, model.Email, model.PhoneNumber, model.Password, model.ConfirmPassword, model.CreateActivated);
//            if (!result.Success)
//                return BaseModel.Failed(result.Message);

//            var userResult = await _securityService.GetUser(model.Email);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (Common.Models.UserClaims)userResult.Data;

//            var userModel = new UserModel
//            {
//                Id = user.Id,
//                FirstName = model.FirstName,
//                LastName = model.LastName,
//                StatusId = preactiveStats.Id
//            };
//            await this.Add(userModel);

//            var address = new AddressModel
//            {
//                UserId = user.Id,
//                CountryId = model.CountryId,
//                CityId = model.CityId,
//                Address = model.Address,
//                ZipCode = model.ZipCode,
//                IsDefault = true
//            };

//            var addressResult = await _addressService.Add(address);
//            if (addressResult == null)
//                return BaseModel.Failed("User added in the system, but address cannot be saved.");

//            if (!model.CreateActivated)
//            {
//                var code = (string)result.Data;
//                var link = boilerplateOptions.WebUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(code);
//                var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailUserRegisteration, NotificationTypes.Email);
//                var emailMessage = template.MessageBody.Replace("#Name", $"{ model.FirstName} { model.LastName}")
//                                                       .Replace("#Link", $"{link}");

//                var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
//                if (!sent)
//                    return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

//                return new BaseModel { Success = true, Message = "Account created successfully. A confirmation link has been sent to your specified email , click the link to confirm your email and proceed to login." };
//            }

//            return BaseModel.Succeed(message: result.Message);
//        }

//        public async Task<BaseModel> CreateMerchant(RegisterMerchantModel model)
//        {
//            var preactiveStats = await _statusService.FirstOrDefaultAsync(s => s.Name.Equals(UserStatusType.Preactive.ToString()));
//            if (preactiveStats == null)
//                throw new Exception($"{UserStatusType.Preactive.ToString()} is not found in the system.");

//            var result = await _securityService.CreateUser(model.FirstName, model.LastName, model.UserName, model.Email, model.PhoneNumber, model.Password, model.ConfirmPassword, model.CreateActivated);
//            if (!result.Success)
//                return BaseModel.Failed(result.Message);

//            var userResult = await _securityService.GetUser(model.Email);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (Common.Models.UserClaims)userResult.Data;

//            var company = new CompanyModel
//            {
//                Name = model.CompanyName,
//                Phone = model.CompanyPhone,
//                Website = model.CompanyWebsite
//            };
//            var companyResult = await _companyService.Add(company);
//            if (companyResult != null)
//                return BaseModel.Failed("User added in the system, but company cannot be saved.");

//            var userModel = new UserModel
//            {
//                Id = user.Id,
//                FirstName = model.FirstName,
//                LastName = model.LastName,
//                CompanyId = company.Id,
//                StatusId = preactiveStats.Id
//            };
//            var userModelResult = await this.Add(userModel);
//            if (userModelResult != null)
//                return BaseModel.Failed("User and company added in the system, but user info cannot be saved.");

//            var addresses = new List<AddressModel>
//            {
//                new AddressModel
//                {
//                    UserId = user.Id,
//                    CountryId = model.CountryId,
//                    CityId = model.CityId,
//                    Address = model.Address,
//                    ZipCode = model.ZipCode,
//                    IsDefault = true
//                },
//                new AddressModel
//                {
//                    CompanyId = company.Id,
//                    CountryId = model.CountryId,
//                    CityId = model.CityId,
//                    Address = model.CompanyAddress,
//                    ZipCode = model.CompanyZipCode,
//                }
//            };

//            var addressResult = await _addressService.AddRange(addresses);
//            if (addressResult != null)
//                return BaseModel.Failed("User, user model and company added in the system, but address cannot be saved.");

//            if (!model.CreateActivated)
//            {
//                var code = (string)result.Data;
//                var link = boilerplateOptions.WebUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(code);
//                var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailUserRegisteration, NotificationTypes.Email);
//                var emailMessage = template.MessageBody.Replace("#Name", $"{ model.FirstName} { model.LastName}")
//                                                       .Replace("#Link", $"{link}");

//                var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
//                if (!sent)
//                    return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

//                return new BaseModel { Success = true, Message = "Account created successfully. A confirmation link has been sent to your specified email , click the link to confirm your email and proceed to login." };
//            }

//            return BaseModel.Succeed(message: result.Message);
//        }

//        public async Task<Common.Models.LoginResponse> CreateExternalUser(RegisterExternalModel model)
//        {
//            var preactiveStats = await _statusService.FirstOrDefaultAsync(s => s.Name.Equals(UserStatusType.Preactive.ToString()));
//            if (preactiveStats == null)
//                throw new Exception($"{UserStatusType.Preactive.ToString()} is not found in the system.");

//            var result = await _securityService.CreateExternalUser(model.FirstName, model.LastName, model.Email, model.Provider, model.ProviderKey, model.ProviderDisplayName);
//            if (result.Status == LoginStatus.Succeded)
//            {
//                var userResult = await _securityService.GetUser(model.Email);
//                if (!userResult.Success)
//                    return new Common.Models.LoginResponse { Status = LoginStatus.Failed, Message = userResult.Message };
//                var user = (Common.Models.UserClaims)userResult.Data;
//                var userModel = new UserModel
//                {
//                    Id = user.Id,
//                    FirstName = model.FirstName,
//                    LastName = model.LastName,
//                    StatusId = preactiveStats.Id
//                };
//                await this.Add(userModel);
//            }
//            return result;
//        }

//        public async Task<BaseModel> ConfirmEmail(string userId, string code)
//        {
//            var response = await _securityService.ConfirmEmail(userId, code);
//            if (response.Success)
//            {
//                var statusResult = await ChangeUserStatus(userId, UserStatusType.Active.ToString());
//                if (!statusResult.Success)
//                    return new BaseModel { Success = false, Message = statusResult.Message };
//            }

//            return response;
//        }

//        public async Task<Common.Models.LoginResponse> Login(string userName, string password, bool persistCookie = false)
//        {
//            var loginResponse = await _securityService.Login(userName, password, persistCookie);
//            if (loginResponse.Status == LoginStatus.Succeded)
//            {
//                return loginResponse;
//            }
//            else if (loginResponse.Status == LoginStatus.RequiresTwoFactor)
//            {
//                // ToDo: Check how SendTwoFactorToken works in SingleSignOn
//                var authenticationResult = await _securityService.GetAuthenticationDetail(userName);
//                if (authenticationResult.Success)
//                {
//                    var authenticationDetail = (Common.Models.UserAuthenticationInfo)authenticationResult.Data;

//                    if (authenticationDetail.TwoFactorType == BoilerplateCore.Common.Utility.Constants.TwoFactorTypes.Email)
//                        await SendTwoFactorToken(userName, BoilerplateCore.Common.Utility.Constants.TwoFactorTypes.Email);
//                    else if (authenticationDetail.TwoFactorType == BoilerplateCore.Common.Utility.Constants.TwoFactorTypes.Phone)
//                        await SendTwoFactorToken(userName, BoilerplateCore.Common.Utility.Constants.TwoFactorTypes.Phone);

//                    return new Common.Models.LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = authenticationDetail };
//                }
//                return new Common.Models.LoginResponse { Status = LoginStatus.Failed, Message = authenticationResult.Message };
//            }
//            else
//            {
//                return loginResponse;
//            }
//        }

//        public async Task<Common.Models.LoginResponse> ExternalLogin(string loginProvider, string providerKey, bool isPersistent = false, bool bypassTwoFactor = false)
//        {
//            var loginResponse = await _securityService.ExternalLogin(loginProvider, providerKey, isPersistent, bypassTwoFactor);
//            if (loginResponse.Status == LoginStatus.Succeded)
//            {
//                return loginResponse;
//            }
//            else if (loginResponse.Status == LoginStatus.RequiresTwoFactor)
//            {
//                // ToDo: Check how SendTwoFactorToken works in SingleSignOn

//                var userName = (string)loginResponse.Data;
//                var authenticationResult = await _securityService.GetAuthenticationDetail(userName);
//                if (authenticationResult.Success)
//                {
//                    var authenticationDetail = (Common.Models.UserAuthenticationInfo)authenticationResult.Data;

//                    if (authenticationDetail.TwoFactorType == Common.Utility.Constants.TwoFactorTypes.Email)
//                        await SendTwoFactorToken(userName, Common.Utility.Constants.TwoFactorTypes.Email);
//                    else if (authenticationDetail.TwoFactorType == Common.Utility.Constants.TwoFactorTypes.Phone)
//                        await SendTwoFactorToken(userName, Common.Utility.Constants.TwoFactorTypes.Phone);

//                    return new Common.Models.LoginResponse { Status = LoginStatus.RequiresTwoFactor, Message = "Requires two factor varification.", Data = authenticationDetail };
//                }
//                return new LoginResponse { Status = LoginStatus.Failed, Message = authenticationResult.Message };
//            }
//            else
//            {
//                return loginResponse;
//            }
//        }

//        public async Task<BaseModel> SetPassword(string userId, string newPassword)
//        {
//            var userResult = await _securityService.GetUserDetail(userId);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (UserInfo)userResult.Data;

//            var result = await _securityService.SetPassword(userId, newPassword);
//            if (result.Success)
//            {
//                var statusResult = await ChangeUserStatus(user.Id, UserStatusType.Preactive.ToString());
//                if (!statusResult.Success)
//                    return new BaseModel { Success = false, Message = statusResult.Message };

//                var emailConfirmationToken = (string)result.Data;
//                var link = boilerplateOptions.WebUrl + "Account/ConfirmEmail?userId=" + user.Id + "&code=" + HttpUtility.UrlEncode(emailConfirmationToken);
//                var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailSetPassword, NotificationTypes.Email);
//                var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
//                                                       .Replace("#Link", $"{link}");

//                var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
//                if (!sent)
//                    return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

//                return new BaseModel { Success = true, Message = $"Password has been set successfully. But to confirm your email address, a confirmation link has been sent to {user.Email}, please verify your email." };
//            }
//            return new BaseModel { Success = false, Message = result.Message ?? "Failed to set password." };
//        }

//        public async Task<BaseModel> ChangeUserStatus(string userId, string status)
//        {
//            status = status == UserStatusType.Preactive.ToString()
//                        ? UserStatus.Preactive.ToString()
//                        : status == UserStatusType.Active.ToString()
//                            ? UserStatus.Active.ToString()
//                            : status == UserStatusType.Inactive.ToString()
//                                ? UserStatus.Inactive.ToString()
//                                : status == UserStatusType.Cancel.ToString()
//                                    ? UserStatus.Canceled.ToString()
//                                    : status == UserStatusType.Freez.ToString()
//                                        ? UserStatus.Frozen.ToString()
//                                        : status == UserStatusType.Block.ToString()
//                                            ? UserStatus.Blocked.ToString()
//                                            : string.Empty;

//            var stats = await _statusService.FirstOrDefaultAsync(s => s.Name.Equals(status));
//            if (stats == null)
//                throw new Exception($"{nameof(status)} is not found in the system.");

//            var appUser = await this.Get(userId);
//            if (appUser == null)
//                return new BaseModel { Success = false, Message = "User not found with specified Id." };

//            if (status.Equals(UserStatusType.Block.ToString()))
//            {
//                var lockoutResult = await _securityService.BlockUser(userId);
//                if (!lockoutResult.Success)
//                    return new BaseModel { Success = false, Message = lockoutResult.Message };
//            }

//            var updateUser = new UserModel
//            {
//                Id = appUser.Id,
//                StatusId = stats.Id
//            };
//            await this.Update(updateUser);

//            return new BaseModel { Success = true, Message = $"User has been successfully { status.ToLower() }." };
//        }

//        public async Task<BaseModel> ForgotPassword(string email)
//        {
//            var userResult = await _securityService.GetUser(email);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (BoilerplateCore.Common.Models.UserClaims)userResult.Data;

//            var resetCodeResult = await _securityService.GeneratePasswordResetToken(email);
//            if (!resetCodeResult.Success)
//                return new BaseModel { Success = false, Message = resetCodeResult.Message };
//            var resetCode = (string)resetCodeResult.Data;

//            var link = boilerplateOptions.WebUrl + "Account/ResetPassword?code=" + HttpUtility.UrlEncode(resetCode);
//            var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailForgotPassword, NotificationTypes.Email);
//            var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
//                                                   .Replace("#Link", $"{link}");

//            var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
//            if (!sent)
//                return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

//            return new BaseModel { Success = true, Message = "Your password reset code has been sent to your specified email address, follow the link to reset your password." };
//        }

//        public async Task<BaseModel> SendEmailCode(string userId, string email)
//        {
//            var userResult = await _securityService.GetUserDetail(userId);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (Common.Models.UserInfo)userResult.Data;

//            var tokenResult = await _securityService.GenerateChangeEmailToken(userId, email);
//            if (!tokenResult.Success)
//                return new BaseModel { Success = false, Message = tokenResult.Message };
//            var token = (string)tokenResult.Data;

//            var link = boilerplateOptions.WebUrl + "Account/ChangeEmail?userId=" + user.Id + "&email=" + email + "&code=" + HttpUtility.UrlEncode(token);
//            var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailChangePassword, NotificationTypes.Email);
//            var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
//                                                   .Replace("#Link", $"{link}");

//            var sent = await _communicationService.SendEmail(template.Subject, emailMessage, email);
//            if (!sent)
//                return BaseModel.Failed("Confirmation link cannot be sent, plz try again latter");

//            return new BaseModel { Success = true, Message = $"A confirmation link has been sent to {email}, please verify your email to change it." };
//        }

//        public async Task<BaseModel> SendPhoneCode(string userId, string phoneNumber)
//        {
//            var codeResult = await _securityService.GenerateChangePhoneNumberToken(userId, phoneNumber);
//            if (!codeResult.Success)
//                return new BaseModel { Success = false, Message = codeResult.Message };

//            // Todo: Phone notification is not done yet.
//            if (!await _communicationService.SendSms()) 
//                return new BaseModel { Success = false, Message = "Sms could not be sent." };

//            return new BaseModel { Success = true, Message = $"Sms has been sent to {phoneNumber}." };
//        }

//        public async Task<BaseModel> SendTwoFactorToken(string userName, string provider)
//        {
//            var userResult = await _securityService.GetUser(userName);
//            if (!userResult.Success)
//                return new BaseModel { Success = false, Message = userResult.Message };
//            var user = (Common.Models.UserClaims)userResult.Data;

//            var tokenResult = await _securityService.GenerateTwoFactorToken(user.Email, provider);
//            if (!tokenResult.Success)
//                return new BaseModel { Success = false, Message = tokenResult.Message };
//            var token = (string)tokenResult.Data;

//            // ToDo: If provder = Email, send code to email Else send code to phone.

//            var template = await _notificationTemplateService.GetNotificationTemplate(NotificationTemplates.EmailTwoFactorToken, NotificationTypes.Email);
//            var emailMessage = template.MessageBody.Replace("#Name", $"{ user.FirstName} { user.LastName}")
//                                                   .Replace("#Token", $"{token}");

//            var sent = await _communicationService.SendEmail(template.Subject, emailMessage, user.Email);
//            if (!sent)
//                return BaseModel.Failed("Code cannot be sent, plz try again latter");

//            return new BaseModel { Success = true, Message = $"A code has been sent to {user.Email}, please verify the code." };
//        }

//        public async Task<BaseModel> UpdateUserDetail(UserModel userInfo)
//        {
//            var user = await Get(userInfo.Id);
//            if (user == null)
//                return new BaseModel { Success = false, Message = "User not exists." };

//            if (!string.IsNullOrWhiteSpace(userInfo.FirstName))
//                user.FirstName = userInfo.FirstName;
//            if (!string.IsNullOrWhiteSpace(userInfo.LastName))
//                user.LastName = userInfo.LastName;
//            //if (!string.IsNullOrWhiteSpace(userInfo.Address))
//            //    user.Address = userInfo.Address;
//            //if (!string.IsNullOrWhiteSpace(userInfo.Gender))
//            //    user.Gender = userInfo.Gender;
//            //if (userInfo.BirthDate.HasValue)
//            //    user.BirthDate = userInfo.BirthDate.Value;
//            //if (!string.IsNullOrWhiteSpace(userInfo.Picture))
//            //    user.Picture = userInfo.Picture;

//            await Update(user);
//            //var userUpdateResult = await Update(user);
//            //if (!userUpdateResult.Succeeded)
//            //{
//            //    var message = userUpdateResult.Errors.FirstOrDefault() != null
//            //                       ? userUpdateResult.Errors.FirstOrDefault().Description
//            //                       : "Faild to update user detail.";
//            //    return new BaseModel { Success = false, Message = message };
//            //}
//            return new BaseModel { Success = true, Message = "User info has been successfully updated." };
//        }
//    }
//}
