using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Controllers
{
    public class FoodDishData
    {
        public FoodDishModel Dish { get; set; }
        public int ItemCount { get; set; }
        public double ItemTotalPrice { get; set; }
        public int? ItemDiscount { get; set; }
        public string CategoryName { get; set; }
    }
}