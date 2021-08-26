using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Food.Data;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Food.Services.Tests.Context
{
    public class FakeContext : IFoodContext
    {
        /// <summary>
        /// </summary>
        public FakeContext()
        {
            Addresses = new FakeDbSet<Address, long>();
            Bankets = new FakeDbSet<Banket, long>();
            CafeManagers = new FakeDbSet<CafeManager, long>();
            CafeMenuPatterns = new FakeDbSet<CafeMenuPattern, long>();
            CafeMenuPatternsDishes = new FakeDbSet<CafeMenuPatternDish, long>();
            Cafes = new FakeDbSet<Cafe, long>();
            CafeOrderNotifications = new FakeDbSet<CafeOrderNotification, long>();
            CafeDiscounts = new FakeDbSet<CafeDiscount, long>();
            Companies = new FakeDbSet<Company, long>();
            CompanyAddresses = new FakeDbSet<CompanyAddress, long>();
            CompanyCurators = new FakeDbSet<CompanyCurator, long>();
            CompanyOrders = new FakeDbSet<CompanyOrder, long>();
            CompanyOrderSchedules = new FakeDbSet<CompanyOrderSchedule, long>();
            CafeNotificationContact = new FakeDbSet<CafeNotificationContact, long>();
            CostOfDelivery = new FakeDbSet<CostOfDelivery, long>();
            CompanyOrderStateTransitions = new FakeDbSet<CompanyOrderStateTransition, long>();
            Discounts = new FakeDbSet<Discount, long>();
            DishVersions = new FakeDbSet<DishVersion, long>();
            DishCategories = new FakeDbSet<DishCategory, long>();
            DishCategoriesInCafes = new FakeDbSet<DishCategoryInCafe, long>();
            Dishes = new FakeDbSet<Dish, long>();
            DishesInMenus = new FakeDbSet<DishInMenu, long>();
            DishesInMenuHistory = new FakeDbSet<DishInMenuHistory, long>();
            Images = new FakeDbSet<Image, long>();
            Kitchens = new FakeDbSet<Kitchen, long>();
            KitchensInCafes = new FakeDbSet<KitchenInCafe, long>();
            LogMessages = new FakeDbSet<LogMessage, long>();
            Notifications = new FakeDbSet<Notification, long>();
            OrderItems = new FakeDbSet<OrderItem, long>();
            Orders = new FakeDbSet<Order, long>();
            OrderStateTransitions = new FakeDbSet<OrderStateTransition, long>();
            ReportStylesheets = new FakeDbSet<ReportStylesheet, long>();
            Roles = new FakeDbSet<Role, long>();
            Rating = new FakeDbSet<Rating, long>();
            ScheduledTask = new FakeDbSet<Data.Entities.ScheduledTask, long>();
            Tags = new FakeDbSet<Tag, long>();
            Users = new FakeDbSet<User, long>();
            UsersInCompanies = new FakeDbSet<UserInCompany, long>();
            UsersInRoles = new FakeDbSet<UserInRole, long>();
            TagObject = new FakeDbSet<TagObject, long>();
            ObjectType = new FakeDbSet<ObjectType, long>();
            OrderStatus = new FakeDbSet<CompanyOrderStatus, long>();
            NotificationChannel = new FakeDbSet<NotificationChannel, short>();
            //UserReferral = new FakeDbSet<UserReferral, long>();
            Ext = new FakeDbSet<Ext, long>();
            CafeOrderNotifications = new FakeDbSet<CafeOrderNotification, long>();
            XsltToCafe = new FakeDbSet<XsltToCafe, long>();
            OrderInfo = new FakeDbSet<OrderInfo, long>();
            SmsCodes = new FakeDbSet<SmsCode, long>();
            UserPasswordResetKeys = new FakeDbSet<UserPasswordResetKey, long>();
            Cities = new FakeDbSet<City, long>();
            Subjects = new FakeDbSet<Subject, int>();
        }

        public void Dispose()
        {
        }


        public DbSet<Address> Addresses { get; set; }
        public DbSet<Banket> Bankets { get; set; }
        public DbSet<CafeManager> CafeManagers { get; set; }
        public DbSet<CafeMenuPattern> CafeMenuPatterns { get; set; }
        public DbSet<CafeMenuPatternDish> CafeMenuPatternsDishes { get; set; }
        public DbSet<Cafe> Cafes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAddress> CompanyAddresses { get; set; }
        public DbSet<CompanyCurator> CompanyCurators { get; set; }
        public DbSet<CompanyOrder> CompanyOrders { get; set; }
        public DbSet<CompanyOrderSchedule> CompanyOrderSchedules { get; set; }
        public DbSet<CompanyOrderStateTransition> CompanyOrderStateTransitions { get; set; }
        public DbSet<DishCategory> DishCategories { get; set; }
        public DbSet<DishCategoryInCafe> DishCategoriesInCafes { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishInMenu> DishesInMenus { get; set; }
        public DbSet<DishInMenuHistory> DishesInMenuHistory { get; set; }

        public DbSet<Kitchen> Kitchens { get; set; }
        public DbSet<KitchenInCafe> KitchensInCafes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStateTransition> OrderStateTransitions { get; set; }
        public DbSet<ReportStylesheet> ReportStylesheets { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ObjectType> ObjectType { get; set; }
        public DbSet<UserInCompany> UsersInCompanies { get; set; }
        public DbSet<UserInRole> UsersInRoles { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<TagObject> TagObject { get; set; }
        public DbSet<DishVersion> DishVersions { get; set; }
        public DbSet<CompanyOrderStatus> OrderStatus { get; set; }
        public DbSet<CafeDiscount> CafeDiscounts { get; set; }
        public DbSet<OrderInfo> OrderInfo { get; set; }
        public DbSet<UserReferral> UserReferral { get; set; }
        public DbSet<Data.Entities.ScheduledTask> ScheduledTask { get; set; }
        public DbSet<ReferralCoefficient> ReferralCoef { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }
        public DbSet<NotificationChannel> NotificationChannel { get; set; }
        public DbSet<NotificationType> NotificationType { get; set; }
        public DbSet<CafeNotificationContact> CafeNotificationContact { get; set; }
        public DbSet<CafeOrderNotification> CafeOrderNotifications { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<CostOfDelivery> CostOfDelivery { get; set; }
        public DbSet<Ext> Ext { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<XsltToCafe> XsltToCafe { get; set; }
        public DbSet<SmsCode> SmsCodes { get; set; }
        public DbSet<UserPasswordResetKey> UserPasswordResetKeys { get; set; }
        public DbSet<DishCategoryLink> DishCategoryLinks { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        public int SaveChanges()
        {
            return 1;
        }

        public DbSet<T> Set<T>() where T : class
        {
            var tName = typeof(T).Name;
            var props = GetType().GetProperty(tName).GetValue(this, null);
            return props as DbSet<T>;
        }

        public async Task<int> SaveChangesAsync()
        {
            return 1;
        }

        public void ShowDeleted()
        {
        }

        public void SetModified(object entity)
        {
        }

        private DbSet<T> MakeSet<T, TId>(List<T> list) where T : EntityBase<TId>
        {
            var set = new FakeDbSet<T, TId>();
            foreach (var item in list)
                set.Add(item);
            return set;
        }
    }
}