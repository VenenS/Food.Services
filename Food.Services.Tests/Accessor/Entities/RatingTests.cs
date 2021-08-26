using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;

namespace AccessorTests.Entites
{
    [TestFixture]
    public class RatingTests
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
        public void GetAllRatingFromUser_Without_Filter()
        {
            SetUp();
            RatingFactory.Create();
            var rating = RatingFactory.Create();
            var anyInt = _random.Next();
            var result = Accessor.Instance.GetAllRatingFromUser(rating.UserId, anyInt, false);
            Assert.True(result.Count == 1);
            Assert.IsTrue(result.First().CreatorId == rating.CreatorId);
        }

        [Test]
        public void GetAllRatingFromUserWithout_Only_Alive()
        {
            SetUp();
            RatingFactory.Create();
            var rating = RatingFactory.Create();
            rating.IsDeleted = true;
            var anyInt = _random.Next();
            var result = Accessor.Instance.GetAllRatingFromUser(rating.UserId, anyInt, false);
            Assert.True(!result.Any());
        }

        [Test]
        public void GetAllRatingFromUser_With_Filter_Only_Alive()
        {
            SetUp();
            var temp = RatingFactory.Create();
            var rating = RatingFactory.Create(temp.User);
            rating.IsDeleted = true;
            var result = Accessor.Instance.GetAllRatingFromUser(rating.UserId, rating.ObjectType, true);
            Assert.True(!result.Any());
        }

        [Test]
        public void GetAllRatingFromUser_With_Filter_Success()
        {
            SetUp();
            var temp = RatingFactory.Create();
            var rating = RatingFactory.Create(temp.User);
            var result = Accessor.Instance.GetAllRatingFromUser(rating.UserId, rating.ObjectType, true);
            Assert.True(result.Count == 1);
            Assert.IsTrue(result.First().CreatorId == rating.CreatorId);
        }

        [Test]
        public void GetAllRatingToObjectTest_Success()
        {
            SetUp();
            var temp = RatingFactory.Create();
            var rating = RatingFactory.Create(temp.User);
            var result = Accessor.Instance.GetAllRatingToObject(rating.ObjectId, rating.ObjectType);
            Assert.True(result.Count == 1);
            Assert.IsTrue(result.First().CreatorId == rating.CreatorId);
        }

        [Test]
        public void GetAllRatingToObjectTest_Only_Alive()
        {
            SetUp();
            var temp = RatingFactory.Create();
            var rating = RatingFactory.Create(temp.User);
            rating.IsDeleted = true;
            var result = Accessor.Instance.GetAllRatingToObject(rating.ObjectId, rating.ObjectType);
            Assert.True(!result.Any());
        }

        [Test]
        public void InsertNewRatingTest_Simple_Success()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var temp = RatingFactory.Create();
            var model = new Rating();
            model.UserId = temp.UserId;
            model.ObjectType = (int)ObjectTypesEnum.CAFE;
            model.ObjectId = cafe.Id;
            model.RatingValue = _random.Next();
            var result =
                Accessor.Instance.InsertNewRating(model.UserId, model.ObjectId, model.ObjectType, model.RatingValue);
            var added = ContextManager.Get().Rating.First(e => e.Id == result);
            Assert.IsTrue(added.RatingValue == model.RatingValue);
        }

        [Test]
        public void GetFinalRateToDishTest()
        {
            SetUp();
            var dish = DishFactory.Create();
            dish.DishRatingCount = 2;
            dish.DishRatingSumm = 10;
            var expected = dish.DishRatingSumm / dish.DishRatingCount;
            var result = Accessor.Instance.GetFinalRateToDish(dish.Id);
            Assert.True(Math.Abs(result - expected) < 0.1);
        }

        [Test]
        public void GetFinalRateToCafeTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.CafeRatingCount = 2;
            cafe.CafeRatingSumm = 10;
            var expected = cafe.CafeRatingSumm / cafe.CafeRatingCount;
            var result = Accessor.Instance.GetFinalRateToCafe(cafe.Id);
            Assert.True(Math.Abs(result - expected) < 0.1);
        }
    }
}