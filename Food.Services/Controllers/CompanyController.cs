using Food.Data;
using Food.Data.Entities;
using Food.Services.Contracts.Companies;
using Food.Services.ExceptionHandling;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/companies")]
    public class CompanyController : ContextableApiController
    {
        public CompanyController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public CompanyController()
        { }

        [HttpPost, Route("")]
        [Authorize(Roles = "Admin, Manager, Consolidator")]
        public IActionResult CreateCompany([FromBody]CompanyModel model)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                if (User.IsInRole(EnumUserRole.Admin))
                {
                    var companyExists = context.Companies.FirstOrDefault(o => o.Name.Trim().ToLower() == model.Name.ToLower());
                    if (companyExists != null)
                        return BadRequest("Компания с таким названием уже существует");
                }

                var currentUser = User.Identity.GetUserId();
                var company = model.GetEntity();
                company.CreationDate = DateTime.Now;
                company.FullName = string.Empty;
                company.JuridicalAddressId = null;
                company.MainDeliveryAddressId = null;
                company.PostAddressId = null;
                company.IsActive = true;
                company.CreatorId = currentUser;
                context.Companies.Add(company);
                context.SaveChanges();

                var role = context.Roles.FirstOrDefault(r => r.RoleName == "Director");
                if (role != null)
                {
                    var userCompanyLink = new UserInCompany
                    {
                        CompanyId = company.Id,
                        CreateDate = DateTime.Now,
                        CreatedBy = currentUser,
                        IsActive = true,
                        StartDate = DateTime.Now,
                        UserId = (int)currentUser,
                        UserRoleId = role.Id
                    };

                    var oldUserCompanyLink =
                        context.UsersInCompanies.FirstOrDefault(
                            uc => uc.CompanyId == userCompanyLink.CompanyId
                                  && uc.UserId == userCompanyLink.UserId
                        );

                    if (oldUserCompanyLink != null)
                    {
                        oldUserCompanyLink.IsDeleted = false;
                        oldUserCompanyLink.IsActive = true;
                    }
                    else
                    {
                        context.UsersInCompanies.Add(userCompanyLink);
                    }
                }

                context.SaveChanges();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPost, Route("filter")]
        public IActionResult GetCompaniesByFilter([FromBody]CompaniesFilterModel filter)
        {
            var context = Accessor.Instance.GetContext();

            var companies = context.Companies.
                Include(c => c.JuridicalAddress)
                .Include(c => c.Addresses)
                .ThenInclude(a => a.Address)
                .Where(c => filter.CompanyId.Contains(c.Id))
                .ToList();
            var companiesModels = companies.Select(c => c.GetContract()).ToList();
            return Ok(companiesModels);
        }

        [HttpGet, Route("")]
        public IActionResult Get()
        {
            try
            {
                var listOfCompany = new List<CompanyModel>();

                var itemFromBase = Accessor.Instance.GetCompanys();

                foreach (var item in itemFromBase)
                {
                    listOfCompany.Add(item.GetContract());
                }

                return Ok(listOfCompany);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("all")]
        public IActionResult GetAll()
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var company = context.Companies.ToList();

                return Ok(company.Select(c => c.GetContract()));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{id:long}")]
        public IActionResult Get(long id)
        {
            try
            {
                var company = Accessor.Instance.GetCompanyById(id);
                return Ok(company.GetContract());
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("GetCompanyByOrdersInfo")]
        public IActionResult GetCompanyByOrdersInfo(long cafeId, long startDate, long endDate)
        {
            try
            {
                var fromUnixTime = DateTimeExtensions.FromUnixTime(startDate);
                var toUnixTime = DateTimeExtensions.FromUnixTime(endDate);

                var companies = new List<CompanyModel>();

                var currentUser =
                    User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    var companiesFromCompanyOrders =
                        Accessor.Instance.GetCompaniesByCompanyOrders(
                            cafeId,
                            fromUnixTime,
                            toUnixTime
                        );

                    foreach (var company in companiesFromCompanyOrders)
                    {
                        companies.Add(company.GetContract());
                    }

                    companies = companies
                        .GroupBy(c => c.Id)
                        .Select(group => group.FirstOrDefault())
                        .ToList();
                }

                return Ok(companies);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("GetListOfCompanyByUserId")]
        public IActionResult GetListOfCompanyByUserId(long userId)
        {
            try
            {
                var listOfCompany = new List<CompanyModel>();

                var itemFromBase =
                    Accessor.Instance.GetListOfCompanysByUserId(userId);

                foreach (var item in itemFromBase)
                {
                    listOfCompany.Add(item.GetContract());
                }

                return Ok(listOfCompany);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete, Route("{id:long}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(long id)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var company = context.Companies.FirstOrDefault(c => c.Id == id);

                if (company != null)
                {
                    company.LastUpdateByUserId = User.Identity.GetUserId();
                    company.LastUpdDate = DateTime.Now;
                    company.IsDeleted = true;
                    company.IsActive = false;
                    context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{id:long}/restore")]
        [Authorize(Roles = "Admin")]
        public IActionResult Restore(long id)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var oldCompany =
                    context.Companies.FirstOrDefault(c => c.Id == id);

                if (oldCompany == null)
                    return NotFound();

                oldCompany.IsActive = true;
                oldCompany.IsDeleted = false;
                context.SaveChanges();

                return Ok(oldCompany.GetContract());
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPut, Route("")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateCompany([FromBody]CompanyModel company)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var userId = User.Identity.GetUserId();
                var oldCompany =
                    context.Companies.Include(c => c.JuridicalAddress).Include(o => o.Addresses).Include(c => c.City)
                        .FirstOrDefault(c => c.Id == company.Id && !c.IsDeleted && c.IsActive);

                if (oldCompany == null)
                    return NotFound();

                oldCompany.FullName = company.FullName;
                oldCompany.JuridicalAddressId = company.JuridicalAddressId;
                oldCompany.LastUpdateByUserId = userId;
                oldCompany.LastUpdDate = DateTime.Now;
                oldCompany.MainDeliveryAddressId = company.DeliveryAddressId;
                oldCompany.Name = company.Name;
                oldCompany.PostAddressId = company.JuridicalAddressId;
                oldCompany.IsDeleted = false;
                oldCompany.IsActive = true;
                oldCompany.CityId = company.CityId;
                if (oldCompany.Addresses != null)
                {
                    oldCompany.Addresses.ToList().ForEach(c =>
                    {
                        var address = company.Addresses.FirstOrDefault(d => d.CompanyAddressId == c.Id);
                        if (address != null)
                        {
                            c.IsActive = address.IsActive;
                            c.LastUpdateByUserId = userId;
                            c.LastUpdDate = DateTime.Now;
                        }
                    });
                }
                context.SaveChanges();

                return Ok(oldCompany.GetContract());
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("addresses/{id:long}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAddressCompany(long id)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var addressCompany =
                    context.CompanyAddresses.Include(c => c.Address).FirstOrDefault(
                        c => !c.IsDeleted && c.Id == id);

                if (addressCompany == null)
                {
                    return BadRequest("Адрес не найден");
                }

                return Ok(addressCompany.GetAddressCompanyModel());

            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{companyId:long}/curators")]
        public IActionResult GetCompanyCurators(long companyId)
        {
            var context = Accessor.Instance.GetContext();

            List<CompanyCurator> curators = context.CompanyCurators.Include(c => c.User)
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .ToList();
            return Ok(curators.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{companyId:long}/curators/{userId:long}")]
        public IActionResult GetCompanyCurator(long companyId, long userId)
        {
            var context = Accessor.Instance.GetContext();

            var curator = context.CompanyCurators.Include(c => c.User)
                .FirstOrDefault(c => c.CompanyId == companyId && !c.IsDeleted && c.UserId == userId);

            if (curator == null)
                return NotFound();
            return Ok(curator.GetContract());
        }

        [Authorize(Roles = "Consolidator")]
        [HttpGet, Route("GetCuratedCompany")]
        public IActionResult GetCuratedCompany()
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                var company = Accessor.Instance.GetCurationCompany(currentUser.Id);

                return Ok(company.GetContract());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet, Route("GetMyCompanies")]
        public IActionResult GetMyCompanies()
        {
            try
            {
                var currentUser =
                    User.Identity.GetUserById();

                var companyList = new List<CompanyModel>();

                foreach (var companyItem in Accessor.Instance.GetCompanies(currentUser.Id))
                {
                    companyList.Add(companyItem.GetContract());
                }

                return Ok(companyList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("InactiveCompanies")]
        public IActionResult GetInactiveCompanies()
        {
            try
            {
                var listOfCompany = new List<CompanyModel>();

                var itemFromBase = Accessor.Instance.GetInactiveCompanies();

                foreach (var item in itemFromBase)
                {
                    listOfCompany.Add(item.GetContract());
                }

                return Ok(listOfCompany);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("curators/add")]
        public IActionResult PostCompanyCurator([FromBody]CompanyCuratorModel model)
        {
            try
            {
                if (Accessor.Instance.AddCompanyCurator(model.ConvertToEntityModel(), User.Identity.GetUserId()))
                    return Ok();
                else
                    return BadRequest("Не удалось добавить куратора в компанию.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new InternalServerError(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("{companyId:long}/curators/delete/{userId:long}")]
        public IActionResult DeleteCompanyCurator(long companyId, long userId)
        {
            var authorId = User.Identity.GetUserId();
            if (Accessor.Instance.DeleteCompanyCurator(companyId, userId, authorId))
                return Ok();
            else
                return BadRequest("Не удалось отвязать куратора от компании");
        }

        [HttpPut, Route("addresses")]
        [Authorize(Roles = "Admin")]
        public IActionResult UpdateAddressCompany([FromBody]DeliveryAddressModel address)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var addressCompany =
                    context.CompanyAddresses.Include(c => c.Address).FirstOrDefault(
                        c => !c.IsDeleted && c.Id == address.CompanyAddressId);

                if (addressCompany == null)
                {
                    return BadRequest("Адрес не найден");
                }
                var newAddress =
                    context.Addresses.FirstOrDefault(c => !c.IsDeleted && c.Id == addressCompany.AddressId);

                if (newAddress == null)
                {
                    return BadRequest("Адрес не найден");
                }

                newAddress.LastUpdateByUserId = User.Identity.GetUserId();
                newAddress.LastUpdDate = DateTime.Now;
                newAddress.AddressComment = address.AddressComment;
                newAddress.BuildingNumber = address.BuildingNumber;
                newAddress.CityName = address.CityName;
                newAddress.FlatNumber = address.FlatNumber;
                newAddress.HouseNumber = address.HouseNumber;
                newAddress.OfficeNumber = address.OfficeNumber;
                newAddress.PostalCode = address.PostalCode;
                newAddress.RawAddress = address.RawAddress;
                newAddress.StreetName = address.StreetName;
                addressCompany.IsActive = true;
                addressCompany.DisplayType = (EnumDisplayAddressType)address.DisplayType;
                addressCompany.IsDeleted = false;
                addressCompany.LastUpdDate = DateTime.Now;
                addressCompany.LastUpdateByUserId = User.Identity.GetUserId();

                context.SaveChanges();

                return Ok();

            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPost, Route("{id:long}/addresses")]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateAddressCompany(long id, [FromBody]DeliveryAddressModel address)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var company = context.Companies.FirstOrDefault(c => !c.IsDeleted && c.Id == id && c.IsActive);
                if (company == null)
                {
                    return BadRequest("Компания не найдена");
                }

                var addressM =
                    context.CompanyAddresses.FirstOrDefault(c =>
                        c.CompanyId == id
                        && !c.IsDeleted
                        && c.Address.CityName.Trim().ToUpper() == address.CityName.Trim().ToUpper()
                        && c.Address.StreetName.Trim().ToUpper() == address.StreetName.Trim().ToUpper()
                        && c.Address.HouseNumber.Trim().ToUpper() == address.HouseNumber.Trim().ToUpper()
                        && c.Address.OfficeNumber.Trim().ToUpper() == address.OfficeNumber.Trim().ToUpper()
                        && !c.Address.IsDeleted
                    );

                if (addressM != null)
                {
                    return BadRequest("Такой адрес уже существует");
                }

                Address newAddress = address.GetEntity();
                newAddress.CreationDate = DateTime.Now;
                newAddress.CreatorId = User.Identity.GetUserId();

                context.Addresses.Add(newAddress);

                var addressCompany = new CompanyAddress()
                {
                    AddressId = newAddress.Id,
                    Address = newAddress,
                    CompanyId = company.Id,
                    CreateDate = DateTime.Now,
                    CreatorId = User.Identity.GetUserId(),
                    IsDeleted = false,
                    IsActive = true,
                    DisplayType = (EnumDisplayAddressType)address.DisplayType
                };

                context.CompanyAddresses.Add(addressCompany);
                context.SaveChanges();

                return Ok(addressCompany.GetAddressCompanyModel());

            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpDelete, Route("addresses/{id:long}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteAddressCompany(long id)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var addressCompany =
                    context.CompanyAddresses.FirstOrDefault(
                        c => !c.IsDeleted && c.Id == id);

                if (addressCompany == null)
                {
                    return BadRequest("Адрес не найден");
                }

                var address = context.Addresses.FirstOrDefault(c => c.Id == addressCompany.AddressId);
                if (address != null)
                    address.IsDeleted = true;
                addressCompany.IsActive = false;
                addressCompany.IsDeleted = true;
                addressCompany.LastUpdDate = DateTime.Now;
                addressCompany.LastUpdateByUserId = User.Identity.GetUserId();

                context.SaveChanges();

                return Ok();

            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("AddUserCompanyLink/{companyId:long}/{userId:long}")]
        public IActionResult AddUserCompanyLink(Int64 companyId, Int64 userId)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var newUserCompanyLink = new UserInCompany()
                    {
                        StartDate = DateTime.Now,
                        EndDate = null,
                        CompanyId = companyId,
                        CreatedBy = currentUser.Id,
                        IsActive = true,
                        UserId = userId,
                        UserRoleId = 1,
                        CreateDate = DateTime.Now
                    };

                    if (CompanyServiceHelper.IsNewUserCompanyLinkAvailable(newUserCompanyLink))
                        return Ok(
                            Accessor
                            .Instance
                            .AddUserToCompanyInRole(newUserCompanyLink));
                    else
                        return Ok(false);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut, Route("EditUserCompanyLink/{companyId:long}/{oldCompanyId:long}/{userId:long}")]
        public IActionResult EditUserCompanyLink(Int64 companyId, Int64 oldCompanyId, Int64 userId)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                if (currentUser != null)
                {
                    return Ok(
                        Accessor
                            .Instance
                            .EditUserCompanyLink(companyId, oldCompanyId, userId, currentUser.Id));
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("RemoveUserCompanyLink/{companyId}/{userId}")]
        public IActionResult RemoveUserCompanyLink(Int64 companyId, Int64 userId)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var removeUserCompanyLink = new UserInCompany()
                    {
                        CompanyId = companyId,
                        UserId = userId,
                        LastUpdateBy = currentUser.Id
                    };

                    return Ok(Accessor.Instance
                        .RemoveUserCompanyLink(removeUserCompanyLink));
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Функция включения/отключения СМС-оповещений для компании
        /// </summary>
        /// <param name="id"></param>
        /// <param name="EnableSms"></param>
        /// <returns></returns>
        [HttpGet, Route("SetSmsNotify/{companyId:long}")]
        public async Task<IActionResult> SetSmsNotify(long companyId, bool enableSms)
        {
            try
            {
                var fc = Accessor.Instance.GetContext();
                var company = await fc.Companies.FirstOrDefaultAsync(c => c.Id == companyId && c.IsDeleted == false);
                if (company == null)
                    return NotFound();

                company.SmsNotify = enableSms;
                fc.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        /// <summary>
        /// Получить компании, которые привязаны к пользователю и у которых есть корп. заказы на дату
        /// </summary>
        [HttpGet]
        [Route("GetMyCompanyForOrder")]
        public async Task<IActionResult> GetMyCompanyForOrder(long? cafeId = null, DateTime? date = null)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name != "anonymous")
            {
                long userId = User.Identity.GetUserId();
                var result = await Accessor.Instance.GetMyCompanyForOrder(userId, cafeId, date);
                return Ok(new ResponseModel() { Result = result.Select(e => new CompanyModel() { Id = e.Id, Name = e.Name }) });
            }
            else
            {
                return Ok(new ResponseModel() { Message = "Пользователь не авторизирован", Status = 3 });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("SetUserCompany")]
        public IActionResult SetUserCompany(long userId, long companyId)
        {
            return Ok(GetAccessor().SetUserCompany(userId, companyId));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("GetUserCompanyId")]
        public IActionResult GetUserCompanyId(long userId)
        {
            return Ok(GetAccessor().GetUserCompanyId(userId));
        }
    }

    public static class CompaniesExtensions
    {
        public static CompanyModel GetContract(this Company company)
        {
            return (company == null)
                ? null
                : new CompanyModel
                {
                    Id = company.Id,
                    Name = company.Name,
                    FullName = company.FullName,
                    DeliveryAddressId = company.MainDeliveryAddressId,
                    JuridicalAddressId = company.JuridicalAddressId,
                    DeliveryAddress = company.MainDeliveryAddressId.HasValue
                        ? company.MainDeliveryAddress.GetContract()
                        : new DeliveryAddressModel(),
                    JuridicalAddress = company.JuridicalAddressId.HasValue
                        ? company.JuridicalAddress.GetContract()
                        : new DeliveryAddressModel(),
                    Addresses = company.Addresses?
                        .Where(c => !c.IsDeleted)
                        .Select(c => c.GetAddressCompanyModel()).Where(c => c != null).ToList(),
                    IsDeleted = company.IsDeleted,
                    IsActive = company.IsActive,
                    SmsNotify = company.SmsNotify,
                    CityId = company.CityId,
                    City = company.City.GetContract()
                };
        }

        public static CompanyCuratorModel GetContract(this CompanyCurator curator)
        {
            return curator == null
                ? null
                : new CompanyCuratorModel()
                {
                    CompanyId = curator.Id,
                    UserId = curator.UserId,
                    User = curator.User?.ToAdminDto()
                };
        }

        public static Company GetEntity(this CompanyModel company)
        {
            return (company == null)
                ? new Company()
                : new Company
                {
                    Id = company.Id,
                    Name = company.Name,
                    FullName = company.FullName,
                    MainDeliveryAddressId = company.DeliveryAddressId,
                    JuridicalAddressId = company.JuridicalAddressId,
                    SmsNotify = company.SmsNotify,
                    CityId = company.CityId
                };
        }
    }
}
