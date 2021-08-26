using System;
using Food.Data.Entities;

using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using ITWebNet.FoodService.Food.DbAccessor;

namespace Food.Services.Services
{
    public class MenuService : IMenuService
    {
        private readonly ICurrentUserService _currentUserService;

        public MenuService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public bool IsCategoryVisible(DishCategoryInCafe category, DateTime when)
        {
            if (SystemDishCategories.GetSystemCtegoriesIds().Contains(category.DishCategoryId))
                return false;

            if (_currentUserService.IsCafeManager(category.CafeId))
                return true;

            if (_currentUserService.IsCompanyEmployee())
            {
                return CanOrderFromCategory(category, when);
            }
            else
            {
                // Для физ. лиц фильтруем по периодам только на сегодня.
                // В др. дни категория всегда отображается.
                var now = DateTime.Now;
                return when.Date > now.Date || CanOrderFromCategory(category, now.Date, now.TimeOfDay);
            }
        }

        public bool CanOrderFromCategory(DishCategoryInCafe category, DateTime orderDay, TimeSpan? deliveryTime = null)
        {
            if (deliveryTime?.TotalHours < 0 || deliveryTime?.TotalHours >= 24)
                throw new ArgumentException($"{nameof(deliveryTime)} - bad time of day");

            if (SystemDishCategories.GetSystemCtegoriesIds().Contains(category.DishCategoryId))
                return false;

            if (_currentUserService.IsCafeManager(category.CafeId))
                return false;

            if (_currentUserService.IsCompanyEmployee())
            {
                var order = _currentUserService.GetUserCompanyOrder(category.CafeId, orderDay.Date);
                return order != null && category.IsDateWithinOrderHoursRange(order.DeliveryDate.Value);
            }
            else
            {
                if (deliveryTime != null)
                {
                    // Доставка в выбранное пользователем время.
                    return category.IsDateWithinOrderHoursRange(orderDay.Date + deliveryTime.Value);
                }
                else
                {
                    // Доставка "как можно скорее". Активность категории определяется по
                    // времени открытия кафе.
                    var cafe = category.Cafe ?? Accessor.Instance.GetCafeById(category.CafeId);
                    if (cafe == null)
                        return false;

                    var openingTime = cafe.WorkingTimeFrom;
                    if (openingTime == null)
                        return false;

                    return category.IsDateWithinOrderHoursRange(orderDay.Date + openingTime.Value);
                }
            }
        }
    }
}
