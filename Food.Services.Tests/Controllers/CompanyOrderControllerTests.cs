using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Mocks;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    class CompanyOrderControllerTests
    {
        private FakeContext _context;
        private CompanyOrderController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            ShedulerQuartz.Scheduler.Instance = new MockScheduler();
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new CompanyOrderController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        private void SetUp(string role)
        {
            ShedulerQuartz.Scheduler.Instance = new MockScheduler();
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new CompanyOrderController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, new[] { role });
        }
        [Test]

        public void AddNewCompanyOrderTest_Admin()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем заказ в кафе
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, null, cafe);
            CompanyOrderModel companyOrderModel = companyOrder.GetContract();

            var responce = _controller.AddNewCompanyOrder(companyOrderModel);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void AddNewCompanyOrderScheduleTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписание
            CompanyOrderSchedule companyOrderSchedule =
                CompanyOrderScheduleFactory.Create(_user, company, cafe);
            CompanyOrderScheduleModel scheduleModel =
                companyOrderSchedule.GetContract();

            var responce = _controller.AddNewCompanyOrderSchedule(scheduleModel);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public async Task EditCompanyOrderTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем заказ в кафе
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, null, cafe);
            CompanyOrderModel companyOrderModel = companyOrder.GetContract();

            var responce = await _controller.EditCompanyOrder(companyOrderModel);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public async Task EditCompanyOrderScheduleTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписание
            CompanyOrderSchedule companyOrderSchedule =
                CompanyOrderScheduleFactory.Create(_user, company, cafe);
            CompanyOrderScheduleModel scheduleModel =
                companyOrderSchedule.GetContract();
            var responce = await _controller.EditCompanyOrderSchedule(scheduleModel);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void GetAvailableCompanyOrdersTest_User()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказы компании
            List<CompanyOrder> companyOrders = CompanyOrderFactory.CreateFew(10, _user, company, cafe);
            for (int i = 0; i < 10; i++)
            {
                companyOrders[i].AutoCloseDate = DateTime.Now.AddDays(1);
                companyOrders[i].OpenDate = DateTime.Now.AddDays(-1);
                companyOrders[i].CompanyId = company.Id;
            }
            //Дата открытия больше, чем дата закрытия, он не должен попасть
            companyOrders[0].OpenDate = DateTime.Now.AddDays(2);
            //Этот заказ удален
            companyOrders[1].IsDeleted = true;
            //Поставим одному другую компанию
            companyOrders[2].CompanyId = company.Id + 1;
            var responce = _controller.GetAvailableCompanyOrders();
            var result = TransformResult.GetObject<List<CompanyOrderModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, итого было 10 заказов, 1 не прошел по дате, 1 удален, 1 у чужой компании
            Assert.IsTrue(result.Count == 7);
        }

        [Test]
        public void GetAvailableCompanyOrdersToUserTest()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Если у пользователя роль менеджер, то даем ему в управление кафе
            if (Thread.CurrentPrincipal.IsInRole(EnumUserRole.Manager))
                _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);

            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказы компании
            List<CompanyOrder> companyOrders = CompanyOrderFactory.CreateFew(10, _user, company, cafe);
            for (int i = 0; i < 10; i++)
            {
                companyOrders[i].AutoCloseDate = DateTime.Now.AddDays(1);
                companyOrders[i].OpenDate = DateTime.Now.AddDays(-1);
                companyOrders[i].CompanyId = company.Id;
            }
            //Дата открытия больше, чем дата закрытия, он не должен попасть
            companyOrders[0].OpenDate = DateTime.Now.AddDays(2);
            //Этот заказ удален
            companyOrders[1].IsDeleted = true;
            //Поставим одному другую компанию
            companyOrders[2].CompanyId = company.Id + 1;
            var responce = _controller.GetAvailableCompanyOrdersToUser(_user.Id, 1);
            var result = TransformResult.GetObject<List<CompanyOrderModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, итого было 10 заказов, 1 не прошел по дате, 1 удален, 1 у чужой компании
            Assert.AreEqual(7, result.Count);
        }

        [Test]
        public void CheckUserIsEmployeeTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказы компании
            List<CompanyOrder> companyOrders = CompanyOrderFactory.CreateFew(10, _user, company, cafe);
            for (int i = 0; i < 10; i++)
            {
                companyOrders[i].AutoCloseDate = DateTime.Now;
                companyOrders[i].OpenDate = DateTime.Now;
                companyOrders[i].CompanyId = company.Id;
            }
            //Дата открытия больше, чем дата закрытия, он не должен попасть
            companyOrders[0].OpenDate = DateTime.Now.AddDays(2);
            //Этот заказ удален
            companyOrders[1].IsDeleted = true;
            //Поставим одному другую компанию
            companyOrders[2].CompanyId = company.Id + 1;
            var responce = _controller.CheckUserIsEmployee(cafe.Id, _user.Id, 1);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void GetAvailableUserOrderFromCompanyOrderTest_Consolidator()
        {
            SetUp(EnumUserRole.Consolidator);
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказ компании
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, company, cafe);
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);
            for (int i = 0; i < 10; i++)
                orders[i].CompanyOrderId = companyOrder.Id;

            var responce = _controller.GetAvailableUserOrderFromCompanyOrder(companyOrder.Id);
            var result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void GetCompanyOrderTest_User()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказ компании
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, company, cafe);

            var responce = _controller.GetCompanyOrder(companyOrder.Id);
            var result = TransformResult.GetObject<CompanyOrderModel>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetCompanyOrderTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Если менеджер, то добавляем кафе в управление
            if (Thread.CurrentPrincipal.IsInRole(EnumUserRole.Manager))
                _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем заказ компании
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, company, cafe);

            var responce = _controller.GetCompanyOrder(companyOrder.Id);
            var result = TransformResult.GetObject<CompanyOrderModel>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetCompanyOrderScheduleByIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписание
            CompanyOrderSchedule companyOrderSchedule =
                CompanyOrderScheduleFactory.Create(_user, company, cafe);
            CompanyOrderScheduleModel scheduleModel =
                companyOrderSchedule.GetContract();

            var responce = _controller.GetCompanyOrderScheduleById(companyOrderSchedule.Id);
            var result = TransformResult.GetObject<CompanyOrderScheduleModel>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            Assert.IsTrue(scheduleModel.Id == result.Id);
        }

        [Test]
        public void GetListOfCompanyOrderByDateTest_Admin()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            var startDate = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var endDate = DateTime.Now.AddDays(5).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Создаем заказы компании
            List<CompanyOrder> companyOrders = CompanyOrderFactory.CreateFew(10, _user, company, cafe);
            for (int i = 0; i < 10; i++)
            {
                companyOrders[i].CreationDate = DateTime.Now.AddDays(1);
                companyOrders[i].CompanyId = company.Id;
            }
            //По дате исключаем этот заказ
            companyOrders[0].CreationDate = DateTime.Now.AddDays(1111);
            var responce = _controller.GetListOfCompanyOrderByDate(company.Id, (long)startDate, (long)endDate, cafe.Id);
            var result = TransformResult.GetObject<List<CompanyOrderModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //в списке что-то должно быть
            Assert.IsTrue(result.Count > 0);

            //Итого 10, 1 исключаем по дате создания
            Assert.IsTrue(result.Count == 9);
        }

        [Test]
        public void GetListOfCompanyOrderScheduleTest_Admin()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            var startDate = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var endDate = DateTime.Now.AddDays(5).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Создаем расписания
            List<CompanyOrderSchedule> companyOrderSchedules = CompanyOrderScheduleFactory.CreateFew(10, _user, company);
            for (int i = 0; i < 10; i++)
            {
                companyOrderSchedules[i].BeginDate = DateTime.Now;
                companyOrderSchedules[i].EndDate = DateTime.Now.AddDays(2);
                companyOrderSchedules[i].CompanyId = company.Id;
                companyOrderSchedules[i].CafeId = cafe.Id;
            }
            //По дате исключаем это расписание
            companyOrderSchedules[0].BeginDate = DateTime.Now.AddDays(-1111);
            //По дате исключаем это расписание
            companyOrderSchedules[1].EndDate = DateTime.Now.AddDays(1111);
            var responce = _controller.GetListOfCompanyOrderSchedule(company.Id, (long)startDate, (long)endDate, cafe.Id);
            var result = TransformResult.GetObject<List<CompanyOrderScheduleModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //в списке что-то должно быть
            Assert.IsTrue(result.Count > 0);

            //Итого 10 расписаний, 2 исключаем по дате создания и по дате окончания
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void GetListOfCompanyOrderScheduleByCafeIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписания
            List<CompanyOrderSchedule> companyOrderSchedules = CompanyOrderScheduleFactory.CreateFew(10, _user, company);
            for (int i = 0; i < 10; i++)
            {
                companyOrderSchedules[i].CafeId = cafe.Id;
            }
            //Присваиваем чужому кафе
            companyOrderSchedules[0].CafeId = cafe.Id + 5;
            //Делаем неактивным
            companyOrderSchedules[1].IsActive = false;
            var responce = _controller.GetListOfCompanyOrderScheduleByCafeId(cafe.Id);
            var result = TransformResult.GetObject<List<CompanyOrderScheduleModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //в списке что-то должно быть
            Assert.IsTrue(result.Count > 0);

            //Итого 10 заказов, 1 чужого кафе, второй неактивный
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void GetListOfCompanyOrderScheduleByCompanyId()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписания
            List<CompanyOrderSchedule> companyOrderSchedules = CompanyOrderScheduleFactory.CreateFew(10, _user, company);
            for (int i = 0; i < 10; i++)
            {
                companyOrderSchedules[i].CompanyId = company.Id;
            }
            //Присваиваем чужому кафе
            companyOrderSchedules[0].CompanyId = company.Id + 5;
            //Делаем неактивным
            companyOrderSchedules[1].IsActive = false;
            var responce = _controller.GetListOfCompanyOrderScheduleByCompanyId(company.Id);
            var result = TransformResult.GetObject<List<CompanyOrderScheduleModel>>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //в списке что-то должно быть
            Assert.IsTrue(result.Count > 0);

            //Итого 10 заказов, 1 чужой компании, второй неактивный
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void RemoveCompanyOrderTest_Admin()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем заказ в кафе
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, null, cafe);

            var responce = _controller.RemoveCompanyOrder(companyOrder.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что удалился заказ
            Assert.IsTrue(result);
        }

        [Test]
        public async Task RemoveCompanyOrderScheduleTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем компанию для заказов:
            Company company = CompanyFactory.Create(_user);
            //Создаем расписание
            CompanyOrderSchedule companyOrderSchedule =
                CompanyOrderScheduleFactory.Create(_user, company, cafe);

            var responce = await _controller.RemoveCompanyOrderSchedule(companyOrderSchedule.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Проверяем, что результат не нулл
            Assert.IsNotNull(result);

            //Проверяем, что удалился
            Assert.IsTrue(result);
        }
    }
}
