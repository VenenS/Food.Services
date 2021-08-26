using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class CompanyAddressFactory
    {
        public static CompanyAddress Create(User creator = null, Company company = null, Address address = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            address = address ?? AddressFactory.Create(creator);
            var companyAddress = new CompanyAddress
            {
                CreateDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                Address = address,
                AddressId = address.Id,
                Company = company,
                CompanyId = company.Id
            };
            ContextManager.Get().CompanyAddresses.Add(companyAddress);
            return companyAddress;
        }

        public static List<CompanyAddress> CreateFew(int count = 3, User creator = null, Company company = null,
            Address address = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            address = address ?? AddressFactory.Create(creator);
            var addresses = new List<CompanyAddress>();
            for (var i = 0; i < count; i++)
                addresses.Add(Create(creator, company, address));
            return addresses;
        }
    }
}