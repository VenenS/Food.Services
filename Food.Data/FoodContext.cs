using Microsoft.EntityFrameworkCore;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Food.Data.Enums;

namespace Food.Data
{
    public class FoodContext : DbContext, IFoodContext
    {
        /// <summary>
        /// Возвращает или задает коллекцию адресов.
        /// </summary>
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Banket> Bankets { get; set; }
        public DbSet<CafeManager> CafeManagers { get; set; }

        /// <summary>
        /// Возвращает или задает шаблоны меню для кафе
        /// </summary>
        public DbSet<CafeMenuPattern> CafeMenuPatterns { get; set; }
        public DbSet<CafeMenuPatternDish> CafeMenuPatternsDishes { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию кафе.
        /// </summary>
        public DbSet<Cafe> Cafes { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию компаний.
        /// </summary>
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAddress> CompanyAddresses { get; set; }
        public DbSet<CompanyCurator> CompanyCurators { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию заказов компаний.
        /// </summary>
        public DbSet<CompanyOrder> CompanyOrders { get; set; }
        public DbSet<CompanyOrderSchedule> CompanyOrderSchedules { get; set; }
        public DbSet<CompanyOrderStateTransition> CompanyOrderStateTransitions { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию категорий блюд.
        /// </summary>
        public DbSet<DishCategory> DishCategories { get; set; }
        public DbSet<DishCategoryInCafe> DishCategoriesInCafes { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию блюд.
        /// </summary>
        public DbSet<Dish> Dishes { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию блюд в меню.
        /// </summary>
        public DbSet<DishInMenu> DishesInMenus { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию истории изменения блюд в меню.
        /// </summary>
        public DbSet<DishInMenuHistory> DishesInMenuHistory { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию кухонь.
        /// </summary>
        public DbSet<Kitchen> Kitchens { get; set; }
        public DbSet<KitchenInCafe> KitchensInCafes { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию уведомлений.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию позиций заказов.
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; }
        /// <summary>
        /// Возвращает или задает статусы корпоративных заказов
        /// </summary>
        public DbSet<CompanyOrderStatus> OrderStatus { get; set; }

        /// <summary>
        /// Возвращает или задает коллекцию заказов.
        /// </summary>
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderStateTransition> OrderStateTransitions { get; set; }
        public DbSet<ReportStylesheet> ReportStylesheets { get; set; }
        public DbSet<XsltToCafe> XsltToCafe { get; set; }
        /// <summary>
        /// Возвращает или задает коллекцию ролей.
        /// </summary>
        public DbSet<Role> Roles { get; set; }
        public DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// возвращает или задает коллекцию пользователей.
        /// </summary>
        public DbSet<User> Users { get; set; }
        public DbSet<UserInCompany> UsersInCompanies { get; set; }
        public DbSet<UserInRole> UsersInRoles { get; set; }

        //TODO: Авторизация через соцсети. Удалить
        //Нет Id, не подходит для тестов
        public DbSet<UserExternalLogin> UserExternalLogin { get; set; }

        /// <summary>
        /// Коды подтверждения, отправленные пользователям по СМС
        /// </summary>
        public DbSet<SmsCode> SmsCodes { get; set; }

        private readonly string _connectionString;
        private readonly ILoggerFactory _loggerFactory;

        public FoodContext() : this("FoodContext", null)
        {
        }

        public FoodContext(string nameOrConnectionString, ILoggerFactory loggerFactory)
        {
            _connectionString = nameOrConnectionString;
            _loggerFactory = loggerFactory;
        }

        public FoodContext(DbContextOptions<FoodContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_loggerFactory != null)
                    optionsBuilder.UseLoggerFactory(_loggerFactory);

                optionsBuilder.UseNpgsql(_connectionString);

                // Раскомментируйте чтобы при клиентском выполнении запроса вызывалось исключение.
                //optionsBuilder.ConfigureWarnings(builder => {
                //    builder.Throw(RelationalEventId.QueryClientEvaluationWarning);
                //});

                optionsBuilder
                    .UseLazyLoadingProxies()
                    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.DetachedLazyLoadingWarning));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
            //TODO: Авторизация через соцсети. Удалить
            //UserExternalLogin
            modelBuilder.Entity<UserExternalLogin>().HasKey(u => new { u.LoginProvider, u.ProviderKey, u.UserId });
            modelBuilder.Entity<UserExternalLogin>()
                .HasOne(ue => ue.User);

            //CompanyAddress
            modelBuilder.Entity<CompanyAddress>()
                .HasOne(ac => ac.Address)
                .WithMany(a => a.CompanyAddresses)
                .HasForeignKey(ac => ac.AddressId);
            modelBuilder.Entity<CompanyAddress>()
                .HasOne(ca => ca.Company)
                .WithMany(c => c.Addresses)
                .HasForeignKey(ca => ca.CompanyId);
            //UserInRole
            modelBuilder.Entity<UserInRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserInRoles)
                .HasForeignKey(r => r.RoleId);
            modelBuilder.Entity<UserInRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserInRoles)
                .HasForeignKey(ur => ur.UserId);
            //UserInCompany
            modelBuilder.Entity<UserInCompany>()
                .HasOne(uc => uc.Company)
                .WithMany(c => c.UserInCompanies)
                .HasForeignKey(uc => uc.CompanyId);
            modelBuilder.Entity<UserInCompany>()
                .HasOne(uc => uc.User)
                .WithMany(uc => uc.UserInCompanies)
                .HasForeignKey(uc => uc.UserId);
            modelBuilder.Entity<UserInCompany>()
                .HasOne(uc => uc.UserRole)
                .WithMany(r => r.UserInCompanies)
                .HasForeignKey(uc => uc.UserRoleId);
            modelBuilder.Entity<UserInCompany>()
                .HasOne(uc => uc.DefaultAddress)
                .WithMany(a => a.DefaultAddressInCompanies)
                .HasForeignKey(uc => uc.DefaultAddressId);
            // CompanyOrderSchedule
            modelBuilder.Entity<CompanyOrderSchedule>()
                .HasOne(x => x.Cafe)
                .WithMany(x => x.CompanyOrderSchedules)
                .HasForeignKey(x => x.CafeId);
            //CafeKitchenLink
            modelBuilder.Entity<KitchenInCafe>()
                .HasOne(ck => ck.Kitchen)
                .WithMany(k => k.KitchenInCafes)
                .HasForeignKey(ck => ck.KitchenId);
            modelBuilder.Entity<KitchenInCafe>()
                .HasOne(ck => ck.Cafe)
                .WithMany(ck => ck.KitchenInCafes)
                .HasForeignKey(ck => ck.CafeId);
            //CafeCategoryLink
            modelBuilder.Entity<DishCategoryInCafe>()
                .HasOne(dc => dc.DishCategory);
            modelBuilder.Entity<DishCategoryInCafe>()
                .HasOne(dc => dc.Cafe)
                .WithMany(c => c.DishCategoriesInCafe)
                .HasForeignKey(dc => dc.CafeId);
            //CafeNotificationContact
            modelBuilder.Entity<CafeNotificationContact>()
                .HasOne(cn => cn.NotificationChannel)
                .WithMany(nc => nc.CafeNotificationContacts)
                .HasForeignKey(nc => nc.NotificationChannelId);
            //Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.NotificationType)
                .WithMany(nt => nt.Notifications)
                .HasForeignKey(n => n.NotificationTypeId);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.NotificationChannel)
                .WithMany(nc => nc.Notifications)
                .HasForeignKey(n => n.NotificationChannelId);
            //Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Banket)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BanketId);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CompanyOrder)
                .WithMany(co => co.Orders)
                .HasForeignKey(o => o.CompanyOrderId);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.OrderInfo)
                .WithOne(o => o.Order);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(o => o.Orders)
                .HasForeignKey(o => o.UserId);
            //OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Order)
                .WithMany(o => o.OrderItems);
            modelBuilder.Entity<OrderItem>()
                .HasOne(o => o.Dish);
            //CafeMenuPatternDish
            modelBuilder.Entity<CafeMenuPatternDish>()
                .HasOne(cm => cm.Dish);
            modelBuilder.Entity<CafeMenuPatternDish>()
                .HasOne(cm => cm.Pattern)
                .WithMany(p => p.Dishes)
                .HasForeignKey(cm => cm.PatternId);
            //Dish
            //modelBuilder.Entity<Dish>()
            //    .HasOne(d => d.CafeCategory);
            //DishVersion
            modelBuilder.Entity<DishVersion>()
                .HasOne(dv => dv.Dish)
                .WithMany(d => d.Versions)
                .HasForeignKey(dv => dv.DishId);
            //DishInMenuHistory
            modelBuilder.Entity<DishInMenuHistory>()
                .HasOne(d => d.Dish);
            //User
            modelBuilder.Entity<User>()
                .HasOne(u => u.Address);
            //SmSCode
            modelBuilder.Entity<SmsCode>()
                .HasOne(s => s.User);
            //CompanyCurators
            modelBuilder.Entity<CompanyCurator>()
                .HasOne(cc => cc.User);
            modelBuilder.Entity<CompanyCurator>()
                .HasOne(cc => cc.Company);
            //CafeManager
            modelBuilder.Entity<CafeManager>()
                .HasOne(cc => cc.User);
            modelBuilder.Entity<CafeManager>()
                .HasOne(cc => cc.Cafe);
            //CompanyOrder
            modelBuilder.Entity<CompanyOrder>()
                .HasOne(co => co.Company);
            //CostOfDelivery
            modelBuilder.Entity<CostOfDelivery>()
                .HasOne(co => co.Cafe);
            //CafeDiscount
            modelBuilder.Entity<CafeDiscount>()
                .HasOne(co => co.Cafe);
            //CafeDiscount
            modelBuilder.Entity<CafeMenuPattern>()
                .HasOne(c => c.Cafe);
            //HsltToCafe
            modelBuilder.Entity<XsltToCafe>()
                .HasOne(x => x.Cafe);
            //HsltToCafe
            modelBuilder.Entity<CafeOrderNotification>()
                .HasOne(n => n.Cafe);
            modelBuilder.Entity<CafeOrderNotification>()
                .HasOne(n => n.User);
            //Discount
            modelBuilder.Entity<Discount>()
                .HasOne(d => d.User);
            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Company);
            modelBuilder.Entity<Discount>()
                .HasOne(d => d.Cafe);
            //Bankets
            modelBuilder.Entity<Banket>()
                .HasOne(d => d.Cafe);
            modelBuilder.Entity<Banket>()
                .HasOne(d => d.Company);
            modelBuilder.Entity<Banket>()
                .HasOne(d => d.Menu);
            //UserPasswordResetKey
            modelBuilder.Entity<UserPasswordResetKey>()
                .HasOne(r => r.User)
                .WithOne(u => u.UserPasswordResetKey);
            modelBuilder.Entity<Cafe>()
                .Property(c => c.PaymentMethod)
                .HasConversion(new ValueConverter<EnumPaymentType, short>(e => (short)e, value => (EnumPaymentType)value));
        }

        public DbSet<DishVersion> DishVersions { get; set; }
        public DbSet<TagObject> TagObject { get; set; }
        public DbSet<ObjectType> ObjectType { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<CostOfDelivery> CostOfDelivery { get; set; }
        public DbSet<Ext> Ext { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<CafeDiscount> CafeDiscounts { get; set; }
        public DbSet<OrderInfo> OrderInfo { get; set; }
        public DbSet<UserReferral> UserReferral { get; set; }
        public DbSet<ScheduledTask> ScheduledTask { get; set; }
        public DbSet<ReferralCoefficient> ReferralCoef { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }
        public DbSet<NotificationChannel> NotificationChannel { get; set; }
        public DbSet<NotificationType> NotificationType { get; set; }
        public DbSet<CafeNotificationContact> CafeNotificationContact { get; set; }
        public DbSet<CafeOrderNotification> CafeOrderNotifications { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<UserPasswordResetKey> UserPasswordResetKeys { get; set; }
        public DbSet<DishCategoryLink> DishCategoryLinks { get; set; }
    }
}
