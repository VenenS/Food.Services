using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Food.Services.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly HttpContext _httpContext;
        private readonly ILogger<CurrentUserService> _logger;
        private User _user;
        private Company _company;
        private List<long> _managedCafes;
        private bool _companyInitialized;
        private bool _managedCafesInitialized;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor,
                                  ILogger<CurrentUserService> logger)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
        }

        public User GetUser()
        {
            if (!_httpContext.User.Identity.IsAuthenticated)
                return null;
            if (_user == null)
                _user = _httpContext.User.Identity.GetUserById();

            return _user;
        }

        public long? GetUserId()
        {
            if (!_httpContext.User.Identity.IsAuthenticated)
                return null;
            return _httpContext.User.Identity.GetUserId();
        }

        public Company GetUserCompany()
        {
            var userId = GetUserId();
            if (userId == null)
                return null;

            if (!_companyInitialized)
            {
                var companies = Accessor.Instance.GetCompanies(userId.Value);
                if (companies.Count > 1)
                {
                    _logger.LogWarning("Пользователь {userId} привязан к {count} компаниям, а должна быть одна",
                                       userId, companies.Count);
                }

                _company = companies.FirstOrDefault();
                _companyInitialized = true;
            }
            return _company;
        }

        public CompanyOrder GetUserCompanyOrder(long cafeId, DateTime when)
        {
            var company = GetUserCompany();
            if (company == null)
                return null;

            return Accessor.Instance.GetCompanyOrderForUserByCompanyId(cafeId, company.Id, when);
        }

        public bool IsCompanyEmployee() => GetUserCompany() != null;

        public bool IsCafeManager(long cafeId)
        {
            var userId = GetUserId();
            if (userId == null)
                return false;

            if (!_managedCafesInitialized)
            {
                var cafes = Accessor.Instance.GetManagedCafes(userId.Value) ?? new List<Cafe>();
                _managedCafes = cafes.Select(x => x.Id).ToList();
                _managedCafesInitialized = true;
            }

            return _managedCafes.Any(id => id == cafeId);
        }

        public void InvalidateCache()
        {
            _user = null;
            _company = null;
            _companyInitialized = false;
            _managedCafesInitialized = false;
        }
    }
}
