using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException

namespace AccessorTests.Entites
{
    [TestFixture]
    public class OrderTests
    {
        private FakeContext _context;
        private readonly Random _random = new Random();

        private void SetUp()
        {
            _context = new FakeContext();
            Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);
        }

        [Test]
        public void GetOrderById_Success()
        {
            SetUp();
            var order = OrderFactory.Create();
            var result = Accessor.Instance.GetOrderById(order.Id);
            Assert.IsTrue(result.Comment== order.Comment);
        }

        [Test]
        public void GetOrderById_Only_Alive()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.GetOrderById(order.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void GetOrderById_Null_If_Not_Exists()
        {
            SetUp();
            var orderId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.GetOrderById(orderId);
            Assert.IsNull(result);
        }

        [Test]
        public void GetUserOrders_Success()
        {
            SetUp();
            var order = OrderFactory.Create();
            var result = Accessor.Instance.GetUserOrders(order.UserId, null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [Test]
        public void GetUserOrders_Only_Alive_Order()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.GetUserOrders(order.UserId, null);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrders_Only_Alive_User()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.User.IsDeleted = true;
            var result = Accessor.Instance.GetUserOrders(order.UserId, null);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrders_Success_With_Date()
        {
            SetUp();
            var date = DateTime.Now;
            var order = OrderFactory.Create();
            order.CreationDate = date;
            var otherOrder = OrderFactory.Create(user: order.User);
            otherOrder.CreationDate = date.AddDays(1);
            var result = Accessor.Instance.GetUserOrders(order.UserId, date);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [Test]
        public void GetUserOrders2_Only_Alive_Order()
        {
            SetUp();
            var startDate = DateTime.Now.AddDays(-1);
            var endDate = DateTime.Now.AddDays(1);
            var order = OrderFactory.Create();
            order.CreationDate = DateTime.Now;
            order.IsDeleted = true;
            var result = Accessor.Instance.GetUserOrders(order.UserId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrders2_Only_Alive_User()
        {
            SetUp();
            var startDate = DateTime.Now.AddDays(-1);
            var endDate = DateTime.Now.AddDays(1);
            var order = OrderFactory.Create();
            order.CreationDate = DateTime.Now;
            order.User.IsDeleted = true;
            var result = Accessor.Instance.GetUserOrders(order.UserId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrders2_Success()
        {
            SetUp();
            var startDate = DateTime.Now.AddDays(-1);
            var endDate = DateTime.Now.AddDays(1);
            var order = OrderFactory.Create();
            order.CreationDate = DateTime.Now;
            var otherOrder = OrderFactory.Create(user: order.User);
            otherOrder.CreationDate = endDate.AddDays(1);
            var result = Accessor.Instance.GetUserOrders(order.UserId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [Test]
        public void GetOrderItemsByOrderId_Success()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var result = Accessor.Instance.GetOrderItemsByOrderId(null, item.OrderId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == item.Comment);
        }

        [Test]
        public void GetOrderItemsByOrderId_Only_Alive()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.IsDeleted = true;
            var result = Accessor.Instance.GetOrderItemsByOrderId(null, item.OrderId);
            Assert.IsTrue(result.Count == 0);
        }

        //Совершенная бессмыслица: забираем order по Id, а потом отсеиваем те item'ы у которых не правильный Order.User.Id(!)
        [Test]
        public void GetOrderItemsByOrderId_With_User_Success()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var result = Accessor.Instance.GetOrderItemsByOrderId(item.Order.UserId, item.OrderId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == item.Comment);
        }

        [Test]
        public void PostOrderTest()
        {
            SetUp();
            var authorId = _random.Next();
            var model = OrderFactory.CreateVirtual();
            Accessor.Instance.PostOrder(model, authorId);
            var created = ContextManager.Get().Orders.FirstOrDefault(e => e.Comment == model.Comment);
            Assert.IsTrue(created != null);
            Assert.IsTrue(created.CreatorId == authorId);
            Assert.IsTrue(created.CreationDate.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void ChangeOrder_Success_Strings()
        {
            SetUp();
            var order = OrderFactory.Create();
            var model = OrderFactory.Clone(order);
            model.PhoneNumber = Guid.NewGuid().ToString("N");
           model.OddMoneyComment = Guid.NewGuid().ToString("N");
            model.Comment = Guid.NewGuid().ToString("N");
            var result = Accessor.Instance.ChangeOrder(model);
            Assert.IsTrue(model.PhoneNumber == order.PhoneNumber);
            Assert.IsTrue(model.OddMoneyComment == order.OddMoneyComment);
            Assert.IsTrue(model.Comment == order.Comment);
        }

        [Test]
        public void ChangeOrder_Success_Numbers()
        {
            SetUp();
            var order = OrderFactory.Create();
            var model = OrderFactory.Clone(order);
            model.DeliveryAddressId = _random.Next();
            model.State = EnumOrderState.Abort;
            model.LastUpdateByUserId = _random.Next();
            var result = Accessor.Instance.ChangeOrder(model);
            Assert.IsTrue(result == order.Id);
            Assert.IsTrue(model.DeliveryAddressId == order.DeliveryAddressId);
            Assert.IsTrue(model.State == order.State);
            Assert.IsTrue(model.LastUpdateByUserId == order.LastUpdateByUserId);
        }

        [Test]
        public void ChangeOrder_Success_Dates()
        {
            SetUp();
            var order = OrderFactory.Create();
            var model = OrderFactory.Clone(order);
            model.DeliverDate = DateTime.Today;
            var result = Accessor.Instance.ChangeOrder(model);
            Assert.IsTrue(model.DeliverDate == order.DeliverDate);
            Assert.IsTrue(order.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void ChangeOrder_No_Such_entity()
        {
            SetUp();
            var order = OrderFactory.Create();
            var model = OrderFactory.Clone(order);
            model.Id = _random.Next(999, Int32.MaxValue);
            var result = Accessor.Instance.ChangeOrder(model);
            Assert.IsTrue(result == -1);
        }

        [TestCase(EnumOrderState.Accepted)]
        [TestCase(EnumOrderState.Created)]
        [TestCase(EnumOrderState.Delivered)]
        [TestCase(EnumOrderState.Delivery)]
        [TestCase(EnumOrderState.Abort)]
        public void GetCurrentListOfOrdersToCafe_Success(EnumOrderState state)
        {
            SetUp();
            var order = OrderFactory.Create();
            order.State = state;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [TestCase(EnumOrderState.Cart)]
        public void GetCurrentListOfOrdersToCafe_BadState(EnumOrderState state)
        {
            SetUp();
            var order = OrderFactory.Create();
            order.State = state;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, null);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafe_OnlyAlive()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, null);
            Assert.IsTrue(result.Count == 0);
        }
        
        [TestCase(EnumOrderState.Accepted)]
        [TestCase(EnumOrderState.Created)]
        [TestCase(EnumOrderState.Delivered)]
        [TestCase(EnumOrderState.Delivery)]
        [TestCase(EnumOrderState.Abort)]
        public void GetCurrentListOfOrdersToCafe2_Success(EnumOrderState state)
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = OrderFactory.Create();
            order.State = state;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [TestCase(EnumOrderState.Cart)]
        public void GetCurrentListOfOrdersToCafe2_BadState(EnumOrderState state)
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = OrderFactory.Create();
            order.State = state;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafe2_OnlyAlive()
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafe2_Wrong_Enddate()
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MinValue;
            var order = OrderFactory.Create();
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafe2_Wrong_StartDate()
        {
            SetUp();
            var startDate = DateTime.MaxValue;
            var endDate = DateTime.MaxValue;
            var order = OrderFactory.Create();
            var result = Accessor.Instance.GetCurrentListOfOrdersToCafe(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrdersWithSameStatus_Success()
        {
            SetUp();
            var order = OrderFactory.Create();
            var result = Accessor.Instance.GetUserOrdersWithSameStatus(order.UserId, (long) order.State, null);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }
        
        [Test]
        public void GetUserOrdersWithSameStatus_By_Date_Success()
        {
            SetUp();
            var date = DateTime.Now;
            var order = OrderFactory.Create();
            order.CreationDate = date;
            var result = Accessor.Instance.GetUserOrdersWithSameStatus(order.UserId, (long)order.State, date);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [Test]
        public void GetUserOrdersWithSameStatus_By_Date_Empty()
        {
            SetUp();
            var date = DateTime.Now;
            var order = OrderFactory.Create();
            order.CreationDate = date.AddDays(1);
            var result = Accessor.Instance.GetUserOrdersWithSameStatus(order.UserId, (long)order.State, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetUserOrdersWithSameStatus_Only_Alive()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.GetUserOrdersWithSameStatus(order.UserId, (long)order.State, null);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void UpdateOrderInformation_No_Such_Id()
        {
            SetUp();
            var randomId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.UpdateOrderInformation(randomId, randomId);
            Assert.IsTrue(result == -1);
        }

        [Test]
        public void SetOrderStatus_No_Such_Id()
        {
            SetUp();
            var randomId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.SetOrderStatus(randomId, (long) EnumOrderState.Created, randomId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void SetOrderStatus_Only_Alive()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.SetOrderStatus(order.Id, (long)EnumOrderState.Created, order.CreatorId.Value);
            Assert.IsTrue(result == false);
        }

        [TestCase(EnumOrderState.Accepted, EnumOrderState.Delivered)]
        public void SetOrderStatus_Success(EnumOrderState before, EnumOrderState next)
        {
            SetUp();
            var order = OrderFactory.Create();
            order.State = before;
            var result = Accessor.Instance.SetOrderStatus(order.Id, (long)next, order.CreatorId.Value);
            Assert.IsTrue(result == true);
            Assert.IsTrue(order.State == next);
            Assert.IsTrue(order.LastUpdateByUserId == order.CreatorId.Value);
            Assert.IsTrue(order.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void SetCompanyOrderStatus_No_Such_Id()
        {
            SetUp();
            var randomId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.SetCompanyOrderStatus(randomId, (long)EnumOrderState.Created, randomId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void SetCompanyOrderStatus_Only_Alive()
        {
            SetUp();
            var order = CompanyOrderFactory.Create();
            order.IsDeleted = true;
            var result = Accessor.Instance.SetCompanyOrderStatus(order.Id, (long)EnumOrderState.Created, order.CreatorId.Value);
            Assert.IsTrue(result == false);
        }

        [TestCase(EnumOrderState.Accepted, EnumOrderState.Delivered)]
        public void SetCompanyOrderStatus_Success(EnumOrderState before, EnumOrderState next)
        {
            SetUp();
            var order = CompanyOrderFactory.Create();
            order.State = (long) before;
            var result = Accessor.Instance.SetCompanyOrderStatus(order.Id, (long)next, order.CreatorId.Value);
            Assert.IsTrue(result == true);
            Assert.IsTrue(order.State == (long) next);
            Assert.IsTrue(order.LastUpdateByUserId == order.CreatorId.Value);
            Assert.IsTrue(order.LastUpdate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void CheckExistingDishInOrders_No_Such_Id()
        {
            SetUp();
            var randomId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.CheckExistingDishInOrders(randomId,null, null, null);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckExistingDishInOrders_Only_Alive()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.IsDeleted = true;
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, null, null);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckExistingDishInOrders_Not_Banket_Orders()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.Order.BanketId = 0;
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, null, null);
            Assert.IsTrue(result == false);
        }

        [TestCase(EnumOrderState.Accepted)]
        [TestCase(EnumOrderState.Created)]
        [TestCase(EnumOrderState.Delivered)]
        [TestCase(EnumOrderState.Delivery)]
        [TestCase(EnumOrderState.Abort)]
        public void CheckExistingDishInOrders_Succes_Order_State(EnumOrderState accepted)
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.Order.State = accepted;
            item.Order.BanketId = null;
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, null, null);
            Assert.IsTrue(result == true);
        }

        [TestCase(EnumOrderState.Cart)]
        public void CheckExistingDishInOrders_Fail_Order_State(EnumOrderState accepted)
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.Order.State = accepted;
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, null, null);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckExistingDishInOrders_Failed_WantedDate()
        {
            SetUp();
            var wantedDate = DateTime.Now;
            var item = OrderItemFactory.Create();
            item.Order.DeliverDate = wantedDate.AddDays(1);
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, wantedDate, null, null);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckExistingDishInOrders_Failed_BeginDate()
        {
            SetUp();
            var begin = DateTime.Now.AddDays(-1);
            var end = DateTime.Now.AddDays(1);
            var item = OrderItemFactory.Create();
            item.Order.DeliverDate = begin.AddDays(-1);
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, begin, end);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckExistingDishInOrders_Failed_EndDate()
        {
            SetUp();
            var begin = DateTime.Now.AddDays(-1);
            var end = DateTime.Now.AddDays(1);
            var item = OrderItemFactory.Create();
            item.Order.DeliverDate = end.AddDays(1);
            var result = Accessor.Instance.CheckExistingDishInOrders(item.DishId, null, begin, end);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void RemoveOrder_No_Such_Id()
        {
            SetUp();
            var randomId = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.RemoveOrder(randomId, randomId);
            Assert.IsTrue(result == -1);
        }

        [Test]
        public void RemoveOrder_Only_Alive()
        {
            SetUp();
            var item = OrderFactory.Create();
            item.IsDeleted = true;
            var result = Accessor.Instance.RemoveOrder(item.Id,item.CreatorId.Value);
            Assert.IsTrue(result == -1);
        }

        [Test]
        public void RemoveOrderSuccess()
        {
            SetUp();
            var item = OrderFactory.Create();
            var result = Accessor.Instance.RemoveOrder(item.Id, item.CreatorId.Value);
            Assert.IsTrue(result == item.Id);
            Assert.IsTrue(item.LastUpdateByUserId == item.CreatorId.Value);
            Assert.IsTrue(item.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void GetOrdersByCompanyOrderId_Success()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.CompanyOrderId = _random.Next();
            var result = Accessor.Instance.GetOrdersByCompanyOrderId(order.CompanyOrderId.Value);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Comment == order.Comment);
        }

        [Test]
        public void GetOrdersByCompanyOrderId_Only_Alive_Order()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.IsDeleted = true;
            order.CompanyOrderId = _random.Next();
            var result = Accessor.Instance.GetOrdersByCompanyOrderId(order.CompanyOrderId.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [TestCase(EnumOrderState.Cart)]
        public void GetOrdersByCompanyOrderId_Fail_Order_State(EnumOrderState state)
        {
            SetUp();
            var order = OrderFactory.Create();
            order.State = state;
            order.CompanyOrderId = _random.Next();
            var result = Accessor.Instance.GetOrdersByCompanyOrderId(order.CompanyOrderId.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetOrdersByFilterTest()
        {

        }

        [Test]
        public void UpdateCompanyOrderSumTest()
        {

        }

        [Test]
        public void GetTotalNumberOfOrdersPerDayTest()
        {

        }

        [Test]
        public void GetCompanyOrderByUserIdTest()
        {

        }

        [Test]
        public void GetCurrentListOfOrdersToCafeTest1()
        {
        }
    }
}