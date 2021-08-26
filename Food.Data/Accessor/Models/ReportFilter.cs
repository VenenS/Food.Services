using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.Data.Accessor.Models
{
    public class ReportFilter
    {
        public List<EnumOrderStatus> AvailableStatusList { get; set; }

        public long? CompanyId { get; set; }

        public long? CafeId { get; set; }

        public List<Int64> CompanyOrdersIdList { get; set; }

        public List<Int64> BanketOrdersIdList { get; set; }

        public DateTime StartDate { get; set; }

        public EnumSearchType SearchType { get; set; }

        public string Search { get; set; }

        public DateTime EndDate { get; set; }

        public bool LoadUserOrders { get; set; }

        public bool LoadOrderItems { get; set; }

        public List<Int64> OrdersIdList { get; set; }

        public Int64 ReportTypeId { get; set; }

        public EnumReportExtension ReportExtension { get; set; }

        public long? UserId { get; set; }

        public EnumReportSortType SortType { get; set; }

        public EnumReportResultOrder ResultOrder { get; set; }
    }
}
