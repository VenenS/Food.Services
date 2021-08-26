using Food.Services.Contracts;
using Food.Services.GenerateXLSX.Model;

namespace Food.Services.Extensions
{
    public static class ReportUserOrdersExtensions
    {
        public static ReportUserOrdersModel GetContract(this ReportUserOrders entity)
        {
            return new ReportUserOrdersModel()
            {
                EndDate = entity.EndDate,
                Orders = entity.Orders,
                StartDate = entity.StartDate,
                TotalSumm = entity.TotalSumm,
                User = entity.User
            };
        }
    }
}