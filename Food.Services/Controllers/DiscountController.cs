using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/discounts")]
    public class DiscountController : ContextableApiController
    {
        public DiscountController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public DiscountController()
        {}

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("")]
        public IActionResult CreateDiscount([FromBody]DiscountModel discount)
        {
            try
            {
                if (discount == null)
                {
                    return BadRequest("Empty Discount");
                }

                User currentUser =
                    User.Identity.GetUserById();

                var discountIntoBase = discount.GetEntity();
                discountIntoBase.CreationDate = DateTime.Now;
                discountIntoBase.IsDeleted = false;
                discountIntoBase.CreatorId = currentUser.Id;
                return
                    Ok(GetAccessor().AddDiscount(discountIntoBase));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut, Route("")]
        public IActionResult UpdateDiscount([FromBody]DiscountModel discount)
        {
            try
            {
                if (discount == null)
                {
                    return BadRequest("Empty Discount");
                }

                User currentUser =
                    User.Identity.GetUserById();

                if (currentUser == null)
                    return Ok(false);

                var editedDiscount = discount.GetEntity();

                editedDiscount.LastUpdateByUserId = currentUser.Id;

                return Ok(GetAccessor().EditDiscount(editedDiscount));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        //[CheckUserInBase(SecurityAction.Demand)]
        [Authorize(Roles = "User")]
        [HttpGet, Route("amount")]
        public IActionResult GetDiscountAmount(long cafeId, DateTime date, long? companyId)
        {
            try
            {
                var currentUser =
                    User.Identity.GetUserById();

                return Ok(
                    Convert.ToInt32(
                        GetAccessor()
                            .GetDiscountValue(cafeId, date, currentUser.Id, companyId)
                        ));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("GetDiscounts")]
        public IActionResult GetDiscounts([FromBody]long[] discountIdList)
        {
            try
            {
                var discountList = new List<DiscountModel>();

                List<Discount> itemFromBase =
                    GetAccessor().GetDiscounts(discountIdList);

                discountList.AddRange(itemFromBase.Select(d => d.GetContract()));

                return Ok(discountList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("user/{userId:long}")]
        public IActionResult GetUserDiscounts(long userId)
        {
            try
            {
                var discounts = GetAccessor().GetUserDiscounts(userId);
                var result = discounts.Select(d => d.GetContract()).ToList();

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                     DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("{discountId:long}")]
        public IActionResult DeleteDiscount(Int64 discountId)
        {
            try
            {
                User currentUser =
                User.Identity.GetUserById();

                Discount discount =
                    Accessor.Instance
                        .GetDiscounts(
                            new List<Int64> { discountId }.ToArray()
                        ).FirstOrDefault();

                if (discount == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }

                return Ok(Accessor.Instance.RemoveDiscount(discount.Id, currentUser.Id));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
    }
}
