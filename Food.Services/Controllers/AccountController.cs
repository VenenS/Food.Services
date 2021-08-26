using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using ITWebNet.Food.Core;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Food.Services.Config;
using Food.Services.Attributes;
using Food.Services.Services;
using System.Web;
using Food.Services.Utils;
using System.Text;
using System.Net;
using Food.Services.Models.Account;
using System.Linq;
using Food.Services.Models;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Controllers
{
    [ValidateModel]
    [AllowAnonymous]
    [Route("api/Account")]
    public class AccountController : BaseController
    {
        private readonly IAuthenticationSchemeProvider _authProviders;
        private readonly IConfigureSettings _config;
        private readonly IEmailService _email;

        public AccountController(IFoodContext context, Accessor accessor, IAuthenticationSchemeProvider authProviders)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
            _authProviders = authProviders;
        }

        [ActivatorUtilitiesConstructor]
        public AccountController(IConfigureSettings config, IEmailService email, IAuthenticationSchemeProvider authProviders)
        {
            _config = config;
            _authProviders = authProviders;
            _email = email;
        }

        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //[AllowAnonymous]
        //[Route("ExternalLogin", Name = "ExternalLogin")]
        //public async Task<IHttpActionResult> GetExternalLogin(string provider, [AllowNull]string error = null)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return new ChallengeResult(provider, this);
        //    }

        //    var externalLoginInfo = await Authentication.GetExternalLoginInfoAsync();

        //    //ExternalLoginData externalLoginInfo = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

        //    if (externalLoginInfo == null)
        //    {
        //        return InternalServerError();
        //    }

        //    if (externalLoginInfo.Login.LoginProvider != provider)
        //    //if (externalLoginInfo.LoginProvider != provider)
        //    {
        //        Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        return new ChallengeResult(provider, this);
        //    }

        //    var user = Accessor.Instance.GetUserByExternalLoginInfo(
        //        externalLoginInfo.Login.LoginProvider,
        //        externalLoginInfo.Login.ProviderKey);

        //    var hasRegistered = user != null;
        //    ClaimsIdentity oAuthIdentity;
        //    string accessToken;
        //    string loginProvider;

        //    if (hasRegistered)
        //    {
        //        var token = user.GenerateAuthenticationToken(user);
        //        accessToken = token.AccessToken;
        //        loginProvider = ClaimsIdentity.DefaultIssuer;
        //    }
        //    else
        //    {
        //        oAuthIdentity = new ClaimsIdentity(externalLoginInfo.ExternalIdentity.Claims, OAuthDefaults.AuthenticationType);
        //        if (!oAuthIdentity.HasClaim(i => i.Type.Equals(IdentityProvider)))
        //            oAuthIdentity.AddClaim(
        //                new Claim(
        //                    IdentityProvider,
        //                    externalLoginInfo.Login.LoginProvider,
        //                    null,
        //                    externalLoginInfo.Login.LoginProvider,
        //                    externalLoginInfo.Login.LoginProvider));

        //        IDictionary<string, string> data = new Dictionary<string, string>();
        //        data.Add("userName", externalLoginInfo.DefaultUserName);

        //        var properties = new AuthenticationProperties(data);
        //        var issuedUtc = DateTime.UtcNow;
        //        var expiresUtc = issuedUtc.AddDays(Startup.OAuthOptions.AccessTokenExpireTimeSpan.TotalDays);
        //        properties.IssuedUtc = issuedUtc;
        //        properties.ExpiresUtc = expiresUtc;

        //        var ticket = new AuthenticationTicket(oAuthIdentity, properties);

        //        accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
        //        loginProvider = externalLoginInfo.Login.LoginProvider;
        //    }

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    var rawRedirectUri = GetQueryString(Request, "redirect_uri");
        //    var redirectUri = new Uri(rawRedirectUri);

        //    var returnUrl = redirectUri.GetLeftPart(UriPartial.Path);
        //    var queryParameters = redirectUri.ParseQueryString();
        //    queryParameters.Add("externalAccessToken", accessToken);
        //    queryParameters.Add("loginProvider", loginProvider);

        //    var redirectUrl = string.Format("{0}?{1}",
        //                                    returnUrl,
        //                                    queryParameters);

        //    return Redirect(redirectUrl);
        //}

        [HttpGet]
        [AllowAnonymous]
        [Route("ConfirmEmail")]
        public async Task<ConfirmEmailResultModel> ConfirmEmail(int userId, string code)
        {
            var user = Accessor.Instance.GetUserById(userId);
            if (user != null)
            {
                if (user.EmailConfirmationCode == code && !user.EmailConfirmed)
                {
                    if (Accessor.Instance.SetConfirmEmail(userId, true))
                    {
                        Accessor.Instance.DeleteConfirmationEmailCodeByUserId(userId);

                        return new ConfirmEmailResultModel { Type = "success", Result = "Регистрация успешно завершена" };
                    }
                }
            }

            return new ConfirmEmailResultModel { Type = "error", Result = "Код подтверждения не действителен" };
        }


        async Task<string> SendEmailConfirmation(long userId, string returnUrl)
        {
            var code = Guid.NewGuid().ToString();//var code = user.GenerateEmailConfirmationTokenAsync(userId);

            if (!Accessor.Instance.SetConfirmationEmailCode(userId, code))
                return "Не удалось создать код подтверждения E-mail";

            var user = Accessor.Instance.GetUserById(userId);

            // создаем ссылку для подтверждения
            var callbackUrl = new StringBuilder(returnUrl)
                .Append(returnUrl.IndexOf("?", StringComparison.Ordinal) > 0 ? "&" : "?")
                .Append($"userId={userId}&code={code}")
                .ToString();
            string text;
            if (user.LastUpdDate == null)
            {
                text = string.Format(
                    "Для завершения регистрации на сайте <a href=\"{0}\">Едовоз</a> перейдите по ссылке: <a href=\"{1}\">завершить регистрацию</a>",
                    returnUrl,
                    callbackUrl);
            }
            else
            {
                text = string.Format(
                    "Для подтверждения почты перейдите по ссылке: <a href=\"{0}\">подтвердить почту</a>",
                    callbackUrl);
            }

            // отправка письма
            await _email.SendAsync(text, "Запрос на подтверждение электронного адреса. \"Едовоз\"", user.Email);

            return text;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("SendEmailConfirmation")]
        public async Task<ActionResult> SendEmailConfirmation([FromBody] EmailConfirmationModel model)
        {
            var user = Accessor.Instance.GetUserByEmail(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist 
                return BadRequest("Пользователь с данным адресом не найден.");
            }

            await SendEmailConfirmation(user.Id, model.ReturnUrl);

            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("resetpassword")]
        public ActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                var user = Accessor.Instance.GetUserByEmail(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist 
                    return BadRequest("Пользователь с данным адресом не найден.");
                }

                var authorId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : user.Id;

                var passwordHash = PasswordHasher.HashPassword(model.Password);

                if (Accessor.Instance.ResetUserPassword(user.Id, model.Code, passwordHash, authorId))
                {
                    return Ok($"Новый пароль успешно установлен");
                }

                return BadRequest($"Не удалось сменить пароль для {user.Name}");
            }
            catch
            {
                return BadRequest($"Не удалось сменить пароль для {model.Email}");
            }
        }

        // Авторизация через соцсети. Удалить
        //[AllowAnonymous]
        //[HttpGet]
        //[Route("ExternalLogins", Name = "ExternalLogins")]
        //public async Task<IActionResult> GetExternalLogins(string returnUrl)
        //{
        //    var result = new List<ExternalLoginModel>();
        //    foreach (var provider in await _authProviders.GetAllSchemesAsync())
        //    {
        //        var virtualUrl = Url.Action("ExternalLogin", new
        //        {
        //            provider = provider.Name.ToLower(),
        //            redirectUrl = returnUrl,
        //        });
        //        var absoluteUrl = Request.Scheme + "://" + Request.Host + virtualUrl;

        //        var login = new ExternalLoginModel
        //        {
        //            Name = provider.Name,
        //            Url = absoluteUrl
        //        };
        //        result.Add(login);
        //    }
        //    return Ok(result);
        //}

        [HttpPost]
        [AllowAnonymous]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = Accessor.Instance.GetUserByEmail(model.Email);
            if (user == null)
            {
                return BadRequest("Пользователь с данным адресом не найден.");
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized();
            }

            var fc = Accessor.Instance;

            var key = Guid.NewGuid().ToString("N");

            var userResetKey = new UserPasswordResetKey
            {
                UserId = user.Id,
                Key = key,
                IssuedTo = DateTime.Now.AddMinutes(30)
            };

            if (!fc.AddUserResetPasswordKey(userResetKey)) 
                return BadRequest("Не удалось создать код сброса пароля");

            await user.SendAuthorizationLinkAsync(Url, model.ReturnUrl, userResetKey.Key, _config);

            return Ok(String.Format("На e-mail {0} выслана ссылка для сброса пароля.", model.Email));
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = Accessor.Instance.GetUserByEmail(model.Email) ??
                       Accessor.Instance.GetUserByLogin(model.Email);

            if (user == null || user.IsDeleted)
                return BadRequest("Неверный логин или пароль");

            if (PasswordHasher.VerifyHashedPassword(user.Password, model.Password))
            {
                if (user.EmailConfirmed == false)
                    return Unauthorized();
                var token = user.GenerateAuthenticationToken(_config.AuthToken);
                return Ok(token);
            }
            else
                return BadRequest("Неверный логин или пароль");
        }
        /// <summary>
        /// Метод сверяющий смс коды
        /// </summary>
        /// <param name="model">Возвращает статус операции</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("loginsms")]
        public async Task<IActionResult> LoginSms([FromBody]LoginSmsModel model)
        {
            model.Phone = model.Phone.Replace("+", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "")
                .Trim();

            var entitySmsCode = await Accessor.Instance.GetSmsCode(model.Phone, model.SmsCode);
            if (entitySmsCode == null || entitySmsCode.User == null 
                                      || entitySmsCode.User.IsDeleted
                                      || entitySmsCode.User.Lockout)
            {
                return Ok(new ResponseModel()
                {
                    Message = "Неверный код подтверждения", 
                    Status = 1
                });
            }

            if (!entitySmsCode.User.PhoneNumberConfirmed)
                return Ok(new ResponseModel()
                {
                    Message = "Телефонный номер не подтвержден",
                    Status = 2
                });

            if (entitySmsCode.IsActive == false)
                return Ok(new ResponseModel()
                {
                    Message = "Код не действительный", 
                    Status = 2
                });
            else
            {
                var token = entitySmsCode.User.GenerateAuthenticationToken(_config.AuthToken);
                return Ok(new ResponseModel()
                {
                    Result = token, Status = 0
                });
            }
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("loginanonymous")]
        public async Task<IActionResult> LoginAnonymous()
        {
            var user = Accessor.Instance.GetUserByLogin("anonymous");

            if (user == null)
                return NotFound();
            //var roles = Accessor.Instance.GetRolesByUserId(user.Id);
            var token = user.GenerateAuthenticationToken(_config.AuthToken);

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            var account = Accessor.Instance.GetUserByEmail(model.Email);
            if (account != null)
                return BadRequest(string.Format("Адрес электронной почты {0} уже используется", model.Email));

            model.PhoneNumber = model.PhoneNumber.Replace("+", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
            account = Accessor.Instance.GetUserByPhone(model.PhoneNumber);
            if (account != null)
                return BadRequest($"Телефон {model.PhoneNumber} уже используется");

            var user = new User
            {
                Name = model.Email,
                Email = model.Email,
                AccessFailedCount = 0,
                UserReferralLink = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                Password = PasswordHasher.HashPassword(model.Password),
                PhoneNumber = new string(model.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()),
                CreationDate = DateTime.Now
            };

            var resultId = Accessor.Instance.AddUser(user);

            if (resultId > 0)
            {
                if (!Accessor.Instance.AddUserToRole(resultId, EnumUserRole.User))
                {
                    return BadRequest("Не удалось добавить роль пользователю");
                }
            }
            else
            {
                return BadRequest("Не удалось создать пользователя");
            }

            var text = await SendEmailConfirmation(resultId, model.ReturnUrl);

            return Ok(text);

            //return Ok(string.Format("Пользователь {0} успешно создан", user.UserName));
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterByDevice")]
        public IActionResult RegisterByDevice([FromBody] RegisterDeviceModel model)
        {
            var user = new User
            {
                Name = model.DeviceUUID,
                DeviceUuid = model.DeviceUUID,
                UserReferralLink = Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                CreationDate = DateTime.Now
            };

            var resultId = Accessor.Instance.AddUser(user);

            if (resultId > 0)
            {
                if (!Accessor.Instance.AddUserToRole(resultId, EnumUserRole.User))
                {
                    return BadRequest("Не удалось добавить роль пользователю");
                }
            }
            else
            {
                return BadRequest("Не удалось создать пользователя");
            }

            return Ok(string.Format("Пользователь {0} успешно создан", user.Name));
        }

        [HttpGet]
        [Route("GetReferralLink")]
        public IActionResult GetReferralLink(string email)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var user = Accessor.Instance.GetUserByEmail(email);
                if (user == null) return BadRequest("Пользователь не найден");
                if (!string.IsNullOrEmpty(user.UserReferralLink)) return Ok(user.UserReferralLink);
                var link = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
                user.UserReferralLink = link;
                if (!Accessor.Instance.EditUser(user))
                {
                    return BadRequest("Не удалось изменить пользователя");
                }

                return Ok(user.UserReferralLink);
            }
            return BadRequest("Неверный запрос");
        }

        [HttpPost]
        [Route("validateresetpasswordkey/user/{userId:long}/key/{key}")]
        public IActionResult ValidateResetPasswordKey(long userId, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                key = Uri.UnescapeDataString(key);

                var resetPasswordModel = Accessor.Instance.ValidatePasswordResetKey(userId, key);

                return Ok(resetPasswordModel);
            }
            return BadRequest("Неверный запрос");
        }
    }
}
