using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;

namespace ITWebNet.Food.Controllers
{
    [Serializable]
    public class ReportData
    {
        public List<CompanyOrdersData> OrdersData { get; set; }
        public List<UserOrdersData> IndividualOrdersData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CafeModel Cafe { get; set; }
        public double? TotalSumm { get; set; }
    }
}