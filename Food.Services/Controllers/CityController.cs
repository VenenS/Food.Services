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
    [Route("api/cities")]
    public class CityController : ContextableApiController
    {
        public static string Name;
        public CityController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public CityController()
        {
        }
        [HttpGet, Route("")]
        public IActionResult Get(string searchString = null)
        {
            try
            {
                var cities = Accessor.Instance.GetCities(searchString);
                return Ok(cities.Select(c => c.GetContract()));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route(nameof(GetDefaultCity))]
        public IActionResult GetDefaultCity()
        {
            return Ok(Accessor.Instance.GetDefaultCity()?.GetContract());
        }

        [HttpGet, Route("GetCityById/{id}")]
        public IActionResult GetCityById(long id)
        {
            return Ok(Accessor.Instance.GetCityById(id)?.GetContract());
        }

        [HttpGet, Route("active")]
        public IActionResult GetActiveCities(string searchString = null)
        {
            try
            {
                var cities = Accessor.Instance.GetActiveCities(searchString);
                return Ok(cities.Select(c => c.GetContract()));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [Route("byname")]
        public IActionResult GetCityByName(string name, string region = null)
        {
            return Ok(Accessor.Instance.GetCityByName(name, region).GetContract());
        }

        [Route("activeforregion")]
        public IActionResult GetActiveCitiesForRegion(string searchString = null)
        {
            var cities = Accessor.Instance.GetActiveCities(searchString);
            var regions = new List<RegionModel>();
            var fc = Accessor.Instance.GetContext();
            foreach (var item in fc.Subjects)
            {
                if (cities.Any(c => c.SubjectId == item.Id))
                {
                    regions.Add(new RegionModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Cities = cities.Where(c => c.SubjectId == item.Id).Select(c => c.GetContract()).OrderBy(c => c.Name).ToList()
                    });
                }
            }
            return Ok(regions.OrderBy(s => s.Name).ToList());
        }
    }
}
