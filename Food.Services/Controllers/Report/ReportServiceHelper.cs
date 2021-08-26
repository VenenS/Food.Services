using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Extensions;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.GenerateXLSX.Model;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Design;

namespace ITWebNet.Food.Controllers
{
    public class ReportServiceHelper
    {
        public static string GetCompanyReportsInXMlByFilter(ReportFilter filter, long currentUser, ReportSortType? sort = null)
        {
            string result;
            var reportData = GetCompanyReportData(filter,  currentUser, sort);
            //if (reportData.ordersData.Count > 0)
            //    || reportData.individualOrdersData.Count > 0)
            //{
            var processor =
                new XmlProcessing<ReportData>();
            result = processor.SerializeObject(reportData);
            //}

            return result;
        }


        /// <summary>
        /// Получение данных о заказах юзера
        /// </summary>
        /// <param name="usersOrders"></param>
        /// <returns></returns>
        public static UserOrdersData GetUserOrdersData(List<global::Food.Data.Entities.Order> usersOrders)
        {
            //TODO вернутся сюда и проверить нужны ли тут новые поля
            var userOrdersData = new UserOrdersData
            {
                Orders = new List<OrderData>()
            };

            foreach (var userOrder in usersOrders)
            {
                var orderItems = Accessor.Instance.GetOrderItemsByOrderId(userOrder.UserId, userOrder.Id);
                var orderDelivery = Accessor.Instance.GetAddressById(userOrder.DeliveryAddressId);
                
                var orderData = new OrderData
                {
                    Order = new OrderModel
                    {
                        Id = userOrder.Id,
                        Create = userOrder.CreationDate,
                        TotalSum = userOrder.TotalPrice,
                        DeliverDate = userOrder.DeliverDate,
                        CafeId = userOrder.CafeId,
                        PhoneNumber = userOrder.PhoneNumber,
                        Status = (long)userOrder.State,
                        OrderInfo = new OrderInfoModel { OrderAddress = userOrder.OrderInfo?.OrderAddress },
                    },

                    Delivery =
                    Accessor.Instance.GetAddressById(userOrder.DeliveryAddressId ?? -1).GetContract(),
                    OrderDishes = new List<FoodDishData>()
                };
                var orderUserName = userOrder.User.FullName;
                if (orderUserName == null || orderUserName == "")
                {
                    orderUserName = userOrder.User.Name;
                }
                orderData.User = new UserModel
                {
                    UserFullName = orderUserName,
                    Id = userOrder.UserId,
                    Email = userOrder.User.Email
                };

                foreach (var orderItem in orderItems)
                {
                    orderData.TotalPrice += orderItem.TotalPrice;
                    var dish = new FoodDishModel
                    {
                        Id = orderItem.Id,
                        Name = orderItem.DishName,
                        BasePrice = orderItem.DishBasePrice,
                        Weight = orderItem.DishWeight,
                        WeightDescription = orderItem.Dish.WeightDescription
                    };
                    
                    var dishId = orderItem.Dish.Id;
                    var dishInfo = Accessor.Instance.GetFoodDishById(dishId);
                    var category = dishInfo.DishCategoryLinks.First().CafeCategory.DishCategory.CategoryName;

                    orderData.OrderDishes.Add(
                        new FoodDishData
                        {
                            CategoryName = category ?? string.Empty,
                            Dish = dish,
                            ItemCount = orderItem.DishCount,
                            ItemDiscount = orderItem.DishDiscountPrc,
                            ItemTotalPrice = orderItem.TotalPrice
                        });
                }

                orderData.OrderStatusReport = GetDescription((EnumOrderStatus)orderData.Order.Status);
                userOrdersData.TotalPrice += orderData.TotalPrice;
                userOrdersData.Orders.Add(orderData);
            }

            return userOrdersData;
        }

        public static string GetDescription(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attribute = fi.GetCustomAttribute<DescriptionAttribute>();

            if (attribute != null)
                return attribute.Description;
            return value.ToString();
        }

        public static ReportData GetCompanyReportData(ReportFilter filter, long currentUser, ReportSortType? sort = null)
        {
            if (filter.CompanyId.HasValue &&
                Accessor.Instance.IsUserCuratorOfCafe(currentUser, filter.CompanyId.Value)
                || filter.CafeId.HasValue &&
                Accessor.Instance.IsUserManagerOfCafe(currentUser, filter.CafeId.Value))
            {
                var workingFilter = new FoodService.Food.Data.Accessor.Models.ReportFilter
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    CompanyOrdersIdList = filter.CompanyOrdersIdList
                                          ?? new List<long>(),
                    OrdersIdList = filter.OrdersIdList ?? new List<long>(),
                    BanketOrdersIdList = filter.BanketOrdersIdList ?? new List<long>(),
                    AvailableStatusList = filter.AvailableStatusList?.Select(p => (EnumOrderStatus)p).ToList() ??
                                          new List<EnumOrderStatus>(),
                    CafeId = filter.CafeId,
                    CompanyId = filter.CompanyId,
                    ReportTypeId = filter.ReportTypeId
                };

                if (workingFilter.CompanyOrdersIdList.Count > 0
                    || workingFilter.OrdersIdList.Count > 0)
                {
                    workingFilter.AvailableStatusList.Clear();
                    workingFilter.StartDate = DateTime.MinValue;
                    workingFilter.EndDate = DateTime.MinValue;
                }

                var reportData = new ReportData
                {
                    TotalSumm = 0,
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    OrdersData = new List<CompanyOrdersData>()
                };
                var cafe =
                    Accessor.Instance.GetCafeById(workingFilter.CafeId ?? -1);

                if (cafe != null)
                    reportData.Cafe = cafe.GetContract();
                //else
                //{
                //    return result;
                //}

                var companyOrders =
                    Accessor
                        .Instance
                        .GetCompanyOrdersByFilter(workingFilter);

                foreach (var companyOrder in companyOrders)
                {
                    var companyOrdersData = new CompanyOrdersData();

                    var company = companyOrder.Company;
                    if (company != null)
                        companyOrdersData.Company = company.GetContract();
                    companyOrdersData.CompanyOrder = companyOrder.GetContract();
                    companyOrdersData.OrderStatusReport =
                        GetDescription((EnumOrderStatus)companyOrdersData.CompanyOrder.OrderStatus);

                    companyOrdersData.Orders = new List<UserOrdersData>();

                    var usersOrders = Accessor.Instance.GetOrdersByCompanyOrderId(companyOrder.Id);

                    if (workingFilter.OrdersIdList.Count > 0)
                        usersOrders =
                            usersOrders
                                .Where(
                                    o =>
                                        workingFilter.OrdersIdList.Contains(o.Id)
                                )
                                .ToList();
                    foreach (var avOrder in usersOrders)
                    {
                        avOrder.OrderItems = avOrder.OrderItems.Where(o =>
                            workingFilter.AvailableStatusList.Contains((EnumOrderStatus)o.Order.State)).ToList();

                    }

                    usersOrders = usersOrders.Where(o => o.OrderItems.Count > 0).ToList();
                    var userOrdersData = GetUserOrdersData(usersOrders);

                    userOrdersData.Id = companyOrder.Id;

                    userOrdersData.TotalPrice = userOrdersData.Orders
                        .Where(e => e.OrderStatusReport != "Заказ отменён")
                        .ToList()
                        .Sum(e => e.TotalPrice);

                    if (userOrdersData.TotalPrice > 0)
                    {
                        companyOrdersData.TotalPrice += userOrdersData.TotalPrice;
                        companyOrdersData.Orders.Add(userOrdersData);
                    }

                    reportData.TotalSumm += companyOrdersData.TotalPrice;

                    if (userOrdersData.Orders.Count > 0)
                        reportData.OrdersData.Add(companyOrdersData);
                }


                //if (workingFilter.OrdersIdList.Count > 0)
                //{
                //    List<Order_m> individualOrdersList =
                //        Accessor
                //            .Instance
                //            .GetOrdersByFilter(workingFilter);
                //    if (individualOrdersList.Count > 0)
                //        reportData.individualOrdersData.Add(
                //            GetUserOrdersData(individualOrdersList));
                //}

                #region Сортировка

                switch (sort)
                {
                    case ReportSortType.OrderByDate:
                        for (var i = 0; i < reportData.OrdersData.Count; i++)
                            for (var j = 0; j < reportData.OrdersData[i].Orders.Count; j++)
                            {
                                var uOrders = reportData.OrdersData[i].Orders[j].Orders
                                    .OrderBy(o => o.Order.DeliverDate)
                                    .OrderBy(o => o.Order.OrderInfo.OrderAddress);
                                reportData.OrdersData[i].Orders[j].Orders = uOrders.ToList();
                            }

                        reportData.OrdersData = reportData.OrdersData
                            .OrderBy(o => o.CompanyOrder.OrderAutoCloseDate)
                            .ToList();
                        break;
                    case ReportSortType.OrderByStatus:
                        for (var i = 0; i < reportData.OrdersData.Count; i++)
                            for (var j = 0; j < reportData.OrdersData[i].Orders.Count; j++)
                            {
                                var uOrders = reportData.OrdersData[i].Orders[j].Orders.OrderBy(o => o.Order.Status)
                                    .OrderBy(o => o.Order.OrderInfo.OrderAddress);
                                reportData.OrdersData[i].Orders[j].Orders = uOrders.ToList();
                            }

                        reportData.OrdersData =
                            reportData.OrdersData.OrderBy(item => item.CompanyOrder.OrderStatus).ToList();
                        break;
                    case ReportSortType.OrderByPrice:
                        for (var i = 0; i < reportData.OrdersData.Count; i++)
                            for (var j = 0; j < reportData.OrdersData[i].Orders.Count; j++)
                            {
                                var uOrders = reportData.OrdersData[i].Orders[j].Orders.OrderBy(o => o.Order.TotalSum)
                                    .ThenBy(o => o.Order.OrderInfo.OrderAddress);
                                reportData.OrdersData[i].Orders[j].Orders = uOrders.ToList();
                            }

                        reportData.OrdersData =
                            reportData.OrdersData.OrderBy(item => item.TotalPrice).ToList();
                        break;
                    case ReportSortType.OrderByOrderNumber:
                        for (var i = 0; i < reportData.OrdersData.Count; i++)
                            for (var j = 0; j < reportData.OrdersData[i].Orders.Count; j++)
                            {
                                var uOrders = reportData.OrdersData[i].Orders[j].Orders.OrderBy(o => o.Order.Id)
                                    .OrderBy(o => o.Order.OrderInfo.OrderAddress);
                                reportData.OrdersData[i].Orders[j].Orders = uOrders.ToList();
                            }

                        reportData.OrdersData =
                            reportData.OrdersData.OrderBy(item => item.CompanyOrder.Id).ToList();
                        break;
                    case ReportSortType.OrderByCafeName:
                        reportData.OrdersData =
                            reportData.OrdersData.OrderBy(item => item.CompanyOrder.Cafe.FullName).ToList();
                        break;
                }
                return reportData;

                #endregion
            }
            throw new SecurityException("Attempt of unauthorized access");
        }

        /// <summary>
        /// Преобразование отчета заказов по сотруднику компании в Xml
        /// </summary>
        public static string GetReportUserOrdersInXmlByFilter(ReportFilter filter)
        {
            var reportData = GetReportUserOrders(filter);
            var processor =
                new XmlProcessing<ReportUserOrders>();
            var result = processor.SerializeObject(reportData);

            return result;
        }

        /// <summary>
        /// Преобразование отчета заказов по сотрудникам компании в Xml
        /// </summary>
        public static string GetReportUsersOrdersInXmlByFilter(ReportFilter filter)
        {
            var reportData = GetReportUsersOrders(filter);
            var processor =
                new XmlProcessing<ReportUsersOrders>();
            var result = processor.SerializeObject(reportData);
            
            return result;
        }
        /// <summary>
        /// Преобразование отчета заказа по id в Xml
        /// </summary>
        public static string GetReportUserOrderIdInXmlByFilter(ReportFilter filter)
        {
            var reportData = GetReportUserOrderId(filter);
            var processor =
                new XmlProcessing<ReportUserOrders>();
            var result = processor.SerializeObject(reportData);

            return result;
        }
        /// <summary>
        /// Получить отчет по id заказа
        /// </summary>
        public static ReportUserOrders GetReportUserOrderId(ReportFilter filter)
        {
            var reportUser = Accessor.Instance.GetUserById(filter.UserId ?? 0);
            if (reportUser != null)
            {
                var model = new ReportUserOrders()
                {
                    User = reportUser.ToDto(),
                    Orders = new List<OrderModel>() { Accessor.Instance.GetOrderById(filter.OrdersIdList.FirstOrDefault()).GetContract() },
                };
                model.TotalSumm = model.Orders.FirstOrDefault().TotalSum.Value;
                return model;
            }
            return null;
        }

        /// <summary>
        /// Получить отчет по заказам сотрудника за период
        /// </summary>
        public static ReportUserOrders GetReportUserOrders(ReportFilter filter)
        {
            var reportUser = Accessor.Instance.GetUserById(filter.UserId ?? 0);

            if (reportUser != null)
            {
                var xlsxModel = new ReportUserOrders()
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    Orders = Accessor.Instance.GetCompanyOrderByUserId(
                        filter.UserId ?? 0, 
                        filter.CompanyId ?? 0, 
                        filter.StartDate, 
                        filter.EndDate, 
                        filter.AvailableStatusList.Select(p => (EnumOrderStatus)p).ToList()).Select(o => o.GetContract()).ToList(),
                    User = reportUser.ToDto(),
                };

                foreach (var el in xlsxModel.Orders)
                {
                    xlsxModel.TotalSumm += el.TotalSum?? 0;
                }

                return xlsxModel;
            }

            return null;
        }

        /// <summary>
        /// Получить список заказов сотрудников компании за период
        /// </summary>
        public static List<ReportUserOrders> GetListReportUserOrders(ReportFilter filter)
        {
            var listReports = new List<ReportUserOrders>();
            var companyUsers = Accessor.Instance.GetListOfUserByCompanyId(filter.CompanyId ?? 0);

            Parallel.ForEach(companyUsers, companyUser =>
            {
                var reportModel = new ReportUserOrders()
                {
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    Orders = Accessor.Instance.GetCompanyOrderByUserId(
                        companyUser.Id, 
                        filter.CompanyId ?? 0, 
                        filter.StartDate, 
                        filter.EndDate, 
                        filter.AvailableStatusList.Select(p => (EnumOrderStatus)p).ToList()).Select(o => o.GetContract()).ToList(),
                    User = companyUser.ToDto(),
                };

                if (reportModel.Orders.Count > 0)
                {
                    foreach (var el in reportModel.Orders)
                    {
                        reportModel.TotalSumm += el.TotalSum ?? 0;
                    }

                    listReports.Add(reportModel);
                }
            }
            );

            var listQuery = new List<ReportUserOrders>();
            if (filter.Search == null)
                filter.Search = string.Empty;
            else
                filter.Search = filter.Search.Trim().ToLower();

            switch (filter.SearchType)
            {
                case SearchType.SearchByCafe:
                    foreach (var item in listReports)
                    {
                        var query = item.Orders.Where(o => o.Cafe.Name.ToLower().Contains(filter.Search)).ToList();
                        if (query.Count > 0)
                        {
                            item.Orders = query;
                            listQuery.Add(item);
                        }
                    }
                    break;
                case SearchType.SearchByDish:
                    foreach (var item in listReports)
                    {
                        var query = item.Orders.Where(t => t.OrderItems.FirstOrDefault(p => p.DishName.ToLower().Contains(filter.Search)) != null).ToList();
                        if (query.Count > 0)
                        {
                            item.Orders = query;
                            listQuery.Add(item);
                        }
                    }
                    break;
                case SearchType.SearchByName:
                    listQuery = listReports.Where(item => item.User.Email.ToLower().Contains(filter.Search) || (item.User.UserFullName == null? false: item.User.UserFullName.ToLower().Contains(filter.Search))).ToList();
                    break;
                case SearchType.SearchByOrderNumber:
                    {
                        long searchId;
                        if (long.TryParse(filter.Search, out searchId))
                        {
                            foreach (var item in listReports)
                            {
                                var query = item.Orders.FirstOrDefault(p => p.Id == searchId);
                                if (query != null)
                                {
                                    item.Orders = new List<OrderModel>();
                                    item.Orders.Add(query);
                                    listQuery.Add(item);
                                }
                            }
                        }
                    }
                    break;
                case SearchType.SearchByPhone:
                    {
                        foreach (var item in listReports)
                        {
                            var query = item.Orders.Where(p => p.PhoneNumber.Contains(filter.Search)).ToList();
                            if (query.Count > 0)
                            {
                                item.Orders = query;
                                listQuery.Add(item);
                            }
                        }
                    }
                    break;
            }

            return listQuery;
        }

        /// <summary>
        /// Получить отчет по заказам сотрудников компании за период
        /// </summary>
        public static ReportUsersOrders GetReportUsersOrders(ReportFilter filter)
        {
            var threadLock = new object();

            var listXlsxModel = GetListReportUserOrders(filter);
            var company = Accessor.Instance.GetCompanyById(filter.CompanyId ?? 0);
            double totalSumm = 0;

            Parallel.ForEach(listXlsxModel, xlsxModel =>
            {
                foreach (var order in xlsxModel.Orders)
                {
                    xlsxModel.TotalSumm += order.TotalSum ?? 0;
                }

                xlsxModel.TotalSumm /= 2;

                lock (threadLock) { totalSumm += xlsxModel.TotalSumm; }
            });

            var model = new ReportUsersOrders()
            {
                Company = company.GetContract(),
                Employee = listXlsxModel,
                EndDate = filter.EndDate,
                StartDate = filter.StartDate,
                TotalSumm = totalSumm
            };

            return model;
        }

    }


    //Добавить конвертацию в DTO
    public static class ReportDataExtensions
    {
        public static ReportDataModel ToDTO(this ReportData entity)
        {
            return entity == null
                ? null
                : new ReportDataModel()
                {
                    Cafe = entity.Cafe,
                    EndDate = entity.EndDate,
                    IndividualOrdersData = entity.IndividualOrdersData?.Select(e => e.ToDTO()).ToList(),
                    OrdersData = entity.OrdersData?.Select(e => e.ToDTO()).ToList(),
                    StartDate = entity.StartDate,
                    TotalSumm = entity.TotalSumm
                };
        }

        public static CompanyOrderDataModel ToDTO(this CompanyOrdersData entity)
        {
            return entity == null
                ? null
                : new CompanyOrderDataModel()
                {
                    Company = entity.Company,
                    CompanyAddress = entity.CompanyAddress,
                    CompanyOrder = entity.CompanyOrder,
                    Orders = entity.Orders.Select(e => e.ToDTO()).ToList(),
                    OrderStatusReport = entity.OrderStatusReport,
                    TotalPrice = entity.TotalPrice
                };
        }

        public static UserOrderDataModel ToDTO(this UserOrdersData entity)
        {
            return entity == null
                ? null
                : new UserOrderDataModel()
                {
                    Id = entity.Id,
                    Orders = entity.Orders.Select(e => e.ToDTO()).ToList(),
                    TotalPrice = entity.TotalPrice
                };
        }

        public static OrderDataModel ToDTO(this OrderData entity)
        {
            return entity == null
                ? null
                : new OrderDataModel()
                {
                    Delivery = entity.Delivery,
                    Order = entity.Order,
                    OrderDishes = entity.OrderDishes.Select(e => e.ToDTO()).ToList(),
                    OrderStatusReport = entity.OrderStatusReport,
                    TotalPrice = entity.TotalPrice,
                    User = entity.User
                };
        }

        public static FoodDishDataModel ToDTO(this FoodDishData entity)
        {
            return entity == null
                ? null
                : new FoodDishDataModel()
                {
                    CategoryName = entity.CategoryName,
                    Dish = entity.Dish,
                    ItemCount = entity.ItemCount,
                    ItemDiscount = entity.ItemDiscount,
                    ItemTotalPrice = entity.ItemTotalPrice
                };
        }
    }
}