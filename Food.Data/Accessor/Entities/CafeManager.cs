using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region CafeManager
        /// <summary>
        /// Возвращает true, если пользователь является управляющим кафе
        /// </summary>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual bool IsUserManagerOfCafe(long userId, long cafeId)
        {
            var fc = GetContext();

            var isManagerOfCafe = fc.CafeManagers.AsNoTracking()
                .Any(cm => cm.CafeId == cafeId
                            && cm.UserId == userId
                            //&& cm.Cafe.IsActive == true
                            && cm.Cafe.IsDeleted == false);

            return isManagerOfCafe;
        }
        
        /// <summary>
        /// Возвращает true, если пользователь является управляющим кафе
        /// #ref too many references to main method at this moment
        /// </summary>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual bool IsUserManagerOfCafeIgnoreActivity(long userId, long cafeId)
        {
            var fc = GetContext();

            var isManagerOfCafe = fc.CafeManagers.AsNoTracking()
                .Any(cm => cm.CafeId == cafeId
                            && cm.UserId == userId
                            && cm.Cafe.IsDeleted == false);

            return isManagerOfCafe;
        }
        #endregion

        #region UserCafeLink

        /// <summary>
        /// Получает список кафе, к которым привязан пользователь.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список кафе.</returns>
        public virtual List<Cafe> GetListOfCafeByUserId(long userId)
        {
            try
            {
                List<Cafe> listOfCafe;
                using (var fc = GetContext())
                {
                    var query = fc.CafeManagers.AsNoTracking().Where(ca => ca.UserId == userId && ca.IsDeleted == false).Select(ca => ca.Cafe);

                    listOfCafe = query.OrderBy(c => c.CafeName).ToList();
                }

                return listOfCafe;
            }
            catch (Exception)
            {
                throw new Exception("Error of getting list.");
            }
        }

        /// <summary>
        /// Добавляет новую привязку пользователя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Новая привязка.</param>
        /// <param name="authorId"></param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        public virtual bool AddUserCafeLink(CafeManager userCafeLink, long authorId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var entityExists = fc.CafeManagers.FirstOrDefault(
                        o => o.UserId == userCafeLink.UserId
                        && o.CafeId == userCafeLink.CafeId);

                    if (entityExists != null)
                    {
                        entityExists.IsDeleted = false;
                        entityExists.LastUpdateBy = authorId;
                        entityExists.LastUpdateDate = DateTime.Now;
                    }
                    else
                    {
                        fc.CafeManagers.Add(userCafeLink);
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
        /// Изменение существуещей привязки пользорвателя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Обновленная привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        public virtual bool EditUserCafeLink(CafeManager userCafeLink)
        {
            using (var fc = GetContext())
            {
                CafeManager oldCafeManagerLink =
                    fc.CafeManagers.FirstOrDefault(
                        t => t.CafeId == userCafeLink.CafeId
                        && t.UserId == userCafeLink.UserId
                        && t.IsDeleted == false
                    );

                if (oldCafeManagerLink != null)
                {
                    oldCafeManagerLink.CafeId = userCafeLink.CafeId;
                    oldCafeManagerLink.UserId = userCafeLink.UserId;
                    oldCafeManagerLink.UserRoleId = userCafeLink.UserRoleId;
                    oldCafeManagerLink.LastUpdateBy = userCafeLink.LastUpdateBy;
                    oldCafeManagerLink.LastUpdateDate = DateTime.Now;
                    oldCafeManagerLink.IsDeleted = false;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Удаление существующей привязки пользователя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        public virtual bool RemoveUserCafeLink(CafeManager userCafeLink)
        {
            using (var fc = GetContext())
            {
                CafeManager oldCafeManagerLink =
                    fc.CafeManagers.FirstOrDefault(
                        t => t.CafeId == userCafeLink.CafeId
                        && t.UserId == userCafeLink.UserId
                        && t.IsDeleted == false
                    );

                if (oldCafeManagerLink != null)
                {
                    oldCafeManagerLink.IsDeleted = true;
                    oldCafeManagerLink.LastUpdateBy = userCafeLink.LastUpdateBy;
                    oldCafeManagerLink.LastUpdateDate = DateTime.Now;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
