using Food.Data;
using Food.Services.Extensions;
using Food.Services.Extensions.Rating;
using ITWebNet.Food.Core.DataContracts.Common;
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
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/rating")]
    public class RatingController : ContextableApiController
    {
        [ActivatorUtilitiesConstructor]
        public RatingController()
        {
            Logger = Log.Logger;
        }

        public RatingController(IFoodContext context, Accessor accessor, Serilog.ILogger logger)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
            Logger = logger;
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("allratingsfromuser/isfilter/{isFilter:bool}/type/{typeOfObject:int}")]
        public IActionResult GetAllRatingFromUser(bool isFilter, int typeOfObject = (int)ObjectTypesEnum.ANOTHER_TYPE)
        {
            try
            {
                var ratingsFromUser = new List<RatingModel>();

                var currentUser = User.Identity.GetUserById();

                if (currentUser == null) return Ok(ratingsFromUser);
                var ratingsFromDataBase = GetAccessor().GetAllRatingFromUser(
                    currentUser.Id, typeOfObject, isFilter
                );

                ratingsFromUser.AddRange(ratingsFromDataBase.Select(rating => rating.GetContract()));

                return Ok(ratingsFromUser);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("allratingstoobject/{objectId:long}/{objectType:int}")]
        public IActionResult GetAllRatingToObject(long objectId, int objectType)
        {
            try
            {
                var ratings = new List<RatingModel>();

                var ratingFromBase =
                    GetAccessor().GetAllRatingToObject(
                        objectId, objectType
                    );

                foreach (var rate in ratingFromBase)
                    ratings.Add(rate.GetContract());

                return Ok(ratings);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("finalrate/cafe/{cafeId:long}")]
        public IActionResult GetFinalRateToCafe(long cafeId)
        {
            try
            {
                return Ok(GetAccessor().GetFinalRateToCafe(cafeId));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("finalrate/dish/{dishId:long}")]
        public IActionResult GetFinalRateToDish(long dishId)
        {
            try
            {
                return Ok(GetAccessor().GetFinalRateToDish(dishId));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("insertrating/{objectId:long}/{typeOfObject:int}/{valueOfRating:int}")]
        public IActionResult InsertNewRating(long objectId, int typeOfObject, int valueOfRating)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                long id = 0;

                if (
                    currentUser != null
                    && currentUser.Name != "anonymous"
                    && valueOfRating >= 1
                    && valueOfRating <= 5
                )
                {
                    id = GetAccessor().InsertNewRating(
                        currentUser.Id,
                        objectId,
                        typeOfObject,
                        valueOfRating
                    );
                }

                return Ok(id);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }
    }
}
