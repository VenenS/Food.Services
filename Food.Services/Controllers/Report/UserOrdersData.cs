using System.Collections.Generic;

namespace ITWebNet.Food.Controllers
{
    public class UserOrdersData
    {
        public List<OrderData> Orders { get; set; }
        public double TotalPrice { get; set; }
        public long Id { get; set; }
    }
}