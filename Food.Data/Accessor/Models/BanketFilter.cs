using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.Data.Accessor.Models
{
    public enum BanketFilterSortType
    {
        OrderByDate,
        OrderByPrice,
        OrderByOrderNumber
    }

    public class BanketFilter
    {
        public List<long> BanketIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? CafeId { get; set; }

        /// <summary>
        /// Тип сортировки
        /// </summary>
        public BanketFilterSortType SortType { get; set; }
    }
}
