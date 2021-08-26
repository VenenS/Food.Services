using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;


namespace AccessorTests.Entites
{
    [TestFixture]
    public class CompanyOrderScheduleTests
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
        public void GetCompanyOrderScheduleByCafeId_Success()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderScheduleByCafeId(schedule.CafeId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Company.Name == schedule.Company.Name);
        }

        [Test]
        public void GetCompanyOrderScheduleByCafeId_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.GetCompanyOrderScheduleByCafeId(schedule.CafeId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByCafeId_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsActive = false;
            var result = Accessor.Instance.GetCompanyOrderScheduleByCafeId(schedule.CafeId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByCompanyId_Success()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderScheduleByCompanyId(schedule.CompanyId);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Company.Name == schedule.Company.Name);
        }

        [Test]
        public void GetCompanyOrderScheduleByCompanyId_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.GetCompanyOrderScheduleByCompanyId(schedule.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByCompanyId_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsActive = false;
            var result = Accessor.Instance.GetCompanyOrderScheduleByCompanyId(schedule.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderSchedules_Success()
        {
            SetUp();
            var schedules = CompanyOrderScheduleFactory.CreateFew();
            var result = Accessor.Instance.GetCompanyOrderSchedules();
            Assert.IsTrue(result.Sum(e => e.Id) >= schedules.Sum(e => e.Id));
        }

        [Test]
        public void GetCompanyOrderSchedules_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsActive = false;
            var scheduleActive = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderSchedules();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Company.Name == scheduleActive.Company.Name);
        }

        [Test]
        public void GetCompanyOrderSchedules_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var scheduleActive = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderSchedules();
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().Company.Name == scheduleActive.Company.Name);
        }

        [Test]
        public void AddCompanyOrderSchedule_Success()
        {
            SetUp();
            var schedule = new CompanyOrderSchedule();
            schedule.Id = -1;
            var result = Accessor.Instance.AddCompanyOrderSchedule(schedule);
            Assert.IsTrue(schedule.Id >= 0);
            Assert.IsTrue(schedule.Id == result);
        }

        [Test]
        public void EditCompanyOrderSchedule_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.EditCompanyOrderSchedule(schedule);
            Assert.IsTrue(result.Item1 == -1);
        }

        [Test]
        public void EditCompanyOrderSchedule_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsActive = false;
            var result = Accessor.Instance.EditCompanyOrderSchedule(schedule);
            Assert.IsTrue(result.Item1 == -1);
        }

        [Test]
        public void EditCompanyOrderSchedule_Success_Dates()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var model = CompanyOrderScheduleFactory.Clone(schedule);
            model.BeginDate = schedule.BeginDate.Value.AddDays(1);
            model.EndDate = schedule.EndDate.Value.AddDays(-1);
            var result = Accessor.Instance.EditCompanyOrderSchedule(model);
            Assert.IsTrue(result.Item1 == schedule.Id);
            Assert.IsTrue(model.BeginDate == schedule.BeginDate);
            Assert.IsTrue(model.EndDate == schedule.EndDate);
        }

        [Test]
        public void EditCompanyOrderSchedule_Success_TimeStamps()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var model = CompanyOrderScheduleFactory.Clone(schedule);
            var difference = new TimeSpan(0, 0, 15);
            model.OrderStopTime = schedule.OrderStopTime.Add(difference);
            model.OrderSendTime = schedule.OrderSendTime.Add(difference);
            model.OrderStartTime = schedule.OrderStartTime.Add(difference);
            var result = Accessor.Instance.EditCompanyOrderSchedule(model);
            Assert.IsTrue(result.Item1 == schedule.Id);
            Assert.IsTrue(model.OrderStopTime == schedule.OrderStopTime);
            Assert.IsTrue(model.OrderSendTime == schedule.OrderSendTime);
            Assert.IsTrue(model.OrderStartTime == schedule.OrderStartTime);
        }

        [Test]
        public void EditCompanyOrderSchedule_Success_Ids()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var model = CompanyOrderScheduleFactory.Clone(schedule);
            model.CafeId = _random.Next();
            model.CompanyId = _random.Next();
            model.LastUpdateByUserId = _random.Next();
            var result = Accessor.Instance.EditCompanyOrderSchedule(model);
            Assert.IsTrue(result.Item1 == schedule.Id);
            Assert.IsTrue(model.CafeId == schedule.CafeId);
            Assert.IsTrue(model.CompanyId == schedule.CompanyId);
            Assert.IsTrue(model.LastUpdateByUserId == schedule.LastUpdateByUserId);
        }

        [Test]
        public void EditCompanyOrderSchedule_Success_State()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var model = CompanyOrderScheduleFactory.Clone(schedule);
            model.IsActive = !schedule.IsActive;
            model.CompanyDeliveryAdress = _random.Next();
            var result = Accessor.Instance.EditCompanyOrderSchedule(model);
            Assert.IsTrue(result.Item1 == schedule.Id);
            Assert.IsTrue(model.IsActive == schedule.IsActive);
            Assert.IsTrue(model.CompanyDeliveryAdress == schedule.CompanyDeliveryAdress);
            Assert.IsTrue(schedule.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void RemoveCompanyOrderSchedule_Success()
        {
            SetUp();
            var authorId = _random.Next();
            var schedule = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.RemoveCompanyOrderSchedule(schedule.Id, authorId);
            Assert.IsTrue(result.Item1 == true);
            Assert.IsTrue(schedule.IsDeleted == true);
            Assert.IsTrue(schedule.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
            Assert.IsTrue(schedule.LastUpdateByUserId == authorId);
        }

        [Test]
        public void RemoveCompanyOrderSchedule_Only_Alive()
        {
            SetUp();
            var authorId = _random.Next();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.RemoveCompanyOrderSchedule(schedule.Id, authorId);
            Assert.IsTrue(result.Item1 == false);
        }

        [Test]
        public void RemoveCompanyOrderSchedule_Only_Active()
        {
            SetUp();
            var authorId = _random.Next();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsActive = false;
            var result = Accessor.Instance.RemoveCompanyOrderSchedule(schedule.Id, authorId);
            Assert.IsTrue(result.Item1 == false);
        }

        [Test]
        public void GetCompanyOrderScheduleById_Success()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderScheduleById(schedule.Id);
            Assert.IsTrue(schedule.Company.Name == result.Company.Name);
        }

        [Test]
        public void GetCompanyOrderScheduleById_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.GetCompanyOrderScheduleById(schedule.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetCompanyOrderScheduleById_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create(); ;
            schedule.IsActive = false;
            var result = Accessor.Instance.GetCompanyOrderScheduleById(schedule.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Success()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, schedule.CafeId, schedule.BeginDate.Value, schedule.EndDate.Value);
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(result.FirstOrDefault(e => e.Company.Name == schedule.Company.Name));
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Right_BeginDate()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.BeginDate = DateTime.Now;
            var beginDate = schedule.BeginDate.Value.AddDays(1);
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, schedule.CafeId, beginDate, schedule.EndDate.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Right_EndDate()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.EndDate = DateTime.Now;
            var endDate = schedule.EndDate.Value.AddDays(-1);
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, schedule.CafeId, schedule.BeginDate.Value, endDate);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Right_Cafe()
        {
            SetUp();
            var cafeId = _random.Next();
            var schedule = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, cafeId, schedule.BeginDate.Value, schedule.EndDate.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Only_Alive()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, schedule.CafeId, schedule.BeginDate.Value, schedule.EndDate.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetCompanyOrderScheduleByRangeDate_Only_Active()
        {
            SetUp();
            var schedule = CompanyOrderScheduleFactory.Create(); ;
            schedule.IsActive = false;
            var result = Accessor.Instance.GetCompanyOrderScheduleByRangeDate(schedule.CompanyId, schedule.CafeId, schedule.BeginDate.Value, schedule.EndDate.Value);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDateTest()
        {

        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Success()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(result.FirstOrDefault(e => e.Company.Name == schedule.Company.Name));
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Right_BeginDate()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.BeginDate = date.AddDays(1);
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Right_EndDate()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.EndDate = date.AddDays(-1);
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Cafe_Active()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.Cafe.IsActive = false;
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Cafe_Alive()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.Cafe.IsDeleted = true;
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Only_Alive()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create();
            schedule.IsDeleted = true;
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetAllCompanyOrderScheduleByDate_Only_Active()
        {
            SetUp();
            var date = DateTime.Now;
            var schedule = CompanyOrderScheduleFactory.Create(); ;
            schedule.IsActive = false;
            var result = Accessor.Instance.GetAllCompanyOrderScheduleByDate(date);
            Assert.IsTrue(result.Count == 0);
        }
    }
}