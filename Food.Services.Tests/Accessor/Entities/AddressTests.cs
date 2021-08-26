using System;
using System.Linq;
using Castle.Core.Internal;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    public class AddressTests
    {
        private FakeContext _context;
        private User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        [Test]
        public void GetAddressByIdTest()
        {
            var address = AddressFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetAddressById(address.Id);
            Assert.IsTrue(address == result);
        }

        [Test]
        public void GetAllAddressesTest()
        {
            foreach (var l in _context.Addresses.Select(e => e.Id))
                _context.Addresses.Remove(_context.Addresses.First(e => e.Id == l));
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetAllAddresses();
            Assert.IsTrue(result.IsNullOrEmpty());
            var address = AddressFactory.CreateFew();
            result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetAllAddresses();
            Assert.IsTrue(result.Count == address.Count);
        }

        [Test]
        public void AddAddressTest()
        {
            var ars = new Address { StreetName = Guid.NewGuid().ToString("N") };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddAddress(ars.CityId, ars.StreetId, ars.HouseId, 0, ars.CityName, ars.StreetName, ars.HouseNumber, ars.BuildingNumber, ars.FlatNumber, ars.OfficeNumber, ars.EntranceNumber, ars.StoreyNumber, ars.IntercomNumber, ars.ExtraInfo, ars.PostalCode, ars.AddressComment, ars.RawAddress);
            Assert.IsTrue(result > -1);
            Assert.IsNotNull(_context.Addresses.FirstOrDefault(e => e.StreetName == ars.StreetName));
        }

        [Test]
        public void UpdateAddressTest()
        {
            var address = AddressFactory.Create();
            var changed = new Address() { StreetName = Guid.NewGuid().ToString("N"), Id = address.Id };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.UpdateAddress(changed, _user.Id);
            Assert.IsTrue(changed.StreetName == address.StreetName);
            Assert.IsTrue(result == address.Id);
            Assert.IsTrue(address.LastUpdateByUserId == _user.Id);
            Assert.IsTrue(address.LastUpdDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void GetCompanyAddressesTest()
        {
            var company = CompanyFactory.Create();
            var addresses = CompanyAddressFactory.CreateFew(company: company);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanyAddresses(company.Id);
            if (result == null)
                throw new Exception("FUUUUUUUUUUCK");
            Assert.IsTrue(result.Sum(e => e.Id) == addresses.Sum(e => e.Id));
        }
    }
}