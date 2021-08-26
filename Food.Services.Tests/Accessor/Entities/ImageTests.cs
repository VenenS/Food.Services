using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.Food.Core.DataContracts.Common;
using NUnit.Framework;
using System;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class ImageTests
    {
        FakeContext _context;
        User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        [Test]
        public void AddImage_Test()
        {
            var image = ImageFactory.Create();
            image.ObjectType = (int)ObjectTypesEnum.CAFE;
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddImage(image);
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Успешное удаление
        /// </summary>
        [Test]
        public void RemoveImage_Test1()
        {
            var lstImages = ImageFactory.CreateFew(count: 3);
            var delImage = lstImages.First();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveImage(delImage);
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Удаление несуществующего элемента
        /// </summary>
        [Test]
        public void RemoveImage_Test2()
        {
            var lstImages = ImageFactory.CreateFew(count: 3);
            var delImage = ImageFactory.Create();
            delImage.IsDeleted = true;
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveImage(delImage);
            //
            Assert.IsFalse(response);
        }

        [Test]
        public void GetImages_Test()
        {
            var lstImages = ImageFactory.CreateFew(count: 3);
            var checkImage = lstImages.First();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetImages(checkImage.ObjectId, checkImage.ObjectType);
            //
            Assert.IsTrue(response.Count == 1);
        }
    }
}
