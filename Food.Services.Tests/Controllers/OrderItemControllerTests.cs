using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    class OrderItemControllerTests
    {
        private FakeContext _context;
        private OrderItemController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new OrderItemController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]
        public void ChangeOrderItemTest_User()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию
            Company company = CompanyFactory.Create(_user);
            //Создаем заказ в кафе
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, null);
            //Создаем итем в заказе
            OrderItem orderItem = OrderItemFactory.Create(_user, dish, order);
            orderItem.DishCount = 10;
            OrderItemModel orderItemModel = orderItem.GetContract();

            var responce = _controller.ChangeOrderItem(orderItemModel);
            var result = TransformResult.GetPrimitive<OrderStatusModel>(responce);

            //Результат не должен быть нулл
            Assert.IsNotNull(result);
        }

        [Test]

        public void PostOrderItemsTest_User()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию
            Company company = CompanyFactory.Create(_user);
            //Создаем список заказов:
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            List<OrderItem> orderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            OrderItemModel[] orderItemModels = new OrderItemModel[10];
            for (int i = 0; i < 10; i++)
            {
                orderItems[i].DishCount = i + 1;
                orderItemModels[i] = orderItems[i].GetContract();
            }

            var responce = _controller.PostOrderItems(orderItemModels, order.Id);
            var result = TransformResult.GetPrimitive<OrderStatusModel>(responce);

            //Результат не должен быть нулл
            Assert.IsNotNull(result);
        }

        [Test]
        public void RemoveOrderItemTest_User()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию
            Company company = CompanyFactory.Create(_user);
            //Создаем список заказов:
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            OrderItem orderItem = OrderItemFactory.Create(_user, null, order);
            order.State = EnumOrderState.Created;
            var responce = _controller.RemoveOrderItem(orderItem.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Результат не должен быть нулл
            Assert.IsNotNull(result);
            //Результат удаления должен быть true
            Assert.IsTrue(result);

        }
    }
}
