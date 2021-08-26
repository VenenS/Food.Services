using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    public class XsltTests
    {
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;
        private readonly Random _rnd = new Random();

        [Test]
        public void GetXsltTest()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXslt(xslt.Id);
            Assert.AreSame(xslt, result);
        }

        [Test]
        public void GetXSLTTest_No_Such_Entity()
        {
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXslt(_rnd.Next(999, int.MaxValue));
            Assert.IsNull(result);
        }

        [Test]
        public void GetXSLTTest_Only_Alive()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            xslt.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXslt(xslt.Id);
            Assert.IsNull(result);
        }

        [Test]
        public void AddXsltTest()
        {
            var xslt = new ReportStylesheet
            {
                Id = -1
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddXslt(xslt);
            Assert.IsTrue(xslt.Id >= 0);
            Assert.IsTrue(result == xslt.Id);
        }

        [Test]
        public void EditXSLTTest_Only_Alive()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            xslt.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditXslt(xslt);
            Assert.IsFalse(result);
        }

        [Test]
        public void EditXSLTTest_No_Such_Entity()
        {
            var model = new ReportStylesheet
            {
                Id = _rnd.Next(999, int.MaxValue)
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditXslt(model);
            Assert.IsFalse(result);
        }

        [Test]
        public void EditXSLTTest_Success_Ids()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            var model = new ReportStylesheet
            {
                Id = xslt.Id,
                CafeId = _rnd.Next(),
                ExtId = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditXslt(model);
            Assert.IsTrue(result);
            Assert.IsTrue(model.CafeId == xslt.CafeId);
            Assert.IsTrue(model.ExtId == xslt.ExtId);
        }

        [Test]
        public void EditXSLTTest_Success__Name_Description_Transformation()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            var model = new ReportStylesheet
            {
                Id = xslt.Id,
                Name = Guid.NewGuid().ToString("N"),
                Description = Guid.NewGuid().ToString("N"),
                Transformation = Guid.NewGuid().ToString("N")
            };
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditXslt(model);
            Assert.IsTrue(model.Name == xslt.Name);
            Assert.IsTrue(model.Description == xslt.Description);
            Assert.IsTrue(model.Transformation == xslt.Transformation);
        }

        [Test]
        public void EditXSLTTest_Success_Log_Information()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            var model = new ReportStylesheet
            {
                ExtId = _rnd.Next(),
                LastUpdDate = DateTime.Now.AddDays(1),
                LastUpdateByUserId = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditXslt(model);
            Assert.IsTrue(result);
            Assert.IsTrue(model.LastUpdDate == xslt.LastUpdDate);
            Assert.IsTrue(model.LastUpdateByUserId == xslt.LastUpdateByUserId);
        }
        
        [Test]
        public void RemoveXSLTTest_Only_Alive()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            xslt.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveXslt(xslt);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveXSLTTest_No_Such_Entity()
        {
            var model = new ReportStylesheet
            {
                Id = _rnd.Next(999, int.MaxValue)
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveXslt(model);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveXSLTTest_Success()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            var model = new ReportStylesheet
            {
                ExtId = _rnd.Next(),
                LastUpdDate = DateTime.Now.AddDays(1),
                LastUpdateByUserId = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveXslt(model);
            Assert.IsTrue(result);
            Assert.IsTrue(xslt.IsDeleted);
            Assert.IsTrue(model.LastUpdDate == xslt.LastUpdDate);
            Assert.IsTrue(model.LastUpdateByUserId == xslt.LastUpdateByUserId);
        }

        [Test]
        public void GetXsltFromCafeTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var xslts = ReportStylesheetFactory.CreateFew(cafe: cafe);
            ReportStylesheetFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXsltFromCafe(cafe.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == xslts.Sum(e => e.Id));
        }

        [Test]
        public void GetXSLTFromCafeTest_No_Such_Cafe()
        {
            SetUp();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXsltFromCafe(_rnd.Next());
            Assert.IsTrue(!result.Any());
        }

        [Test]
        public void GetXSLTFromCafeTest_Get_Common()
        {
            SetUp();
            var xslt = ReportStylesheetFactory.Create();
            xslt.IsCommon = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetXsltFromCafe(_rnd.Next());
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().Name == xslt.Name);
        }
    }
}