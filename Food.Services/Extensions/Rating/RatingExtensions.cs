using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.Rating
{
    public static class RatingExtensions
    {
        public static RatingModel GetContract(this Data.Entities.Rating rating)
        {
            return rating == null
                ? null
                : new RatingModel
                {
                    Id = rating.Id,
                    ObjectId = rating.ObjectId,
                    TypeOfObject = (ObjectTypesEnum)rating.ObjectType,
                    UserId = rating.UserId,
                    Value = rating.RatingValue
                };
        }

        public static Data.Entities.Rating GetEntity(this RatingModel rating)
        {
            return rating == null
                ? new Data.Entities.Rating()
                : new Data.Entities.Rating
                {
                    Id = rating.Id,
                    ObjectId = rating.ObjectId,
                    ObjectType = (int)rating.TypeOfObject,
                    UserId = rating.UserId,
                    RatingValue = rating.Value
                };
        }
    }
}
