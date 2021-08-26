using Food.Data;
using Food.Data.Entities;
using Food.Services.ExceptionHandling;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/addresses")]
    public class AddressController : ContextableApiController
    {
        public static string Name;
        public AddressController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public AddressController()
        {
        }

        [HttpPost, Route("")]
        [Authorize]
        public IActionResult Create([FromBody]DeliveryAddressModel model)
        {
            try
            {
                var addressM = Accessor.Instance.GetAddressByComponents(
                    cityName: model.CityName,
                    streetName: model.StreetName,
                    houseNumber: model.HouseNumber,
                    officeNumber: model.OfficeNumber
                );

                if (addressM != null)
                    return Ok(addressM.Id);

                var address = model.GetEntity();
                address.CreationDate = DateTime.Now;
                address.CreatorId = User.Identity.GetUserById().Id;

                var id = Accessor.Instance.AddAddress(address);

                return Ok(address.Id);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("")]
        public IActionResult Get()
        {
            try
            {
                var addresses = Accessor.Instance.GetAllAddresses();
                return Ok(addresses.Select(c => c.GetContract()));
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
                var address = Accessor.Instance.GetAddressById(id);
                if (address == null || address.IsDeleted)
                    return NotFound();

                return Ok(address.GetContract());
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpDelete, Route("{id:long}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(long id)
        {
            try
            {
                return Ok(Accessor.Instance.DeleteAddress(id, User.Identity.GetUserId()));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpPut, Route("")]
        public IActionResult Update([FromBody]DeliveryAddressModel address)
        {
            try
            {
                var newAddress = address.GetEntity();
                newAddress.Id = address.Id;
                var id = Accessor.Instance.UpdateAddress(newAddress, User.Identity.GetUserId());

                if (id != -1)
                {
                    return Ok(Accessor.Instance.GetAddressById(id).GetContract());
                }

                return Ok((DeliveryAddressModel)null);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("company/{id:long}")]
        public IActionResult GetCompanyAddresses(long id)
        {
            var company = Accessor.Instance.GetCompanyById(id);
            var companyAddresses = new List<DeliveryAddressModel>();
            if (company == null) return Ok(companyAddresses);
            var mainAddressCompany = company.MainDeliveryAddressId.HasValue
                ? Accessor.Instance.GetAddressById(company.MainDeliveryAddressId.Value)
                : null;
            var addresses = Accessor.Instance.GetCompanyAddresses(id);

            if (addresses.Count > 1 && mainAddressCompany != null)
            {
                var mainAdress = addresses.FirstOrDefault(a => a.AddressId == mainAddressCompany.Id);
                if (mainAdress != null)
                {
                    addresses.Remove(mainAdress);
                    addresses.Insert(0, mainAdress);
                }
            }

            foreach (var item in addresses)
            {
                var address = Accessor.Instance.GetAddressById(item.AddressId);
                companyAddresses.Add(address.GetContract());
            }

            return Ok(companyAddresses);
        }
    }   
}
