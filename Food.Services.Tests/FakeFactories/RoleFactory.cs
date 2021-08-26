using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    static class RoleFactory
    {
        public static Role CreateRole()
        {
            var role = new Role
            {
                RoleName = Guid.NewGuid().ToString("n"),
            };
            ContextManager.Get().Roles.Add(role);
            return role;
        }
        public static Role CreateRole(string roleName)
        {
            var role = new Role
            {
                IsDeleted=false,
                RoleName = roleName
            };
            ContextManager.Get().Roles.Add(role);
            return role;
        }

        public static List<Role> CreateFewRoles(int count = 3)
        {
            var roles = new List<Role>();
            for (var i = 0; i < count; i++)
                roles.Add(CreateRole());
            return roles;
        }
    }
}