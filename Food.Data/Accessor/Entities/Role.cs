using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Role
        /// <summary>
        /// Возвращает список ролей
        /// </summary>
        /// <returns></returns>
        public List<Role> GetRoles()
        {
            List<Role> roles;

            using (var fc = GetContext())
            {
                roles = fc.Roles.AsNoTracking().Where(r => r.IsDeleted == false).ToList();
            }

            return roles;
        }

        /// <summary>
        /// Возвращает роль по имени
        /// </summary>
        /// <param name="roleName">имя роли</param>
        /// <returns></returns>
        public Role GetRoleByName(string roleName)
        {
            Role role;

            using (var fc = GetContext())
            {
                role = fc.Roles.AsNoTracking().FirstOrDefault(
                    r => r.RoleName == roleName
                         && r.IsDeleted == false
                    );
            }

            return role;
        }

        /// <summary>
        /// Возвращает роль по идентификатору
        /// </summary>
        /// <param name="roleId">идентификатор роли</param>
        /// <returns></returns>
        public Role GetRoleById(long roleId)
        {
            Role role;

            using (var fc = GetContext())
            {
                role = fc.Roles.AsNoTracking().FirstOrDefault(
                    r => r.Id == roleId && r.IsDeleted == false
                    );
            }

            return role;
        }

        /// <summary>
        /// Возвращает роли пользователя
        /// </summary>
        /// <param name="userId">идентификатор роли</param>
        public List<Role> GetRolesByUserId(long userId)
        {
            List<Role> roles;

            using (var fc = GetContext())
            {
                roles = fc.UsersInRoles.AsNoTracking().Where(
                    r => r.UserId == userId
                    && r.IsDeleted == false
                    && !r.Role.IsDeleted)
                    .Select(o => o.Role).ToList();
            }

            return roles;
        }

        #endregion
    }
}
