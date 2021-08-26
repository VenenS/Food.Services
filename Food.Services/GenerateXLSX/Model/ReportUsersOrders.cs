using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;

namespace Food.Services.GenerateXLSX.Model
{
    /// <summary>
    /// Заказы сотрудников за переиод
    /// </summary>
    public class ReportUsersOrders
    {
        public CompanyModel Company { get; set; }
        public double TotalSumm { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ReportUserOrders> Employee { get; set; }
    }
}