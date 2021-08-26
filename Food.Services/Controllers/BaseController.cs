using Food.Data;
using Food.Data.Entities;
using Food.Services.ExceptionHandling;
using Food.Services.Extensions;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    public class BaseController : ControllerBase
    {
        protected const string IdentityProvider =
            "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";

        protected const string LocalLoginProvider = "Local";

        private User _userManager;

        protected IFoodContext Context;
        protected bool TestMode;
        protected Accessor Accessor;

        public BaseController()
        {
        }

        //public BaseController(User userManager, ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        //{
        //    UserManager = userManager;
        //    AccessTokenFormat = accessTokenFormat;
        //}

        /// <summary>
        ///     Обеспечение автотестирования
        /// </summary>
        /// <returns>IDataContext</returns>
        protected Accessor GetAccessor()
        {
            if (!TestMode) return Accessor.Instance;
            Accessor.SetTestingModeOn(Context);
            return Accessor;
        }

        protected virtual ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //protected FoodContext Context
        //{
        //    get
        //    {
        //        return HttpContext.Current.GetOwinContext().Get<FoodContext>();
        //    }
        //}

        protected virtual User UserManager
        {
            get
            {
                return _userManager ?? User.Identity.GetUserById();
            }

            private set
            {
                _userManager = value;
            }
        }
        protected virtual IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return new InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    var plain = string.Join(Environment.NewLine, result.Errors);

                    return BadRequest(plain);
                }
            }

            return null;
        }

        //TODO: Авторизация через соцсети. Удалить
        //protected class ExternalLoginData
        //{
        //    public string LoginProvider { get; set; }
        //    public string ProviderKey { get; set; }
        //    public string UserName { get; set; }

        //    public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
        //    {
        //        if (identity == null)
        //        {
        //            return null;
        //        }

        //        var providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

        //        if (providerKeyClaim == null || string.IsNullOrEmpty(providerKeyClaim.Issuer)
        //            || string.IsNullOrEmpty(providerKeyClaim.Value))
        //        {
        //            return null;
        //        }

        //        if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
        //        {
        //            return null;
        //        }

        //        return new ExternalLoginData
        //        {
        //            LoginProvider = providerKeyClaim.Issuer,
        //            ProviderKey = providerKeyClaim.Value,
        //            UserName = identity.Claims.First(c => c.Type == ClaimTypes.Name).Value
        //        };
        //    }

        //    public IList<Claim> GetClaims()
        //    {
        //        IList<Claim> claims = new List<Claim>();

        //        claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));
        //        claims.Add(new Claim(IdentityProvider, LoginProvider, null, LoginProvider));

        //        if (UserName != null)
        //        {
        //            claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
        //        }

        //        return claims;
        //    }
        //}
    }
}
