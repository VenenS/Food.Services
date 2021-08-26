using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    public static class UserServiceHelper
    {
        public static bool IsNewUserRoleLinkAvailable(UserInRole userRole)
        {
            if (userRole == null)
                return false;

            var user =
                Accessor.Instance.GetUserById(userRole.UserId);

            var role =
                Accessor.Instance.GetRoleById(userRole.RoleId);

            if (user != null && role != null)
            {
                var userRoles =
                    Accessor.Instance.GetListRoleToUser(user.Id);

                var isExist =
                    userRoles.All(r => r.Id != role.Id);

                if (!isExist)
                    throw new Exception("Уже существует данная привязка роли для данного пользователя.");

                return true;
            }

            throw new Exception("Отсутствуют роль или пользователь.");
        }
    }
}
