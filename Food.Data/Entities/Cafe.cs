using Food.Data.Enums;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
// ReSharper disable ValueParameterNotUsed

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет кафе.
    /// </summary>
    [Table("cafe")]
    public class Cafe : EntityBase<long>
    {
        [Column("address")]
        public string Address { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [StringLength(4096)]
        [Column("cafe_description")]
        public string Description { get; set; }

        [Column("average_delivery_time")]
        public int? AverageDelivertyTime { get; set; }

        [Column("business_hours")]
        public string BusinessHours { get; set; }

        [Column("cafe_user_type")]
        public string CafeAvailableOrdersType { get; set; }

        [StringLength(256)]
        [Column("cafe_full_name")]
        public string CafeFullName { get; set; }

        [StringLength(50)]
        [Column("cafe_name")]
        public string CafeName { get; set; }

        [Column("cafe_rating_count")]
        public long CafeRatingCount { get; set; }

        [Column("cafe_rating_sum")]
        public long CafeRatingSumm { get; set; }

        [StringLength(256)]
        [Column("cafe_short_description")]
        public string CafeShortDescription { get; set; }

        [Column("clean_url_name")]
        public string CleanUrlName { get; set; }

        [StringLength(1024)]
        [Column("delivery_comment")]
        public string DeliveryComment { get; set; }

        [Column("delivery_price_rub")]
        public double? DeliveryPriceRub { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("minimum_order_rub")]
        public double? MinimumSumRub { get; set; }

        //[Column("online_payment_sign")]
        //public bool? OnlinePaymentSign { get; set; }

        [Column("cafe_specialization_id")]
        public long? SpecializationId { get; set; }

        [Column("guid")]
        public Guid Uuid { get; set; }
        /// <summary>
        /// Возвращает или задает идентификатор города.
        /// </summary>
        [Column("city_id")]
        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        /// <summary>
        /// Логотип кафе - маленькая картинка макс. 48х48 (выводится 32х32) в Base64
        /// </summary>
        [Column("logo")]
        public string Logo { get; set; }

        /// <summary>
        /// Рабочие часы на сегодня и на следующие 6 дней. Если на определенный день
        /// список рабочих часов пустой - это выходной или праздник.
        /// </summary>
        [NotMapped]
        public List<BusinessHoursItem>[] WorkingHours
        {
            get
            {
                var normalHours = CafeExtensions.GetBusinessHours(BusinessHours);
                var hoursArray = new List<BusinessHoursItem>[7] {
                    normalHours.Sunday,
                    normalHours.Monday,
                    normalHours.Tuesday,
                    normalHours.Wednesday,
                    normalHours.Thursday,
                    normalHours.Friday,
                    normalHours.Saturday
                }.Select(x => x ?? new List<BusinessHoursItem> { }).ToList();
                var workingHours = new List<BusinessHoursItem>[7];
                var now = DateTime.Now.Date;

                for (var i = 0; i < workingHours.Length; i++)
                {
                    var currentDate = now.AddDays(i).Date;
                    var alteredSched = normalHours?.Departures?.FirstOrDefault(x => x?.Date.Date == currentDate);
                    workingHours[i] = alteredSched == null
                        ? hoursArray[(int)currentDate.DayOfWeek]
                        : (!alteredSched.IsDayOff ? (alteredSched.Items ?? new List<BusinessHoursItem> { }) : new List<BusinessHoursItem> { });
                }

                return workingHours;
            }
        }

        [NotMapped]
        public TimeSpan? WorkingTimeFrom
        {
            get
            {
                return CafeExtensions.GetWorkingTime(
                    CafeExtensions.GetBusinessHours(BusinessHours), true);
            }
            set { }
        }

        [NotMapped]
        public TimeSpan? WorkingTimeTo
        {
            get
            {
                return CafeExtensions.GetWorkingTime(
                    CafeExtensions.GetBusinessHours(BusinessHours));
            }
            set { }
        }

        /// <summary>
        /// Выходной
        /// </summary>
        [NotMapped]
        public bool IsRest
        {
            get
            {
                return CafeExtensions.IsRest(
              CafeExtensions.GetBusinessHours(BusinessHours));
            }
            set { }
        }

        [StringLength(30)]
        [Column("working_week_days")]
        public string WorkingWeekDays { get; set; }

        [Column("allow_payment_by_points")]
        public bool AllowPaymentByPoints { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateBy { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdateDate { get; set; }

        [Column("cafe_small_image")]
        public string SmallImage { get; set; }

        [Column("cafe_big_image")]
        public string BigImage { get; set; }

        [Column("week_menu_is_active")]
        public bool WeekMenuIsActive { get; set; }

        [Column("deferred_order")]
        public bool DeferredOrder { get; set; }

        [Column("daily_corp_order_sum")]
        public double? DailyCorpOrderSum { get; set; }

        [Column("order_abort_time")]
        public TimeSpan? OrderAbortTime { get; set; }

        [Column("payment_method", TypeName = "smallint")]
        public EnumPaymentType PaymentMethod { get; set; }

        [Column("delivery_regions")]
        public string DeliveryRegions { get; set; }

        public virtual ICollection<KitchenInCafe> KitchenInCafes { get; set; }
        public virtual ICollection<DishCategoryInCafe> DishCategoriesInCafe { get; set; }
        public virtual ICollection<CompanyOrderSchedule> CompanyOrderSchedules { get; set; }
    }
}
