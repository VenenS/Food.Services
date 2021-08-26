using System.Collections.Generic;

namespace Food.Data.Accessor.Models.DishInMenuHistory
{
    public class PageDishInMenuHistory
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public double? Weight { get; set; }

        public decimal Description { get; set; }

        public List<Entities.DishInMenuHistory> DishesHistory { get; set; }
    }
}
