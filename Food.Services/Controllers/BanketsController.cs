using DocumentFormat.OpenXml.Office.CustomUI;
using Food.Data;
using Food.Data.Entities;
using Food.Services.ExceptionHandling;
using Food.Services.Extensions;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.ShedulerQuartz;
using Food.Services.ShedulerQuartz;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/bankets")]
    public class BanketsController : ContextableApiController
    {
        [ActivatorUtilitiesConstructor]
        public BanketsController()
        { }

        public BanketsController(IFoodContext context, Accessor accessor)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, Route("")]
        public async Task<IActionResult> Post([FromBody] BanketModel model)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var context = accessor.GetContext();

                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), model.CafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                var banket = Accessor.Instance.GetBanketByEventDate(model.EventDate);
                if (banket != null)
                {
                    return BadRequest("Банкет на указанную дату уже существует");
                }
                string url = GenerateCharactersSequence(8);
                var newBanket = new Banket()
                {
                    CompanyId = model.CompanyId,
                    EventDate = model.EventDate.Date,
                    MenuId = model.MenuId,
                    OrderEndDate = model.OrderEndDate.Date.AddDays(1).AddMinutes(-1),
                    OrderStartDate = model.OrderStartDate,
                    Name = model.Name,
                    Status = (EnumBanketStatus)model.Status,
                    Url = url,
                    IsDeleted = false,
                    CafeId = model.CafeId
                };

                if ((newBanket.Id = Accessor.Instance.AddBanket(newBanket)) == -1)
                {
                    return new InternalServerError();
                }

                await ShedulerQuartz.Scheduler.Instance.DispatchBanquetOrderNotificationAt(newBanket.OrderEndDate, newBanket.Id);
                return Ok(newBanket.GetContract());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("")]
        public IActionResult Get()
        {
            var bankets = Accessor.Instance.GetAllBankets();
            return Ok(bankets.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{id:long}")]
        public IActionResult Get(long id)
        {
            BanketModel model = GetBanketById(Accessor.Instance, id);
            if (model == null)
                return NotFound();

            return Ok(model);
        }

        /// <summary>
        /// Получить банкеты по фильтру
        /// </summary>
        [HttpPost, Route("filter")]
        public IActionResult GetBanketsByFilter([FromBody] BanketsFilterModel filter)
        {
            try
            {
                BanketFilterSortType sortType;
                switch (filter.SortType)
                {
                    case ReportSortType.OrderByPrice:
                        sortType = BanketFilterSortType.OrderByPrice;
                        break;
                    case ReportSortType.OrderByOrderNumber:
                        sortType = BanketFilterSortType.OrderByOrderNumber;
                        break;
                    default:
                        sortType = BanketFilterSortType.OrderByDate;
                        break;
                }

                var banketFilter = new BanketFilter
                {
                    BanketIds = filter.BanketIds,
                    StartDate = filter.StartDate,
                    EndDate = filter.EndDate,
                    CafeId = filter.CafeId,
                    SortType = sortType
                };

                var bankets = Accessor.Instance.GetBanketsByFilter(banketFilter);
                var banketIds = bankets.Select(d => d.Id);
                List<Order> orders;
                if (filter.LoadOrders)
                {
                    orders = Accessor.Instance.ListOrdersInBankets(banketIds);
                }
                else
                    orders = new List<Order>();

                var banketsModels = new List<BanketModel>();
                bankets.ForEach(banket =>
                {
                    var banketModel = banket.GetContract();
                    var banketOrders = orders.Where(c => c.BanketId == banketModel.Id).Select(c => c.GetContract())
                        .ToList();
                    banketModel.Orders = banketOrders;
                    banketsModels.Add(banketModel);
                });
                return Ok(banketsModels);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{banketId:long}/user/{userId:long}")]
        public IActionResult GetOrderItemsByUserId(long banketId, long userId)
        {
            var order = Accessor.Instance.GetOrderInBanketByUserId(banketId, userId);
            if (order == null)
                return Ok();

            return Ok(order.GetContract());
        }

        [HttpGet, Route("{banketId:long}/orders")]
        public IActionResult GetOrdersInBanket(long banketId)
        {
            var orders = Accessor.Instance.ListOrdersInBanket(banketId);
            return Ok(orders.Select(c => c.GetContract()));
        }

        [Authorize]
        [HttpPost, Route("order")]
        public IActionResult PostOrders([FromBody] OrderModel model)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var context = accessor.GetContext();

                if (model == null)
                    return BadRequest();
                if (model.OrderItems.Count == 0)
                {
                    return BadRequest("Не выбрано ни одного блюда");
                }
                var banket = Accessor.Instance.GetBanketById(model.BanketId.Value);
                if (banket == null)
                    return BadRequest();

                var order = model.GetEntity();
                var userId = User.Identity.GetUserId();
                order.State = EnumOrderState.Created;
                order.TotalPrice = model.OrderItems.Sum(c => c.TotalPrice);
                order.DeliverDate = banket.EventDate.Date;

                Accessor.Instance.PostOrder(order, userId);

                //пересчитываем сумму банкета
                Accessor.Instance.RecalculateBanketTotalSum(banket);

                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize]
        [HttpPut, Route("order")]
        public IActionResult UpdateOrder([FromBody] OrderModel model)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var context = accessor.GetContext();

                if (model == null)
                    return BadRequest();
                var today = DateTime.Now.Date;
                var userId = User.Identity.GetUserId();
                var order = context.Orders
                    .Include(c => c.Banket)
                    .Include(c => c.OrderItems).FirstOrDefault(c =>
                    !c.IsDeleted && c.BanketId == model.BanketId && !c.Banket.IsDeleted &&
                    c.Banket.OrderStartDate <= today && today <= c.Banket.OrderEndDate &&
                    c.UserId == userId);
                if (order == null)
                    return BadRequest();
                var currentDishes = order.OrderItems;
                var deletedDishes = currentDishes.Where(c => !c.IsDeleted).Select(c => c.DishId)
                    .Except(model.OrderItems.Select(c => c.FoodDishId)).ToList();
                foreach (var dish in currentDishes)
                {
                    if (deletedDishes.Contains(dish.DishId))
                    {
                        dish.IsDeleted = true;
                    }
                }
                var addedDishes = model.OrderItems.Select(c => c.FoodDishId).Except(currentDishes.Select(s => s.DishId))
                    .ToList();
                var changedIds = model.OrderItems.Select(c => c.FoodDishId)
                    .Intersect(currentDishes.Select(c => c.DishId))
                    .ToList();

                var changed = context.OrderItems
                    .Where(c => changedIds.Contains(c.DishId) && c.OrderId == order.Id).ToList();

                foreach (var item in changed)
                {
                    var newDish = model.OrderItems.FirstOrDefault(c => c.FoodDishId == item.DishId);
                    if (newDish == null)
                        continue;

                    if (newDish.DishCount <= 0)
                        item.IsDeleted = true;
                    else
                        item.DishCount = newDish.DishCount;
                    item.TotalPrice = newDish.TotalPrice;
                    item.IsDeleted = false;
                }

                var newDishes = model.OrderItems.Where(c => addedDishes.Contains(c.FoodDishId)).Select(
                    c => new OrderItem()
                    {
                        CreationDate = DateTime.Now,
                        CreatorId = userId,
                        DishId = c.FoodDishId,
                        DishBasePrice = c.DishBasePrice,
                        DishName = c.DishName,
                        OrderId = order.Id,
                        TotalPrice = c.TotalPrice,
                        DishCount = c.DishCount
                    }).ToList();

                context.OrderItems.AddRange(newDishes);
                context.SaveChanges();

                var items = context.OrderItems.Where(c => !c.IsDeleted && c.OrderId == order.Id);
                order.TotalPrice = items.Count() > 0 ? items.Sum(c => c.TotalPrice) : 0;
                context.SaveChanges();

                Accessor.Instance.RecalculateBanketTotalSum(order.Banket);

                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete, Route("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var banket = Accessor.Instance.GetBanketById(id);
                if (banket == null)
                {
                    return Ok(false);
                }

                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), banket.CafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                await ShedulerQuartz.Scheduler.Instance.CancelBanquetOrderNotification(banket.Id);
                return Ok(Accessor.Instance.DeleteBanket(banket.Id));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete, Route("orderitems/{orderItemId:long}")]
        public IActionResult DeleteOrderItem(long orderItemId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var context = accessor.GetContext();

                var item = Accessor.Instance.GetOrderItemById(orderItemId);
                if (item == null || item.Order == null || item.Order.IsDeleted ||
                    item.Order.Banket == null || item.Order.Banket.IsDeleted)
                    return BadRequest();

                Accessor.Instance.DeleteOrderItem(orderItemId, User.Identity.GetUserId());
                Accessor.Instance.RecalculateOrderTotalSum(item.Order);
                Accessor.Instance.RecalculateBanketTotalSum(item.Order.Banket);

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("")]
        public async Task<IActionResult> Put([FromBody] BanketModel model)
        {
            try
            {
                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), model.CafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                var bankets = Accessor.Instance.GetAllBankets();
                var banket = Accessor.Instance.GetBanketById(model.Id);
                if (banket == null)
                    return BadRequest("Банкет не найден");

                if (model.EventDate.Date != banket.EventDate.Date)
                {
                    if (Accessor.Instance.GetBanketByEventDate(model.EventDate.Date) != null)
                    {
                        return BadRequest("Выберите другую дату проведения банкета");
                    }
                }

                if (banket.Orders.Count == 0)
                {
                    banket.CompanyId = model.CompanyId;
                    banket.MenuId = model.MenuId;
                }

                banket.EventDate = model.EventDate;
                banket.OrderStartDate = model.OrderStartDate;
                banket.OrderEndDate = model.OrderEndDate;

                await ShedulerQuartz.Scheduler.Instance.CancelBanquetOrderNotification(banket.Id);
                await ShedulerQuartz.Scheduler.Instance.DispatchBanquetOrderNotificationAt(model.OrderEndDate, banket.Id);

                return Ok(Accessor.Instance.EditBanket(banket));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        public static BanketModel GetBanketById(Accessor accessor, long id)
        {
            var banket = accessor.GetBanketById(id);
            if (banket == null)
                return null;

            return banket.GetContract();
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("setstatus/{banketId:long}/cafe/{cafeId:long}")]
        [Route("setstatus/{banketId:long}")]
        public IActionResult SetBanketStatus(long banketId, int status, long? cafeId = null)
        {
            try
            {
                if (!Enum.IsDefined(typeof(EnumBanketStatus), status))
                {
                    throw new ArgumentException("Invalid integer value for banket status enum");
                }
                Accessor accessor = GetAccessor();
                var newStatus = (EnumBanketStatus)status;
                long userId = User.Identity.GetUserId();

                if (cafeId.HasValue && accessor.IsUserManagerOfCafe(userId, cafeId.Value))
                {
                    Banket banket = accessor.GetBanketById(banketId);
                    switch (banket.Status)
                    {
                        case EnumBanketStatus.Projected:
                            {
                                if (newStatus != EnumBanketStatus.Formed)
                                    return base.Ok(false);
                            }
                            break;
                        case EnumBanketStatus.Formed:
                            {
                                if (newStatus != EnumBanketStatus.Preparing)
                                    return base.Ok(false);
                            }
                            break;
                        case EnumBanketStatus.Preparing:
                            {
                                if (newStatus != EnumBanketStatus.Projected)
                                    return base.Ok(false);
                            }
                            break;
                        default:
                            return base.Ok(false);
                    }

                    accessor.SetBanketStatus(banket.Id, newStatus);
                    return Ok(true);
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        private string GenerateCharactersSequence(int sequenceLength)
        {
            var random = new Random();
            string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var sequence = new StringBuilder(sequenceLength);
            for (int i = 0; i < sequenceLength; i++)
            {
                sequence.Append(characters[random.Next(characters.Length)]);
            }
            return sequence.ToString();
        }
    }

    public static class BanketExtensions
    {
        public static BanketModel GetContract(this Banket banket)
        {
            return banket == null
                ? null
                : new BanketModel()
                {
                    Id = banket.Id,
                    CompanyId = banket.CompanyId,
                    EventDate = banket.EventDate,
                    MenuId = banket.MenuId,
                    OrderEndDate = banket.OrderEndDate,
                    OrderStartDate = banket.OrderStartDate,
                    CafeId = banket.CafeId,
                    Cafe = banket.Cafe?.GetContract(),
                    Company = banket.Company?.GetContract(),
                    Orders = banket.Orders?.Select(c => c.GetContract()).ToList() ?? new List<OrderModel>(),
                    Menu = banket.Menu?.GetContract(),
                    TotalSum = banket.TotalSum,
                    Name = banket.Name,
                    Status = (BanketStatus)banket.Status,
                    Url = banket.Url
                };
        }
    }
}
