using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;

namespace Food.Services.GenerateXLSX.Model
{
    /// <summary>
    /// Заказы сотрудника за переиод
    /// </summary>
    public class ReportUserOrders
    {
        public double TotalSumm { get; set; } = 0;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OrderModel> Orders { get; set; }
        public UserModel User { get; set; }
    }
}