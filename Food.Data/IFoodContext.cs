using System;
using Microsoft.EntityFrameworkCore;
using Food.Data.Entities;
using System.Threading.Tasks;

namespace Food.Data
{
    public interface IFoodContext : IDisposable
    {
        /// <summary>
        ///     Возвращает или задает коллекцию адресов.
        /// </summary>
        DbSet<Address> Addresses { get; set; }

        DbSet<Banket> Bankets { get; set; }
        DbSet<CafeManager> CafeManagers { get; set; }

        /// <summary>
        ///     Возвращает или задает шаблоны меню для кафе
        /// </summary>
        DbSet<CafeMenuPattern> CafeMenuPatterns { get; set; }

        DbSet<CafeMenuPatternDish> CafeMenuPatternsDishes { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию кафе.
        /// </summary>
        DbSet<Cafe> Cafes { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию компаний.
        /// </summary>
        DbSet<Company> Companies { get; set; }

        DbSet<CompanyAddress> CompanyAddresses { get; set; }
        DbSet<CompanyCurator> CompanyCurators { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию заказов компаний.
        /// </summary>
        DbSet<CompanyOrder> CompanyOrders { get; set; }

        DbSet<CompanyOrderSchedule> CompanyOrderSchedules { get; set; }
        DbSet<CompanyOrderStateTransition> CompanyOrderStateTransitions { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию категорий блюд.
        /// </summary>
        DbSet<DishCategory> DishCategories { get; set; }

        DbSet<DishCategoryInCafe> DishCategoriesInCafes { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию блюд.
        /// </summary>
        DbSet<Dish> Dishes { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию блюд в меню.
        /// </summary>
        DbSet<DishInMenu> DishesInMenus { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию истории изменения блюд в меню.
        /// </summary>
        DbSet<DishInMenuHistory> DishesInMenuHistory { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию кухонь.
        /// </summary>
        DbSet<Kitchen> Kitchens { get; set; }

        DbSet<KitchenInCafe> KitchensInCafes { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию уведомлений.
        /// </summary>
        DbSet<Notification> Notifications { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию позиций заказов.
        /// </summary>
        DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>
        ///     Возвращает или задает коллекцию заказов.
        /// </summary>
        DbSet<Order> Orders { get; set; }

        DbSet<OrderStateTransition> OrderStateTransitions { get; set; }
        DbSet<ReportStylesheet> ReportStylesheets { get; set; }
        DbSet<XsltToCafe> XsltToCafe { get; set; }
        /// <summary>
        ///     Возвращает или задает коллекцию ролей.
        /// </summary>
        DbSet<Role> Roles { get; set; }

        DbSet<Tag> Tags { get; set; }

        /// <summary>
        ///     возвращает или задает коллекцию пользователей.
        /// </summary>
        DbSet<User> Users { get; set; }
        DbSet<ObjectType> ObjectType { get; set; }
        DbSet<UserInCompany> UsersInCompanies { get; set; }
        DbSet<UserInRole> UsersInRoles { get; set; }
        DbSet<Rating> Rating { get; set; }
        DbSet<TagObject> TagObject { get; set; }
        DbSet<DishVersion> DishVersions { get; set; }
        DbSet<CompanyOrderStatus> OrderStatus { get; set; }
        DbSet<CafeDiscount> CafeDiscounts { get; set; }
        DbSet<OrderInfo> OrderInfo { get; set; }
        DbSet<UserReferral> UserReferral { get; set; }
        DbSet<ScheduledTask> ScheduledTask { get; set; }
        DbSet<ReferralCoefficient> ReferralCoef { get; set; }
        DbSet<RefreshToken> RefreshTokens { get; set; }
        DbSet<LogMessage> LogMessages { get; set; }
        DbSet<NotificationChannel> NotificationChannel { get; set; }
        DbSet<NotificationType> NotificationType { get; set; }
        DbSet<CafeNotificationContact> CafeNotificationContact { get; set; }
        DbSet<CafeOrderNotification> CafeOrderNotifications { get; set; }

        DbSet<Discount> Discounts { get; set; }
        DbSet<CostOfDelivery> CostOfDelivery { get; set; }
        DbSet<Ext> Ext { get; set; }
        DbSet<Image> Images { get; set; }
        DbSet<Client> Clients { get; set; }

        DbSet<SmsCode> SmsCodes { get; set; }

        DbSet<City> Cities { get; set; }

        DbSet<Subject> Subjects { get; set; }

        DbSet<UserPasswordResetKey> UserPasswordResetKeys { get; set; }

        DbSet<DishCategoryLink> DishCategoryLinks { get; set; }

        int SaveChanges();

        DbSet<T> Set<T>() where T : class;
    }
}
