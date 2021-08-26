using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region City
        /// <summary>
        /// Возвращает список городов
        /// </summary>
        /// <returns></returns>
        public List<City> GetCities(string searchString = null)
        {
            var fc = GetContext();

            var cities = fc.Cities.Include(c => c.Subject).AsNoTracking().Where(c => c.IsDeleted == false);

            if (searchString != null)
            {
                cities = cities.Where(c => c.Name.Contains(searchString));
            }

            return cities.ToList();
        }

        /// <summary>
        /// Возвращает город по имени
        /// </summary>
        /// <param name="name">имя города</param>
        /// <param name="region">имя субъекта</param>
        /// <returns></returns>
        public City GetCityByName(string name, string region = null)
        {
            var fc = GetContext();

            var city = GetActiveCities(name).FirstOrDefault(
                c => c.Name.Trim().ToLower() == name.Trim().ToLower() && c.IsDeleted == false
                && ((region != null && c.Subject.Name.Trim().ToLower() == region.Trim().ToLower()) || region == null));

            return city;
        }

        /// <summary>
        /// Возвращает город по умолчанию (Киров или первый активный)
        /// </summary>
        /// <param name="name">имя города</param>
        /// <returns></returns>
        public City GetDefaultCity()
        {
            var fc = GetContext();
            var activeCities = GetActiveCities();
            var city = activeCities.FirstOrDefault(
                c => c.Name.ToLower() == "киров"
                && c.Subject.Name.ToLower() == "кировская обл"
                && c.IsDeleted == false
                ) ?? activeCities.FirstOrDefault();

            return city;
        }

        public City GetCityById(long id)
        {
            var fc = GetContext();

            var city = fc.Cities.FirstOrDefault(c => c.Id == id);

            return city;
        }

        /// <summary>
        /// Возвращает список активных городов
        /// </summary>
        /// <returns></returns>
        public List<City> GetActiveCities(string searchString = null)
        {
            var fc = GetContext();

            var cities = fc.Cities.Include(c => c.Subject).AsNoTracking().Where(c => c.IsDeleted == false && c.Cafes != null && c.Cafes.Count > 0);

            if (searchString != null)
            {
                searchString = searchString.Trim().ToLower();
                cities = cities.Where(c => c.Name.Trim().ToLower().Contains(searchString));
            }

            var cafes = fc.Cafes.Where(c => c.CafeAvailableOrdersType == "COMPANY_PERSON" || c.CafeAvailableOrdersType == "PERSON_ONLY").AsNoTracking();

            var activeCities = new List<City>();
            foreach (var city in cities)
            {
                if (!cafes.Any(c => c.IsActive && !c.IsDeleted && c.CityId == city.Id)) continue;
                // Хотя бы в одном кафе активно меню на неделю
                if (cafes.Any(
                    c => c.IsActive
                    && !c.IsDeleted
                    && (c.WeekMenuIsActive || c.DeferredOrder)
                    && c.CityId == city.Id)
                    // Или в данный момент есть работающие кафе
                    || cafes.Where(
                        c => c.IsActive
                        && !c.IsDeleted
                        && c.CityId == city.Id).ToList()
                        .Any(
                        c => c.WorkingHours[0].Any(
                            b => b.OpeningTime <= DateTime.Now
                            && b.ClosingTime > DateTime.Now)))
                {
                    activeCities.Add(city);
                }
            }

            return activeCities;
        }
        #endregion
    }
}
