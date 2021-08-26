using System.Collections.Generic;

namespace Food.Data.Accessor.Models.DishInMenuHistory
{
    public class PageDishInMenuHistoryCategory
    {
        public string CategoryName { get; set; }

        public List<PageDishInMenuHistory> PageDisthes { get; set; }
    }
}
