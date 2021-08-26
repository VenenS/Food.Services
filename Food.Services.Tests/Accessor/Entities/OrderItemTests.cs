
using System;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException

namespace AccessorTests.Entites
{
    [TestFixture]
    public class OrderItemTests
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
        public void GetOrderItemById_Success()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var result = Accessor.Instance.GetOrderItemById(item.Id);
            Assert.IsTrue(item.Comment == result.Comment);
        }

        [Test]
        public void GetOrderItemById_Only_Alive()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            item.IsDeleted = true;
            var result = Accessor.Instance.GetOrderItemById(item.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetOrderItemById__No_Such_Id()
        {
            SetUp();
            var id = _random.Next();
            var result = Accessor.Instance.GetOrderItemById(id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void PostOrderItem_Success()
        {
            SetUp();
            var model = OrderItemFactory.CreateVirtual();
            var result = Accessor.Instance.PostOrderItem(model);
            Assert.IsTrue(result[0] == model.OrderId);
            Assert.IsTrue(result[1] == model.Id);
        }

        [Test]
        public void PostOrderItem_Success_Logs()
        {
            SetUp();
            var model = OrderItemFactory.CreateVirtual();
            Accessor.Instance.PostOrderItem(model);
            Assert.IsTrue(model.Id >= 0);
            Assert.IsTrue(model.CreationDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void PostOrderItem_Success_Links()
        {
            SetUp();
            var model = OrderItemFactory.CreateVirtual();
            Accessor.Instance.PostOrderItem(model);
            Assert.IsTrue(model.ImageId == 1);
            Assert.IsTrue(model.Dish == null);
            Assert.IsTrue(model.Order == null);
        }

        [Test]
        public void DeleteOrderItem_Only_Alive()
        {
            SetUp();
            var authorId = _random.Next();
            var item = OrderItemFactory.Create();
            item.IsDeleted = true;
            var result = Accessor.Instance.DeleteOrderItem(item.Id, authorId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void DeleteOrderItem_Success()
        {
            SetUp();
            var authorId = _random.Next();
            var item = OrderItemFactory.Create();
            var result = Accessor.Instance.DeleteOrderItem(item.Id, authorId);
            Assert.IsTrue(result == true);
            Assert.IsTrue(item.IsDeleted == true);
            Assert.IsTrue(item.LastUpdateByUserId == authorId);
            Assert.IsTrue(item.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void ChangeOrderItem_No_Such_Item()
        {
            SetUp();
            var model = OrderItemFactory.CreateVirtual();
            model.Id = _random.Next();
            var result = Accessor.Instance.ChangeOrderItem(model);
            Assert.IsTrue(result == -1);
        }

        [Test]
        public void ChangeOrderItem_Success_Ids()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var model = OrderItemFactory.Clone(item);
            model.ImageId = _random.Next();
            model.OrderId = _random.Next();
            model.DishId = _random.Next();
            model.LastUpdateByUserId = _random.Next();
            var result = Accessor.Instance.ChangeOrderItem(model);
            Assert.IsTrue(result == item.Id);
            Assert.IsTrue(item.ImageId == model.ImageId);
            Assert.IsTrue(item.OrderId == model.OrderId);
            Assert.IsTrue(item.DishId == model.DishId);
            Assert.IsTrue(item.LastUpdateByUserId == model.LastUpdateByUserId);
        }

        [Test]
        public void ChangeOrderItem_Success_Others()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var model = OrderItemFactory.Clone(item);
            model.Comment =Guid.NewGuid().ToString("N");
            model.TotalPrice = _random.Next();
            Accessor.Instance.ChangeOrderItem(model);
            Assert.IsTrue(item.Comment == model.Comment);
            Assert.IsTrue(item.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
            Assert.IsTrue(Math.Abs(item.TotalPrice - model.TotalPrice) < 0.1);
        }

        [Test]
        public void ChangeOrderItem_Success_Dish_Fields()
        {
            SetUp();
            var item = OrderItemFactory.Create();
            var model = OrderItemFactory.Clone(item);
            model.DishName = Guid.NewGuid().ToString("N");
            model.DishBasePrice = _random.Next();
            model.DishKcalories = _random.Next();
            model.DishWeight = _random.Next();
            model.DishCount = _random.Next();
            model.DishDiscountPrc = _random.Next();
            //
            Accessor.Instance.ChangeOrderItem(model);
            //
            Assert.IsTrue(item.DishName == model.DishName);
            Assert.IsTrue(Equals(item.DishBasePrice, model.DishBasePrice));
            Assert.IsTrue(Equals(item.DishKcalories, model.DishKcalories));
            Assert.IsTrue(Equals(item.DishWeight, model.DishWeight));
            Assert.IsTrue(item.DishCount == model.DishCount);
            Assert.IsTrue(item.DishDiscountPrc == model.DishDiscountPrc);
        }
    }
}