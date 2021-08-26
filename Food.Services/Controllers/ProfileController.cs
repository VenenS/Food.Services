using Food.Data.Entities;
using Food.Data;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Food.Services.Extensions;
using System.Linq;
using Food.Services.Config;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Food.Services.Services;

namespace Food.Services.Controllers
{
    [Authorize]
    [Route("api/Profile")]
    public class ProfileController : BaseController
    {
        protected const string LocalLoginProvider = "Local";
        private readonly IConfigureSettings _config;
        private readonly IEmailService _email;

        public ProfileController(IFoodContext context, Accessor accessor)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public ProfileController(IConfigureSettings config, IEmailService emailService)
        {
            _config = config;
            _email = emailService;
        }

        //[Route("AddExternalLogin")]
        //public async Task<IActionResult> AddExternalLogin([FromBody]AddExternalLoginModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    await HttpContext.SignOutAsync();

        //    var ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || (ticket.Properties != null
        //                                                      && ticket.Properties.ExpiresUtc.HasValue
        //                                                      && ticket.Properties.ExpiresUtc.Value <
        //                                                      DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    var externalData = ExternalLoginData.FromIdentity(ticket.Principal.Identity as ClaimsIdentity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    if (!Accessor.Instance.AddExternalLogin(
        //        new UserExternalLogin()
        //        {
        //            LoginProvider = externalData.LoginProvider,
        //            ProviderKey = externalData.ProviderKey,
        //            UserId = User.Identity.GetUserId()
        //        }))
        //    {
        //        return BadRequest("Не удалось добавить внешний логин для пользователя");
        //    }

        //    return Ok();
        //}

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Authorize(string email, string code, string returnUrl)
        {
            var user = Accessor.Instance.GetUserByEmail(email);

            var decodedToken = HttpUtility.UrlDecode(code);


            if (user != null && ITWebNet.Food.Core.PasswordHasher.VerifyHashedPassword(decodedToken, user.Id.ToString())) //if (user != null && await UserManager.VerifyAuthorizeTokenAsync(user.Id, decodedToken))
            {
                //await UserManager.RemovePasswordAsync(user.Id);
                var token = user.GenerateAuthenticationToken(_config.AuthToken);

                var redirectUrl = string.Format("{0}?token={1}", returnUrl, token.AccessToken);

                return Redirect(redirectUrl);
            }

            return Redirect(returnUrl);
        }

        [HttpPost]
        [Route("changepassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Accessor = GetAccessor();
            using (var fc = Accessor.GetContext())
            {
                var userId = User.Identity.GetUserId();
                var user = fc.Users.FirstOrDefault(o => !o.IsDeleted && o.Id == userId);
                if (user == null)
                    return BadRequest("Пользователь не найден");

                if (!ITWebNet.Food.Core.PasswordHasher.VerifyHashedPassword(user.Password, model.OldPassword))
                    return BadRequest("Неверный текущий пароль");

                user.Password = ITWebNet.Food.Core.PasswordHasher.HashPassword(model.NewPassword);
                fc.SaveChanges();
            }
            return Ok("Пароль успешно изменен.");
        }

        [Route("UserInfo")]
        [HttpGet]
        public IActionResult GetUserInfo()
        {
            var userId = User.Identity.GetUserId();

            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var user = context.Users.Include(u => u.UserInCompanies)
                .Include(u => u.Address)
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == userId);

            if (user == null)
                return BadRequest("Очень странно, но вы не найдены у нас в базе данных");

            var model = new UserInfoModel
            {
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactor,
                UserFullName = user.FullName,
                HasPassword = !string.IsNullOrWhiteSpace(user.Password),
                DefaultAddressId = user.DefaultAddressId,
                DefaultAddress = user.DefaultAddressId.HasValue
                    ? user.Address.RawAddress
                    : string.Empty,
                City = user.Address.CityId,
                Street = user.Address.StreetName,
                House = user.Address.HouseNumber,
                Building = user.Address.BuildingNumber,
                Flat = user.Address.FlatNumber,
                Office = user.Address.OfficeNumber,
                Entrance = user.Address.EntranceNumber,
                Storey = user.Address.StoreyNumber,
                Intercom = user.Address.IntercomNumber,
                AddressComment = user.Address.AddressComment,
                PersonalPoints = user.PersonalPoints,
                ReferralPoints = user.ReferralPoints,
                PercentOfOrder = user.PercentOfOrder,
                UserReferralLink = user.UserReferralLink,
                UserInCompanies = user.UserInCompanies.Where(c => c.IsActive && !c.IsDeleted).Select(c =>
                    new UserInCompanyModel()
                    {
                        Id = c.Id,
                        CompanyId = c.CompanyId,
                        UserId = c.UserId,
                        DeliveryAddressId = c.DefaultAddressId
                    }).ToList(),
                IsSendCode = context.SmsCodes.Any(c => c.IsActive == true && !c.IsDeleted && c.ValidTime >= DateTime.Now && c.UserId == user.Id),
                IsSendEmailConfirmationCode = !string.IsNullOrWhiteSpace(user.EmailConfirmationCode)
            };

            return Ok(model);
        }

        [Route("ResetPasswordUserInfo")]
        [HttpGet]
        public IActionResult ResetPasswordUserInfo(string key)
        {
            var userId = int.Parse(HttpUtility.UrlDecode(key));

            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var user = context.Users.Include(u => u.UserInCompanies)
                .Include(u => u.Address)
                .FirstOrDefault(c => c.Id == userId);

            if (user == null)
                return BadRequest("Очень странно, но вы не найдены у нас в базе данных");

            var model = new UserInfoModel
            {
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactor,
                UserFullName = user.FullName,
                HasPassword = !string.IsNullOrWhiteSpace(user.Password),
                DefaultAddressId = user.DefaultAddressId,
                DefaultAddress = user.DefaultAddressId.HasValue
                    ? user.Address.RawAddress
                    : string.Empty,
                PersonalPoints = user.PersonalPoints,
                ReferralPoints = user.ReferralPoints,
                PercentOfOrder = user.PercentOfOrder,
                UserReferralLink = user.UserReferralLink,
                UserInCompanies = user.UserInCompanies.Where(c => c.IsActive && !c.IsDeleted).Select(c =>
                    new UserInCompanyModel()
                    {
                        Id = c.Id,
                        CompanyId = c.CompanyId,
                        UserId = c.UserId,
                        DeliveryAddressId = c.DefaultAddressId
                    }).ToList()
            };

            return Ok(model);
        }

        [HttpGet, Route("companies")]
        public IActionResult GetMyCompanies()
        {
            var userId = User.Identity.GetUserId();
            var currentUser = Accessor.Instance.GetUserById(userId);

            if (currentUser == null)
                return BadRequest("Пользователь не найден");

            Accessor = GetAccessor();
            var context = Accessor.GetContext();

            var query =
                context
                    .UsersInCompanies
                    .Include("Company")
                    .Where(c => c.IsActive
                                && c.IsDeleted == false && c.Company.IsActive && c.UserId == userId)
                    .Select(c => c.Company).Include("Addresses").ToList();

            var companyList = query.Select(companyItem => companyItem.GetContract()).ToList();

            return Ok(companyList);
        }

        [HttpGet, Route("companies/{id:long}")]
        public IActionResult GetMyCompanyById(long id)
        {
            var currentUser = User.Identity.GetUserId();
            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var query =
                context
                    .UsersInCompanies
                    .Include(c => c.Company)
                    .Where(c => c.IsActive
                                && c.IsDeleted == false && c.Company.IsActive && c.CompanyId == id &&
                                c.UserId == currentUser).Include(c => c.Company.Addresses).FirstOrDefault();

            if (query != null)
                return Ok(new UserInCompanyModel()
                {
                    Company = query.Company?.GetContract(),
                    DeliveryAddressId = query.Company.MainDeliveryAddressId != null ? query.Company.MainDeliveryAddressId : query.DefaultAddressId,
                    CompanyId = query.CompanyId
                });

            return NotFound();
        }

        [HttpGet, Route("bankets/{cafeId:long}")]
        public IActionResult GetAvailabelBankets(long? cafeId = null)
        {
            var currentUser = User.Identity.GetUserId();
            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var today = DateTime.Now.Date;
            var query = context.Bankets.Include(c => c.Menu).Include(c => c.Company).Join(context.UsersInCompanies,
                    b => b.CompanyId,
                    c => c.CompanyId, (b, c) => new
                    {
                        Banket = b,
                        UserInCompany = c
                    }).Where(f =>
                    f.UserInCompany.IsDeleted == false && !f.Banket.IsDeleted &&
                    f.Banket.OrderStartDate <= today &&
                    today <= f.Banket.OrderEndDate && f.UserInCompany.IsActive &&
                    f.Banket.Company.IsActive &&
                    f.UserInCompany.UserId == currentUser).OrderBy(c => c.Banket.EventDate).Select(c => c.Banket);

            if (cafeId != null)
                query = query.Where(x => x.CafeId == cafeId);

            var list = query.ToList();
            return Ok(list.Select(c => c.GetContract()));
        }

        [Route("RemoveLogin")]
        public IActionResult RemoveLogin([FromBody] RemoveLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity.GetUserId();
            Accessor = GetAccessor();
            var fc = Accessor.GetContext();
            var user = fc.Users.FirstOrDefault(o => o.Id == userId);
            if (user != null)
                return BadRequest("Пользователь не найден");


            if (model.LoginProvider == LocalLoginProvider)
            {
                //var result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId<int>());
            }
            else
            {
                //var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(),
                //    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
            }

            return Ok();
        }

        [Route("UserInfo"), HttpPost]
        public async Task<IActionResult> SaveUserInfo([FromBody] UserInfoModel model)
        {
            model.PhoneNumber = model.PhoneNumber.Replace("+", "").Replace("(", "").Replace(")", "").Replace(" ", "").Trim();
            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var userId = User.Identity.GetUserId();
            var user = context.Users.Include(c => c.UserInCompanies)
                .FirstOrDefault(c => c.Id == userId && !c.IsDeleted);

            if (user == null)
                return BadRequest("Очень странно, но вы не найдены у нас в базе данных");

            if (context.Users.FirstOrDefault(
                    c => c.PhoneNumber == new string(model.PhoneNumber.Where(u => char.IsDigit(u)).ToArray())
                    && c.Id != userId
                    && !c.IsDeleted
                    && !c.Lockout) != null)
            {
                return BadRequest("PhoneNumber&Телефон уже используется");
            }

            if (!string.Equals(user.Email.Trim(), model.Email.Trim(), StringComparison.CurrentCultureIgnoreCase)
                && context.Users.FirstOrDefault(c => c.Email == model.Email.ToLower().Trim() && !c.IsDeleted) != null)
            {
                return BadRequest("Email&Email уже используется");
            }
            if (model.EmailConfirmed)
            {
                user.EmailConfirmed = context.Users.FirstOrDefault(
                                            c => c.Email == model.Email
                                            && c.Id == userId
                                            && !c.IsDeleted
                                            && !c.Lockout) != null;
            }
            else
            {
                user.EmailConfirmed = false;
            }
            user.Email = model.Email;
            user.FullName = model.UserFullName;
            //Если номер телефона подтвержден, то тогда проверяем не изменился ли он
            if (model.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = context.Users.FirstOrDefault(
                                            c => c.PhoneNumber == new string(model.PhoneNumber.Where(u => char.IsDigit(u)).ToArray())
                                            && c.Id == userId
                                            && !c.IsDeleted
                                            && !c.Lockout) != null;
            }
            else
            {
                //Иначе телефон не подтвержден (был не подтвержден и остается неподтвержден)
                user.PhoneNumberConfirmed = false;
            }
            user.PhoneNumber = new string(model.PhoneNumber.Where(u => char.IsDigit(u)).ToArray());
            user.DefaultAddressId = model.DefaultAddressId;
            user.LastUpdDate = DateTime.Now;
            var CurrentUser = User.Identity.GetUserId();
            user.LastUpdateByUserId = CurrentUser;
            var userInCompanies = context.UsersInCompanies.Where(c => c.IsActive && !c.IsDeleted);
            foreach (var userInCompany in userInCompanies)
            {
                var modelAddress = model.UserInCompanies?.FirstOrDefault(c => c.Id == userInCompany.Id);
                if (modelAddress != null)
                {
                    userInCompany.DefaultAddressId = modelAddress.DeliveryAddressId;
                }
            }
            context.SaveChanges();

            var token = user.GenerateAuthenticationToken(_config.AuthToken);

            return Ok(token);
        }

        [Route("confirmphone"), HttpPost]
        public async Task<IActionResult> ConfirmPhone([FromBody] UserInfoModel model)
        {
            Accessor = GetAccessor();
            var context = Accessor.GetContext();
            var userId = User.Identity.GetUserId();
            var smsCode = context.SmsCodes.FirstOrDefault(c => c.UserId == userId && c.Phone == new string(model.PhoneNumber.Where(u => char.IsDigit(u)).ToArray()) && c.IsActive && !c.IsDeleted);

            if (smsCode != null)
            {
                var user = context.Users.FirstOrDefault(c => c.Id == userId && !c.IsDeleted);

                if (smsCode.ValidTime >= DateTime.Now && smsCode.Code == model.SmsCode)
                {
                    user.PhoneNumberConfirmed = true;
                }
            }

            model.IsSendCode = false;

            context.SaveChanges();

            return Ok(model);
        }

        [Route("SetPassword"), HttpPost]
        public IActionResult SetPassword([FromBody] SetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserId();

            Accessor = GetAccessor();
            using (var fc = Accessor.GetContext())
            {
                var user = fc.Users.FirstOrDefault(o => !o.IsDeleted && o.Id == userId);
                if (user == null)
                    return BadRequest("Пользователь не найден");

                user.Password = ITWebNet.Food.Core.PasswordHasher.HashPassword(model.NewPassword);

                fc.SaveChanges();
            }

            return Ok("Пароль успешно создан.");
        }

        [Route("GetUserCompanyId"), HttpGet]
        public IActionResult GetUserCompanyId()
        {
            var userId = User.Identity.GetUserId();
            var fc = Accessor.Instance.GetContext();
            long? companyId = fc.UsersInCompanies.FirstOrDefault(u => u.UserId == userId && u.IsActive)?.CompanyId;
            return Ok(companyId);
        }

        [Route("SendEmailConfirmation")]
        public async Task<IActionResult> SendEmailConfirmation()
        {
            var user = User.Identity.GetUserById();
            string code = new NotificationHelper().GetRandomCode(6, Models.SMSCodeTypeEnum.Mixed);
            if (!Accessor.Instance.SetConfirmationEmailCode(user.Id, code))
                return Ok(false);

            await _email.SendAsync($"Ваш код подтверждения {code}", "Потверждение Email", user.Email);

            return Ok(true);
        }

        [HttpPost, Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] UserInfoModel model)
        {
            var user = User.Identity.GetUserById();
            if (user != null)
            {
                if (user.EmailConfirmationCode == model.EmailCode && !user.EmailConfirmed)
                {
                    if (Accessor.Instance.SetConfirmEmail(user.Id, true))
                    {
                        Accessor.Instance.DeleteConfirmationEmailCodeByUserId(user.Id);
                        model.EmailConfirmed = true;
                    }
                }
            }
            return Ok(model);
        }
    }
}
