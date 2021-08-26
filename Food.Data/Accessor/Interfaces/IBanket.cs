using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IBanket
    {
        /// <summary>
        /// Добавить новую сущность банкета в БД.
        /// </summary>
        /// <param name="b">Сущность банкета</param>
        /// <returns>ID новой сущности или -1, в случае ошибки.</returns>
        long AddBanket(Banket b);

        /// <summary>
        /// Редактировать сущействующий банкет.
        /// </summary>
        /// <param name="b">Сущность содержащая изменения</param>
        bool EditBanket(Banket b);

        /// <summary>
        /// Удалить банкет и все связанные с банкетом заказы.
        /// </summary>
        /// <param name="id">Идентификатор банкета</param>
        /// <returns>true, если банкет был найден и успешно удален. false в противном случае.</returns>
        bool DeleteBanket(long id);

        /// <summary>
        /// Возвращает все сущности банкетов.
        /// </summary>
        List<Banket> GetAllBankets();

        Banket GetBanketByEventDate(DateTime date);

        /// <summary>
        /// Возвращает список заказов в банкете.
        /// </summary>
        /// <param name="banketId">Идентификатор банкета</param>
        /// <param name="includeStatuses">
        ///   Фильтр по статусу заказа. Если null, фильтрация по статусу не производится
        /// </param>
        List<Order> ListOrdersInBanket(long banketId, IEnumerable<EnumOrderState> includeStatuses = null);

        /// <summary>
        /// Возвращает список заказов связанных с одним или несколькими банкетами.
        /// </summary>
        /// <param name="banketIds">Список ID банкетов заказы которых нужно вернуть</param>
        /// <param name="includeStatuses">Фильтр по статусу заказа</param>
        List<Order> ListOrdersInBankets(IEnumerable<long> banketIds, IEnumerable<EnumOrderState> includeStatuses = null);

        /// <summary>
        /// Фильтрует банкеты согласно указанному фильтру.
        /// </summary>
        List<Banket> GetBanketsByFilter(BanketFilter filter);

        Order GetOrderInBanketByUserId(long banketId, long userId);

        /// <summary>
        /// Возвращает банкет по идентификатору.
        /// </summary>
        Banket GetBanketById(long id);

        /// <summary>
        /// Пересчитывает общую сумму банкета.
        /// </summary>
        void RecalculateBanketTotalSum(Banket banket0);

        /// <summary>
        /// Пересчитывает общую сумму заказа.
        /// </summary>
        /// <param name="order"></param>
        void RecalculateOrderTotalSum(Order order);
    }
}
