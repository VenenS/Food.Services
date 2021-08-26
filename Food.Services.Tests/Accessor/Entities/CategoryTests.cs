using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class CategoryTests
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
        public void GetFoodCategoryById_Test()
        {
            var dishCategories = DishCategoryFactory.CreateFew(count: 3);
            var seacrhEntity = dishCategories.First();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFoodCategoryById(seacrhEntity.Id);
            //
            Assert.NotNull(response);
            Assert.IsTrue(seacrhEntity.Uuid == response.Uuid);
        }

        [Test]
        public void GetFoodCategories_Test()
        {
            var dishCategories = DishCategoryFactory.CreateFew(count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFoodCategories();
            //
            Assert.NotNull(response);
            Assert.IsTrue(response.Count == 3);
        }

        [Test]
        public void GetFoodCategoriesForManager_Test()
        {
            DishCategoryFactory.CreateFew(count: 3);
            var cafe = CafeFactory.Create();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFoodCategoriesForManager(null, cafe.Id);
            //
            Assert.NotNull(response);
            Assert.NotNull(response.Keys.Count == 3);
        }

        [Test]
        public void GetFoodCategoriesVersionForManager_Test()
        {
            DishCategoryFactory.CreateFew(count: 3);
            var cafe = CafeFactory.Create();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFoodCategoriesVersionForManager(null, cafe.Id);
            //
            Assert.NotNull(response);
            Assert.NotNull(response.Keys.Count == 3);
        }

        [Test]
        public void GetFoodCategoriesByCafeId_Test()
        {
            var lstDishCategoryInCafe = DishCategoryInCafeFactory.CreateFew(count: 3);
            var cafe = CafeFactory.Create();
            var firstDCIC = lstDishCategoryInCafe.First();
            firstDCIC.CafeId = cafe.Id;
            var dishCategory = DishCategoryFactory.Create();
            firstDCIC.Cafe = cafe;
            firstDCIC.DishCategory = dishCategory;
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFoodCategoriesByCafeId(cafe.Id);
            //
            Assert.NotNull(response);
            Assert.NotNull(response.Count == 1);
        }

        /// <summary>
        /// Категория существует и активна
        /// </summary>
        [Test]
        public void AddCafeFoodCategory_Test1()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(cafe: cafe);
            var categoryInCafe = DishCategoryInCafeFactory.Create(cafe: cafe, category: category);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCafeFoodCategory(cafe.Id, category.Id, 0, _user.Id);
            //
            Assert.NotNull(response);
            Assert.NotNull(response == category.Id);
        }

        /// <summary>
        /// Категория существует и не активна
        /// </summary>
        [Test]
        public void AddCafeFoodCategory_Test2()
        {
            var cafe = CafeFactory.Create();
            var category = DishCategoryFactory.Create(cafe: cafe);
            category.IsActive = false;
            var categoryInCafe = DishCategoryInCafeFactory.Create(cafe: cafe, category: category);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCafeFoodCategory(cafe.Id, category.Id, 0, _user.Id);
            //
            Assert.NotNull(response);
            Assert.NotNull(response == category.Id);
        }

        [Test]
        public void ShiftCategories_Test()
        {
            var cafe = CafeFactory.Create();
            DishCategoryInCafeFactory.CreateFew(cafe: cafe);
            var category = _context.DishCategoriesInCafes.First();
            //
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.ShiftCategories(cafe.Id, category.Id, category.Index.Value, 0, false);
            var categoryTest = _context.DishCategoriesInCafes.FirstOrDefault(e => e.Id == category.Id);
            //
            Assert.IsTrue(category.Index == categoryTest.Index);
        }

        /// <summary>
        /// Успешное удаление привязки категории к кафе
        /// </summary>
        [Test]
        public void RemoveCafeFoodCategory_Test1()
        {
            var cafe = CafeFactory.Create();
            DishCategoryInCafeFactory.CreateFew(cafe: cafe);
            var categoryInCafe = _context.DishCategoriesInCafes.FirstOrDefault();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCafeFoodCategory(cafe.Id, categoryInCafe.DishCategoryId, _user.Id);
            //
            Assert.NotNull(response);
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Не удачное удаление привязки категории к кафе
        /// </summary>
        [Test]
        public void RemoveCafeFoodCategory_Test2()
        {
            var cafe = CafeFactory.Create();
            DishCategoryInCafeFactory.CreateFew();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCafeFoodCategory(cafe.Id, -1, _user.Id);
            //
            Assert.NotNull(response);
            Assert.IsFalse(response);
        }

        [Test]
        public void UpdateIndexCategories_Test()
        {
            DishCategoryInCafeFactory.CreateFew(count: 3);
            //
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.UpdateIndexCategories(0);
            var lstDishCategoryInCafe = _context.DishCategoriesInCafes.OrderBy(e => e.Index).ToList();
            bool check = true;
            for (int i = 0; check && i < 3; i++)
            {
                if (lstDishCategoryInCafe[i].Index != 10 + i * 10) check = false;
            }
            //
            Assert.IsTrue(check);
        }

        [Test]
        public void MoveDishesFromCategory_Test()
        {
            DishCategoryInCafeFactory.CreateFew(count: 3);
            var deleteCategory = _context.DishCategoriesInCafes.Last();
            deleteCategory.DishCategoryId = -1;
            var dishes = DishFactory.CreateFew(3);
            foreach (var item in dishes) 
                item.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId = 0;
            _context.SaveChanges();
            //
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.MoveDishesFromCategory(0, 0);
            bool check = true;
            foreach (var item in dishes)
            {
                if (item.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId == 0) { check = false; break; }
            }
            //
            Assert.IsTrue(check);
        }
    }
}
