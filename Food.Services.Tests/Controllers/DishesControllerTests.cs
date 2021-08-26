using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    class DishesControllerTests
    {
        private FakeContext _context;
        private DishesController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new DishesController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]

        public void CreateDishTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);
            dish.BasePrice = 100;
            var model = dish.GetContractDish();
            dish.DishName = dish.DishName + "_";
            //Пробуем добавить блюдо
            var responce = _controller.CreateDish(cafe.Id, model);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Блюдо должно добавиться
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void ChangeDishIndexTst_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);
            dish.BasePrice = 100;
            dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).DishIndex = 0;
            //Создаем список блюд
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).DishIndex = i + 1;
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
            }
            //Пробуем добавить блюдо
            var responce = _controller.ChangeDishIndex(cafe.Id, dishCategory.Id,
                (int)dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex + 10,
                (int)dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex);

            //Результат не должен быть равен null
            Assert.IsNotNull(responce);
        }

        [Test]
        public void CheckUniqueNameWithinCafeTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);
            dish.BasePrice = 100;
            dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).DishIndex = 0;
            //Создаем список блюд
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).DishIndex = i + 1;
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
            }
            //Меняем, такого блюда не должно быть
            dish.Cafe.Id = cafe.Id + 1;
            //Пробуем добавить блюдо
            var responce = _controller.CheckUniqueNameWithinCafe(dish.DishName, cafe.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);
            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Не должно быть одинаколвых названий блюд
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateDishTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            List<Dish> dishes = DishFactory.CreateFew(100, _user);
            dishes[2].BasePrice = 100;
            var model = dishes[2].GetContractDish();
            model.Name += "_";
            model.Description = "Измененное описание блюда";
            //Пробуем изменить блюдо
            var responce = _controller.UpdateDish(model);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Блюдо должно измениться
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void GetFoodDishesByFilterTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            List<Dish> dishes = DishFactory.CreateFew(100, _user);

            Random r = new Random();
            List<long> filterDishes = new List<long>();
            for (int i = 0; i < 50; i++)
                filterDishes.Add(r.Next(0, 100));
            DishFilterModel filter = new DishFilterModel
            {
                DishIds = filterDishes
            };
            //Пробуем получить блюда по фильтру
            var responce = _controller.GetFoodDishesByFilter(filter);
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Кол-во блюд точно > 0, но точно неизвестно сколько, т.к. рандомом IDшники могут повториться 
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void GetFoodDishByDateTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user);
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            dish.VersionTo = DateTime.Now.AddDays(1);
            //Дата для запроса в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Пробуем получить блюда по фильтру
            var responce = _controller.GetFoodDishByDate(dish.Id, (long)today);
            var result = TransformResult.GetObject<FoodDishModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetDishesBySearchTermAndTagListAndCafeIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            List<TagObject> tagObjects = TagObjectFactory.CreateFew(10);
            for (int i = 0; i < tagObjects.Count; i++)
            {
                tagObjects[i].ObjectId = cafe.Id;
                tagObjects[i].ObjectTypeId = (int)ObjectTypesEnum.CAFE;
            }
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            dish.VersionTo = DateTime.Now.AddDays(1);
            //Дата для запроса в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            string searchTerm = dish.DishName.Substring(1, 3);
            //Пробуем получить блюда по фильтру
            var responce = _controller.GetDishesBySearchTermAndTagListAndCafeId(searchTerm, new List<long>(), cafe.Id);
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Должно найтись 1 блюдо
            Assert.IsTrue(result.Count == 1);
        }

        [Test]
        public void GetFirst100DishesTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            List<Order> orders = OrderFactory.CreateFew(1000, _user);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].CafeId = 1;//Так нужно для этого метоада
            }
            //Пробуем получить заказы по фильтру
            var responce = _controller.GetFirst100Dishes();
            var result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Должны найтись все заказы.. 
            Assert.IsTrue(result.Count == 1000);
        }

        [Test]
        public void GetFoodCafeByDishIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            List<Dish> dishes = DishFactory.CreateFew(1000, _user);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
            }
            //Пробуем получить модель
            var responce = _controller.GetFoodCafeByDishId(dishes[0].Id);
            var result = TransformResult.GetObject<CafeModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetFoodCategoryAndFoodDishesByCafeIdAndDateTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
                dishes[i].Cafe.Id = cafe.Id;
                dishes[i].VersionFrom = DateTime.Now.AddDays(-1);
                dishes[i].VersionTo = DateTime.Now.AddDays(1);
            }
            dishes[1].VersionFrom = DateTime.Now.AddDays(1);
            dishes[2].VersionTo = DateTime.Now.AddDays(-1);
            dishes[4].IsActive = false;
            dishes[5].IsDeleted = true;
            //Пробуем получить модель
            var responce = _controller.GetFoodCategoryAndFoodDishesByCafeIdAndDate(cafe.Id, null);
            var result = TransformResult.GetObject<IEnumerable<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Категория 1,поэтому должны получить в результате 1
            Assert.IsTrue(result.Count() == 1);
        }

        [Test]
        public void GetFoodCategoryByDishIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);

            //Пробуем получить модель
            var responce = _controller.GetFoodCategoryByDishId(dish.Id);
            var result = TransformResult.GetObject<FoodCategoryModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetFoodDishAllVersionTest()
        {

            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user, cafeCategory);
            //Пробуем получить список моделей
            var responce = _controller.GetFoodDishAllVersion(dish.Id);
            var result = TransformResult.GetObject<List<FoodDishVersionModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Резульаттом должен быть 1 элемент
            Assert.IsTrue(result.Count == 1);
        }

        [Test]
        public void GetFoodDishByIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user);
            //Пробуем получить модель
            var responce = _controller.GetFoodDishById(dish.Id);
            var result = TransformResult.GetObject<FoodDishModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetFoodDishByIdAndDateTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюдо
            Dish dish = DishFactory.Create(_user);
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            dish.VersionTo = DateTime.Now.AddDays(1);
            //Дата для запроса в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Пробуем получить модель
            var responce = _controller.GetFoodDishByIdAndDate(dish.Id, (long)today);
            var result = TransformResult.GetObject<FoodDishModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetFoodDishesByCafeIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюда
            List<Dish> dishes = DishFactory.CreateFew(10, _user, cafe: cafe);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].VersionFrom = DateTime.Now.AddDays(-1);
                dishes[i].VersionTo = DateTime.Now.AddDays(1);
            }
            //Дата для запроса в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Пробуем получить список моделей
            var responce = _controller.GetFoodDishesByCafeId(cafe.Id, (long)today);
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Должно быть 10 моделей, т.к. все подходят под условия
            Assert.IsTrue(result.Count == 10);
        }

        [Test]
        public void GetFoodDishesByCategoryIdAndCafeIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюда
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].VersionFrom = DateTime.Now.AddDays(-1);
                dishes[i].VersionTo = DateTime.Now.AddDays(1);
                dishes[i].DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
                dishes[i].DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategory = dishCategory;
                dishes[i].DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory.DishCategoryId = dishCategory.Id;
            }
            //Дата для запроса в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //Пробуем получить список моделей
            var responce = _controller.GetFoodDishesByCategoryIdAndCafeId(cafe.Id, dishCategory.Id, DateTime.Now);
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Должно быть 10 моделей, т.к. все подходят под условия
            Assert.IsTrue(result.Count == 10);
        }

        [Test]
        public void DeleteDishTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюда
            Dish dish = DishFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
            //Создаем категорию удаленные блюда в кафе
            DishCategory dishCategory2 = DishCategoryFactory.Create(_user, cafe);
            dishCategory2.CategoryName = SystemDishCategories.DeletedDishesCategory;
            dishCategory2.Id = -2;
            var responce = _controller.DeleteDish(dish.Id);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Успешное удаление должно вернуть не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void UpdateDishIndexInFirstCategoryTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюда
            Dish dish = DishFactory.Create(_user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
            dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex = 0;
            //Создаем новую категорию у блюда
            DishCategory dishCategory2 = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory2 = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory2);
            var result = _controller.UpdateDishIndexInFirstCategory(cafe.Id, dishCategory2.Id,
                dishCategory.Id, 1, (int)dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).DishIndex, dish.Id);
            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void UpdateDishIndexInSecondCategoryTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем блюда
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            //Создаем категорию блюда
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            //Создаем категорию в кафе
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).DishIndex = i;
            }

            var result = _controller.UpdateDishIndexInSecondCategory(cafe.Id, dishCategory.Id, 5, dishes[0].Id);
            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }
    }
}
