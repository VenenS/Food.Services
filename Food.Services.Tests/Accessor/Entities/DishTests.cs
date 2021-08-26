using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using NUnit.Framework;

namespace ITWebNet.FoodService.Food.DbAccessor.Tests
{
    [TestFixture]
    public class DishTests
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
        public void CheckUniqueNameWithinCafeTest_Not_Unique()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(dish.DishName, dish.Cafe.Id);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Not_Unique_Instead_Of_UpperCase()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(dish.DishName.ToUpper(), dish.Cafe.Id);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Not_Unique_Instead_Of_Spaces()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(" " + dish.DishName + "  ", dish.Cafe.Id);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Unique()
        {
            SetUp();
            var dish = DishFactory.Create();
            var name = Guid.NewGuid().ToString("N");
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(name, dish.Cafe.Id);
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Unique_For_Cafe()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(dish.DishName, _random.Next(999, int.MaxValue));
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Unique_With_Id()
        {
            SetUp();
            DishFactory.Create();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.CheckUniqueNameWithinCafe(dish.DishName, dish.Cafe.Id, dish.Id);
            Assert.IsTrue(result);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_No_One()
        {
            SetUp();
            var result = Accessor.Instance.GetFoodDishesByCategoryId(_random.Next(999, int.MaxValue));
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_Success()
        {
            SetUp();
            var category = DishCategoryInCafeFactory.Create();
            var dish = DishFactory.Create(category: category);
            var result = Accessor.Instance.GetFoodDishesByCategoryId(category.Id);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().DishName == dish.DishName);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_Category_not_Active()
        {
            SetUp();
            var category = DishCategoryInCafeFactory.Create();
            var dish = DishFactory.Create(category: category);
            category.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCategoryId(category.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_Dish_not_Active()
        {
            SetUp();
            var category = DishCategoryInCafeFactory.Create();
            var dish = DishFactory.Create(category: category);
            dish.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCategoryId(category.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_Category_Deleted()
        {
            SetUp();
            var category = DishCategoryInCafeFactory.Create();
            var dish = DishFactory.Create(category: category);
            dish.IsDeleted = true;
            category.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCategoryId(category.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdTest_Dish_Deleted()
        {
            SetUp();
            var category = DishCategoryInCafeFactory.Create();
            var dish = DishFactory.Create(category: category);
            dish.IsDeleted = true;
            category.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCategoryId(category.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_Dish_Not_Active()
        {
            SetUp();
            var dish = DishFactory.Create();
            dish.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_CafeCategory_Not_Active()
        {
            SetUp();
            var dish = DishFactory.Create();
            //dish.CafeCategory.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_DishCategory_Not_Active()
        {
            SetUp();
            var dish = DishFactory.Create();
            //dish.CafeCategory.DishCategory.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_DishCategory_Deleted()
        {
            SetUp();
            var dish = DishFactory.Create();
            //dish.CafeCategory.DishCategory.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_CafeCategory_Deleted()
        {
            SetUp();
            var dish = DishFactory.Create();
            //dish.CafeCategory.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_Dish_Deleted()
        {
            SetUp();
            var dish = DishFactory.Create();
            dish.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_Success()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.GetFoodDishesByCafeId(dish.Cafe.Id);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dish.DishName));
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_WishDate_and_VersionFrom()
        {
            SetUp();
            var wishDate = DateTime.Now;
            var dishMinDate = DishFactory.Create();
            var dishMaxDate = DishFactory.Create(/*category: dishMinDate.CafeCategory*/);
            var dishNullDate = DishFactory.Create(/*category: dishMinDate.CafeCategory*/);
            dishMaxDate.VersionFrom = DateTime.MaxValue;
            dishMinDate.VersionFrom = DateTime.MinValue;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dishMinDate.Cafe.Id, wishDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dishMinDate.DishName));
        }

        [Test]
        public void GetFoodDishesByCafeIdTest_WishDate_VersionTo()
        {
            SetUp();
            var wishDate = DateTime.Now;
            var dishMinDate = DishFactory.Create();
            var dishMaxDate = DishFactory.Create(/*category: dishMinDate.CafeCategory*/);
            dishMaxDate.VersionFrom = DateTime.MinValue;
            dishMinDate.VersionFrom = DateTime.MinValue;
            dishMaxDate.VersionTo = DateTime.MinValue;
            var result = Accessor.Instance.GetFoodDishesByCafeId(dishMinDate.Cafe.Id, wishDate);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dishMinDate.DishName));
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeIdTest_Dish_Not_Active()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            dish.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_CafeCategory_Not_Active()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            //dish.CafeCategory.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_DishCategory_Not_Active()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            //dish.CafeCategory.DishCategory.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_DishCategory_Deleted()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            //dish.CafeCategory.DishCategory.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_CafeCategory_Deleted()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            //dish.CafeCategory.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_Dish_Deleted()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            dish.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_Success()
        {
            SetUp();
            var date = DateTime.Now;
            var dish = DishFactory.Create();
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dish.Cafe.Id,
                dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dish.DishName));
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_WishDate_and_VersionFrom()
        {
            SetUp();
            var date = DateTime.Now;
            var dishMinDate = DishFactory.Create();
            var dishMaxDate = DishFactory.Create(/*category: dishMinDate.CafeCategory*/);
            dishMaxDate.VersionFrom = DateTime.MaxValue;
            dishMinDate.VersionFrom = DateTime.MinValue;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dishMinDate.Cafe.Id,
                dishMinDate.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dishMinDate.DishName));
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeId_WishDate_VersionTo()
        {
            SetUp();
            var date = DateTime.Now;
            var dishMinDate = DishFactory.Create();
            var dishMaxDate = DishFactory.Create(/*category: dishMinDate.CafeCategory*/);
            dishMaxDate.VersionFrom = DateTime.MinValue;
            dishMinDate.VersionFrom = DateTime.MinValue;
            dishMaxDate.VersionTo = DateTime.MinValue;
            var result = Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(dishMinDate.Cafe.Id,
                dishMinDate.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId, date);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dishMinDate.DishName));
        }

        [Test]
        public void GetFoodDishesById_Success()
        {
            SetUp();
            var dish = DishFactory.Create();
            var result = Accessor.Instance.GetFoodDishesById(new[] { dish.Id });
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == dish.DishName));
        }

        [Test]
        public void GetFoodDishesById_Success_Empty()
        {
            SetUp();
            long id = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.GetFoodDishesById(new[] { id });
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByIdTest_Only_Active()
        {
            SetUp();
            var dish = DishFactory.Create();
            dish.IsActive = false;
            var result = Accessor.Instance.GetFoodDishesById(new[] { dish.Id });
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishesByIdTest_Not_Deleted()
        {
            SetUp();
            var dish = DishFactory.Create();
            dish.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishesById(new[] { dish.Id });
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishVersionsById_Success()
        {
            SetUp();
            var version = DishVersionFactory.Create();
            var result = Accessor.Instance.GetFoodDishVersionsById(version.Id);
            Assert.IsTrue(result.Count == 1);
            Assert.IsNotNull(result.First(e => e.DishName == version.DishName));
        }

        [Test]
        public void GetFoodDishVersionsById_Success_Empty()
        {
            SetUp();
            long id = _random.Next(999, int.MaxValue);
            var result = Accessor.Instance.GetFoodDishVersionsById(id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishVersionsById_Only_Active()
        {
            SetUp();
            var version = DishVersionFactory.Create();
            version.IsActive = false;
            var result = Accessor.Instance.GetFoodDishVersionsById(version.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetFoodDishVersionsById_Not_Deleted()
        {
            SetUp();
            var version = DishVersionFactory.Create();
            version.IsDeleted = true;
            var result = Accessor.Instance.GetFoodDishVersionsById(version.Id);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void AddDish_No_Such_CafeCategory()
        {
            SetUp();
            var result = Accessor.Instance.AddDish(_random.Next(), new long[] { _random.Next() }, new Dish(), _random.Next());
            Assert.IsTrue(result == -1);
        }

        [Test]
        public void AddDish_Success()
        {
            SetUp();
            var dish = DishFactory.Create();
            var dishModel = new Dish();
            dishModel.DishName = Guid.NewGuid().ToString("N");
            var creatorId = _random.Next();
            var result = Accessor.Instance.AddDish(dish.Cafe.Id, dish.DishCategoryLinks.Select(d => d.CafeCategory.DishCategoryId).ToArray(), dishModel, creatorId);
            Assert.IsTrue(dishModel.Id == result);
            //Assert.IsTrue(dishModel.CategoryId == dish.CategoryId);
            Assert.IsTrue(dishModel.CreatorId == creatorId);
            Assert.IsTrue(dishModel.Id > 0);
        }

        [Test]
        public void AddDish_Success_Check_DishIndex()
        {
            SetUp();
            var dish = DishFactory.Create();
            //dish.DishIndex = _random.Next();
            var dishModel = new Dish();
            dishModel.DishName = Guid.NewGuid().ToString("N");
            var creatorId = _random.Next();
            Accessor.Instance.AddDish(dish.Cafe.Id, dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory.DishCategoryId).ToArray(), dishModel, creatorId);
            Assert.IsTrue(dishModel.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex
                == ++dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex);
        }

        [Test]
        public void EditDish_No_Such_Dish()
        {
            SetUp();
            var creatorId = _random.Next();
            var dish = new Dish();
            dish.Id = int.MaxValue;
            var result = Accessor.Instance.EditDish(dish, new[] { -1L }, creatorId);
            Assert.IsTrue(dish.Id != int.MaxValue);
            Assert.IsTrue(dish.Id == result);
            Assert.IsTrue(dish.CreatorId == creatorId);
            Assert.IsTrue(dish.CreationDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void EditDish_Success()
        {
            SetUp();
            var updaterId = _random.Next();
            var toUpdate = DishFactory.Create();
            var model = DishFactory.Clone(toUpdate);
            var result = Accessor.Instance.EditDish(model, new[] { -1L }, updaterId);
            Assert.IsTrue(model.Id == toUpdate.Id && toUpdate.Id == result);
            Assert.IsTrue(toUpdate.LastUpdateByUserId == updaterId);
            Assert.IsTrue(toUpdate.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void RemoveDish_No_Such_Dish()
        {
            SetUp();
            var dishId = _random.Next();
            var removerId = _random.Next();
            var result = Accessor.Instance.RemoveDish(dishId, removerId);
            Assert.IsTrue(result == -1);
        }

        /// <summary>
        /// Очень печальный тест, переполненный условностями, но все они идут из тестируемого кода...
        /// </summary>
        [Test]
        public void RemoveDish_Success()
        {
            SetUp();
            CafeFactory.Create();
            DishCategoryFactory.Create();
            var deletedCategory = DishCategoryFactory.Create();
            deletedCategory.CategoryName = SystemDishCategories.DeletedDishesCategory;
            deletedCategory.Id = -2; //кто-нибудь в курсе, что это за трэш?
            var dish = DishFactory.Create();
            var removerId = _random.Next();
            var result = Accessor.Instance.RemoveDish(dish.Id, removerId);
            Assert.IsTrue(result == dish.Id);
            Assert.IsTrue(dish.LastUpdateByUserId == removerId);
            Assert.IsTrue(dish.LastUpdDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void ChangeDishIndexTest()
        {
            SetUp();
            var dish = DishFactory.Create();
            var newIndex = _random.Next();
            Accessor.Instance.ChangeDishIndex(dish.Cafe.Id, dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId, newIndex, -1);
            var dishCount = ContextManager.Get().Dishes.Count();
            var expectedIndex = dishCount - 1;
            Assert.IsTrue(dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex.Value == expectedIndex);
        }

        [Test]
        public void ChangeDishIndex_Few_Dishes()
        {
            SetUp();
            var temp = DishFactory.Create();
            //temp.DishIndex = 0;
            var dish = DishFactory.Create(/*category: temp.CafeCategory*/);
            //dish.DishIndex = 1;
            var newIndex = 0;
            Accessor.Instance.ChangeDishIndex(dish.Cafe.Id, dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId, 
                newIndex, dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex.Value);
            Assert.IsTrue(dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex.Value == newIndex);
        }

        /*

        [Test()]
        public void UpdateDishIndexInSecondCategoryTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void UpdateDishIndexInFirstCategoryTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void GetDishesByScheduleByDateTest()
        {
            Assert.Fail();
        }*/
    }
}