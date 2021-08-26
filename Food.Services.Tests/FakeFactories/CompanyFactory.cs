using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    //Не работает, доделать!
    static class CompanyFactory
    {
        public static Company Create(User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var juridicalAddress = AddressFactory.Create();
            var mainDeliveryAddress = AddressFactory.Create();
            var postAdress = AddressFactory.Create();
            var company = new Company()
            {
                Name = Guid.NewGuid().ToString("n").Substring(0, 7), CreationDate = DateTime.Now.AddYears(-1), CreatorId = creator.Id,
                JuridicalAddress = juridicalAddress, JuridicalAddressId = juridicalAddress.Id, MainDeliveryAddress = mainDeliveryAddress,
                MainDeliveryAddressId = mainDeliveryAddress.Id, PostAddress = postAdress, PostAddressId = postAdress.Id, IsActive = true
            };
            ContextManager.Get().Companies.Add(company);
            return company;
        }

        public static List<Company> CreateFew(int count = 3, User creator = null)
        {
            var companies = new List<Company>();
            for (var i = 0; i < count; i++)
                companies.Add(Create(creator));
            return companies;
        }
    }
}
