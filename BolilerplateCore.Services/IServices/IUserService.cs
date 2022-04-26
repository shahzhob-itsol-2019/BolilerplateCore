//using BoilerplateCore.Common.Models;
//using BoilerplateCore.Common.Options;
//using BoilerplateCore.Core.Entities;
//using BoilerplateCore.Data.Entities;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;

//namespace BoilerplateCore.Services.IService
//{
//    public interface IUserService : IBaseService<UserModel, ApplicationUser, string>
//    {
//        Task<BaseModel> CreateUser(RegisterUserModel model);
//        Task<BaseModel> CreateMerchant(RegisterMerchantModel model);
//        Task<LoginResponse> CreateExternalUser(RegisterExternalModel model);
//        Task<BaseModel> ConfirmEmail(string userId, string code);
//        Task<BaseModel> SetPassword(string userId, string newPassword);
//        Task<BaseModel> ChangeUserStatus(string userId, string status);
//        Task<BaseModel> ForgotPassword(string email);
//        Task<BaseModel> SendEmailCode(string userId, string email);
//        Task<BaseModel> SendPhoneCode(string userId, string phoneNumber);
//        Task<BaseModel> SendTwoFactorToken(string userName, string provider);
//        Task<LoginResponse> Login(string userName, string password, bool persistCookie = false);
//        Task<LoginResponse> ExternalLogin(string loginProvider, string providerKey, bool isPersistent = false, bool bypassTwoFactor = false);
//        Task<BaseModel> UpdateUserDetail(UserModel userInfo);
//    }
//}
