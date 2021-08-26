using Food.Data.Entities;
using Food.Services;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;

namespace ITWebNet.Food.Controllers
{
    [Route("api/reports")]
    public class ReportLayoutController : ControllerBase
    {
        public ILogger<ReportLayoutController> Logger { get; set; }

        public ReportLayoutController(ILogger<ReportLayoutController> Logger)
        {
            this.Logger = Logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("addcafexlstadm")]
        public IActionResult AddCafeToXsltAdm(long id)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
                var allCafes = Accessor.Instance.GetCafes();
                var xsltCafes = Accessor.Instance.GetCafesToXslt(id);
                var report = Accessor.Instance.GetXslt(id);
                List<LayoutToCafeModel> cafes = new List<LayoutToCafeModel>();
                foreach (var i in allCafes)
                {
                    var tmp = xsltCafes.FirstOrDefault(e => e.Id == i.Id);
                    if (tmp == null) cafes.Add(new LayoutToCafeModel()
                    {
                        CafeId = i.Id,
                        CafeName = i.CafeName,
                        Xslt = report.GetContract()
                    });
                }
                return Ok(cafes);
            }
            catch (SecurityException e)
            {
                Logger.LogError("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getxslttocafeadm")]
        public IActionResult GetXsltToCafe(long layoutId)
        {
            try
            {
                List<LayoutToCafeModel> toCafe = new List<LayoutToCafeModel>();
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
                var report = Accessor.Instance.GetXslt(layoutId);
                if (report == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }
                var xsltCafes = Accessor.Instance.GetCafesToXslt(layoutId);
                foreach (var i in xsltCafes)
                {
                    var tmp = Accessor.Instance.GetCafeById(i.CafeId);
                    toCafe.Add(new LayoutToCafeModel()
                    {
                        CafeId = tmp.Id,
                        CafeName = tmp.CafeName,
                        Xslt = report.GetContract(),
                        Id = i.Id
                    });
                }
                if (toCafe.Count == 0)
                {
                    toCafe.Add(new LayoutToCafeModel()
                    {
                        CafeId = -1,
                        CafeName = "Шаблон не используется ни одним кафе",
                        Xslt = report.GetContract(),
                        Id = -1
                    });
                }
                return Ok(toCafe);

            }
            catch (SecurityException e)
            {
                Logger.LogError("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("addxslttocafeadm")]
        public IActionResult AddXsltToCafe([FromBody] LayoutToCafeModel toCafe)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
                XsltToCafe model = new XsltToCafe();
                model.CafeId = toCafe.CafeId;
                model.XsltId = toCafe.Xslt.Id;
                model.CreatorId= currentUser.Id;
                model.IsDeleted = false;
                model.CreateDate = DateTime.Now;
                return Ok( Accessor.Instance.AddCafeToXslt(model));
            }
            catch (SecurityException e)
            {
                Logger.LogError("[{0} {1}] - {2} in {3}",
                     DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("delxslttocafeadm")]
        public IActionResult DeleteXsltToCafe(long idLayToCafe)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
                var layToCafe = Accessor.Instance.GetXsltToCafeById(idLayToCafe);
                var xslt = Accessor.Instance.GetXslt(layToCafe.XsltId).GetContract();
                var cafe = Accessor.Instance.GetCafeById(layToCafe.CafeId);
                var model = new LayoutToCafeModel()
                {
                    CafeId = cafe.Id,
                    CafeName = cafe.CafeName,
                    Xslt = xslt,
                    Id = layToCafe.Id
                };
                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.LogError("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }

        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("delxslttocafeconfirmadm")]
        public IActionResult RemoveXsltToCafe([FromBody] LayoutToCafeModel idLay)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
                
                return Ok(Accessor.Instance.RemoveXsltToCafe(idLay.Id, currentUser.Id));
            }
            catch (SecurityException e)
            {
                Logger.LogError("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }

        }
    }
}