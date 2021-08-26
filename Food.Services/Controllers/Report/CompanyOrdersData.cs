using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace ITWebNet.Food.Controllers
{
    public class CompanyOrdersData
    {
        public CompanyModel Company { get; set; }
        public string CompanyAddress { get; set; }
        public CompanyOrderModel CompanyOrder { get; set; }
        public List<UserOrdersData> Orders { get; set; }
        public double TotalPrice { get; set; }
        public string OrderStatusReport { get; set; }
    }
}