using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace Food.Data.Entities
{
    /// <summary>
    /// Связь Категории блюда с Кафе
    /// </summary>
    [Table("cafe_category_link")]
    [DebuggerDisplay("{Cafe?.CafeName} {CafeId} - {DishCategory?.CategoryName} {DishCategoryId}")]
    public class DishCategoryInCafe : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }

        public virtual Cafe Cafe { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("category_id")]
        public long DishCategoryId { get; set; }

        public virtual DishCategory DishCategory { get; set; }

        [Column("category_index")]
        public int? Index { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        #region OrderHours

        private string _orderHours { get; set; }

        [NotMapped]
        private BusinessHours _businessHours;

        [Column("order_hours")]
        public string OrderHours
        {
            get => _orderHours;
            set
            {
                _businessHours = null;
                _orderHours = value;
            }
        }

        /// <summary>
        /// Расписание активности категории. Если на определенный день список часов пуст или
        /// null, считается что категория активна весь день.
        /// </summary>
        [NotMapped]
        public BusinessHours WorkingHours
        {
            get
            {
                if (_businessHours == null)
                    _businessHours = CafeExtensions.GetBusinessHours(OrderHours);
                return _businessHours;
            }
        }

        [NotMapped]
        public TimeSpan? WorkingTimeFrom
        {
            get
            {
                return CafeExtensions.GetWorkingTime(
                    CafeExtensions.GetBusinessHours(OrderHours), true);
            }
        }

        [NotMapped]
        public TimeSpan? WorkingTimeTo
        {
            get
            {
                return CafeExtensions.GetWorkingTime(
                    CafeExtensions.GetBusinessHours(OrderHours));
            }
        }

        /// <summary>
        /// Выходной
        /// </summary>
        [NotMapped]
        public bool IsRest
        {
            get
            {
                return CafeExtensions.IsRest(CafeExtensions.GetBusinessHours(OrderHours));
            }
        }

        /// <summary>
        /// Возвращает возможно ли производить заказы из данной категории на указанную
        /// дату.
        /// </summary>
        public bool IsDateWithinOrderHoursRange(DateTime when)
        {
            if (WorkingHours == null)
                return true;

            List<BusinessHoursItem> hours;

            var departure = WorkingHours.Departures?.FirstOrDefault(x => x.Date.Date == when);
            if (departure != null)
            {
                if (departure.IsDayOff)
                    return false;

                hours = departure.Items;
            }
            else
            {
                hours = WorkingHours.GetBusinessHours(when);
            }

            var whenTod = when.TimeOfDay;
            return hours == null || !hours.Any() ||
                hours.Any(x => x.OpeningTime.TimeOfDay <= whenTod && whenTod <= x.ClosingTime.TimeOfDay);
        }

        #endregion
    }
}
