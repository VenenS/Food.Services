using ITWebNet.FoodService.Food.Data.Accessor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    //Конвертирование из Food.Services.Models в Food.Data
    public static class ModelsExtensions
    {
        public static CompanyCuratorModel ConvertToEntityModel(this ITWebNet.Food.Core.DataContracts.Common.CompanyCuratorModel model)
        {
            return model == null ?
                new CompanyCuratorModel() :
                new CompanyCuratorModel()
                {
                    CompanyId = model.CompanyId,
                    Id = model.Id,
                    UserId = model.UserId,
                };
        }
    }
}
