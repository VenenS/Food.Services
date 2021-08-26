using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace ITWebNet.Food.Controllers
{
    //TODO = тут надо цену которая была в заказе.
    //double result = 0.0;
    //foreach (var dish in orderdishes) result += dish.BasePrice;
    //return result;
    public class OrderData
    {
        public UserModel User { get; set; }
        public OrderModel Order { get; set; }
        public List<FoodDishData> OrderDishes { get; set; }
        public double TotalPrice { get; set; }
        public DeliveryAddressModel Delivery { get; set; }
        public string OrderStatusReport { get; set; }
    }
}