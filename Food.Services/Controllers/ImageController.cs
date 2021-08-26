using Food.Data.Entities;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/images")]
    public class ImageController : ContextableApiController
    {
        [Authorize(Roles = "Manager")]
        [HttpPost, Route("{cafeId:long}/{objectId:long}/{type:int}")]
        public IActionResult AddImage(string hash, long cafeId, long objectId, int type)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (
                    currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id, cafeId
                    )
                )
                {
                    var image = new Image()
                    {
                        CreateDate = DateTime.Now,
                        LastUpdateByUserId = currentUser.Id,
                        LastUpdDate = DateTime.Now,
                        CreatorId = currentUser.Id,
                        Hash = hash,
                        IsDeleted = false,
                        ObjectId = objectId,
                        ObjectType = type,
                    };

                    return Ok(Accessor.Instance.AddImage(image));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete, Route("{cafeId:long}/{objectId:long}/{type:int}")]
        public IActionResult RemoveImage(string hash, long cafeId, long objectId, int type)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (
                    currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id, cafeId
                    )
                )
                {
                    var image = new Image()
                    {
                        LastUpdDate = DateTime.Now,
                        LastUpdateByUserId = currentUser.Id,
                        Hash = hash,
                        IsDeleted = true,
                        ObjectId = objectId,
                        ObjectType = type,
                    };

                    return Ok(Accessor.Instance.RemoveImage(image));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }
    }
}
