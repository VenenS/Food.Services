using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region UserRoleLink

        /// <summary>
        /// Получение списка всех существующих привязок пользователей к ролям.
        /// </summary>
        /// <returns>Список привязок.</returns>
        public List<UserInRole> GetListUserRole()
        {
            List<UserInRole> listOfUserRoleM;

            using (var fc = GetContext())
            {
                listOfUserRoleM = fc.UsersInRoles.AsNoTracking().Where(ur => ur.IsDeleted == false).ToList();
            }

            return listOfUserRoleM;
        }

        /// <summary>
        /// Получение списка ролей, к которым привязан пользователь.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список ролей.</returns>
        public List<Role> GetListRoleToUser(Int64 userId)
        {
            List<Role> listOfRolesM;

            using (var fc = GetContext())
            {
                listOfRolesM = fc.UsersInRoles.AsNoTracking().Where(us => us.UserId == userId && us.IsDeleted == false).Select(e => e.Role).ToList();
            }

            return listOfRolesM;
        }

        /// <summary>
        /// Добавление привязки пользователя к роли.
        /// </summary>
        /// <param name="newUserRole">Роль пользователя.</param>
        /// <returns>true - добавление прошло успешно. 
        /// false - добавление прошло с ошибкой.</returns>
        public bool AddUserToRole(UserInRole newUserRole)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var entityExists = fc.UsersInRoles.FirstOrDefault(o => o.RoleId == newUserRole.RoleId && o.UserId == newUserRole.UserId);

                    if (entityExists != null)
                    {
                        entityExists.IsDeleted = false;
                    }
                    else
                    {
                        fc.UsersInRoles.Add(newUserRole);
                    }

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Добавление привязки пользователя к роли.
        /// </summary>
        public bool AddUserToRole(long userId, string roleName)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var role = fc.Roles.FirstOrDefault(o => !o.IsDeleted && o.RoleName == roleName);
                    if (role != null)
                    {

                        var entityExists = fc.UsersInRoles.FirstOrDefault(o => o.RoleId == role.Id && o.UserId == userId);
                        if (entityExists != null)
                        {
                            entityExists.IsDeleted = false;
                        }
                        else
                        {
                            fc.UsersInRoles.Add(new UserInRole() { UserId = userId, RoleId = role.Id });
                        }

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Изменение привязки пользователя к роли
        /// </summary>
        /// <param name="userRoleLink">Роль пользователя.</param>
        /// <returns>true - изменение прошло успешно. 
        /// false - изменение прошло с ошибкой.</returns>
        public bool EditUserRole(UserInRole userRoleLink)
        {
            try
            {
                using (var fc = GetContext())
                {
                    UserInRole oldUserRoleLinkM =
                        fc.UsersInRoles.FirstOrDefault(
                            uc => uc.Id == userRoleLink.Id
                                  && uc.IsDeleted == false
                        );

                    if (oldUserRoleLinkM != null)
                    {
                        oldUserRoleLinkM.RoleId = userRoleLink.RoleId;
                        oldUserRoleLinkM.UserId = userRoleLink.UserId;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Удаление привязки пользователя к роли.
        /// </summary>
        /// <param name="userRoleLink">Роль пользователя.</param>
        /// <returns>true - удаление прошло успешно. 
        /// false - удаление прошло с ошибкой.</returns>
        public bool RemoveUserRole(UserInRole userRoleLink)
        {
            try
            {
                using (var fc = GetContext())
                {
                    UserInRole oldUserRoleLinkM =
                        fc.UsersInRoles.FirstOrDefault(
                            uc => uc.Id == userRoleLink.Id
                                  && uc.IsDeleted == false
                        );

                    if (oldUserRoleLinkM != null)
                    {
                        oldUserRoleLinkM.IsDeleted = true;
                    }
                    else
                    {
                        return false;
                    }
                        
                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Удаление привязки пользователя к роли.
        /// </summary>
        public bool RemoveUserRole(long userId, string roleName)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var role = fc.Roles.FirstOrDefault(o => !o.IsDeleted && o.RoleName.Trim().ToLower() == roleName);

                    if (role.RoleName == EnumUserRole.Manager)
                    {
                        //Проверка наличия привязки пользователя к кафе
                        var cafeLink = fc.CafeManagers.FirstOrDefault(c => c.UserId == userId && !c.IsDeleted);
                        if (cafeLink != null)
                            return true;
                    }

                    var entityExists = fc.UsersInRoles.FirstOrDefault(o => o.RoleId == role.Id && o.UserId == userId);
                    if (entityExists != null)
                    {
                        entityExists.IsDeleted = true;
                        fc.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
