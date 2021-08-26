using Food.Data.Entities;
using Food.Services;
using Food.Services.Controllers;
using Food.Services.Extensions;
using Food.Services.GenerateXLSX;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.ServiceModel;
using DinkToPdf.Contracts;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ITWebNet.Food.Controllers
{
    [Route("api/reports")]
    public class ReportController : ContextableApiController
    {
        //Это для PDF, не трогай если не хочешь сломать PDF
        private IConverter _converter;
        public ReportController(IConverter converter)
        {
            _converter = converter;
        }

        [Authorize(Roles = "Manager")]
        [HttpPut]
        [Route("xslt")]
        public IActionResult AddXslt([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser == null || !Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id, model.CafeId ?? -1
                    )) throw new SecurityException("Attempt of unauthorized access");
                var xslt = new ReportStylesheet
                {
                    CafeId = model.CafeId,
                    Name = model.Name,
                    Description = model.Description,
                    Transformation = model.Transformation,
                    CreatorId = currentUser.Id,
                    LastUpdateByUserId = currentUser.Id,
                    LastUpdDate = DateTime.Now
                };

                return Ok(Accessor.Instance.AddXslt(xslt));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("xslt")]
        public IActionResult EditXslt([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var xslt = Accessor.Instance.GetXslt(model.Id);

                if (xslt == null)
                    throw new Exception("Attempt to work with unexisting object");

                if (
                    currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        xslt.CafeId ?? -1
                    )
                )
                {
                    xslt.CafeId = xslt.CafeId;
                    xslt.Name = xslt.Name;
                    xslt.Description = xslt.Description;
                    xslt.Transformation = xslt.Transformation;
                    xslt.LastUpdateByUserId = currentUser.Id;
                    xslt.LastUpdDate = DateTime.Now;

                    return Ok(Accessor.Instance.EditXslt(xslt));
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager,Consolidator")]
        [HttpGet]
        [Route("companyreport/json/{companyId:long}/from/{startDate:long}/to/{endDate:long}")]
        public IActionResult GetCompanyReportDataInJson(long companyId, long startDate, long endDate)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager,Consolidator")]
        [HttpGet]
        [Route("cafereport/xml/{cafeId:long}/from/{startDate:long}/to/{endDate:long}")]
        [Route("cafereport/xml/from/{startDate:datetime}/to/{endDate:long}")]
        public IActionResult GetCompanyReportDataInXml(long? cafeId, long startDate, long endDate)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();
                var startDateTime = DateTimeExtensions.FromUnixTime(startDate);
                var endDateTime = DateTimeExtensions.FromUnixTime(endDate);
                var result =
                    ReportServiceHelper.GetCompanyReportsInXMlByFilter(
                        new ReportFilter
                        {
                            AvailableStatusList = null,
                            CafeId = cafeId ?? -1,
                            CompanyOrdersIdList = null,
                            EndDate = endDateTime,
                            OrdersIdList = null,
                            BanketOrdersIdList = null,
                            ReportTypeId = -1,
                            StartDate = startDateTime
                        }, 
                        currentUser
                    );

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                     DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager,Consolidator")]
        [HttpGet]
        [Route("customerreport/json/{customerId:long}/from/{startDate:long}/to/{endDate:long}")]
        public IActionResult GetCustomerReportDataInJson(long customerId, long startDate, long endDate)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager,Consolidator")]
        [HttpGet]
        [Route("customerreport/xml/{customerId:long}/from/{startDate:long}/to/{endDate:long}")]
        public IActionResult GetCustomerReportDataInXml(long customerId, long startDate, long endDate)
        {
            try
            {
                var startDateTime = DateTimeExtensions.FromUnixTime(startDate);
                var endDateTime = DateTimeExtensions.FromUnixTime(endDate);
                var user = Accessor.Instance.GetUserById(customerId);

                var usersOrders = Accessor.Instance.GetUserOrders(user.Id, startDateTime, endDateTime);

                var userOrdersData = ReportServiceHelper.GetUserOrdersData(usersOrders);

                var processor = new XmlProcessing<UserOrdersData>();

                var result = processor.SerializeObject(userOrdersData);

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("getuserorderdetailsreport")]
        public IActionResult GetUserOrderDetailsReport([FromBody]ReportFilter filter)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser == null || !Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        filter.CafeId.Value
                    )) throw new SecurityException("Attempt of unauthorized access");
                
                switch (filter.ReportExtension)
                {
                    
                    
                    case ReportExtension.HTML:
                        var xmlReportBody =
                            ReportServiceHelper.GetReportUserOrderIdInXmlByFilter(
                                filter
                            );
                        var xsltTransform =
                            Accessor.Instance.GetXslt(filter.ReportTypeId);
                        if (xsltTransform != null
                            && !string.IsNullOrWhiteSpace(xsltTransform.Transformation)
                            && (xsltTransform.CafeId == filter.CafeId || xsltTransform.IsCommon)
                        )
                        {
                            ReportCreatorBase report = new InMemoryReport();

                            ReportBase reportBody = new HtmlReportBody();

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBody,
                                    NameTemplate = xsltTransform.Name,
                                    XsltTransform = xsltTransform.Transformation
                                };

                            reportBody.FormReportBody(inputData);

                            report.FormFile(reportBody);

                            return Ok(report.GetFileBody());
                        }
                        else
                        {
                            throw new SecurityException("Attempt of unauthorized access");
                        }
                    case ReportExtension.PDF:
                        //filter.ReportExtension = ReportExtension.HTML;
                        var xmlReportBodyPDF =
                            ReportServiceHelper.GetReportUserOrderIdInXmlByFilter(
                                filter
                            );

                        var xsltTransformPDF =
                            Accessor.Instance.GetXslt(filter.ReportTypeId);
                        //filter.ReportExtension = ReportExtension.PDF;
                        if (xsltTransformPDF != null
                            && !string.IsNullOrWhiteSpace(xsltTransformPDF.Transformation)
                            && (xsltTransformPDF.CafeId == filter.CafeId || xsltTransformPDF.IsCommon)
                        )
                        {
                            ReportCreatorBase reportPDF = new InMemoryReport();

                            ReportBase reportBody = new ReportBodyPdf(_converter);

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBodyPDF,
                                    NameTemplate = xsltTransformPDF.Name,
                                    XsltTransform = xsltTransformPDF.Transformation
                                };
                            
                            reportBody.FormReportBody(inputData);

                            reportPDF.FormFile(reportBody);

                            return Ok(reportPDF.GetFileBody());

                        }
                        else
                        {
                            throw new SecurityException("Attempt of unauthorized access");
                        }
                    case ReportExtension.XLS:
                        var reportData =
                            ReportServiceHelper.GetReportUserOrderId(
                                filter
                            );
                        ReportCreatorBase reportXls = new InMemoryReport();
                        

                        ReportBase reportBodyXls = new XLSUserDetailsReportBody(reportData);

                        reportXls.FormFile(reportBodyXls);

                        return Ok(reportXls.GetFileBody());
                    default:
                        throw new SecurityException("Attempt of unauthorized access");
                }
                //return Ok();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("getreportbyfilter")]
        public IActionResult GetReportFileByFilter([FromBody]ReportFilter filter)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                if (currentUser == 0 || filter == null || !Accessor.Instance.IsUserManagerOfCafe(
                    currentUser,
                    filter.CafeId.Value
                )) throw new SecurityException("Attempt of unauthorized access");
                switch (filter.ReportExtension)
                {
                    case ReportExtension.HTML:
                        var xmlReportBody =
                            ReportServiceHelper.GetCompanyReportsInXMlByFilter(
                                filter,
                                currentUser,
                                filter.SortType
                            );

                        var xsltTransform =
                            Accessor.Instance.GetXslt(filter.ReportTypeId);

                        if (xsltTransform != null
                            && !string.IsNullOrWhiteSpace(xsltTransform.Transformation)
                            && (xsltTransform.CafeId == filter.CafeId || xsltTransform.IsCommon)
                        )
                        {
                            ReportCreatorBase report = new InMemoryReport();

                            ReportBase reportBody = new HtmlReportBody();

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBody,
                                    NameTemplate = xsltTransform.Name,
                                    XsltTransform = xsltTransform.Transformation
                                };

                            reportBody.FormReportBody(inputData);

                            report.FormFile(reportBody);

                            return Ok(report.GetFileBody());
                        }
                        else
                        {
                            throw new SecurityException("Attempt of unauthorized access");
                        }
                    case ReportExtension.XLS:
                        var reportData =
                            ReportServiceHelper.GetCompanyReportData(
                                filter,
                                currentUser,
                                filter.SortType
                            );
                        
                        ReportCreatorBase reportXls = new InMemoryReport();

                        ReportBase reportBodyXls = new XlsReportBody(reportData);
                        
                        reportXls.FormFile(reportBodyXls);
                        
                        return Ok(reportXls.GetFileBody());
                    case ReportExtension.PDF:
                        filter.ReportExtension = ReportExtension.HTML;
                        var xmlReportBodyPDF =
                            ReportServiceHelper.GetCompanyReportsInXMlByFilter(
                                filter,
                                currentUser,
                                filter.SortType
                            );

                        var xsltTransformPDF =
                            Accessor.Instance.GetXslt(filter.ReportTypeId);

                        if (xsltTransformPDF != null
                            && !string.IsNullOrWhiteSpace(xsltTransformPDF.Transformation)
                            && (xsltTransformPDF.CafeId == filter.CafeId || xsltTransformPDF.IsCommon)
                        )
                        {
                            ReportCreatorBase reportPDF = new InMemoryReport();

                            ReportBase reportBody = new ReportBodyPdf(_converter);

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBodyPDF,
                                    NameTemplate = xsltTransformPDF.Name,
                                    XsltTransform = xsltTransformPDF.Transformation
                                };
                            filter.ReportExtension = ReportExtension.PDF;
                            reportBody.FormReportBody(inputData);

                            reportPDF.FormFile(reportBody);

                            return Ok(reportPDF.GetFileBody());

                        }
                        else
                        {
                            throw new SecurityException("Attempt of unauthorized access");
                        }

                    default:
                        throw new SecurityException("Attempt of unauthorized access");
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
            catch (Exception ex)
            {
                Accessor.Instance.LogError("GetReportFileByFilter",
                    $"Oшибка {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Отчет html по заказам сотрудника компании
        /// </summary>
        [Authorize(Roles = "Consolidator")]
        [HttpPost]
        [Route("getreportbyfilter/consolidator/user")]
        public IActionResult GetReportOrdersEmployeeInHtml([FromBody]ReportFilter filter)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                if (currentUser == 0 || filter == null || !Accessor.Instance.IsUserCuratorOfCafe(currentUser, filter.CompanyId ?? -1)
                    ) throw new SecurityException("Пользователь не является куратором компании");
                switch (filter.ReportExtension)
                {
                    case ReportExtension.HTML:
                        var xmlReportBody =
                            ReportServiceHelper.GetReportUserOrdersInXmlByFilter(filter);

                        string xsltTransform;
                        using (var sr = new StreamReader(Path.Combine(Environment.CurrentDirectory,
                            @"GenerateXLSX\CompanyOrderReportForUser.xslt")))
                        {
                            xsltTransform = sr.ReadToEnd();
                        }

                        if (!string.IsNullOrWhiteSpace(xsltTransform))
                        {
                            ReportCreatorBase report = new InMemoryReport();

                            ReportBase reportBody = new HtmlReportBody();

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBody,
                                    NameTemplate = String.Empty,
                                    XsltTransform = xsltTransform
                                };

                            reportBody.FormReportBody(inputData);

                            report.FormFile(reportBody);

                            return Ok(report.GetFileBody());
                        }
                        else
                        {
                            throw new SecurityException("Нет прав на чтение шаблона xslt");
                        }
                    case ReportExtension.XLS:
                        var reportData =
                            ReportServiceHelper.GetCompanyReportData(
                                filter, currentUser
                            );
                        ReportCreatorBase reportXls = new InMemoryReport();

                        ReportBase reportBodyXls = new XlsReportBody(reportData);

                        reportXls.FormFile(reportBodyXls);

                        return Ok(reportXls.GetFileBody());
                    case ReportExtension.PDF:
                        break;
                    default:
                        throw new SecurityException("Attempt of unauthorized access");
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Отчет html по заказам сотрудников компании
        /// </summary>
        [Authorize(Roles = "Consolidator")]
        [HttpPost]
        [Route("getreportbyfilter/consolidator/users")]
        public IActionResult GetReportOrdersAllEmployeeInHtml([FromBody]ReportFilter filter)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                if (currentUser == null || filter == null || !Accessor.Instance.IsUserCuratorOfCafe(currentUser, filter.CompanyId ?? -1)
                    ) throw new SecurityException("Пользователь не является куратором компании");
                switch (filter.ReportExtension)
                {
                    case ReportExtension.HTML:
                        var xmlReportBody =
                            ReportServiceHelper.GetReportUsersOrdersInXmlByFilter(filter);

                        string xsltTransform;
                        using (var sr = new StreamReader(Path.Combine(Environment.CurrentDirectory,
                            @"GenerateXLSX\CompanyOrderReportForUsers.xslt")))
                        {
                            xsltTransform = sr.ReadToEnd();
                        }

                        if (!string.IsNullOrWhiteSpace(xsltTransform))
                        {
                            ReportCreatorBase report = new InMemoryReport();

                            ReportBase reportBody = new HtmlReportBody();

                            var inputData =
                                new ReportInputData
                                {
                                    InitialInfo = xmlReportBody,
                                    NameTemplate = String.Empty,
                                    XsltTransform = xsltTransform
                                };

                            reportBody.FormReportBody(inputData);

                            report.FormFile(reportBody);

                            return Ok(report.GetFileBody());
                        }
                        else
                        {
                            throw new SecurityException("Нет прав на чтение шаблона xslt");
                        }
                    case ReportExtension.XLS:
                        var reportData =
                            ReportServiceHelper.GetCompanyReportData(
                                filter, currentUser
                            );
                        ReportCreatorBase reportXls = new InMemoryReport();

                        ReportBase reportBodyXls = new XlsReportBody(reportData);

                        reportXls.FormFile(reportBodyXls);

                        return Ok(reportXls.GetFileBody());
                    case ReportExtension.PDF:
                        break;
                    default:
                        throw new SecurityException("Attempt of unauthorized access");
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Отчет xlsx по заказам сотрудника компании
        /// </summary>
        [HttpPost]
        [Route("orderreport/user/xlsx")]
        public IActionResult GetReportOrdersEmployee([FromBody]ReportFilter filter)
        {
            var reportModel = new ReportModel();
            var xlsxModel = ReportServiceHelper.GetReportUserOrders(filter);
            var xmlByte = new CompanyOrderReportForUser().CreatePackageAsBytes(xlsxModel);
            reportModel.FileBody = xmlByte;
            reportModel.FileName = $"Отчет по заказам {xlsxModel.User.UserFullName} за период {filter.StartDate.Date.ToShortDateString()} - {filter.EndDate.Date.ToShortDateString()}";

            return Ok(reportModel);
        }

        /// <summary>
        /// Отчет xlsx по заказам сотрудников компании
        /// </summary>
        [HttpPost]
        [Route("orderreport/users/xlsx")]
        public IActionResult GetReportOrdersAllEmployee([FromBody]ReportFilter filter)
        {
            var model = ReportServiceHelper.GetReportUsersOrders(filter);

            var xmlByte = new CompanyOrderReportForUsers().CreatePackageAsBytes(model);

            var reportModel = new ReportModel()
            {
                FileBody = xmlByte,
                FileName = $"Отчет по заказам сотрудников за период {filter.StartDate.Date.ToShortDateString()} - {filter.EndDate.Date.ToShortDateString()}"
            };

            return Ok(reportModel);
        }

        [HttpGet]
        [Route("xlstfromcafe/{cafeId:long}")]
        public IActionResult GetXsltFromCafe(long cafeId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser == null || !Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        cafeId
                    )) throw new SecurityException("Attempt of unauthorized access");
                var xslts = new List<XSLTModel>();

                var xsltFromBase =
                    Accessor.Instance.GetXsltFromCafe(cafeId);

                foreach (var xslt in xsltFromBase)
                {
                    xslt.Transformation = string.Empty;
                    xslts.Add(xslt.GetContract());
                }

                return Ok(xslts);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [Route("removexlst")]
        public IActionResult RemoveXslt([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var xslt = Accessor.Instance.GetXslt(model.Id);

                if (xslt == null)
                    throw new Exception("Attempt to work with unexisting object");

                if (
                    currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        xslt.CafeId ?? -1
                    )
                )
                    return Ok(Accessor.Instance.RemoveXslt(xslt));
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Получение Html отчета по фильтру для Куратора
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <param name="sort">Поле для сортировки</param>
        /// <returns></returns>
        [Authorize(Roles = "Consolidator")]
        [HttpPost]
        [Route("companyreport/html")]
        public IActionResult GetCompanyReportInHtml([FromBody]ReportFilter filter, ReportSortType sort)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();
                var reportData = ReportServiceHelper.GetCompanyReportData(filter, currentUser, sort);
                var reportDataModel = reportData.ToDTO();

                return Ok(reportDataModel);
            }
            catch(Exception e)
            {
                return BadRequest("Не удалось");
            }
        }

        /// <summary>
        /// Отчет xlsx по заказам сотрудников компании
        /// </summary>
        [Authorize(Roles = "Consolidator")]
        [HttpPost]
        [Route("companyreport/xlsx")]
        public IActionResult GetCompanyReportInXlsx([FromBody]ReportFilter filter, ReportSortType sort)
        {
            var currentUser = User.Identity.GetUserId();
            var model = ReportServiceHelper.GetCompanyReportData(filter, currentUser, sort);
            var xmlByte = new CompanyOrdersReportForCompany().CreatePackageAsBytes(model);

            var reportModel = new ReportModel()
            {
                FileBody = xmlByte,
                FileName = $"Отчет по заказам сотрудников за период {filter.StartDate.Date.ToShortDateString()} - {filter.EndDate.Date.ToShortDateString()}"
            };

            return Ok(reportModel);
        }

        [Authorize(Roles = "Consolidator")]
        [HttpPost]
        [Route("getEmployeesReport")]
        public IActionResult GetEmployeesReport([FromBody]ReportFilter filter)
        {
            var model = ReportServiceHelper.GetListReportUserOrders(filter);
            var resultModel = model.Select(o => o.GetContract()).ToList();

            return Ok(resultModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getxlstList")]
        public IActionResult GetXsltList()
        {
            try
            {
                var xslts = new List<XSLTModel>();
                var xsltFromBase =
                    Accessor.Instance.GetXsltList();

                foreach (var xslt in xsltFromBase)
                {
                    xslts.Add(xslt.GetContract());
                }

                return Ok(xslts);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("addxsltadm")]
        public IActionResult AddXsltAdmin([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser == null)
                { throw new SecurityException("Attempt of unauthorized access"); }
                var xslt = new ReportStylesheet
                {
                    CafeId = model.CafeId,
                    Name = model.Name,
                    Description = model.Description,
                    Transformation = model.Transformation,
                    CreatorId = currentUser.Id,
                    LastUpdateByUserId = currentUser.Id,
                    LastUpdDate = DateTime.Now,
                    IsCommon = model.IsCommon,
                    CreateDate = DateTime.Now
                };

                return Ok(Accessor.Instance.AddXslt(xslt));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("uniquexsltadm")]
        [HttpGet]
        public IActionResult IsUniqueNameXslt(string name)
        {
            return Ok(Accessor.Instance.IsUniqueNameXslt(name));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("editxsltadm")]
        public IActionResult EditXsltAdm([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var xslt = Accessor.Instance.GetXslt(model.Id);

                if (xslt == null)
                    throw new Exception("Attempt to work with unexisting object");

                if (currentUser != null)

                {
                    xslt.CafeId = model.CafeId;
                    xslt.Name = model.Name;
                    xslt.Description = model.Description;
                    xslt.Transformation = model.Transformation;
                    xslt.LastUpdateByUserId = currentUser.Id;
                    xslt.LastUpdDate = DateTime.Now;
                    xslt.IsCommon = model.IsCommon;

                    return Ok(Accessor.Instance.EditXslt(xslt));
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("removexlstadm")]
        public IActionResult RemoveXsltAdm([FromBody]XSLTModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var xslt = Accessor.Instance.GetXslt(model.Id);

                if (xslt == null)
                    throw new Exception("Attempt to work with unexisting object");

                if (currentUser != null)
                    return Ok(Accessor.Instance.RemoveXslt(xslt));
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("getxsltbyidadm")]
        public IActionResult GetXsltById(long modelId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var report = Accessor.Instance.GetXslt(modelId);

                if (report == null)
                    throw new Exception("Attempt to work with unexisting object");
                var xslt = new XSLTModel()
                {
                    Description = report.Description,
                    Id = report.Id,
                    IsCommon = report.IsCommon,
                    Name = report.Name,
                    Transformation = report.Transformation
                };
                if (currentUser != null)
                    return Ok(xslt);
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Для добавления коммента в БД
        /// </summary>
        /// <param name="comment">коммент</param>
        /// <param name="id">id заказа</param>
        /// <returns>возращает флаг с результатом операции</returns>
        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("sendcomment/manager")]
        public IActionResult SendCommentManager(string comment, long id)
        {
            var currentUser = User.Identity.GetUserById();
            var result = Accessor.Instance.AddManagerCommentToTheOrder(comment, id, currentUser);
            return Ok(result);
        }
    }
}
