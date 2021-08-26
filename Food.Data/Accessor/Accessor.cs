using Food.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        private static volatile Accessor _instance = new Accessor();
        private static object _syncRoot = new Object();
        private static IFoodContext _testingContext;
        private static bool _testingMode = false;
        public static string ConnectionString { get; set; }
        public static ILoggerFactory LoggerFactory { get; set; }
        public Accessor() { }

        public static Accessor Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_syncRoot)
                {
                    if (_instance == null)
                        _instance = new Accessor();
                }

                return _instance;
            }
        }

        public static void SetTestingModeOn(IFoodContext context)
        {
            _testingContext = context;
            _testingMode = true;
        }

        public IFoodContext GetContext()
        {
            return _testingMode ? _testingContext : new FoodContext(ConnectionString, LoggerFactory);
        }

        private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 32 * 1024 * 1024 });

        private static IMemoryCache _cafeCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 8 * 1024 * 1024 });

        private static IMemoryCache _categoryCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 8 * 1024 * 1024 });

        private static IMemoryCache _dishCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 32 * 1024 * 1024 });

        private static IMemoryCache _scheduleCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 8 * 1024 * 1024 });

        private static void SetObjectInCache(IMemoryCache cache, object value, string key)
        {
#if false
            CacheItem cacheItem =
                    new CacheItem(
                        key,
                        value
                    );

            CacheItemPolicy cacheItemPolicy =
                new CacheItemPolicy()
                {
                    SlidingExpiration = new TimeSpan(15, 0, 0, 0)
                };

            cache.Set(cacheItem, cacheItemPolicy);
#endif
        }
    }

    //Компоратор для сравнения расписаний с признаком Exclude
    class ScheduleExcludeComparer : IEqualityComparer<DishInMenu>
    {
        public bool Equals(DishInMenu x, DishInMenu y)
        {
            if (x.DishId == y.DishId
                && (
                        x.Type == "E"
                        || y.Type == "E"
                   ))
                return true;
            else
                return false;
        }

        public int GetHashCode(DishInMenu obj)
        {
            return obj.DishId.GetHashCode();
        }
    }

    enum ObjectTypesEnum
    {
        /// <summary>
        /// Тип объекта - кафе
        /// </summary>
        Cafe = 1,
        /// <summary>
        /// Тип объекта - блюдо
        /// </summary>
        Category = 2,
        /// <summary>
        /// Тип объекта - Категория
        /// </summary>
        Dish = 3,
        /// <summary>
        /// Тип объекта - что-то непонятное
        /// </summary>
        AnotherType = 0,
    }

    /// <summary>
    /// Типы уведомлений
    /// </summary>
    public enum NotificationTypeEnum
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Default = 0,
        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        UserRegistration = 1,
        /// <summary>
        /// Создание нового заказа
        /// </summary>
        OrderCreate = 2,
        /// <summary>
        /// Отмена заказа
        /// </summary>
        OrderAbort = 3
    }

    /// <summary>
    /// Типы каналов уведомлений
    /// </summary>
    public enum NotificationChannelEnum
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Default = 0,
        /// <summary>
        /// Email
        /// </summary>
        Email = 1,
        /// <summary>
        /// SMS
        /// </summary>
        Sms = 2,
        /// <summary>
        /// Автоматический звонок
        /// </summary>
        PhoneAuto = 3,
        /// <summary>
        /// Звонок от оператора
        /// </summary>
        PhoneOperator = 4
    }

    /// <summary>
    /// Тип баллов
    /// </summary>
    public enum TypePoints
    {
        /// <summary>
        /// Использую для списывания
        /// </summary>
        Default = 0,
        /// <summary>
        /// Баллы от заказов и акций
        /// </summary>
        Personally = 1,
        /// <summary>
        /// От реферальной программы
        /// </summary>
        Referral = 2
    }
}
