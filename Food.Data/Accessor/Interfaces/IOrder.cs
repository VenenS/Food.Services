using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IOrder
    {
        #region Order
        /// <summary>
        /// Возвращает заказ
        /// </summary>
        /// <param name="id">идентификатор заказа</param>
        /// <returns></returns>
        Order GetOrderById(long id);

        /// <summary>
        /// Возвращает все заказы пользователя на дату
        /// </summary>
        /// <param name="userId">ид пользователя</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<Order> GetUserOrders(long userId, DateTime? date, DateTime? startDeliverDate = null, DateTime? endDeliverDate = null);

        /// <summary>
        /// Возвращает заказы пользователя за указанный период
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        List<Order> GetUserOrders(long userId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Возвращает все блюда заказа пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="orderId">идентификатор заказа</param>
        /// <returns></returns>
        List<OrderItem> GetOrderItemsByOrderId(long? userId, long orderId);

        /// <summary>
        /// Отправка заказа
        /// </summary>
        /// <param name="order">заказ</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        int PostOrder(Order order, long userId);

        /// <summary>
        /// Изменить заказ
        /// </summary>
        /// <param name="order">заказ</param>
        /// <returns></returns>
        int ChangeOrder(Order order);

        /// <summary>
        /// Обновить информацию в заказе
        /// </summary>
        /// <param name="orderId">идентификатор заказа</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        int UpdateOrderInformation(long orderId, long userId);

        /// <summary>
        /// Возвращает список заказов кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="wantedDateTime">дата</param>
        /// <returns></returns>
        List<Order> GetCurrentListOfOrdersToCafe(long cafeId, DateTime? wantedDateTime);

        /// <summary>
        /// Возвращает список заказов кафе за указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        List<Order> GetCurrentListOfOrdersToCafe(long cafeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Меняет статус заказа
        /// </summary>
        /// <param name="orderId">идентификтаор заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        bool SetOrderStatus(long orderId, long orderStatus, long userId);

        /// <summary>
        /// Меняет статус у списка заказов
        /// </summary>
        /// <param name="orderIds">Список идентификаторов заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        List<Order> SetOrderStatus(HashSet<long> orderIds, long orderStatus, long userId);

        /// <summary>
        /// Проверяет есть ли блюдо в заказах за указанный период или дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="wantedDate">дата</param>
        /// <param name="beginDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        bool CheckExistingDishInOrders(long dishId, DateTime? wantedDate, DateTime? beginDate, DateTime? endDate);

        /// <summary>
        /// Возвращает список заказов пользователя с укзанным статусом на дату
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<Order> GetUserOrdersWithSameStatus(long userId, long orderStatus, DateTime? date);

        /// <summary>
        /// Удалить заказ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        int RemoveOrder(long orderId, long userId);

        /// <summary>
        /// Возвращает список всех заказов в компанейском заказе
        /// </summary>
        /// <param name="companyOrderId">идентификатор компанейского заказа</param>
        /// <param name="orderState"></param>
        /// <returns></returns>
        List<Order> GetOrdersByCompanyOrderId(long companyOrderId, List<EnumOrderStatus> orderState = null);

        /// <summary>
        /// Возвращает список заказов по фильтру
        /// </summary>
        /// <returns></returns>
        Task<List<Order>> GetOrdersByFilter(ReportFilter filter);

        /// <summary>
        /// Обновить сумму компанейского заказа
        /// </summary>
        /// <param name="companyOrderId">идентификатор компанейского заказа</param>
        /// <returns></returns>
        bool UpdateCompanyOrderSum(long companyOrderId);

        /// <summary>
        /// Возвращает список всех заказов за день
        /// </summary>
        /// <returns></returns>
        long GetTotalNumberOfOrdersPerDay();

        /// <summary>
        /// Возвращает список заказов пользователя за период сделанные в рамках компании
        /// </summary>
        List<Order> GetCompanyOrderByUserId(long userId, long companyId, DateTime startDate, DateTime endDate, List<EnumOrderStatus> availableStatusList);

        #endregion
    }
}
