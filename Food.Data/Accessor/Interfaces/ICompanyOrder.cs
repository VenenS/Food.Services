using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using ITWebNet.FoodService.Food.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICompanyOrder
    {
        /// <summary>
        /// Воазвращает список заказов, осуществляемых компанией
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        List<CompanyOrder> GetCompanyOrders(long companyId);

        /// <summary>
        /// Воазвращает список заказов, осуществляемых компанией, на указанную дату
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="time">дата</param>
        /// <returns></returns>
        List<CompanyOrder> GetAvailableCompanyOrdersForTime(long companyId, DateTime? time);

        /// <summary>
        /// Возвращает список компанейских заказов связанных с данной компанией на
        /// сегодня и на будущее.
        /// </summary>
        /// <param name="companyId">Идентификатор компании</param>
        List<CompanyOrder> GetCompanyOrdersAvailableToEmployees(long companyId);

        /// <summary>
        /// TODO: проверить работает ли планировщик
        /// Воазвращает список всех компанейских заказов от указанной даты
        /// </summary>
        List<CompanyOrder> GetAllCompanyOrdersGreaterDate(DateTime? date);

        /// <summary>
        /// Возвращает заказ, осуществляемый компанией, по идентификатору
        /// </summary>
        /// <param name="companyOrderId">идентификатор компании</param>
        /// <returns></returns>
        CompanyOrder GetCompanyOrderById(long companyOrderId);

        /// <summary>
        /// Возвращает список индивидуальных заказов из заказа компании
        /// </summary>
        /// <param name="companyOrderId">идентификатор заказа компании</param>
        /// <returns></returns>
        List<Order> GetUserOrderFromCompanyOrder(long companyOrderId);

        /// <summary>
        /// Возвращает список компанейских заказов для кафе на указанный период
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">дата начала</param>
        /// <param name="endDate">дата окончания</param>
        /// <returns></returns>
        List<CompanyOrder> GetCompanyOrdersByDate(long companyId, long? cafeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Возвращает список компанейских заказов для кафе за день
        /// </summary>
        /// <param name="date">дата начала</param>
        /// <returns></returns>
        List<CompanyOrder> GetCompanyOrdersByDate(DateTime date);

        /// <summary>
        /// Возвращает список компании, осуществляющие компанейский заказ, для выбранного кафе на указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">дата начала</param>
        /// <param name="endDate">дата окончания</param>
        /// <returns></returns>
        List<Company> GetCompaniesByCompanyOrders(long cafeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Возвращает список компанейских заказов по фильтру
        /// </summary>
        /// <returns></returns>
        List<CompanyOrder> GetCompanyOrdersByFilter(ReportFilter filter);

        /// <summary>
        /// Добавление нового заказа компании.
        /// </summary>
        /// <param name="companyOrder">Заказ компании.</param>
        /// <returns>Id заказа, если добавление успешно. Иначе - -1.</returns>
        long AddCompanyOrder(CompanyOrder companyOrder);

        CompanyOrderStateTransition AddCompanyOrderStatusTransition(long orderId, long newStatus, long userId);

        /// <summary>
        /// Меняет статус компанейского заказа
        /// </summary>
        /// <param name="orderId">идентификтаор заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        bool SetCompanyOrderStatus(long orderId, long orderStatus, long userId);

        /// <summary>
        /// Изменение заказа компании.
        /// </summary>
        /// <param name="companyOrder">Заказ компании.</param>
        /// <returns>Id заказа, если изменение успешно, иначе - -1.</returns>
        long EditCompanyOrder(CompanyOrder companyOrder);

        /// <summary>
        /// Удаление заказа компании.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор заказа.</param>
        /// <param name="userId">Идентификатор администратора.</param>
        /// <returns>true - если удаление успешно, иначе - false.</returns>
        bool RemoveCompanyOrder(long companyOrderId, long userId);

        /// <summary>
        /// Получить корпоративные заказы для пользователя по списку кафе на дату
        /// </summary>
        List<CompanyOrder> GetCompanyOrderForUserByCompanyId(HashSet<long> cafeIds, long companyId, HashSet<DateTime> dateTimes = null);

        IEnumerable<Tuple<string, int, double>>
        CalculateShippingCostsForCompanyOrder(long companyOrderId, double extraCash = 0);

        Tuple<string, int, double>
        CalculateShippingCostsToSingleCompanyAddress(string companyAddress, long companyOrderId, double extraCash = 0);

        /// <summary>
        ///   Создает компанейские заказы от лица каждой компании (или всех компаний) согласно
        ///   расписаниям куратора для указанного кафе (или всех связанных с компанией кафе).
        /// 
        ///   Если в кафе включены заказы на неделю, будет создано 7 корпоративных заказов где
        ///   на каждый день недели приходится по заказу.
        /// </summary>
        /// 
        /// <param name="cafeId">
        ///   Кафе для которого необходимо создать заказы (если null, все кафе, связанные с
        ///   хотя бы одной компанией).
        /// </param>
        /// <param name="companyId">
        ///   Компания от лица которой необходимо создать заказы (если null, заказы создаются
        ///   для всех компаний согласно их расписаниям).
        /// </param>
        void CreateCompanyOrders(long? cafeId, long? companyId);

        /// <summary>
        ///   Отменяет компанейские заказы попадающие в предоставленный временной промежуток. Только заказы со
        ///   статусом `EnumOrderState.Created` будут отменены.
        /// </summary>
        /// <param name="start">Начальная дата</param>
        /// <param name="end">Конечная дата (включительно)</param>
        /// <param name="companyId">
        ///   Идентификатор компании, заказы которой следует отменить (в случае null, заказы все
        ///   компаний будут подходящих по остальным критериям будут отменены).
        /// </param>
        /// <param name="cafeId">
        ///   Заказы предназначенные для данного кафе следует отменить (если null, заказы всем
        ///   кафе будут рассматироваться).
        /// </param>
        /// <param name="userId">Идентификатор пользователя производящего изменения.</param>
        IEnumerable<Order> CancelCompanyOrdersBetween(DateTime start, DateTime end, long? companyId, long? cafeId, long userId);
    }
}
