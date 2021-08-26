using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class AddressFactory
    {
        public static Address Create(User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var address = new Address
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false, 

            };
            ContextManager.Get().Addresses.Add(address);
            return address;
        }

        public static List<Address> CreateFew(int count = 3, User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var addresses = new List<Address>();
            for (var i = 0; i < count; i++)
                addresses.Add(Create(creator));
            return addresses;
        }
    }
}
