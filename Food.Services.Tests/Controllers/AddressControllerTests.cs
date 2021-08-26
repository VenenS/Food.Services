using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    public class AddressControllerTests
    {
        FakeContext _context;
        AddressController _controller;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;
        Address _address;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new AddressController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _address = AddressFactory.Create();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
        }

        [Test]
        public void CreateTest()
        {
            SetUp();

            var model = new DeliveryAddressModel()
            {
                CityName = "",
                StreetName = "",
                HouseNumber = "",
                OfficeNumber = ""
            };

            var response = _controller.Create(model);
            var result = TransformResult.GetPrimitive<long>(response);
            Assert.IsTrue(result >= 0);
        }

        [Test]
        public void GetTest()
        {
            SetUp();
            _context.Addresses.Add(_address);

            var response = _controller.Get();
            var result = TransformResult.GetObject<IEnumerable<DeliveryAddressModel>>(response).ToList();
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void GetTestById()
        {
            SetUp();
            _context.Addresses.Add(_address);

            var response = _controller.Get(_address.Id);
            var result = TransformResult.GetObject<DeliveryAddressModel>(response);
            Assert.IsTrue(result.Id == _address.Id);
        }

        [Test]
        public void DeleteTest()
        {
            SetUp();
            _context.Addresses.Add(_address);

            var response = _controller.Delete(_address.Id);
            Assert.IsTrue(TransformResult.GetPrimitive<bool>(response));
        }

        [Test]
        public void UpdateTest()
        {
            SetUp();
            _context.Addresses.Add(_address);

            var model = new DeliveryAddressModel()
            {
                CityName = "b",
                StreetName = "",
                HouseNumber = "",
                OfficeNumber = "",

                Id = _address.Id
            };

            var response = _controller.Update(model);
            var result = TransformResult.GetObject<DeliveryAddressModel>(response);
            Assert.IsTrue(result.CityName == _address.CityName);
        }

        [Test]
        public void GetCompanyAddressesTest()
        {
            SetUp();
            var company = CompanyFactory.Create();
            var address = CompanyAddressFactory.Create();

            company.MainDeliveryAddressId = address.Id;
            address.CompanyId = company.Id;
            address.Company = company;

            var response = _controller.GetCompanyAddresses(company.Id);
            var result = TransformResult.GetObject<List<DeliveryAddressModel>>(response);
            Assert.IsTrue(result.Count > 0);
            Assert.IsTrue(result[0].Id == address.AddressId);
        }


    }
}
