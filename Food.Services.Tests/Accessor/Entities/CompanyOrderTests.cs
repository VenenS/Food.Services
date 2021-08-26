using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using ITWebNet.FoodService.Food.DbAccessor;
using Moq;
using NUnit.Framework;

namespace AccessorTests.Entites
{
    [TestFixture]
    public class CompanyOrderTests
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
        public void AddCompanyOrderTest()
        {
        }

        [Test]
        public void EditCompanyOrderTest()
        {
            
        }

        [Test]
        public void GetAllCompanyOrdersGreaterDateTest()
        {
            SetUp();
            var company = CompanyFactory.Create();
            CompanyOrderFactory.CreateFew(company: company);
            var order = CompanyOrderFactory.Create(company: company);
            order.OpenDate = DateTime.Now.AddDays(2);
            CompanyOrderFactory.CreateFew(company: company);
            var result = Accessor.Instance.GetAllCompanyOrdersGreaterDate(DateTime.Now.AddDays(1));
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().ContactEmail == order.ContactEmail);
        }

        [Test]
        public void GetAvailableCompanyOrdersForTimeTest()
        {
            SetUp();
            var company = CompanyFactory.Create();
            CompanyOrderFactory.CreateFew(company: company);
            var order = CompanyOrderFactory.Create(company: company);
            order.AutoCloseDate = DateTime.Now.AddDays(2);
            CompanyOrderFactory.CreateFew(company: company);
            var result = Accessor.Instance.GetAvailableCompanyOrdersForTime(company.Id, DateTime.Now.AddDays(1));
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().ContactEmail == order.ContactEmail);
        }

        [Test]
        public void GetCompanyOrderByIdTest()
        {
            SetUp();
            var order = CompanyOrderFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderById(order.Id);
            Assert.IsTrue(order.ContactEmail == result.ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByDateTest_Only_Right_StartDate()
        {

            SetUp();
            var startDate = DateTime.Now;
            var endDate = DateTime.MaxValue;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(company:order.Company);
            order.CreationDate = startDate.AddHours(1);
            temp.CreationDate = startDate.AddHours(-1);
            var result = Accessor.Instance.GetCompanyOrdersByDate(order.CompanyId, null, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByDateTest_Only_Right_EndDate()
        {

            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.Now;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(company: order.Company);
            order.CreationDate = endDate.AddHours(-1);
            temp.CreationDate = endDate.AddHours(1);
            var result = Accessor.Instance.GetCompanyOrdersByDate(order.CompanyId, null, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByDateTest_Concrete_Cafe()
        {

            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = CompanyOrderFactory.Create();
            CompanyOrderFactory.Create(company: order.Company);
            var result = Accessor.Instance.GetCompanyOrdersByDate(order.CompanyId, order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByDateTest_Only_Alive()
        {

            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(company: order.Company);
            temp.IsDeleted = true;
            var result = Accessor.Instance.GetCompanyOrdersByDate(order.CompanyId, null, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByFilterTest()
        {
            
        }

        [Test]
        public void GetCompanyOrdersTest()
        {
            SetUp();
            var company = CompanyFactory.Create();
            var orders = CompanyOrderFactory.CreateFew(company: company);
            var result = Accessor.Instance.GetCompanyOrders(company.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == orders.Sum(e => e.Id));
        }

        [Test]
        public void GetUserOrderFromCompanyOrderTest()
        {
            SetUp();
            var order = OrderFactory.Create();
            order.CompanyOrderId = _random.Next();
            var result = Accessor.Instance.GetUserOrderFromCompanyOrder(order.CompanyOrderId.Value);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.Comment == result.First().Comment);
        }

        [Test]
        public void GetCompaniesByCompanyOrders_Only_Right_StartDate()
        {
            SetUp();
            var startDate = DateTime.Now;
            var endDate = DateTime.MaxValue;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(company: order.Company, cafe: order.Cafe);
            order.DeliveryDate = startDate.AddHours(1);
            temp.DeliveryDate = startDate.AddHours(-1);
            var result = Accessor.Instance.GetCompaniesByCompanyOrders(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.Company.FullName == result.First().FullName);
        }

        [Test]
        public void GetCompaniesByCompanyOrders_Only_Right_EndDate()
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.Now;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(company: order.Company, cafe: order.Cafe);
            order.DeliveryDate = endDate.AddHours(-1);
            temp.DeliveryDate = endDate.AddHours(1);
            var result = Accessor.Instance.GetCompaniesByCompanyOrders(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.Company.FullName == result.First().FullName);
        }

        [Test]
        public void GetCompaniesByCompanyOrders_Only_Alive()
        {
            SetUp();
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var order = CompanyOrderFactory.Create();
            var temp = CompanyOrderFactory.Create(cafe: order.Cafe);
            temp.IsDeleted = true;
            var result = Accessor.Instance.GetCompaniesByCompanyOrders(order.CafeId, startDate, endDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(order.Company.FullName == result.First().FullName);
        }

        [Test]
        public void GetCompanyOrdersByFilter_Only_Right_StartDate()
        {
            SetUp();
            var companyOrder = CompanyOrderFactory.Create();
            companyOrder.Orders = OrderFactory.CreateFew();
            var filter = new ReportFilter();
            filter.StartDate = DateTime.Now;
            filter.EndDate = DateTime.MaxValue;
            filter.CafeId = companyOrder.CafeId;
            filter.CompanyId = companyOrder.CompanyId;
            filter.CompanyOrdersIdList = new List<long>();
            var temp = CompanyOrderFactory.Create(company: companyOrder.Company, cafe: companyOrder.Cafe);
            companyOrder.DeliveryDate = filter.StartDate.AddHours(1);
            temp.DeliveryDate = filter.StartDate.AddHours(-1);
            var result = Accessor.Instance.GetCompanyOrdersByFilter(filter);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(companyOrder.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByFilter_Only_Right_EndDate()
        {
            SetUp();
            var companyOrder = CompanyOrderFactory.Create();
            companyOrder.Orders = OrderFactory.CreateFew();
            var filter = new ReportFilter();
            filter.StartDate = DateTime.MinValue;
            filter.EndDate = DateTime.Now;
            filter.CafeId = companyOrder.CafeId;
            filter.CompanyId = companyOrder.CompanyId;
            filter.CompanyOrdersIdList = new List<long>();
            var temp = CompanyOrderFactory.Create(company: companyOrder.Company, cafe: companyOrder.Cafe);
            companyOrder.DeliveryDate = filter.EndDate.AddHours(-1);
            temp.DeliveryDate = filter.EndDate.AddHours(1);
            var result = Accessor.Instance.GetCompanyOrdersByFilter(filter);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(companyOrder.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByFilter_Concrete_Cafe()
        {
            SetUp();
            var companyOrder = CompanyOrderFactory.Create();
            companyOrder.Orders = OrderFactory.CreateFew();
            CompanyOrderFactory.Create();
            var filter = new ReportFilter();
            filter.StartDate = DateTime.MinValue;
            filter.EndDate = DateTime.MinValue;
            filter.CafeId = companyOrder.CafeId;
            filter.CompanyId = companyOrder.CompanyId;
            filter.CompanyOrdersIdList = new List<long>();
            var result = Accessor.Instance.GetCompanyOrdersByFilter(filter);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(companyOrder.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void GetCompanyOrdersByFilter_Only_Alive()
        {
            SetUp();
            var companyOrder = CompanyOrderFactory.Create();
            companyOrder.Orders = OrderFactory.CreateFew();
            var temp = CompanyOrderFactory.Create(company: companyOrder.Company, cafe:companyOrder.Cafe);
            temp.IsDeleted = true;
            var filter = new ReportFilter();
            filter.StartDate = DateTime.MinValue;
            filter.EndDate = DateTime.MinValue;
            filter.CafeId = companyOrder.CafeId;
            filter.CompanyId = companyOrder.CompanyId;
            filter.CompanyOrdersIdList = new List<long>();
            var result = Accessor.Instance.GetCompanyOrdersByFilter(filter);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(companyOrder.ContactEmail == result.First().ContactEmail);
        }

        [Test]
        public void RemoveCompanyOrder_Success()
        {
            SetUp();
            var order = CompanyOrderFactory.Create();
            var authorId = _random.Next();
            var result = Accessor.Instance.RemoveCompanyOrder(order.Id, authorId);
            Assert.IsTrue(result == true);
            Assert.IsTrue(order.IsDeleted == true);
            Assert.IsTrue(order.LastUpdate.Value.ToString("g") == DateTime.Now.ToString("g"));
            Assert.IsTrue(order.LastUpdateByUserId == authorId);
        }

        [Test]
        public void RemoveCompanyOrder_Only_Alive()
        {
            SetUp();
            var order = CompanyOrderFactory.Create();
            order.IsDeleted = true;
            var authorId = _random.Next();
            var result = Accessor.Instance.RemoveCompanyOrder(order.Id, authorId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CreateCompanyOrdersForCafeTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var companies = CompanyFactory.CreateFew(3);
            var schedules = companies.Select(x => CompanyOrderScheduleFactory.Create(null, x, cafe)).ToList();
            var companiesWithNoOrders = new List<Company> { };

            cafe.CafeAvailableOrdersType = "COMPANY_ONLY";

            // Создать компанию без расписания и с просроченным расписанием.
            {
                var companyWithoutSched = CompanyFactory.Create();
                var companyWithExpiredSched = CompanyFactory.Create();
                var sched = CompanyOrderScheduleFactory.Create(null, companyWithExpiredSched, cafe);

                sched.BeginDate = DateTime.MinValue;
                sched.EndDate = sched.BeginDate.Value.AddDays(360);

                companiesWithNoOrders.Add(companyWithoutSched);
                companiesWithNoOrders.Add(companyWithExpiredSched);
            }

            // Кафе не позволяет заказать на неделю.
            cafe.WeekMenuIsActive = false;
            Accessor.Instance.CreateCompanyOrders(cafe.Id, null);
            companies.ForEach(x => Assert.AreEqual(1, Accessor.Instance.GetCompanyOrders(x.Id).Count, $"Bad order count (cid={x.Id})"));

            // Кафе позволяет заказать на неделю.
            cafe.WeekMenuIsActive = true;
            Accessor.Instance.CreateCompanyOrders(cafe.Id, null);
            companies.ForEach(x => Assert.IsTrue(Accessor.Instance.GetCompanyOrders(x.Id).Count > 1));

            // Отменить заказы 1 компании и проверить создаются ли новые заказы вместо отмененных.
            {
                var orders = Accessor.Instance.GetCompanyOrders(companies[0].Id);
                Assert.IsTrue(orders.Count > 0);

                Accessor.Instance.CancelCompanyOrdersBetween(
                    orders[0].OpenDate.Value.Date, orders[orders.Count - 1].AutoCloseDate.Value.Date,
                    companies[0].Id, null, 0);

                Accessor.Instance
                    .GetCompanyOrders(companies[0].Id)
                    .ForEach(x => Assert.AreEqual(x.State, (long)EnumOrderState.Abort));

                Accessor.Instance.CreateCompanyOrders(null, companies[0].Id);

                orders = Accessor.Instance
                    .GetCompanyOrders(companies[0].Id)
                    .Where(x => x.State != (long)EnumOrderState.Abort)
                    .ToList();
                Assert.IsTrue(orders.Count > 0);
            }

            companiesWithNoOrders.ForEach(x => Assert.AreEqual(Accessor.Instance.GetCompanyOrders(x.Id).Count, 0));
        }

        [Test]
        public void RemoveCompanyOrdersBetweenTest()
        {
            SetUp();
            var now = DateTime.Now.Date;
            var cafe = CafeFactory.Create();
            var pastOrders = CompanyOrderFactory.CreateFew(10, cafe: cafe);
            var futureOrders = CompanyOrderFactory.CreateFew(5, cafe: cafe);
            var random = new Random();

            for (var i = 0; i < pastOrders.Count; i++)
            {
                pastOrders[i].OpenDate = DateTime.Now.AddDays(-i - 1).Date + new TimeSpan(0, 7, 0);
            }

            for (var i = 0; i < futureOrders.Count; i++)
            {
                var order = futureOrders[i];

                order.OpenDate = DateTime.Now.AddDays(i + 1).Date + new TimeSpan(0, 7, 0);
                order.AutoCloseDate = order.OpenDate.Value.Date + new TimeSpan(0, 22, 0);
                order.OrderCreateDate = order.OpenDate + new TimeSpan(0, 0, 0);
                order.DeliveryDate = order.OpenDate.Value.Date + new TimeSpan(0, 23, 0);
            }

            foreach (var order in pastOrders.Concat(futureOrders))
            {
                order.State = (long)EnumOrderStatus.Created;
                order.AutoCloseDate = order.OpenDate.Value.Date + new TimeSpan(0, 22, 0);
                order.OrderCreateDate = order.OpenDate + new TimeSpan(0, 0, 0);

                var individualOrders = OrderFactory.CreateFew(random.Next(5), cafe: cafe);
                foreach (var i in individualOrders)
                    i.CompanyOrderId = order.Id;
                order.Orders = individualOrders;
            }

            Accessor.Instance.CancelCompanyOrdersBetween(now, DateTime.MaxValue, null, cafe.Id, 0);

            foreach (var order in pastOrders)
            {
                Assert.AreNotEqual(order.State, (long)EnumOrderState.Abort);
                foreach (var i in order.Orders)
                    Assert.AreNotEqual(order.State, (long)EnumOrderState.Abort);
            }

            foreach (var order in futureOrders)
            {
                Assert.AreEqual(order.State, (long)EnumOrderState.Abort);
                foreach (var i in order.Orders)
                    Assert.AreEqual(order.State, (long)EnumOrderState.Abort);
            }
        }

        [Test]
        public void CalculateShippingCostsForCompanyOrderTest()
        {
            // Инициализация тестовых данных.
            SetUp();
            var cafe = CafeFactory.Create();
            var company = CompanyFactory.Create();
            var companyOrder = CompanyOrderFactory.Create(company: company, cafe: cafe);
            var addrs = new List<string> { "ADDRESS_1", "ADDRESS_2" };

            CostOfDeliveryFactory.Create(CafeFactory.Create(), 100000, 0, int.MaxValue);
            CostOfDeliveryFactory.Create(cafe, 900, 0, 99);
            CostOfDeliveryFactory.Create(cafe, 750, 100, 199);
            CostOfDeliveryFactory.Create(cafe, 600, 200, int.MaxValue);

            Order CreateOrder(string addr, IEnumerable<double> items)
            {
                var info = OrderInfoFactory.CreateVirtual();
                info.OrderAddress = addr;
                var order = OrderFactory.Create(cafe: cafe, companyOrder: companyOrder, orderInfo: info);
                if (order.OrderItems == null)
                    order.OrderItems = new List<OrderItem>();
                foreach (var price in items)
                {
                    var item = OrderItemFactory.Create(order: order);
                    item.TotalPrice = price;
                    order.OrderItems.Add(item);
                }
                return order;
            }

            void CalcAndVerify(Action<Dictionary<string, Tuple<int, double>>> fn)
            {
                var result = Accessor.Instance
                    .CalculateShippingCostsForCompanyOrder(companyOrder.Id)
                    .ToDictionary(x => x.Item1, x => Tuple.Create(x.Item2, x.Item3));
                fn(result);
            }

            // Собственно сам тест.
            for (var i = 0; i < addrs.Count; ++i)
            {
                var addr = addrs[i];
                CalcAndVerify(x => Assert.AreEqual(i, x.Count));

                CreateOrder(addr, Enumerable.Repeat(50d, 1));
                CalcAndVerify(x => {
                    Assert.AreEqual(i + 1, x.Count);
                    Assert.IsTrue(x.ContainsKey(addr));
                    Assert.AreEqual(1, x[addr].Item1);
                    Assert.That(x[addr].Item2, Is.EqualTo(900d).Within(0.001d));
                });

                CreateOrder(addr, Enumerable.Repeat(12.5d, 2));
                CalcAndVerify(x => {
                    Assert.AreEqual(2, x[addr].Item1);
                    Assert.That(x[addr].Item2, Is.EqualTo(900d).Within(0.001d));
                });

                CreateOrder(addr, Enumerable.Repeat(25d, 3));
                CalcAndVerify(x => {
                    Assert.AreEqual(3, x[addr].Item1);
                    Assert.That(x[addr].Item2, Is.EqualTo(750d).Within(0.001d));
                });

                var order = CreateOrder(addr, Enumerable.Repeat(100d, 1));
                CalcAndVerify(x => {
                    Assert.AreEqual(4, x[addr].Item1);
                    Assert.That(x[addr].Item2, Is.EqualTo(600d).Within(0.001d));
                });

                // Отменить последний заказ.
                order.State = EnumOrderState.Abort;
                CalcAndVerify(x => {
                    Assert.AreEqual(3, x[addr].Item1);
                    Assert.That(x[addr].Item2, Is.EqualTo(750d).Within(0.001d));
                });
            }
        }
    }
}