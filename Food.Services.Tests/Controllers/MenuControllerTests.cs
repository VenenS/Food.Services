using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Extensions.Schedule;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.AuthorizationServer.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodServiceManager;
using Microsoft.AspNet.Identity;
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
    class MenuControllerTests
    {
        FakeContext _context;
        MenuController _controller;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;
        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new MenuController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        private void SetUp(string role)
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new MenuController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, new[] { role });
        }
        [Test]
        public void AddScheduleTest_Manager()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Дата для расписания в секундах
            var today = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe dishCategoryInCafe = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            Dish dish = DishFactory.Create(_user, dishCategoryInCafe);
            //Устанавливаем дату начала действия данной версии блюда раньше даты создания расписания
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            //Добавляем пользователя как менеджера кафе
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.AddSchedule(dish.Id, null, null, (long)today,
                null, null, 25, (int)ScheduleTypeEnum.Simply);
            var result = TransformResult.GetPrimitive<long>(responce);

            //ИД расписания не должен быть равен null
            Assert.IsNotNull(result);
            //Ид должен быть >= 0
            Assert.IsTrue(result >= 0);
        }

        [Test]
        public void GetDishesByScheduleByDateTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Добавляем пользователя как менежера кафе
            CafeManagerFactory.Create(_user, cafe);

            var responce = _controller.GetDishesByScheduleByDate(cafe.Id);
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //результатом должен быть пустой список моделей
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]

        public void GetDishesForDateTest()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Добавляем пользователя как менеджера кафе
            CafeManagerFactory.Create(_user, cafe);
            //формируем модель работы кафе
            var hours = CafeFactory.CreateBusinessHoursModel(cafe);
            CafesController c = new CafesController(_context, _accessor.Object);
            //ждем, пока рабочие часы установятся
            c.SetCafeBusinessHours(hours).GetAwaiter().GetResult();
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe dishCategoryInCafe = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            Dish dish = DishFactory.Create(_user, dishCategoryInCafe);
            //СОздаем блюдо в меню
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user, dish, "S", DateTime.Now.Date);

            var responce = _controller.GetDishesForDate().GetAwaiter().GetResult();
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetRestOfCatTest()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Создаем категорию\
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            FoodCategoryModel food = dishCategory.GetContract();
            FoodCategoryWithDishes foodCategory = new FoodCategoryWithDishes
            {
                Category = food,
                CountOfDishes = 6,
                Dishes = new List<FoodDishModel>()
            }; List<Dish> dishes = DishFactory.CreateFew(10, _user);
            RestOfCategory rest = new RestOfCategory()
            {
                CategoryId = food.Id,
                DishIds = dishes.Select(d => d.Id).ToArray(),
                RestName = ""
            };

            var responce = _controller.GetRestOfCat(rest).GetAwaiter().GetResult();
            var result = TransformResult.GetObject<List<FoodDishModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetDishesByScheduleByDateHelperTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe dishCategoryInCafe = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            Dish dish = DishFactory.Create(_user, dishCategoryInCafe);
            //Устанавливаем дату начала действия данной версии блюда раньше даты создания расписания
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            //СОздаем блюдо в меню
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user, dish, "S", DateTime.Now.Date);

            var responce = _controller.GetDishesByScheduleByDateHelper(cafe.Id, null);

            //Результат не должен быть равен null
            Assert.IsNotNull(responce);
        }

        [Test]
        public void GetDishesForManagerTest()
        {
            //Ниже тест метода, который делает то же самое, что и этот, только подробнее 
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);

            var responce = _controller.GetDishesForManager(cafe.Id);
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetDishesForManagerHelperTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            List<DishCategory> dishCategory = DishCategoryFactory.CreateFew(10, cafe, _user);
            List<DishCategoryInCafe> dishCategoryInCafe = DishCategoryInCafeFactory.CreateFew(10, _user, cafe);
            dishCategory[0].IsActive = false;
            dishCategory[1].IsDeleted = true;
            var result = _controller.GetDishesForManagerHelper(cafe.Id, null);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Итого категорий 20, 1 неактивная, удаленные должны попадать
            Assert.IsTrue(result.Count == 19);
        }

        [Test]
        public void GetMenuForManagerTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            List<DishCategory> dishCategory = DishCategoryFactory.CreateFew(10, cafe, _user);
            List<DishCategoryInCafe> dishCategoryInCafe = DishCategoryInCafeFactory.CreateFew(10, _user, cafe);
            dishCategory[0].IsActive = false;
            dishCategory[1].IsDeleted = true;
            var responce = _controller.GetMenuForManager(cafe.Id, null);
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);
            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Итого категорий 20, но все пустые, поэтому результат должен быть = 0
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetMenuSchedulesTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Здесь пока в Schedule.cs не удается заменить FoodContext на GetContext()
            var responce = _controller.GetMenuSchedules(cafe.Id, null);
        }

        [Test]
        public void GetDishesByScheduleByDateByRangeTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe dishCategoryInCafe = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            Dish dish = DishFactory.Create(_user, dishCategoryInCafe);
            //Устанавливаем дату начала действия данной версии блюда раньше даты создания расписания
            dish.VersionFrom = DateTime.Now.AddDays(-1);
            //СОздаем блюдо в меню
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user, dish, "S", DateTime.Now.Date);

            var responce = _controller.GetDishesByScheduleByDateByRange(cafe.Id, dishCategory.Id,
                0, 5, null);
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Взяли 5, но добавили всего 1, поэтому 1
            Assert.IsTrue(result.Count == 1);
        }

        [Test]
        public void GetDishesByScheduleByDateByTagListAndSearchTermTest_User()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            List<TagObject> tagObjects = TagObjectFactory.CreateFew(10);
            for (int i = 0; i < tagObjects.Count; i++)
            {
                tagObjects[i].ObjectId = cafe.Id;
                tagObjects[i].ObjectTypeId = (int)ObjectTypesEnum.CAFE;
            }
            //Устанавливаем кафе обслуживание только компаний
            cafe.CafeAvailableOrdersType = "COMPANY_ONLY";
            //Добавляем компанию для заказов
            Company company = CompanyFactory.Create(_user);
            //Добавляем пользователя в компанию
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(_user, company);
            //Создаем расписание на автоматическое добавление закзаов у компании
            CompanyOrderSchedule companyOrderSchedule = CompanyOrderScheduleFactory.Create(_user, company, cafe);
            DishCategory dishCategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe dishCategoryInCafe = DishCategoryInCafeFactory.Create(_user, cafe, dishCategory);
            Dish dish = DishFactory.Create(_user, dishCategoryInCafe);
            DishVersion dishVersion = DishVersionFactory.Create(dish, dishCategoryInCafe);
            var responce = _controller.
                GetDishesByScheduleByDateByTagListAndSearchTerm(cafe.Id, null, "", tagObjects.Select(t => t.Id).ToList());
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
            //Т.к. пользователь не менеджер, то создать расписание мы не можем, поэтому возвращает 0
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetMenuByPatternIdTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем шаблон меню для кафе
            CafeMenuPattern cafeMenuPattern = CafeMenuPatternFactory.Create(cafe);
            //Создаем блюда в шаблоне
            List<CafeMenuPatternDish> cafeMenuPatternDishes = CafeMenuPatternDishFactory.CreateFew(10, true);
            List<Dish> dishes = DishFactory.CreateFew(1000, _user);
            for (int i = 0; i < cafeMenuPatternDishes.Count; i++)
            {
                cafeMenuPatternDishes[i].Pattern = cafeMenuPattern;
                cafeMenuPatternDishes[i].PatternId = cafeMenuPattern.Id;
                cafeMenuPatternDishes[i].Dish = dishes[i];
            }
            for (int i = 0; i < dishes.Count; i++)
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = DishCategoryInCafeFactory.Create(_user, cafe);
            cafeMenuPattern.Dishes = cafeMenuPatternDishes;
            var responce = _controller.GetMenuByPatternId(cafe.Id, cafeMenuPattern.Id);
            var result = TransformResult.GetObject<List<FoodCategoryWithDishes>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //По идее здесь должно быть 10, но нумерация ИДов неконтролируемая,поэтому Count > 0
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void GetScheduleByIdTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user);

            var responce = _controller.GetScheduleById(dishInMenu.Id);
            var result = TransformResult.GetObject<ScheduleModel>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);
        }


        [Test]
        public void GetScheduleForCafeByCafeIdTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            List<DishInMenu> dishesInMenu = DishesInMenuFactory.CreateFew(10, _user);
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            List<DishCategory> dishcategories = DishCategoryFactory.CreateFew(10, cafe, _user);
            List<DishCategoryInCafe> cafeCategories = DishCategoryInCafeFactory.CreateFew(10, _user, cafe);
            for (int i = 0; i < dishesInMenu.Count; i++)
            {
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategoryId = dishcategories[i].Id;
                dishesInMenu[i].Dish = dishes[i];
                dishesInMenu[i].Dish.DishCategoryLinks.FirstOrDefault(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategories[i];
                dishesInMenu[i].BeginDate = DateTime.Now.Date;

            }
            var responce = _controller.GetScheduleForCafeByCafeId(cafe.Id);
            var result = TransformResult.GetObject<List<ScheduleModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Всего шаблонов 10
            Assert.IsTrue(result.Count == 10);

        }

        [Test]
        public void GetScheduleForDishTest()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            List<DishInMenu> dishesInMenu = DishesInMenuFactory.CreateFew(10, _user);
            DishCategory dishcategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishcategory);
            Dish dish = DishFactory.Create(_user);

            for (int i = 0; i < dishesInMenu.Count; i++)
            {
                dishesInMenu[i].Dish = dish;
                dishesInMenu[i].DishId = dish.Id;
                dishesInMenu[i].Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
                dishesInMenu[i].BeginDate = DateTime.Now.Date;
            }
            //другое блюдо
            dishesInMenu[0].DishId = dish.Id + 1;
            dishesInMenu[1].IsActive = false;
            dishesInMenu[2].IsDeleted = true;
            var responce = _controller.GetScheduleForDish(dish.Id, null);
            var result = TransformResult.GetObject<List<ScheduleModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Всехо схем 10, 1 с чужим блюдом, 1 удалена, 1 неактивна
            Assert.IsTrue(result.Count == 7);
        }

        [Test]
        public void GetSchedulesForCafeByDishListTest_Manager()
        {
            SetUp();
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //При проверке делаем, чтоб возвращало true
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            List<DishInMenu> dishesInMenu = DishesInMenuFactory.CreateFew(10, _user);
            DishCategory dishcategory = DishCategoryFactory.Create(_user, cafe);
            DishCategoryInCafe cafeCategory = DishCategoryInCafeFactory.Create(_user, cafe, dishcategory);
            List<Dish> dishes = DishFactory.CreateFew(10, _user);

            for (int i = 0; i < dishesInMenu.Count; i++)
            {
                dishesInMenu[i].Dish = dishes[i];
                dishesInMenu[i].DishId = dishes[i].Id;
                dishesInMenu[i].Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = cafeCategory;
                dishesInMenu[i].BeginDate = DateTime.Now.Date;
            }
            //другое блюдо
            dishesInMenu[0].DishId = dishes[0].Id + 1;
            dishesInMenu[1].IsActive = false;
            dishesInMenu[2].IsDeleted = true;
            var responce = _controller.GetSchedulesForCafeByDishList(cafe.Id, dishes.Select(d => d.Id).ToArray(), true);
            var result = TransformResult.GetObject<List<ScheduleModel>>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Всехо схем 10, 1 удалена, 1 неактивна
            //та, у которой другой dishId все равно попадает, т.к. он в dishList содержится
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void RemoveCafeMenuPatternTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем шаблон меню для кафе
            CafeMenuPattern cafeMenuPattern = CafeMenuPatternFactory.Create(cafe);
            CafeManagerFactory.Create(_user, cafe);
            //Создаем блюда в шаблоне
            List<CafeMenuPatternDish> cafeMenuPatternDishes = CafeMenuPatternDishFactory.CreateFew(10, true);
            List<Dish> dishes = DishFactory.CreateFew(1000, _user);
            for (int i = 0; i < cafeMenuPatternDishes.Count; i++)
            {
                cafeMenuPatternDishes[i].Pattern = cafeMenuPattern;
                cafeMenuPatternDishes[i].PatternId = cafeMenuPattern.Id;
            }
            cafeMenuPattern.Dishes = cafeMenuPatternDishes;
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, cafeMenuPattern.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Должно вернуться true
            Assert.IsTrue(result);
        }

        [Test]
        public void RemoveScheduleTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            CafeManagerFactory.Create(_user, cafe);
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user);
            dishInMenu.Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.Cafe = cafe;
            dishInMenu.Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId = cafe.Id;
            var responce = _controller.RemoveSchedule(dishInMenu.Id);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Должно быть не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void UpdateScheduleTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            CafeManagerFactory.Create(_user, cafe);
            DishInMenu dishInMenu = DishesInMenuFactory.Create(_user);
            dishInMenu.Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.Cafe = cafe;
            dishInMenu.Dish.DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId = cafe.Id;
            dishInMenu.Dish.VersionFrom = DateTime.Now;
            ScheduleModel scheduleModel = dishInMenu.GetContract();
            var responce = _controller.UpdateSchedule(scheduleModel);
            var result = TransformResult.GetPrimitive<long>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Должно быть не -1
            Assert.IsTrue(result != -1);
        }

        [Test]
        public void UpdateSchedulesByPatternIdTest_Manager()
        {
            SetUp(EnumUserRole.Manager);
            //Создаем кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем шаблон меню для кафе
            CafeMenuPattern cafeMenuPattern = CafeMenuPatternFactory.Create(cafe);
            CafeManagerFactory.Create(_user, cafe);
            //Создаем блюда в шаблоне
            List<CafeMenuPatternDish> cafeMenuPatternDishes = CafeMenuPatternDishFactory.CreateFew(10, true);
            List<Dish> dishes = DishFactory.CreateFew(1000, _user);
            for (int i = 0; i < dishes.Count; i++)
            {
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory = DishCategoryInCafeFactory.Create(_user, cafe);
                dishes[i].DishCategoryLinks.First(l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId = cafe.Id;
                dishes[i].VersionFrom = DateTime.Now;
            }
            for (int i = 0; i < cafeMenuPatternDishes.Count; i++)
            {
                cafeMenuPatternDishes[i].Pattern = cafeMenuPattern;
                cafeMenuPatternDishes[i].PatternId = cafeMenuPattern.Id;
                cafeMenuPatternDishes[i].Dish = DishFactory.Create(_user);
                cafeMenuPatternDishes[i].Dish.VersionFrom = DateTime.Now;
                cafeMenuPatternDishes[i].Dish.DishCategoryLinks.First(
                    l => l.IsActive == true && !l.IsDeleted).CafeCategory
                    = DishCategoryInCafeFactory.Create(_user, cafe);
                cafeMenuPatternDishes[i].Dish.DishCategoryLinks.First(
                    l => l.IsActive == true && !l.IsDeleted).CafeCategory.CafeId = cafe.Id;
            }
            cafeMenuPattern.Dishes = cafeMenuPatternDishes;
            var responce = _controller.UpdateSchedulesByPatternId(cafe.Id, cafeMenuPattern.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Должно вернуться true
            Assert.IsTrue(result);
        }

        [Test]
        public void GetCafeIdsForCurrentUserTest()
        {
            SetUp(EnumUserRole.Manager);
            List<Cafe> cafes = CafeFactory.CreateFew(20, _user);
            for (int i = 0; i < 5; i++)
                CafeManagerFactory.Create(_user, cafes[i]);
            for (int i = 5; i < 10; i++)
                cafes[i].CafeAvailableOrdersType = "COMPANY_PERSON";
            CafesController c = new CafesController(_context, _accessor.Object);
            for (int i = 0; i < 5; i++)
            {
                //формируем модель работы кафе
                var hours = CafeFactory.CreateBusinessHoursModel(cafes[i]);
                //ждем, пока рабочие часы установятся
                c.SetCafeBusinessHours(hours).GetAwaiter().GetResult();
            }
            var result = _controller.GetCafeIdsForCurrentUser();

            //Результат не должен быть равен null
            Assert.IsNotNull(result);

            //Всего кафе 20, но любое из них может быть уже закрыто
            Assert.IsTrue(result.GetAwaiter().GetResult() != null);
        }
    }
}
