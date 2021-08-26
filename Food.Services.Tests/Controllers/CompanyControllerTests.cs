using Food.Data.Entities;
using Food.Services.Contracts.Companies;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services.Tests.Controllers
{
    class CompanyControllerTests
    {
        FakeContext _context;
        CompanyController _controller;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new CompanyController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]
        public void CreateCompanyTest()
        {
            SetUp();
            //Создаем модель и пытается по ней добавить компанию 
            CompanyModel m = new CompanyModel()
            {
                FullName = "Новая компания",
                IsActive = true,
                IsDeleted = false,
                Name = "Компания",
                SmsNotify = false
            };
            var response = _controller.CreateCompany(m);
            var result = TransformResult.GetPrimitive<bool>(response);

            //Проверяем, что результат есть и он не null
            Assert.IsNotNull(result);

            //Проверяем, что результат положительный (компания добавлена)
            Assert.IsTrue(result);
        }

        [Test]
        public void GetCompaniesByFilterTest()
        {
            SetUp();
            //Создаем 10 компаний
            List<Company> companies = CompanyFactory.CreateFew(10, _user);
            //Создаем фильтр, в который добавляем каждую 2ю из созданных компаний
            CompaniesFilterModel filter = new CompaniesFilterModel()
            {
                CompanyId = companies.Where(d => d.Id % 2 == 0).Select(c => c.Id).ToArray()
            };

            //Пытаемся получить компании по фильтру ИДов
            var response = _controller.GetCompaniesByFilter(filter);
            var result = TransformResult.GetObject<List<CompanyModel>>(response);

            //Проверяем, что результат пришел
            Assert.IsNotNull(result);
            //Проверяем, что компаний пришло 5 штук, т.к. ИДшники идут по порядку, а по модулю2 из 10и будет ровно 5 всегда
            Assert.IsTrue(result.Count == 5);

        }

        [Test]
        public void GetTest()
        {
            SetUp();
            //Создаем 10 компаний
            List<Company> companies = CompanyFactory.CreateFew(10, _user);
            companies[0].IsActive = false;
            companies[1].IsDeleted = true;
            //Пытаемся получить компании, которые являются активными и неудаленными
            var response = _controller.Get();
            var result = TransformResult.GetObject<List<CompanyModel>>(response);

            //Проверяем, что результат пришел
            Assert.IsNotNull(result);
            //Проверяем, что компаний пришло 8 штук,
            //Всего компаний 10, но 1 удалена, а одна неактивна
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void GetAllTest()
        {
            SetUp();
            //Создаем 10 компаний
            List<Company> companies = CompanyFactory.CreateFew(10, _user);
            companies[0].IsActive = false;
            companies[1].IsDeleted = true;
            //Пытаемся получить компании, которые являются активными и неудаленными
            var response = _controller.GetAll();
            var result = TransformResult.GetObject<IEnumerable<CompanyModel>>(response);

            //Проверяем, что результат пришел
            Assert.IsNotNull(result);
            //Проверяем, что компаний пришло 10 штук,
            //Всего компаний 10,не смотря на то, что 1 компания удалена, а другая неактивна, должны получить 10 компаний
            Assert.IsTrue(result.ToList().Count == 10);
        }

        [Test]
        public void Get_Test()
        {
            SetUp();
            //Созданием компанию
            Company company = CompanyFactory.Create(_user);

            //Пытаемся проверить получение компании по ИД
            var response = _controller.Get(company.Id);
            var result = TransformResult.GetObject<CompanyModel>(response);

            //Проверяем, что результат есть
            Assert.IsNotNull(result);
            //Проверяем, что пришел нужный результат
            Assert.IsTrue(result.Id == company.Id);
        }

        [Test]
        public void GetCompanyByOrdersInfoTest_Manager()
        {
            SetUp();
            //Создаем кафе для вызова метода и установки пользователя менеджером
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            Company company = CompanyFactory.Create(_user);
            //Дата для запроса в секундах
            var startDate = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            //2я дата для запроса в секундах
            var endDate = DateTime.Now.AddDays(6).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            List<CompanyOrder> companyOrders = CompanyOrderFactory.CreateFew(10, _user, company, cafe);
            for (int i = 0; i < companyOrders.Count; i++)
            {
                companyOrders[i].DeliveryDate = DateTime.Now.AddDays(i / 2);
            }

            //пытаемся по информации о заказах получить список компаний, но у нас она 1
            var response = _controller.GetCompanyByOrdersInfo(cafe.Id, (long)startDate, (long)endDate);
            var result = TransformResult.GetObject<List<CompanyModel>>(response);

            //Проверяем, что результат есть
            Assert.IsNotNull(result);

            //Проверяем, что в списке 1 компания
            Assert.IsTrue(result.Count == 1);
        }

        [Test]
        public void GetListOfCompanyByUserIdTest_Admin()
        {
            SetUp();
            List<Company> companies = CompanyFactory.CreateFew(10, _user);
            List<UserInCompany> users = new List<UserInCompany>();
            for (int i = 0; i < companies.Count - 1; i++)
            {
                users.Add(UserInCompanyFactory.CreateRoleForUser(_user, companies[i]));
            }
            users[0].IsActive = false;
            users[1].IsDeleted = true;

            // Пытаемся получить список компаний, привязанных к пользователю
            var response = _controller.GetListOfCompanyByUserId(_user.Id);
            var result = TransformResult.GetObject<List<CompanyModel>>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);

            // Всего компаний 10, 1 привязка неактивная, 1 удаленная, 1 компания не привязана к пользователю, должно остаться 7
            Assert.IsTrue(result.Count == 7);
        }

        [Test]
        public void DeleteTest_Admin()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);

            // Должен быть результат "Ok":
            var response = _controller.Delete(company.Id);
            var result = response as OkResult;
            Assert.IsNotNull(result);

            // Компания должна быть удалена и неактивна:
            Assert.IsTrue(company.IsDeleted && !company.IsActive);
        }

        [Test]
        public void RestoreTest_Admin()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Удаление компании:
            company.IsDeleted = true;
            company.IsActive = false;

            // Попытка восстановления:
            var response = _controller.Restore(company.Id);
            CompanyModel result = TransformResult.GetObject<CompanyModel>(response);

            // Должен быть возвращен результат - модель компании:
            Assert.IsNotNull(result);

            // Компания не должна быть удалена и должна быть активна:
            Assert.IsTrue(!company.IsDeleted && company.IsActive);
        }

        [Test]
        public void UpdateCompanyTest_Admin()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);

            // Создание модели компании с новыми данными:
            CompanyModel model = new CompanyModel();
            model.Id = company.Id;
            model.IsActive = true;
            model.IsDeleted = false;
            Random random = new Random(DateTime.Now.Millisecond);
            string randStr = random.Next(10, 10000).ToString();
            model.FullName = $"Test company name {randStr}";
            model.Name = $"Company {randStr}";
            model.JuridicalAddressId = random.Next(10, 100);
            model.DeliveryAddressId = random.Next(10, 100);

            // Попытка обновления:
            var response = _controller.UpdateCompany(model);
            CompanyModel result = TransformResult.GetObject<CompanyModel>(response);

            // Должен быть возвращен результат - модель компании:
            Assert.IsNotNull(result);

            // Данные компании должны быть обновлены:
            Assert.IsTrue(!company.IsDeleted && company.IsActive);
            Assert.IsTrue(company.Name == model.Name);
            Assert.IsTrue(company.FullName == model.FullName);
            Assert.IsTrue(company.JuridicalAddressId == model.JuridicalAddressId);
            Assert.IsTrue(company.MainDeliveryAddressId == model.DeliveryAddressId);
        }

        [Test]
        public void GetAddressCompanyTest_Admin()
        {
            SetUp();
            // Создание нескольких адресов и выбор одного из них:
            CompanyAddress address = CompanyAddressFactory.CreateFew(3, _user)[1];
            // Попытка получения адреса:
            var response = _controller.GetAddressCompany(address.Id);
            DeliveryAddressModel result = TransformResult.GetObject<DeliveryAddressModel>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);

            // Должен быть возвращен именно запрошенный адрес:
            Assert.IsTrue(result.Id == address.Address.Id);
            Assert.IsTrue(result.RawAddress == address.Address.RawAddress);
            Assert.IsTrue(result.PostalCode == address.Address.PostalCode);
            Assert.IsTrue(result.CityName == address.Address.CityName);
        }

        [Test]
        public void GetCompanyCuratorsTest()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание нескольких сотрудников:
            List<User> users = UserFactory.CreateFew(10);
            // Назначение 3 из них (3, 4, 5) кураторами компании:
            User[] curators = new User[3];
            for (int i = 0; i < 3; ++i)
                curators[i] = users.ElementAt(i + 3);
            CompanyCuratorFactory.CreateFew(curators, company, true);
            // Назначаем ещё трёх кураторов для другой компании:
            User[] otherCurators = new User[3];
            Company otherCompany = CompanyFactory.Create(_user);
            for (int i = 0; i < 3; ++i)
                otherCurators[i] = users.ElementAt(i + 6);
            CompanyCuratorFactory.CreateFew(otherCurators, otherCompany, true);

            // Получение списка кураторов:
            var response = _controller.GetCompanyCurators(company.Id);
            IEnumerable<CompanyCuratorModel> result =
                TransformResult.GetObject<IEnumerable<CompanyCuratorModel>>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);

            // Кураторов должно быть три:
            Assert.IsTrue(result.Count() == curators.Length);
            // И это должны быть те кураторы, которые назначены для нужной компании:
            var curatorIds = new HashSet<long>(curators.Select(c => c.Id));
            Assert.IsTrue(result.All(c => curatorIds.Contains(c.UserId)));
        }

        [Test]
        public void GetCompanyCuratorTest()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание нескольких сотрудников:
            List<User> users = UserFactory.CreateFew(10);
            // Назначение 3 из них (3, 4, 5) кураторами компании:
            User[] curators = new User[3];
            for (int i = 0; i < 3; ++i)
                curators[i] = users.ElementAt(i + 3);
            CompanyCuratorFactory.CreateFew(curators, company, true);
            // Назначаем ещё трёх кураторов для другой компании:
            User[] otherCurators = new User[3];
            Company otherCompany = CompanyFactory.Create(_user);
            for (int i = 0; i < 3; ++i)
                otherCurators[i] = users.ElementAt(i + 6);
            CompanyCuratorFactory.CreateFew(otherCurators, otherCompany, true);

            // Получение куратора:
            User curator = curators[1];
            var response = _controller.GetCompanyCurator(company.Id, curator.Id);
            CompanyCuratorModel result = TransformResult.GetObject<CompanyCuratorModel>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);
            // Метод должен вернуть указанного куратора:
            Assert.IsTrue(result.UserId == curator.Id);
        }

        [Test]
        public void GetCuratedCompanyTest_Consolidator()
        {
            SetUp();
            // Создание нескольких компаний:
            List<Company> companies = CompanyFactory.CreateFew(3, _user);
            // Назначение пользователя куратором одной компании:
            Company company = companies.ElementAt(1);
            CompanyCuratorFactory.Create(company, _user, true);

            // Выбираем курируемую компанию:
            var response = _controller.GetCuratedCompany();
            CompanyModel result = TransformResult.GetObject<CompanyModel>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);
            // Куратором должно быть тот, который указан:
            Assert.IsTrue(result.Id == company.Id && result.Name == company.Name);
        }

        [Test]
        public void GetMyCompaniesTest_User()
        {
            SetUp();

            // Метод GetMyCompanies выбирает компании, в которых состоит пользователь
            // (есть записи в таблице БД UsersInCompanies)
            // Создание нескольких компаний:
            List<Company> companies = CompanyFactory.CreateFew(8, _user);

            // Назначение пользователя в две компании:
            Company company1 = companies.ElementAt(3);
            UserInCompanyFactory.CreateRoleForUser(_user, company1);
            Company company2 = companies.ElementAt(5);
            UserInCompanyFactory.CreateRoleForUser(_user, company2);

            // Выбираем компании пользователя:
            var response = _controller.GetMyCompanies();
            List<CompanyModel> result = TransformResult.GetObject<List<CompanyModel>>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);
            // Должно быть две компании:
            Assert.IsTrue(result.Count == 2);
            // Должны быть выбраны указанные компании:
            Assert.IsTrue(result.Any(c => c.Id == company1.Id && c.Name == company1.Name) &&
                result.Any(c => c.Id == company2.Id && c.Name == company2.Name));
        }

        [Test]
        public void GetInactiveCompaniesTest_Admin()
        {
            SetUp();

            // Метод GetInactiveCompanies выбирает компании, у которых сброшен флаг активности
            // и поднят флаг удаления (!IsActive && IsDeleted)

            // Создание нескольких компаний:
            List<Company> companies = CompanyFactory.CreateFew(8, _user);

            // Удаление двух компаний:
            Company company1 = companies.ElementAt(2);
            company1.IsActive = false;
            company1.IsDeleted = true;
            Company company2 = companies.ElementAt(6);
            company2.IsActive = false;
            company2.IsDeleted = true;

            // Получение неактивных компаний:
            var response = _controller.GetInactiveCompanies();
            List<CompanyModel> result = TransformResult.GetObject<List<CompanyModel>>(response);

            // Проверяем, что результат есть
            Assert.IsNotNull(result);
            // Должно быть две компании:
            Assert.IsTrue(result.Count == 2);
            // Должны быть выбраны указанные компании:
            Assert.IsTrue(result.Any(c => c.Id == company1.Id && c.Name == company1.Name) &&
                result.Any(c => c.Id == company2.Id && c.Name == company2.Name));
        }

        [Test]
        public void PostCompanyCuratorTest_Admin()
        {
            SetUp();

            // Метод PostCompanyCurator добавляет куратора компании

            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание пользователя:
            User user = UserFactory.CreateUser();
            // Добавление пользователя в кураторы компании:
            CompanyCuratorModel curatorModel = new CompanyCuratorModel();
            curatorModel.UserId = user.Id;
            curatorModel.CompanyId = company.Id;
            var response = _controller.PostCompanyCurator(curatorModel);
            // Должен быть результат "Ok":
            var result = response as OkResult;
            Assert.IsNotNull(result);
            // Запись о том, что указанный пользователь является куратором указанной компании, должна
            // добавиться в БД:
            Assert.IsTrue(_context.CompanyCurators.Count(cc =>
                cc.UserId == user.Id && cc.CompanyId == company.Id) == 1);
        }

        [Test]
        public void DeleteCompanyCuratorTest_Admin()
        {
            SetUp();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание пользователя:
            User user = UserFactory.CreateUser();
            // Добавление пользователя в кураторы компании:
            CompanyCurator curator = CompanyCuratorFactory.Create(company, user, true);
            // Удаление через метод контроллера:
            var response = _controller.DeleteCompanyCurator(company.Id, user.Id);
            // Должен быть результат "Ok":
            var result = response as OkResult;
            Assert.IsNotNull(result);
            // Запись куратора должна быть удалена:
            Assert.IsTrue(curator.IsDeleted);
        }

        [Test]
        public void UpdateAddressCompanyTest_Admin()
        {
            SetUp();

            // Редактирование адреса компании

            // Создание адреса компании:
            CompanyAddress companyAddress = CompanyAddressFactory.Create(_user);
            // Создание модели:
            DeliveryAddressModel model = CreateAddressModel();
            model.CompanyAddressId = companyAddress.Id;

            // Изменение адреса через метод контроллера:
            var response = _controller.UpdateAddressCompany(model);
            // Должен быть результат "Ok":
            var result = response as OkResult;
            Assert.IsNotNull(result);

            // Адрес должен содержать новые данные:
            Address address = companyAddress.Address;
            Assert.IsTrue(address.PostalCode == model.PostalCode && address.CityName == model.CityName &&
                address.StreetName == model.StreetName && address.HouseNumber == model.HouseNumber &&
                address.BuildingNumber == model.BuildingNumber && address.OfficeNumber == model.OfficeNumber &&
                address.FlatNumber == model.FlatNumber &&
                address.AddressComment == model.AddressComment && address.RawAddress == model.RawAddress);

        }

        [Test]
        public void CreateAddressCompanyTest_Admin()
        {
            SetUp();

            // Добавление адреса компании

            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание модели адреса:
            DeliveryAddressModel model = CreateAddressModel();
            // Добавление адреса через метод контроллера:
            var response = _controller.CreateAddressCompany(company.Id, model);
            // Должна получиться модель адреса компании:
            DeliveryAddressModel result = TransformResult.GetObject<DeliveryAddressModel>(response);
            Assert.IsNotNull(result);
            // Данные модели должны быть такими же, как у той модели, которую передавали контроллеру:
            Assert.IsTrue(result.PostalCode == model.PostalCode && result.CityName == model.CityName &&
                result.StreetName == model.StreetName && result.HouseNumber == model.HouseNumber &&
                result.BuildingNumber == model.BuildingNumber && result.OfficeNumber == model.OfficeNumber &&
                result.FlatNumber == model.FlatNumber && result.AddressComment == model.AddressComment && result.RawAddress == model.RawAddress);
            // В БД должен быть такой же адрес:
            Address dbAddress = _context.Addresses.FirstOrDefault(a => a.Id == result.Id && !a.IsDeleted);
            Assert.IsNotNull(dbAddress);
            Assert.IsTrue(dbAddress.PostalCode == model.PostalCode && dbAddress.CityName == model.CityName &&
                dbAddress.StreetName == model.StreetName && dbAddress.HouseNumber == model.HouseNumber &&
                dbAddress.BuildingNumber == model.BuildingNumber && dbAddress.OfficeNumber == model.OfficeNumber &&
                dbAddress.FlatNumber == model.FlatNumber && dbAddress.AddressComment == model.AddressComment && dbAddress.RawAddress == model.RawAddress);
        }

        [Test]
        public void CreateAddressCompanyTest_Admin_AlreadyExists()
        {
            SetUp();

            // Добавление адреса компании в случае, если такой же адрес уже существует

            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Создание адреса компании:
            CompanyAddress companyAddress = CompanyAddressFactory.Create(_user, company);
            // Создание модели адреса:
            DeliveryAddressModel model = CreateAddressModel();
            // Установка данных адреса такими же, как у создаваемого:
            Address address = companyAddress.Address;
            address.CityName = model.CityName;
            address.StreetName = model.StreetName;
            address.HouseNumber = model.HouseNumber;
            address.OfficeNumber = model.OfficeNumber;
            address.PostalCode = model.PostalCode;
            address.BuildingNumber = model.BuildingNumber;
            address.FlatNumber = model.FlatNumber;
            address.AddressComment = model.AddressComment;
            address.RawAddress = model.RawAddress;
            // Добавление адреса через метод контроллера:
            var response = _controller.CreateAddressCompany(company.Id, model);
            // Должен прийти ответ BadRequest:
            var result = response as BadRequestObjectResult;
            Assert.IsNotNull(result);
        }

        [Test]
        public void DeleteAddressCompanyTest_Admin()
        {
            SetUp();

            // Создание адреса компании:
            CompanyAddress companyAddress = CompanyAddressFactory.Create(_user);
            // Удаление адреса через метод контроллера:
            var response = _controller.DeleteAddressCompany(companyAddress.Id);
            // Должен быть результат "Ok":
            var result = response as OkResult;
            Assert.IsNotNull(result);
            // Адрес должен удалиться:
            Assert.IsTrue(companyAddress.IsDeleted && !companyAddress.IsActive);
        }

        [Test]
        public void AddUserCompanyLinkTest_Admin()
        {
            SetUp();

            // Создание пользователя:
            User user = UserFactory.CreateUser();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Добавление пользователя в компанию через метод контроллера:
            var response = _controller.AddUserCompanyLink(company.Id, user.Id);
            // Должнен придти ответ true:
            bool result = TransformResult.GetPrimitive<bool>(response);
            Assert.IsTrue(result);
            // В БД должна появиться связь:
            UserInCompany link = _context.UsersInCompanies.FirstOrDefault(uic => uic.UserId == user.Id &&
                uic.CompanyId == company.Id && !uic.IsDeleted && uic.IsActive);
            Assert.IsNotNull(link);
        }

        [Test]
        public void EditUserCompanyLinkTest_Admin()
        {
            SetUp();

            // Создание пользователя:
            User user = UserFactory.CreateUser();
            // Создание двух компаний:
            Company companOld = CompanyFactory.Create(_user);
            Company companNew = CompanyFactory.Create(_user);
            // Назначение пользователя в компанию:
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(user, companOld);
            // Добавление пользователя в другую компанию через метод контроллера:
            var response = _controller.EditUserCompanyLink(companNew.Id, companOld.Id, user.Id);
            // Должнен придти ответ true:
            bool result = TransformResult.GetPrimitive<bool>(response);
            Assert.IsTrue(result);
            // В БД должна связь должна измениться:
            Assert.IsTrue(userInCompany.CompanyId == companNew.Id);
        }

        [Test]
        public void RemoveUserCompanyLinkTest_Admin()
        {
            SetUp();

            // Создание пользователя:
            User user = UserFactory.CreateUser();
            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            // Назначение пользователя в компанию:
            UserInCompany userInCompany = UserInCompanyFactory.CreateRoleForUser(user, company);
            // Удаление назначения через метод контроллера:
            var response = _controller.RemoveUserCompanyLink(company.Id, user.Id);
            // Должнен придти ответ true:
            bool result = TransformResult.GetPrimitive<bool>(response);
            Assert.IsTrue(result);
            // В БД должна связь должна быть удалена:
            Assert.IsTrue(userInCompany.IsDeleted && !userInCompany.IsActive);
        }

        [Test]
        public async Task SetSmsNotifyTest()
        {
            SetUp();

            // Создание компании:
            Company company = CompanyFactory.Create(_user);
            Random random = new Random(DateTime.Now.Millisecond);
            bool smsNotify = random.Next(1, 10) > 5;
            company.SmsNotify = smsNotify;
            // Изменение флага смс-оповещения через метод контроллера:
            smsNotify = !smsNotify;
            var response = await _controller.SetSmsNotify(company.Id, smsNotify);
            // Должен быть результат "Ok":
            var result = response as OkResult;
            Assert.IsNotNull(result);
            // В БД у компании флаг должен поменяться:
            Assert.IsTrue(company.SmsNotify == smsNotify);
        }

        [Test]
        public async Task GetMyCompanyForOrderTest()
        {
            SetUp();

            // Метод GetMyCompanyForOrder выбирает компании, в которых состоит пользователь
            // (есть записи в таблице БД UsersInCompanies) и у которых есть корпоративные заказы
            // на указанную дату.
            // Создаём несколько компаний:
            List<Company> companies = CompanyFactory.CreateFew(8, _user);

            // Назначение пользователя в две компании:
            Company company1 = companies.ElementAt(1);
            UserInCompanyFactory.CreateRoleForUser(_user, company1);
            Company company2 = companies.ElementAt(3);
            UserInCompanyFactory.CreateRoleForUser(_user, company2);
            Company company3 = companies.ElementAt(5);
            UserInCompanyFactory.CreateRoleForUser(_user, company3);

            // Создание корпоративных заказов для компаний 2 и 3:
            CompanyOrderFactory.Create(_user, company2);
            CompanyOrderFactory.Create(_user, company3);

            // Выбираем через контроллер компании пользователя, у которых есть корпоративные заказы:
            var response = await _controller.GetMyCompanyForOrder();
            ResponseModel result = TransformResult.GetObject<ResponseModel>(response);
            // Результат должен быть успешным:
            Assert.IsTrue(result.Status == 0);
            // Компаний должно быть две:
            var compsWithOrders = (IEnumerable<CompanyModel>)result.Result;
            Assert.IsTrue(compsWithOrders.Count() == 2);
            // Должны быть именно те компании, для которых созданы корпоративные заказы:
            Assert.IsTrue(compsWithOrders.Any(c => c.Id == company2.Id) &&
                compsWithOrders.Any(c => c.Id == company3.Id));
        }


        private DeliveryAddressModel CreateAddressModel()
        {
            DeliveryAddressModel model = new DeliveryAddressModel();
            Random random = new Random(DateTime.Now.Millisecond);
            string randStr = random.Next(10, 10000).ToString();
            model.PostalCode = random.Next(100000, 999999).ToString();
            model.CityName = $"City-{randStr}";
            model.StreetName = $"Street-{randStr}";
            model.HouseNumber = random.Next(10, 80).ToString();
            model.BuildingNumber = random.Next(1, 10).ToString();
            model.OfficeNumber = random.Next(1, 10).ToString();
            model.FlatNumber = random.Next(1, 10).ToString();
            model.AddressComment = random.Next(100, 900).ToString();
            model.RawAddress = $"{model.PostalCode}, г. {model.CityName}, ул. {model.StreetName}, д. {model.HouseNumber}, оф. {model.OfficeNumber}";
            return model;
        }

    }
}
