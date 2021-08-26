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
    [Route("api/kitchens")]
    public class KitchenController : ContextableApiController
    {
        public KitchenController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public KitchenController()
        {}

        [Authorize(Roles = "Manager")]
        [HttpPost, Route("cafe/{cafeId:long}/kitchen/{kitchenId:long}")]
        public IActionResult AddKitchenToCafe(Int64 cafeId, Int64 kitchenId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (GetAccessor().IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    return Ok(true);
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("{cafeId:long}")]
        public IActionResult GetCurrentListOfKitchenToCafe(Int64 cafeId)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                if (GetAccessor().IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    var kitchens =
                        GetAccessor().GetListOfKitchenToCafe(cafeId);

                    var currentKitchens = new List<KitchenModel>();

                    foreach (var kitchen in kitchens)
                    {
                        currentKitchens.Add(kitchen.GetContract());
                    }

                    return Ok(currentKitchens.ToArray());
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("")]
        public IActionResult GetFullListOfKitchen()
        {
            try
            {
                var kitchens = GetAccessor().GetListOfKitchen();

                var currentKitchens = new List<KitchenModel>();

                foreach (var kitchen in kitchens)
                    currentKitchens.Add(kitchen.GetContract());

                return Ok(currentKitchens.ToArray());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete, Route("cafe/{cafeId:long}/kitchen/{kitchenId:long}")]
        public IActionResult RemoveKitchenFromCafe(Int64 cafeId, Int64 kitchenId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (GetAccessor().IsUserManagerOfCafe(currentUser.Id, cafeId))
                    return Ok(true);
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }
    }

    public static class KitchenExtensions
    {
        public static KitchenModel GetContract(this Kitchen kitchen)
        {
            return (kitchen == null)
                ? null
                : new KitchenModel
                {
                    Id = kitchen.Id,
                    Name = kitchen.Name
                };
        }


        public static KitchenModel GetContract(this KitchenInCafe kitchen)
        {
            return (kitchen == null)
                ? null
                : new KitchenModel()
                {
                    Id = kitchen.Id,
                    Name = kitchen.Kitchen.Name
                };
        }

        public static Kitchen GetEntity(this KitchenModel kitchen)
        {
            return (kitchen == null)
                ? new Kitchen()
                : new Kitchen
                {
                    Id = kitchen.Id,
                    Name = kitchen.Name
                };
        }
    }
}
