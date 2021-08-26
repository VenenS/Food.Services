using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class CityExtensions
    {
        public static CityModel GetContract(this City city)
        {
            return city == null
                ? null
                : new CityModel
                {
                    Id = city.Id,
                    Name = city.Name,
                    Region = city.Subject.GetContract()
                };
        }

        public static City GetEntity(this CityModel city)
        {
            return city == null
                ? new City()
                : new City
                {
                    Id = city.Id,
                    Name = city.Name
                };
        }
    }

    public static class SubjectExtensions
    {
        public static RegionModel GetContract(this Subject subject)
        {
            return subject == null
                ? null
                : new RegionModel
                {
                    Id = subject.Id,
                    Name = subject.Name
                };
        }

        public static Subject GetEntity(this RegionModel region)
        {
            return region == null
                ? new Subject()
                : new Subject
                {
                    Id = region.Id,
                    Name = region.Name
                };
        }
    }
}
