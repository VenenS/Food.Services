using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Controllers;
using Food.Services.Services;
using ITWebNet.Food.Core;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;

namespace Food.Services.Extensions
{
    public static class UserExtensions
    {
        const string IdentityProvider =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
        public static User FromDto(this User user, UserAdminModel model)
        {
            user.AccessFailedCount = model.AccessFailedCount;
            user.CreationDate = model.CreateDate;
            user.CreatorId = model.CreatedBy;
            user.Email = model.Email;
            user.EmailConfirmed = model.EmailConfirmed;
            user.Id = (int)model.Id;
            user.IsDeleted = model.IsDeleted;
            user.LastUpdateByUserId = model.LastUpdateBy;
            user.LastUpdDate = model.LastUpdateDate;
            user.Lockout = model.LockoutEnabled;
            user.LockoutEnddate = model.LockoutEndDateUtc;
            user.PhoneNumber = model.PhoneNumber;
            user.PhoneNumberConfirmed = model.PhoneNumberConfirmed;
            user.SmsNotify = model.SmsNotify;
            user.TwoFactor = model.TwoFactorEnabled;
            user.FullName = model.UserFullName;
            user.FullName = model.Email;

            return user;
        }

        public static UserModel GetContract(this User user)
        {
            var model = new UserModel
            {
                AccessFailedCount = (int)user.AccessFailedCount,
                CreateDate = user.CreationDate,
                CreatedBy = user.CreatorId,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                Id = user.Id,
                IsDeleted = user.IsDeleted,
                LastUpdateBy = user.LastUpdateByUserId,
                LastUpdateDate = user.LastUpdDate,
                LockoutEnabled = user.Lockout,
                LockoutEndDateUtc = user.LockoutEnddate,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                SmsNotify = user.SmsNotify,
                TwoFactorEnabled = user.TwoFactor,
                UserFullName = user.FullName,
                DeliveryAddressId = user.DefaultAddressId
            };

            return model;
        }

        public static UserAdminModel ToAdminDto(this User user)
        {
            return user == null
                ? null
                : new UserAdminModel
                {
                    DeliveryAddressId = user.DefaultAddressId,
                    Email = user.Email,
                    UserFullName = user.FullName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    SmsNotify = user.SmsNotify,
                    PersonalPoints = user.PersonalPoints,
                    PercentOfOrder = user.PersonalPoints,
                    ReferralPoints = user.ReferralPoints,
                    UserReferralLink = user.UserReferralLink
                };
        }

        public static UserModel ToDto(this User user)
        {
            return user == null
                ? null
                : new UserModel
                {
                    DeliveryAddressId = user.DefaultAddressId,
                    Email = user.Email,
                    UserFullName = user.FullName,
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    SmsNotify = user.SmsNotify
                };
        }

        public static User FromDto(this UserAdminModel user)
        {
            return user == null
                ? new User()
                : new User
                {
                    Id = (int)user.Id,
                    FullName = user.UserFullName,
                    DefaultAddressId = user.DeliveryAddressId,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    SmsNotify = user.SmsNotify
                };
        }

        public static User GetEntity(this UserWithLoginModel user)
        {
            return user == null
                ? new User()
                : new User
                {
                    Id = user.Id,
                    Name = user.Name,
                    FullName = user.FullName,
                    FirstName = user.UserFirstName,
                    LastName = user.UserSurname,
                    DefaultAddressId = user.DeliveryAddressId,
                    Email = user.EmailAdress,
                    PhoneNumber = user.PhoneNumber,
                    IsDeleted = user.IsDeleted,
                    DisplayName = user.DisplayName,
                    Lockout = user.Lockout
                };
        }

        public static ClaimsIdentity CreateIdentity(this User user, string authenticationType)
        {
            var claims = new List<Claim>();
            var roles = Accessor.Instance.GetListRoleToUser(user.Id);
            if (roles != null && roles.Count > 0)
            {
                foreach (var role in roles)
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
            }
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
            //временно, для тестирования веба на mvc 5
            claims.Add(new Claim(IdentityProvider, "ASP.NET Identity"));
            if (!string.IsNullOrWhiteSpace(user.FullName))
                claims.Add(new Claim(ClaimTypes.GivenName, user.FullName));
            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            return new ClaimsIdentity(claims, authenticationType);
        }


        public static TokenModel GenerateAuthenticationToken(this User user, Config.Sections.AuthToken tokenSettings)
        {
            var identity = user.CreateIdentity(JwtBearerDefaults.AuthenticationScheme);
            var now = DateTime.UtcNow;
            var tokenLifetime = TimeSpan.FromMinutes(tokenSettings.TokenLifeTime);
            var jwt = new JwtSecurityToken
            (
                tokenSettings.Issuer,
                tokenSettings.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(tokenLifetime),
                signingCredentials: new SigningCredentials(tokenSettings.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new TokenModel
            {
                AccessToken = token,
                UserName = user.Name,
                TokenType = JwtBearerDefaults.AuthenticationScheme,
                Expires = now.Add(tokenLifetime),
                ExpiresIn = (uint)tokenLifetime.TotalSeconds,
                Issued = now
            };
        }

        public static UserWithLoginModel GetContractUserLogin(this User user)
        {
            return user == null
                ? new UserWithLoginModel()
                : new UserWithLoginModel
                {
                    Lockout = user.Lockout,
                    IsDeleted = user.IsDeleted,
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    DeliveryAddressId = user.DefaultAddressId,
                    EmailAdress = user.Email,
                    FullName = user.FullName,
                    Name = user.Name,
                    Password = user.Password,
                    PhoneNumber = user.PhoneNumber,
                    UserFirstName = user.FirstName,
                    UserSurname = user.LastName
                };
        }

        public static async Task SendAuthorizationLinkAsync(this User user, IUrlHelper urlHelper, string callbackUrl, string key, IConfigureSettings configure)
        {
            callbackUrl += $"?token={key}&userId={user.Id}";

            var mailBody = string.Format(
                "<p>Ваш логин: {1}{0}Вы можете создать/сбросить свой пароль перейдя по этой <a href=\"{2}\">ссылке</a></p>" +
                "<p>Ссылка станет неактивной после изменения пароля</p>",
                Environment.NewLine,
                user.Email,
                callbackUrl);

            await new EmailService(configure).SendAsync(mailBody, "Едовоз - восстановление пароля", user.Email);
        }
    }
}
