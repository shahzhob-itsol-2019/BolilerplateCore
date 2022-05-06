using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using BoilerplateCore.Common.Helpers;
//using BoilerplateCore.Common.Helpers;
using BoilerplateCore.Common.Helpers.Interfaces;
using BoilerplateCore.Web.Models;
//using BoilerplateCore.Common.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Web.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : BaseController
    {
        private const string RecoveryCodesKey = nameof(RecoveryCodesKey);

        public AccountController()
        {
        }

        [TempData]
        public string ErrorMessage { get; set; }

        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrWhiteSpace(model.UserName))
                model.UserName = model.Email;
            if (ModelState.IsValid)
            {
                var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<RegisterUserViewModel>("Account/Register", model);
                if (result.Success)
                    return RedirectToAction(nameof(EmailConfirmationStatus), "Account", new { isConfirmed = false });

                ModelState.AddModelError("", result.Message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/RegisterMerchant
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterMerchant(RegisterMerchantViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (string.IsNullOrWhiteSpace(model.UserName))
                model.UserName = model.Email;
            if (ModelState.IsValid)
            {
                var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<RegisterMerchantViewModel>("Account/RegisterMerchant", model);
                if (result.Success)
                    return RedirectToAction(nameof(EmailConfirmationStatus), "Account", new { isConfirmed = false });

                ModelState.AddModelError("", result.Message);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return View("Error");

            BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(null);
            if (!string.IsNullOrWhiteSpace(GetUserId()))
            {
                var logoutResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ChangePasswordViewModel>("Account/Logout", null);
                if (!logoutResult.Success)
                    return View("Error");

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            var model = new ConfirmEmailViewModel
            {
                UserId = userId,
                Code = code
            };
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ConfirmEmailViewModel>("Account/Activation", model);
            return result.Success
                            ? RedirectToAction(nameof(EmailConfirmationStatus), new { isConfirmed = true })
                            : RedirectToAction("Error", "Account");
        }

        //
        // GET: /Account/EmailConfirmationStatus
        [HttpGet]
        [AllowAnonymous]
        public ActionResult EmailConfirmationStatus(bool isConfirmed)
        {
            ViewBag.IsConfirmed = isConfirmed;
            return View("ConfirmEmail");
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null, string errorMessage = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<List<ExternalAuthenticationProvider>>("Account/GetLoginProviders");
            var model = new LoginViewModel();
            if (result.Success)
                model.ExternalAuthenticationProviders = result.Data;
            else
                model.ExternalAuthenticationProviders = new List<ExternalAuthenticationProvider>();

            if(errorMessage != null)
                ModelState.AddModelError(string.Empty, errorMessage);

            return View(model);
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            TempData["UserName"] = model.Email;

            if (!ModelState.IsValid)
                return View(model);

            //// This doesn't count login failures towards account lockout
            //// To enable password failures to trigger account lockout, change to shouldLockout: true
            if (string.IsNullOrWhiteSpace(model.UserName))
                model.UserName = model.Email;

            var tokenResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.TokenAsync("Account/Login", model);
            if (tokenResponse.Status == LoginStatus.Succeded)
            {
                var accessToken = tokenResponse.Data.ToString();
                BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(accessToken);
                var userResponse = await IdentityLogin(model.Email, accessToken, model.RememberMe);
                if (userResponse.Item1)
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //return RedirectToLocal(returnUrl);

                //ModelState.AddModelError(string.Empty, userResponse.Item2);
                //return View(model);
                return RedirectToAction(nameof(Login), new { returnUrl, errorMessage = userResponse.Item2 });
            }
            else if (tokenResponse.Status == LoginStatus.RequiresTwoFactor)
            {
                if(tokenResponse.Data != null)
                {
                    var authenticationDetail = JsonSerializer.Deserialize<UserAuthenticationInfo>(tokenResponse.Data.ToString());
                    TempData["Provider"] = authenticationDetail.TwoFactorType;
                    return RedirectToAction(nameof(TwoFactorLogin), new { returnUrl, model.RememberMe });
                }
                //ModelState.AddModelError(string.Empty, tokenResponse.Message);
                //return View(model);
                return RedirectToAction(nameof(Login), new { returnUrl, errorMessage = tokenResponse.Message });
            }
            //else if (result.IsLockedOut)
            //{
            //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            //    return RedirectToAction(nameof(Lockout));
            //}
            //ModelState.AddModelError(string.Empty, tokenResponse.Message);
            //return View(model);
            return RedirectToAction(nameof(Login), new { returnUrl, errorMessage = tokenResponse.Message });
        }

        //
        // GET: /Account/TwoFactorLogin
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactorLogin(bool rememberMe, string returnUrl = null)
        {
            var userName = TempData["UserName"].ToString();
            var provider = TempData["Provider"].ToString();

            var model = new VerifyCodeViewModel { UserName = userName, Provider = provider, RememberMe = rememberMe };

            TempData["UserName"] = userName;
            TempData["Provider"] = provider;
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        //
        // POST: /Account/TwoFactorLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactorLogin(VerifyCodeViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var authenticatorCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var tokenResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.TokenAsync("Account/TwoFactorLogin", model);
            if (tokenResponse.Status == LoginStatus.Succeded)
            {
                var accessToken = tokenResponse.Data.ToString();
                BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(accessToken);
                var userResponse = await IdentityLogin(model.UserName, accessToken, model.RememberMe);
                if (userResponse.Item1)
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //return RedirectToLocal(returnUrl);

                ModelState.AddModelError(string.Empty, userResponse.Item2);
                return View(model);
            }
            //else if (result.IsLockedOut)
            //{
            //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            //    return RedirectToAction(nameof(Lockout));
            //}
            else
            {
                ModelState.AddModelError(string.Empty, tokenResponse.Message);
                return View(model);
            }
        }

        //
        // GET: /Account/RecoveryCodeLogin
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RecoveryCodeLogin(string returnUrl = null)
        {
            string userName = string.Empty;
            if(TempData["UserName"] != null)
                userName = TempData["UserName"].ToString();

            var model = new VerifyCodeViewModel { UserName = userName };
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/RecoveryCodeLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoveryCodeLogin(VerifyCodeViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["UserName"] = model.UserName;
                return View(model);
            }

            if(string.IsNullOrWhiteSpace(model.UserName))
            {
                TempData["UserName"] = model.UserName;
                ModelState.AddModelError(string.Empty, "Error accored.");
                return View(model);
            }

            var recoveryCode = model.Code.Replace(" ", string.Empty);

            var tokenResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.TokenAsync("Account/RecoveryCodeLogin", model);
            if (tokenResponse.Status == LoginStatus.Succeded)
            {
                var accessToken = tokenResponse.Data.ToString();
                BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(accessToken);
                var userResponse = await IdentityLogin(model.UserName, accessToken, model.RememberMe);
                if (userResponse.Item1)
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //return RedirectToLocal(returnUrl);

                ModelState.AddModelError(string.Empty, userResponse.Item2);
                return View(model);
            }
            //else if (result.IsLockedOut)
            //{
            //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            //    return RedirectToAction(nameof(Lockout));
            //}
            else
            {
                ModelState.AddModelError(string.Empty, tokenResponse.Message);
                return View(model);
            }
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var loginProperties = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<AuthenticationProperties>("Account/GetLoginProperties?provider=" + provider + "&redirectUrl=" + redirectUrl);
            if (loginProperties.Success)
                return new ChallengeResult(provider, loginProperties.Data);
            else
                return RedirectToAction(nameof(Login));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            var loginInfo = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (loginInfo == null)
                return RedirectToAction(nameof(ExternalLoginFailure));

            var loginProvder = loginInfo.Properties.Items.FirstOrDefault(x => x.Key == "LoginProvider").Value;
            var providerKey = loginInfo.Principal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            var firstName = loginInfo.Principal.Claims.Where(c => c.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault();
            var lastName = loginInfo.Principal.Claims.Where(c => c.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
            var emailAddress = loginInfo.Principal.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();

            var model = new ExternalLoginViewModel
            {
                LoginProvider = loginProvder,
                ProviderKey = providerKey
            };

            var tokenResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.TokenAsync("Account/ExternalLogin", model);
            if (tokenResponse.Status == LoginStatus.Succeded)
            {
                var accessToken = tokenResponse.Data.ToString();
                BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(accessToken);
                var userResponse = await IdentityExternalLogin(loginInfo.Principal.Claims, accessToken, loginProvder, providerKey);
                if (userResponse.Item1)
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //return RedirectToLocal(returnUrl);

                return RedirectToAction(nameof(ExternalLoginFailure));
            }
            else if (tokenResponse.Status == LoginStatus.RequiresTwoFactor)
            {
                if (tokenResponse.Data != null)
                {
                    var authenticationDetail = JsonSerializer.Deserialize<UserAuthenticationInfo>(tokenResponse.Data.ToString());
                    TempData["UserName"] = emailAddress;
                    TempData["Provider"] = authenticationDetail.TwoFactorType;
                    return RedirectToAction(nameof(TwoFactorLogin), new { returnUrl, rememberMe = false});
                }
                //ModelState.AddModelError(string.Empty, tokenResponse.Message);
                //return View(model);
                return RedirectToAction(nameof(Login), new { returnUrl, errorMessage = tokenResponse.Message });
            }
            //else if (result.IsLockedOut)
            //{
            //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            //    return RedirectToAction(nameof(Lockout));
            //}
            else
            {
                ViewData["ReturnUrl"] = returnUrl;

                var registerExternalModel = new RegisterExternalViewModel
                {
                    Email = emailAddress,
                    FirstName = firstName,
                    LastName = lastName,
                    Provider = loginProvder,
                    ProviderKey = providerKey,
                    ProviderDisplayName = loginProvder
                };
                return View("ExternalLogin", registerExternalModel);
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(RegisterExternalViewModel model, string returnUrl = null)
        {
            var loginInfo = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (loginInfo == null)
                return RedirectToAction(nameof(ExternalLoginFailure));

            var tokenResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.TokenAsync("Account/RegisterExternal", model);
            if (tokenResponse.Status == LoginStatus.Succeded)
            {
                var accessToken = tokenResponse.Data.ToString();
                BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(accessToken);
                var userResponse = await IdentityExternalLogin(loginInfo.Principal.Claims, accessToken, model.Provider, model.ProviderKey);
                if (userResponse.Item1)
                    return RedirectToAction(nameof(HomeController.Index), "Home"); //return RedirectToLocal(returnUrl);

                return RedirectToAction(nameof(ExternalLoginFailure));
            }
            //else if (result.IsLockedOut)
            //{
            //    _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            //    return RedirectToAction(nameof(Lockout));
            //}
            else
            {
                return RedirectToAction(nameof(ExternalLoginFailure));
            }
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        private async Task<Tuple<bool, string>> IdentityLogin(string userName, string accessToken, bool rememberBrowser = false, bool rememberMachine = false)
        {
            var userResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserClaim>("Account/GetUser");
            if (userResponse.Success)
            {
                var claim = userResponse.Data;
                List<Claim> claims = new List<Claim>();
                claims.AddRange(new List<Claim>
                {
                    new Claim(ClaimTypes.PrimarySid, accessToken),
                    new Claim(ClaimTypes.Sid, claim.Id),
                    new Claim(ClaimTypes.Name, claim.UserName),
                    new Claim(ClaimTypes.Email, claim.Email),
                    new Claim(CustomClaimTypes.User.ToString(), JsonSerializer.Serialize(claim))
                });

                if (!string.IsNullOrWhiteSpace(claim.FirstName))
                    claims.Add(new Claim(CustomClaimTypes.FirstName.ToString(), claim.FirstName));
                if (!string.IsNullOrWhiteSpace(claim.LastName))
                    claims.Add(new Claim(CustomClaimTypes.LastName.ToString(), claim.LastName));

                foreach (var role in claim.Roles)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                var claimPrincipal = new ClaimsPrincipal(claimsIdentity);
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                    IsPersistent = rememberBrowser,
                };

                Thread.CurrentPrincipal = claimPrincipal;
                //await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(_httpContext, claimPrincipal);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimPrincipal,
                    properties
                );
                return new Tuple<bool, string>(true, "");
            }

            return new Tuple<bool, string>(false, userResponse.Message);
        }

        private async Task<Tuple<bool, string>> IdentityExternalLogin(IEnumerable<Claim> externalClaims, string accessToken, string loginProvider, string providerKey, bool rememberBrowser = false, bool rememberMachine = false)
        {
            var userResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserClaim>("Account/GetExternalUser?loginProvider=" + loginProvider + "&providerKey=" + providerKey);
            if (userResponse.Success)
            {
                var claim = userResponse.Data;
                var name = externalClaims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();
                var givenName = externalClaims.Where(c => c.Type == ClaimTypes.GivenName).Select(c => c.Value).SingleOrDefault();
                if (string.IsNullOrWhiteSpace(givenName))
                    givenName = name.Split(' ').FirstOrDefault();
                var surName = externalClaims.Where(c => c.Type == ClaimTypes.Surname).Select(c => c.Value).SingleOrDefault();
                if (string.IsNullOrWhiteSpace(surName))
                    surName = name.Split(' ').LastOrDefault();
                var emailAddress = externalClaims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).SingleOrDefault();
                List<Claim> claims = new List<Claim>();
                claims.AddRange(new List<Claim>
                        {
                            new Claim(ClaimTypes.PrimarySid, accessToken),
                            //new Claim(ClaimTypes.Sid, claim.Id),
                            //new Claim(ClaimTypes.Name, claim.UserName),
                            new Claim(ClaimTypes.Email, emailAddress),
                            new Claim(CustomClaimTypes.FirstName.ToString(), givenName),
                            new Claim(CustomClaimTypes.LastName.ToString(), surName),
                            new Claim(CustomClaimTypes.User.ToString(), JsonSerializer.Serialize(claim))
                        });

                //foreach (var role in claim.Roles)
                //    claims.Add(new Claim(ClaimTypes.Role, role));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                var claimPrincipal = new ClaimsPrincipal(claimsIdentity);
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                    IsPersistent = rememberBrowser,
                };

                Thread.CurrentPrincipal = claimPrincipal;
                //await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignInAsync(_httpContext, claimPrincipal);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimPrincipal,
                    properties
                );
                return new Tuple<bool, string>(true, "");
            }

            return new Tuple<bool, string>(false, userResponse.Message);
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ForgotPasswordViewModel>("Account/ForgotPassword", model);
                if (result.Success)
                {
                    ViewBag.Message = result.Message;
                    return View("ForgotPasswordConfirmation");
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ResetPasswordViewModel>("Account/ResetPassword", model);
                if (result.Success)
                {
                    return RedirectToAction(nameof(ResetPasswordConfirmation));
                }
                ModelState.AddModelError(string.Empty, result.Message);
            }
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/UserProfile
        public async Task<IActionResult> UserProfile(string message = null)
        {
            ViewBag.StatusMessage = message;

            var response = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserInfo>("Account/GetUserDetail");
            if (response.Success)
                return View(response.Data);

            return View("Error");
        }

        //
        // GET: /Account/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ChangePasswordViewModel>("Account/ChangePassword", model);
            if (result.Success)
                return RedirectToAction(nameof(UserProfile), new { message = "Your password has been changed." });

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        //
        // GET: /Account/SetPassword
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Account/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<SetPasswordViewModel>("Account/SetPassword", model);
            if (result.Success)
                return RedirectToAction(nameof(UserProfile), new { message = result.Message });

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        //
        // POST: /Account/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_ChangeEmail", model);

            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ChangeEmailViewModel>("Account/SendEmailCode", model);
            if (result.Success)
                return RedirectToAction(nameof(UserProfile), new { message = result.Message });

            ModelState.AddModelError(string.Empty, result.Message);
            return PartialView("_ChangeEmail", model);
        }

        //
        // GET: /Account/ChangeEmail
        [AllowAnonymous]
        public async Task<IActionResult> ChangeEmail(string userId, string email, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(email))
                return View("Error");

            var model = new ConfirmEmailViewModel
            {
                UserId = userId,
                Email = email,
                Code = code
            };
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ConfirmEmailViewModel>("Account/ChangeEmail", model);
            return result.Success
                            ? RedirectToAction(nameof(UserProfile), new { message = result.Message })
                            : RedirectToAction(nameof(UserProfile));
        }

        //
        // GET: /Account/AddPhoneNumber
        public IActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Account/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddPhoneNumber", model);
            }
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<AddPhoneNumberViewModel>("Account/SendPhoneCode", model);
            if (result.Success)
                return RedirectToAction(nameof(VerifyPhoneNumber), new { PhoneNumber = model.PhoneNumber });

            ModelState.AddModelError(string.Empty, result.Message);
            return PartialView("_AddPhoneNumber", model);
        }

        //
        // GET: /Account/VerifyPhoneNumber
        public async Task<IActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            //return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
            return PartialView("_VerifyPhoneNumber", new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Account/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_VerifyPhoneNumber", model);

            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PutAsync<VerifyPhoneNumberViewModel>("Account/ChangePhoneNumber", model);
            if (result.Success)
                return RedirectToAction(nameof(UserProfile), new { message = "Your phone number is been added successfully." });

            ModelState.AddModelError(string.Empty, result.Message);
            return PartialView("_VerifyPhoneNumber", model);
        }

        //
        // POST: /Account/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePhoneNumber()
        {
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.DeleteAsync<string>("Account/RemovePhoneNumber");
            if (result.Success)
                return RedirectToAction(nameof(UserProfile), new { message = "Your phone number is been deleted successfully." });

            ModelState.AddModelError(string.Empty, result.Message);
            return RedirectToAction(nameof(UserProfile), new { message = result.Message });
        }

        //
        // GET: /Account/TwoFactorAuthentication
        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication(string message = null)
        {
            ViewBag.StatusMessage = message;

            var authenticationResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserAuthenticationInfo>("Account/GetAuthenticationDetail");
            if (authenticationResponse.Success)
                return View(authenticationResponse.Data);

            ModelState.AddModelError(string.Empty, authenticationResponse.Message);
            return View(authenticationResponse.Data);
        }

        //
        // GET: /Account/EnableAuthenticator
        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var model = new AuthenticatorViewModel ();
            var response = await LoadSharedKeyAndQrCodeUri(model);
            if (response.Item1)
                return View(model);

            ModelState.AddModelError(string.Empty, response.Item2);
            return View(model);
        }

        private async Task<Tuple<bool, string>> LoadSharedKeyAndQrCodeUri(AuthenticatorViewModel model)
        {
            var response = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<AuthenticatorViewModel>("Account/GetSharedKeyAndQrCodeUri");
            if (!response.Success)
                return new Tuple<bool, string>(response.Success, response.Message);

            model.AuthenticatorUri = response.Data.AuthenticatorUri;
            model.SharedKey = response.Data.SharedKey;
            return new Tuple<bool, string>(response.Success, response.Message);
        }

        //
        // POST: /Account/EnableAuthenticator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(AuthenticatorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var response = await LoadSharedKeyAndQrCodeUri(model);
                return View(model);
            }

            // Strip spaces and hypens
            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var verifyCodeModel = new VerifyCodeViewModel
            {
                Provider = Common.Utility.Constants.TwoFactorTypes.Authenticator,
                Code = model.Code
            };
            var authenticatorResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<IEnumerable<string>>("Account/EnableTwoFactorAuthentication", verifyCodeModel);
            if (!authenticatorResult.Success)
            {
                ModelState.AddModelError("Code", authenticatorResult.Message);
                var response = await LoadSharedKeyAndQrCodeUri(model);
                return View(model);
            }

            var recoveryCodes = authenticatorResult.Data;
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();

            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        //
        // GET: /Account/ResetAuthenticatorWarning
        [HttpGet]
        public IActionResult ResetAuthenticatorWarning()
        {
            return View(nameof(ResetAuthenticator));
        }

        //
        // POST: /Account/ResetAuthenticator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator()
        {
            var resetAuthenticatorResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<string>("Account/ResetAuthenticator");

            return RedirectToAction(nameof(EnableAuthenticator), new { message = resetAuthenticatorResult.Message });
        }

        //
        // GET: /Account/EnableEmailOtp
        [HttpGet]
        public async Task<ActionResult> EnableEmailOtp()
        {
            var model = new SendCodeViewModel
            {
                SelectedProvider = Common.Utility.Constants.TwoFactorTypes.Email
            };

            var otpResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<string>("Account/SendCode", model);
            if (!otpResult.Success)
                ModelState.AddModelError("Code", otpResult.Message);

            return View(nameof(EnableOtp), new VerifyCodeViewModel { Provider = model.SelectedProvider });
        }

        //
        // POST: /Account/EnableOtp
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableOtp(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var otpResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<IEnumerable<string>>("Account/EnableTwoFactorAuthentication", model);
            if (!otpResult.Success)
            {
                ModelState.AddModelError("Code", otpResult.Message);
                return View(model);
            }

            var recoveryCodes = otpResult.Data;
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();

            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        //
        // GET: /Account/DisableTwoFactorAuthenticationWarning
        [HttpGet]
        public async Task<IActionResult> DisableTwoFactorAuthenticationWarning()
        {
            return View(nameof(DisableTwoFactorAuthentication));
        }

        //
        // POST: /Account/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactorAuthentication()
        {
            var disable2faResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<string>("Account/DisableTwoFactorAuthentication");
            ///if (!disable2faResult.Success)

            return RedirectToAction(nameof(TwoFactorAuthentication), new { message = disable2faResult.Message });
        }

        //
        // GET: /Account/GenerateRecoveryCodesWarning
        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodesWarning()
        {
            return View(nameof(GenerateRecoveryCodes));
        }

        //
        // POST: /Account/GenerateRecoveryCodes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            var recoveryCodesResult = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<IEnumerable<string>>("Account/GenerateTwoFactorRecoveryCodes");
            if (!recoveryCodesResult.Success)
            {
                ModelState.AddModelError("", recoveryCodesResult.Message);
                return View(nameof(ShowRecoveryCodes), new RecoveryCodesViewModel());
            }

            var recoveryCodes = recoveryCodesResult.Data;
            TempData[RecoveryCodesKey] = recoveryCodes.ToArray();
            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        //
        // GET: /Account/ShowRecoveryCodes
        [HttpGet]
        public IActionResult ShowRecoveryCodes()
        {
            var recoveryCodes = (string[])TempData[RecoveryCodesKey];
            if (recoveryCodes == null)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            var model = new RecoveryCodesViewModel { RecoveryCodes = recoveryCodes };
            return View(model);
        }


        ////
        //// GET: /Account/SendCode
        ////[AllowAnonymous]
        ////public async Task<IActionResult> SendCode(string returnUrl, bool rememberMe)
        ////{
        ////    //var userId = await SignInManager.GetVerifiedUserIdAsync();
        ////    //if (userId == null)
        ////    //{
        ////    //    return View("Error");
        ////    //}
        ////    //var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        ////    //var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        ////    //return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        ////    return null;
        ////}

        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SendCode(SendCodeViewModel model)
        //{
        //    var otpResult = await HttpClientHelper.PostAsync<string>("Account/SendCode", model);
        //    if (!otpResult.Success)
        //    {
        //        ModelState.AddModelError("Code", otpResult.Message);
        //        return View();
        //    }
        //    return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        ////
        //// GET: /Account/VerifyCode
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    model.UserId = GetUserId();
        //    var otpVerificationResult = await HttpClientHelper.PostAsync<string>("Account/VerifyCode", model);
        //    if (!otpVerificationResult.Success)
        //    {
        //        ModelState.AddModelError("Code", otpVerificationResult.Message);
        //        return View();
        //    }
        //    return RedirectToAction(nameof(EnableOtp), new { OtpOption = "test" });
        //}

        //
        // GET: /Account/ManageLogins
        public async Task<IActionResult> ManageLogins(string message = null)
        {
            ViewBag.StatusMessage = message;

            var authenticationResponse = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<UserAuthenticationInfo>("Account/GetAuthenticationDetail");
            if (authenticationResponse.Success)
                return View("ExternalLogins", new ExternalLoginsViewModel
                {
                    CurrentLogins = authenticationResponse.Data.Logins,
                    OtherLogins = authenticationResponse.Data.OtherLogins,
                    ShowRemoveButton = authenticationResponse.Data.HasPassword || authenticationResponse.Data.Logins.Count > 1
                });

            ModelState.AddModelError(string.Empty, authenticationResponse.Message);
            return View("ExternalLogins", new ExternalLoginsViewModel());
        }

        //
        // POST: /Account/AddLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(AddLoginCallback), "Account");
            var userId = GetUserId();
            var loginProperties = await BoilerplateCore.Web.Helpers.HttpClientHelper.GetAsync<AuthenticationProperties>("Account/GetLoginProperties?provider=" + provider + "&redirectUrl=" + redirectUrl + "&userId=" + userId);
            if (loginProperties.Success)
                return new ChallengeResult(provider, loginProperties.Data);
            else
                return RedirectToAction(nameof(ManageLogins), new { message = loginProperties.Message });
        }
    
        //
        // GET: /Account/AddLoginCallback
        public async Task<IActionResult> AddLoginCallback()
        {
            var login = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            var loginProvder = login.Properties.Items.FirstOrDefault(x => x.Key == "LoginProvider").Value;
            var providerKey = login.Principal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
            
            var model = new AddLoginViewModel
            {
                Provider = loginProvder,
                ProviderKey = providerKey,
                ProviderDisplayName = loginProvder
            };

            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<string>("Account/AddLogin", model);

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return RedirectToAction(nameof(ManageLogins), new { message = result.Message });
        }

        //
        // POST: /Account/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(string provider, string providerKey)
        {
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<string>("Account/RemoveLogin", new RemoveLoginViewModel { Provider = provider, ProviderKey = providerKey });
            return RedirectToAction(nameof(ManageLogins), new { message = result.Message });
        }

        //
        // GET: /Account/Lockout
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        //
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()   
        {
            var result = await BoilerplateCore.Web.Helpers.HttpClientHelper.PostAsync<ChangePasswordViewModel>("Account/Logout", null);
            if (!result.Success)
                return View("Error");

            BoilerplateCore.Web.Helpers.HttpClientHelper.SetBearerToken(null);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //
        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        

        #region Helpers

        //private void AddErrors(IdentityResult result)
        //{
        //    foreach (var error in result.Errors)
        //    {
        //        ModelState.AddModelError(string.Empty, error.Description);
        //    }
        //}

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion Helpers
    }
}
