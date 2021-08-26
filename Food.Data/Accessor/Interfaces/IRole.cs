using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IRole
    {
        #region Role
        /// <summary>
        /// Возвращает список ролей
        /// </summary>
        /// <returns></returns>
        List<Role> GetRoles();

        /// <summary>
        /// Возвращает роль по имени
        /// </summary>
        /// <param name="roleName">имя роли</param>
        /// <returns></returns>
        Role GetRoleByName(string roleName);

        /// <summary>
        /// Возвращает роль по идентификатору
        /// </summary>
        /// <param name="roleId">идентификатор роли</param>
        /// <returns></returns>
        Role GetRoleById(long roleId);

        /// <summary>
        /// Возвращает роли пользователя
        /// </summary>
        /// <param name="userId">идентификатор роли</param>
        List<Role> GetRolesByUserId(long userId);

        #endregion
    }
}
