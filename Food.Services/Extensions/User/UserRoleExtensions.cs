using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class UserRoleExtensions
    {
        public static UserRoleModel GetContract(this UserInRole role)
        {
            return role == null
                ? null
                : new UserRoleModel
                {
                    Id = role.Id,
                    RoleId = role.RoleId,
                    UserId = role.UserId
                };
        }

        public static UserInRole GetEntity(this UserRoleModel role)
        {
            return role == null
                ? new UserInRole()
                : new UserInRole
                {
                    Id = role.Id,
                    RoleId = role.RoleId,
                    UserId = role.UserId
                };
        }
    }
}
