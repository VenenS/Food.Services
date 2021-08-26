using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class RoleExtensions
    {
        public static RoleModel GetContract(this Role role)
        {
            return role == null
                ? null
                : new RoleModel
                {
                    Id = role.Id,
                    RoleName = role.RoleName
                };
        }

        public static Role GetEntity(this RoleModel role)
        {
            return role == null
                ? new Role()
                : new Role
                {
                    Id = role.Id,
                    RoleName = role.RoleName
                };
        }
    }
}
