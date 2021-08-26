using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IUserRole
    {
        #region UserRoleLink

        /// <summary>
        /// Получение списка всех существующих привязок пользователей к ролям.
        /// </summary>
        /// <returns>Список привязок.</returns>
        List<UserInRole> GetListUserRole();

        /// <summary>
        /// Получение списка ролей, к которым привязан пользователь.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список ролей.</returns>
        List<Role> GetListRoleToUser(Int64 userId);

        /// <summary>
        /// Добавление привязки пользователя к роли.
        /// </summary>
        /// <param name="newUserRole">Роль пользователя.</param>
        /// <returns>true - добавление прошло успешно. 
        /// false - добавление прошло с ошибкой.</returns>
        bool AddUserToRole(UserInRole newUserRole);

        /// <summary>
        /// Добавление привязки пользователя к роли.
        /// </summary>
        bool AddUserToRole(long userId, string roleName);

        /// <summary>
        /// Изменение привязки пользователя к роли
        /// </summary>
        /// <param name="userRoleLink">Роль пользователя.</param>
        /// <returns>true - изменение прошло успешно. 
        /// false - изменение прошло с ошибкой.</returns>
        bool EditUserRole(UserInRole userRoleLink);

        /// <summary>
        /// Удаление привязки пользователя к роли.
        /// </summary>
        /// <param name="userRoleLink">Роль пользователя.</param>
        /// <returns>true - удаление прошло успешно. 
        /// false - удаление прошло с ошибкой.</returns>
        bool RemoveUserRole(UserInRole userRoleLink);

        /// <summary>
        /// Удаление привязки пользователя к роли.
        /// </summary>
        bool RemoveUserRole(long userId, string roleName);

        #endregion
    }
}
