using BoilerplateCore.Common.Filters;
using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Models;
using BoilerplateCore.Common.Options;
using BoilerplateCore.Core.ISecurity;
using BoilerplateCore.RestApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;
using LoginResponse = BoilerplateCore.Common.Models.LoginResponse;

namespace BoilerplateCore.RestApi.Controllers
{
    //[Route("api/Account")]
    //[ApiController]
    public class AccountController : BaseController
    {
        private readonly ISecurityService _securityService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly SecurityOptions securityOptions;
        private readonly CurrentUser currentUser;
        public AccountController(
                ISecurityService securityService,
                IHttpContextAccessor httpContextAccessor,
                IOptionsSnapshot<SecurityOptions> securityOptions,
                CurrentUser currentUser
            )
        {
            _securityService = securityService;
            this.httpContextAccessor = httpContextAccessor;
            this.securityOptions = securityOptions.Value;
            this.currentUser = currentUser;
        }


        #region public actions

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ActionName("Register")]
        [Route("Account/Register")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> Register(RegisterUserModel model)
        {
            try
            {
                var result = await _securityService.CreateUser(model);

                if (!result.Success)
                    return new ObjectResult(BaseModel.Failed(result.Message));

                // Todo:
                //var userInfo = JToken.Parse(result.Data);
                //var userId = userInfo["data"]["userId"].ToString();
                //var emailConfirmationToken = userInfo["data"]["emailConfirmationToken"].ToString();

                return new ObjectResult(BaseModel.Succeed(message: result.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Create(success: false, message: ex.InnerException.Message));

            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ActionName("Login")]
        [Route("Account/Login")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(LoginResponse))]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var result = await _securityService.Login(model.Email, model.Password, model.RememberMe);

                if (result.Status == LoginStatus.Failed)
                    return new OkObjectResult(new LoginResponse { Status = LoginStatus.Failed, Data = null, Message = result.Message });
                var userId = GetUserId();
                return new OkObjectResult(new LoginResponse { Status = result.Status, Data = result.Data, Message = result.Message });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new LoginResponse { Message = "There was an error processing your request, please try again." });
            }
        }

        //
        // POST: /Account/RegisterExternal
        [HttpPost]
        [AllowAnonymous]
        [ActionName("RegisterExternal")]
        [Route("Account/RegisterExternal")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(LoginResponse))]
        public async Task<IActionResult> RegisterExternal(RegisterExternalModel model)
        {
            try
            {
                var result = await _securityService.CreateExternalUser(model);

                if (result.Status == LoginStatus.Failed)
                    return new OkObjectResult(new LoginResponse { Status = LoginStatus.Failed, Data = null, Message = result.Message });

                return new OkObjectResult(new LoginResponse { Status = result.Status, Data = result.Data, Message = result.Message });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new LoginResponse { Message = "There was an error processing your request, please try again." });
            }
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        //[AllowAnonymous]
        //[Authorize(Policy = "Claim.DoB")]
        [ActionName("ExternalLogin")]
        [Route("Account/ExternalLogin")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(LoginResponse))]
        public async Task<IActionResult> ExternalLogin(ExternalLoginModel model)
        {
            try
            {
                var result = await _securityService.ExternalLogin(model.LoginProvider, model.ProviderKey, model.IsPersistent, model.BypassTwoFactor);

                if (result.Status == LoginStatus.Failed)
                    return new OkObjectResult(new LoginResponse { Status = LoginStatus.Failed, Data = null, Message = result.Message });

                return new OkObjectResult(new LoginResponse { Status = result.Status, Data = result.Data, Message = result.Message });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new LoginResponse { Message = "There was an error processing your request, please try again." });
            }
        }

        //
        // POST: /Account/Activation
        [HttpPost]
        [AllowAnonymous]
        [ActionName("Activation")]
        [Route("Account/Activation")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> Activation(ConfirmEmailModel model)
        {
            try
            {
                var response = await _securityService.ConfirmEmail(model.UserId, model.Code);
                if (response.Success)
                    return new ObjectResult(BaseModel.Succeed(message: "Email confirmed. Please login."));

                return new OkObjectResult(BaseModel.Failed(response.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

         //
        // POST: /Account/TwoFactorLogin
        [HttpPost]
        [AllowAnonymous]
        [ActionName("TwoFactorLogin")]
        [Route("Account/TwoFactorLogin")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(LoginResponse))]
        public async Task<IActionResult> TwoFactorLogin(VerifyCodeModel model)
        {
            try
            {
                var result = await _securityService.TwoFactorLogin(model.Provider, model.Code, model.RememberBrowser, model.RememberMachine);

                if (result.Status == LoginStatus.Failed)
                    return new OkObjectResult(new LoginResponse { Status = LoginStatus.Failed, Data = null, Message = result.Message });

                return new OkObjectResult(new LoginResponse { Status = result.Status, Data = result.Data, Message = result.Message });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new LoginResponse { Message = "There was an error processing your request, please try again." });
            }
        }

        //
        // POST: /Account/RecoveryCodeLogin
        [HttpPost]
        [AllowAnonymous]
        [ActionName("RecoveryCodeLogin")]
        [Route("Account/RecoveryCodeLogin")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(LoginResponse))]
        public async Task<IActionResult> RecoveryCodeLogin(VerifyCodeModel model)
        {
            try
            {
                var result = await _securityService.RecoveryCodeLogin(model.Code);

                if (result.Status == LoginStatus.Failed)
                    return new OkObjectResult(new LoginResponse { Status = LoginStatus.Failed, Data = null, Message = result.Message });

                return new OkObjectResult(new LoginResponse { Status = result.Status, Data = result.Data, Message = result.Message });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new LoginResponse { Message = "There was an error processing your request, please try again." });
            }
        }

        //
        // GET: /Account/GetLoginProviders
        [HttpGet]
        [AllowAnonymous]
        [ActionName("GetLoginProviders")]
        [Route("Account/GetLoginProviders")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetLoginProviders()
        {
            try
            {
                var result = await _securityService.GetLoginProviders();
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetLoginProperties
        [HttpGet]
        [AllowAnonymous]
        [ActionName("GetLoginProperties")]
        [Route("Account/GetLoginProperties")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetLoginProperties(string provider, string redirectUrl, string userId = null)
        {
            try
            {
                var result = await _securityService.GetLoginProperties(provider, redirectUrl, userId);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/AddLogin
        [HttpPost]
        [Authorize]
        [ActionName("AddLogin")]
        [Route("Account/AddLogin")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> AddLogin(AddLoginModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.AddLogin(userId, model.Provider, model.ProviderKey, model.ProviderDisplayName);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/RemoveLogin
        [HttpPost]
        [Authorize]
        [ActionName("RemoveLogin")]
        [Route("Account/RemoveLogin")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> RemoveLogin(RemoveLoginModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.RemoveLogin(userId, model.Provider, model.ProviderKey);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetAuthenticationDetail
        [HttpGet]
        [ActionName("GetAuthenticationDetail")]
        [Route("Account/GetAuthenticationDetail")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetAuthenticationDetail()
        {
            try
            {
                var userName = GetUserName();
                var result = await _securityService.GetAuthenticationDetail(userName);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetUser
        [HttpGet]
        [Authorize]
        [ActionName("GetUser")]
        [Route("Account/GetUser")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var userName = GetUserName();
                var result = await _securityService.GetUser(userName);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetExternalUser
        [HttpGet]
        [Authorize]
        [ActionName("GetExternalUser")]
        [Route("Account/GetExternalUser")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetExternalUser(string loginProvider, string providerKey)
        {
            try
            {
                var result = await _securityService.GetExternalUser(loginProvider, providerKey);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetUserDetail
        [HttpGet]
        [Authorize]
        [ActionName("GetUserDetail")]
        [Route("Account/GetUserDetail")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetUserDetail()
        {
            try
            {
                var userId = GetUserId();
                var result = await _securityService.GetUserDetail(userId);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/UpdateUserDetail
        [HttpPost]
        [Authorize]
        [ActionName("UpdateUserDetail")]
        [Route("Account/UpdateUserDetail")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> UpdateUserDetail(UserModel userInfo)
        {
            try
            {
                userInfo.Id = GetUserId();
                if (!ModelState.IsValid)
                {
                    var errorMessage = ModelState.Select(x => x.Value.Errors).FirstOrDefault()?.FirstOrDefault()?.ErrorMessage;
                    return new BadRequestObjectResult(BaseModel.Failed(message: errorMessage));
                }

                var result = await _securityService.UpdateUserDetail(userInfo);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/AddUserClaim
        [HttpPost]
        [Authorize]
        [ActionName("AddUserClaim")]
        [Route("Account/AddUserClaim")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<BaseModel> AddUserClaim(string claimType, string claimValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimType))
                    return BaseModel.Failed(message: "Invalid claim type");

                if (string.IsNullOrWhiteSpace(claimType))
                    return BaseModel.Failed(message: "Invalid claim value");

                var userId = GetUserId();
                var response = await _securityService.AddUserClaim(userId, claimType, claimValue);
                var baseModel = JsonSerializer.Deserialize<BaseModel>(response.Data);

                return baseModel;
            }
            catch (Exception ex)
            {
                return BaseModel.Failed(message: "An error occured while processing your request, please try again");
            }
        }

        //
        // GET: /Account/GetUserClaim
        [HttpGet]
        [Authorize]
        [ActionName("GetUserClaim")]
        [Route("Account/GetUserClaim")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> GetUserClaim()
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.GetUserClaim(userId);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An error occured while processing your request, please try again"));
            }
        }

        //
        // POST: /Account/RemoveUserClaim
        [HttpPost]
        [Authorize]
        [ActionName("RemoveUserClaim")]
        [Route("Account/RemoveUserClaim")]
        [Produces("application/json", Type = typeof(BaseModel))]
        private async Task<BaseModel> RemoveUserClaim(string claimType, string claimValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimType))
                    return BaseModel.Failed(message: "Invalid claim type");

                if (string.IsNullOrWhiteSpace(claimType))
                    return BaseModel.Failed(message: "Invalid claim value");

                var userId = GetUserId();
                var response = await _securityService.RemoveUserClaim(userId, claimType, claimValue);
                var baseModel = JsonSerializer.Deserialize<BaseModel>(response.Data);

                return baseModel;
            }
            catch (Exception ex)
            {
                return BaseModel.Failed(message: "An error occured while processing your request, please try again");
            }
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ActionName("ForgotPassword")]
        [Route("Account/ForgotPassword")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> ForgotPassword(ResetModel model)
        {
            try
            {
                var response = await _securityService.ForgotPassword(model.Email);
                if (!response.Success)
                    return new OkObjectResult(BaseModel.Failed(response.Message));

                //var baseModel = JsonSerializer.Deserialize<BaseModel>(response.Data);
                //if (baseModel.success)
                //{
                //    var info = JToken.Parse(response.Data);
                //    var userId = info["data"]["userId"].ToString();
                //    var resetCode = info["data"]["resetCode"].ToString();
                //}

                return new ObjectResult(BaseModel.Succeed(message: response.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request, please try again."));
            }
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ActionName("ResetPassword")]
        [Route("Account/ResetPassword")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> ResetPassword(PasswordResetModel model)
        {
            try
            {
                var result = await _securityService.ResetPassword(model.Email, model.Code, model.NewPassword);
                if (result.ResponseType == ResponseType.Success)
                    return new ObjectResult(BaseModel.Succeed(message: result.Data));

                return new ObjectResult(BaseModel.Failed(message: result.Data));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request, please try again."));
            }
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ActionName("ChangePassword")]
        [Route("Account/ChangePassword")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> ChangePassword(PasswordChangeModel model)
        {
            try
            {
                var userName = GetUserName();
                var response = await _securityService.ChangePassword(userName, model.OldPassword, model.NewPassword);
                if (response.ResponseType == ResponseType.Success)
                    return new ObjectResult(BaseModel.Succeed(message: response.Data));

                return new ObjectResult(BaseModel.Failed(response.Data));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request. Please try again later."));
            }
        }

        //
        // POST: /Account/SetPassword
        [HttpPost]
        [Authorize]
        [ActionName("SetPassword")]
        [Route("Account/SetPassword")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> SetPassword(SetPasswordModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.SetPassword(userId, model.NewPassword);
                if (!response.Success)
                    return new ObjectResult(BaseModel.Failed(response.Message));

                var updateUserInfoModel = new UserModel
                {
                    Id = userId,
                    FirstName = GetUserFistName(),
                    LastName = GetUserLastName()
                };
                var result = await _securityService.UpdateUserDetail(updateUserInfoModel);
                if (!result.Success)
                    return new ObjectResult(BaseModel.Failed(result.Message));

                return new ObjectResult(BaseModel.Succeed(message: response.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request. Please try again later."));
            }
        }

        ////
        //// POST: /Account/SendEmailCode
        //[HttpPost]
        //[Authorize]
        //[ActionName("SendEmailCode")]
        //[Route("Account/SendEmailCode")]
        //[ServiceFilter(typeof(ValidateModelState))]
        //[Produces("application/json", Type = typeof(BaseModel))]
        //public async Task<IActionResult> SendEmailCode(ChangeEmailModel model)
        //{
        //    try
        //    {
        //        var userId = GetUserId();
        //        var response = await _securityService.SendEmailCode(userId, model.Email);
        //        if (response.Success)
        //            return new ObjectResult(BaseModel.Succeed(message: response.Message));

        //        return new ObjectResult(BaseModel.Failed(response.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request. Please try again later."));
        //    }
        //}

        //
        // POST: /Account/ChangeEmail
        [HttpPost]
        [Authorize]
        [ActionName("ChangeEmail")]
        [Route("Account/ChangeEmail")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> ChangeEmail(ConfirmEmailModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.ChangeEmail(userId, model.Email, model.Code);
                if (response.Success)
                    return new ObjectResult(BaseModel.Succeed(message: response.Message));

                return new ObjectResult(BaseModel.Failed(response.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "An errro occured while processing your request. Please try again later."));
            }
        }

        //
        // POST: /Account/SendPhoneCode
        //[HttpPost]
        //[Authorize]
        //[ActionName("SendPhoneCode")]
        //[Route("Account/SendPhoneCode")]
        //[ServiceFilter(typeof(ValidateModelState))]
        //[Produces("application/json", Type = typeof(BaseModel))]
        //public async Task<IActionResult> SendPhoneCode(AddPhoneNumberModel model)
        //{
        //    try
        //    {
        //        var userId = GetUserId();
        //        var response = await _securityService.SendPhoneCode(userId, model.PhoneNumber);
        //        return new OkObjectResult(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
        //    }
        //}

        //
        // POST: /Account/VerifyPhoneNumber
        [HttpPost]
        [Authorize]
        [ActionName("VerifyPhoneNumber")]
        [Route("Account/VerifyPhoneNumber")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.ValidateChangePhoneNumberToken(userId, model.PhoneNumber, model.Code);
                if (response.Success)
                    return await ChangePhoneNumber(model);

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/ChangePhoneNumber
        [HttpPut]
        [Authorize]
        [ActionName("ChangePhoneNumber")]
        [Route("Account/ChangePhoneNumber")]
        [ServiceFilter(typeof(ValidateModelState))]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> ChangePhoneNumber(VerifyPhoneNumberModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.ChangePhoneNumber(userId, model.PhoneNumber, model.Code);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/RemovePhoneNumber
        [HttpDelete]
        [Authorize]
        [ActionName("RemovePhoneNumber")]
        [Route("Account/RemovePhoneNumber")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.RemovePhoneNumber(userId);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // GET: /Account/GetSharedKeyAndQrCodeUri
        [HttpGet]
        [Authorize]
        [ActionName("GetSharedKeyAndQrCodeUri")]
        [Route("Account/GetSharedKeyAndQrCodeUri")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> GetSharedKeyAndQrCodeUri()
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.GetSharedKeyAndQrCodeUri(userId);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/GenerateTwoFactorRecoveryCodes
        [HttpGet]
        [Authorize]
        [ActionName("GenerateTwoFactorRecoveryCodes")]
        [Route("Account/GenerateTwoFactorRecoveryCodes")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> GenerateTwoFactorRecoveryCodes(int numberOfCodes = 0)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.GenerateTwoFactorRecoveryCodes(userId, numberOfCodes);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/EnableTwoFactorAuthentication
        [HttpPost]
        [Authorize]
        [ActionName("EnableTwoFactorAuthentication")]
        [Route("Account/EnableTwoFactorAuthentication")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> EnableTwoFactorAuthentication(VerifyCodeModel model)
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.EnableTwoFactorAuthentication(userId, model.Provider, model.Code);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/ResetAuthenticator
        [HttpGet]
        [Authorize]
        [ActionName("ResetAuthenticator")]
        [Route("Account/ResetAuthenticator")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> ResetAuthenticator()
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.ResetAuthenticator(userId);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/DisableTwoFactorAuthentication
        [HttpGet]
        [Authorize]
        [ActionName("DisableTwoFactorAuthentication")]
        [Route("Account/DisableTwoFactorAuthentication")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            try
            {
                var userId = GetUserId();
                var response = await _securityService.DisableTwoFactorAuthentication(userId);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [ActionName("SendCode")]
        [Route("Account/SendCode")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> SendCode(SendCodeModel model)
        {
            try
            {
                var userName = GetUserName();
                var response = await _securityService.SendTwoFactorToken(userName, model.SelectedProvider);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [ActionName("VerifyCode")]
        [Route("Account/VerifyCode")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<ActionResult> VerifyCode(VerifyCodeModel model)
        {
            try
            {
                var response = await _securityService.VerifyTwoFactorToken(model.UserName, model.Provider, model.Code);
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        //
        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ActionName("Logout")]
        [Route("Account/Logout")]
        [Produces("application/json", Type = typeof(BaseModel))]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var response = await _securityService.Logout();
                if (response.Success)
                    return new ObjectResult(BaseModel.Succeed(message: response.Message));

                return new ObjectResult(BaseModel.Failed(response.Message));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(BaseModel.Failed(message: "There was an error processing your request, please try again."));
            }
        }

        #endregion



        #region private methods

        private IActionResult CreateResponse(Common.Models.AuthenticationResponse response, string ErrorMessage)
        {
            if (response.ResponseType == ResponseType.Error)
                return new BadRequestObjectResult(BaseModel.Failed(message: response.Data));

            if (response.ResponseType == ResponseType.Success)
            {
                var baseModel = JsonSerializer.Deserialize<BaseModel>(response.Data);
                return new OkObjectResult(baseModel);
            }
            return new BadRequestObjectResult(BaseModel.Failed(message: ErrorMessage));
        }

        #endregion
    }
}
