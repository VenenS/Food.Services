using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Models.Order
{
    public class OrderWithItemsAndListOfDishes
    {
        public OrderModel Order { get; set; }
        public List<FoodDishModel> Dishes { get; set; }
    }
}
