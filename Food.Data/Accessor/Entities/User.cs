using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region User

        /// <summary>
        /// Добавляем пользователя
        /// </summary>
        public long AddUser(User user)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.Users.Add(user);
                    fc.SaveChanges();

                    return user.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        //TODO: Авторизация через соцсети. Удалить
        //public bool AddExternalLogin(UserExternalLogin login)
        //{
        //    try
        //    {
        //        using (var fc = new FoodContext())
        //        {
        //            fc.UserExternalLogin.Add(login);
        //            fc.SaveChanges();
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Проверяет уникальное ли email у пользователя
        /// </summary>
        public bool CheckUniqueEmail(string email, long userId = -1)
        {
            bool result;
            var fc = GetContext();
            User existEntity;
            email = email.Trim().ToLower();
            if (userId <= 0)
                existEntity = fc.Users.AsNoTracking().FirstOrDefault(o => o.Email.Trim().ToLower() == email);
            else
                existEntity = fc.Users.AsNoTracking().FirstOrDefault(o => o.Id != userId && o.Email.Trim().ToLower() == email);

            result = existEntity == null;

            return result;
        }

        /// <summary>
        /// Вернуть юзера по логину
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public User GetUserByLogin(string login)
        {
            if (String.IsNullOrEmpty(login))
            {
                return null;
            }

            var fc = GetContext();

            var query = from u in fc.Users.AsNoTracking()
                        where (
                            u.Name.ToLower() == login.ToLower()
                            && u.IsDeleted == false
                        )
                        select u;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Get user by e-mail
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var fc = GetContext();

            var query = from u in fc.Users.AsNoTracking()
                        where (
                            u.Email.ToLower() == email.ToLower()
                            && u.IsDeleted == false
                        )
                        select u;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Получение пользователя по номеру телефона
        /// (не удален и не заблокирован)
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public User GetUserByPhone(string phone)
        {
            return GetContext().Users.AsNoTracking().FirstOrDefault(e => !e.IsDeleted && !e.Lockout && e.PhoneNumber == new string(phone.Where(c => char.IsDigit(c)).ToArray()));
        }

        //TODO: Авторизация через соцсети. Удалить
        /// <summary>
        /// Получение пользователя по externalLoginInfo
        /// </summary>
        //public User GetUserByExternalLoginInfo(string loginProvider, string providerKey)
        //{
        //    User entity;
        //    using (var fc = new FoodContext())
        //    {
        //        var extLogin = fc.UserExternalLogin.AsNoTracking().FirstOrDefault(
        //            o => o.LoginProvider == loginProvider
        //            && o.ProviderKey == providerKey);

        //        entity = extLogin?.User;
        //    }

        //    return entity;
        //}

        /// <summary>
        /// Вурнуть юзера по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual User GetUserById(long id)
        {
            IFoodContext fc = GetContext();
            
            var query = from u in fc.Users.AsNoTracking()
                        where (u.Id == id && u.IsDeleted == false)
                        select u;
            
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Получает список пользователей, которые имеют определенную роль.
        /// </summary>
        /// <param name="roleId">Идентификатор роли.</param>
        /// <returns>Список пользователей.</returns>
        public List<User> GetUserByRoleId(Int64 roleId)
        {
            List<User> listOfUser;

            using (var fc = GetContext())
            {
                var query = from uc in fc.UsersInRoles.AsNoTracking()
                            where uc.RoleId == roleId && uc.IsDeleted == false
                            select uc.User;

                listOfUser = query.ToList();
            }

            return listOfUser;
        }

        /// <summary>
        /// Получает список пользователей, привязанных к определенному кафе/компании.
        /// </summary>
        /// <param name="id">Идентификатор организации</param>
        /// <returns>Список пользователей.</returns>
        public List<User> GetUsersByCafe(long id)
        {
            List<User> listOfUsers;

            using (var fc = GetContext())
            {
                var query = from uc in fc.CafeManagers.AsNoTracking()
                            where uc.CafeId == id && uc.IsDeleted == false
                            select uc.User;

                listOfUsers = query.ToList();
            }

            return listOfUsers;
        }

        /// <summary>
        /// Получает пользователя по device_uuid
        /// </summary>
        public User GetUserByDeviceUuid(string deviceUuid)
        {
            User user;

            using (var fc = GetContext())
            {
                user = fc.Users.AsNoTracking().FirstOrDefault(o => !o.IsDeleted && o.DeviceUuid == deviceUuid);
            }

            return user;
        }

        /// <summary>
        /// Получает список пользователей без кураторов, которым назначено кафе
        /// </summary>
        /// <returns></returns>
        public List<User> GetUsersWithoutCurators()
        {
            List<User> listOfUsers;

            using (var fc = GetContext())
            {
                var query = fc.CompanyCurators.AsNoTracking().Where(o => !o.IsDeleted).ToList().Select(o => o.UserId).ToList();
                var hashSetUsers = new HashSet<long>(query);

                listOfUsers = fc.Users.AsNoTracking().Where(
                    o => !o.IsDeleted
                    && !hashSetUsers.Contains(o.Id)).ToList();
            }

            return listOfUsers;
        }

        /// <summary>
        /// Получение полного списка пользователей.
        /// </summary>
        /// <returns>Список пользователей.</returns>
        public List<User> GetFullListOfUsers()
        {
            List<User> listOfUsers;

            using (var fc = GetContext())
            {
                var query = from u in fc.Users.AsNoTracking()
                            where u.IsDeleted == false
                            select u;

                listOfUsers = query.ToList();
            }

            return listOfUsers;
        }

        /// <summary>
        /// Изменение данных пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <returns>true - успешно, false - ошибка изменения.</returns>
        public bool EditUser(User user)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldUser =
                        fc.Users.FirstOrDefault(c => c.Id == user.Id && c.IsDeleted == false);
                    
                    var phone = new string(user.PhoneNumber.Where(c => char.IsDigit(c)).ToArray());

                    if (oldUser != null)
                    {
                        oldUser.Email = user.Email;
                        oldUser.EmailConfirmed = user.Email == oldUser.Email;
                        oldUser.FullName = user.FullName;
                        oldUser.DefaultAddressId = user.DefaultAddressId;
                        oldUser.DisplayName = user.DisplayName;
                        oldUser.LastUpdateByUserId = user.LastUpdateByUserId;
                        oldUser.LastUpdDate = DateTime.Now;
                        oldUser.Name = user.Name;
                        oldUser.PhoneNumberConfirmed = phone == oldUser.PhoneNumber;
                        oldUser.PhoneNumber = phone;
                        oldUser.FirstName = user.FirstName;
                        oldUser.LastName = user.LastName;
                        oldUser.IsDeleted = user.IsDeleted;
                        //TODO добавить работу с блокировкой юзера
                        oldUser.Lockout = user.Lockout;
                        //oldUser.LockoutEnddate = user.LockoutEnddate;
                        //oldUser.EmailConfirmed = user.EmailConfirmed;

                        fc.SaveChanges();
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
        /// Установка значения для подтверждения email
        /// </summary>
        public bool SetConfirmEmail(long userId, bool emailConfirmed = false)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var user = fc.Users.FirstOrDefault(c => c.Id == userId && c.IsDeleted == false);

                    if (user != null)
                    {
                        user.EmailConfirmed = emailConfirmed;
                        user.LastUpdDate = DateTime.Now;
                        user.LastUpdateByUserId = userId;
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
        /// Начисление/списание баллов пользователю по логину
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="typePoints">Тип баллов: личные или от рефералов</param>
        /// <param name="points">количество баллов: при начисление число > 0, при списании число меньше 0</param>
        /// <returns></returns>
        public bool EditUserPointsByLogin(string login, long typePoints, double points)
        {
            var fc = GetContext();

            User user = fc.Users.FirstOrDefault(u => u.Name.ToLower() == login.ToLower() && u.IsDeleted == false);

            if (user == null) return false;

            //начислениие
            if (points > 0)
            {
                if (typePoints == (long)TypePoints.Personally) user.PersonalPoints += points;
                else user.ReferralPoints += points;
            }
            else//списание
            {
                //если количество списуемых баллов больше, то выходим
                if (user.PersonalPoints + user.ReferralPoints < Math.Abs(points)) return false;
                //иначе списываем
                user.ReferralPoints += points;
                if (user.ReferralPoints < 0)
                {
                    user.PersonalPoints += user.ReferralPoints;
                    user.ReferralPoints = 0;
                }
            }

            fc.SaveChanges();

            return true;
        }

        /// <summary>
        /// Начисление/списание баллов пользователю по логину, получаемых от суммы заказа
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="totalPrice">сумма заказа</param>
        /// <returns></returns>
        public bool EditUserPointsByLoginAndTotalPrice(string login, double totalPrice)
        {
            var fc = GetContext();

            User user = fc.Users.FirstOrDefault(u => u.Name.ToLower() == login.ToLower() && u.IsDeleted == false);

            if (user == null || totalPrice < 0) return false;

            //начислениие %-та с заказа
            user.PersonalPoints += totalPrice * user.PercentOfOrder / 100;

            fc.SaveChanges();

            return true;
        }

        /// <summary>
        /// Получение баллов пользователя через логин
        /// </summary>
        /// <param name="login">логин</param>
        /// <returns></returns>
        public double GetUserPointsByLogin(string login)
        {
            var fc = GetContext();

            User user = fc.Users.AsNoTracking().FirstOrDefault(u => u.Name.ToLower() == login.ToLower() && u.IsDeleted == false);

            return user.PersonalPoints + user.ReferralPoints;
        }

        public User GetUserByReferralLink(string referralLink)
        {
            var fc = GetContext();

            var query = from u in fc.Users.AsNoTracking()
                        where (
                            u.UserReferralLink.ToLower() == referralLink.ToLower()
                            && u.IsDeleted == false
                        )
                        select u;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// DANGEROUS, NOT FOR USE
        /// </summary>
        /// <returns></returns>
        public bool GenerateReferralLinksForUsers()
        {
            using (var fc = new FoodContext())
            {
                using (var foodContextTransaction = fc.Database.BeginTransaction())
                {
                    try
                    {
                        var users = fc.Users.Where(c => string.IsNullOrEmpty(c.UserReferralLink)).ToList();
                        foreach (var user in users)
                        {
                            user.UserReferralLink = Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
                        }
                        fc.SaveChanges();
                        foodContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        foodContextTransaction.Rollback();
                        throw;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Изменить пароль для пользователя
        /// <params>
        /// <paramref name="authorId"/>
        /// <paramref name="code"/>
        /// <paramref name="passwordHash"/>
        /// <paramref name="userId"/>
        /// </params>
        /// </summary>
        public bool ResetUserPassword(long userId, string code, string passwordHash, long authorId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var user = fc.Users.FirstOrDefault(c => c.Id == userId 
                        && c.IsDeleted == false 
                        && c.UserPasswordResetKey.Key == code
                        && c.UserPasswordResetKey.IssuedTo > DateTime.Now);

                    // Изменяем пароль и удаляем токен восстановления пароля

                    if (user != null)
                    {
                        user.Password = passwordHash;
                        user.LastUpdateByUserId = authorId;
                        user.LastUpdDate = DateTime.Now;
                        fc.UserPasswordResetKeys.Remove(user.UserPasswordResetKey);

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
        /// Установка флага СМС-оповещений для пользователя о заказах
        /// </summary>
        /// <param name="UserLogin">Логин пользователя, для которого надо включить-отключить оповещения</param>
        /// <param name="EnableSms">true - включает СМС-оповещения, false - отключает</param>
        /// <returns>Возвращает true, если удалось успешно сохранить изменения в БД, false - если пользователь не найден</returns>
        public async Task<bool> UserSetSmsNotifications(string UserLogin, bool EnableSms)
        {
            if (string.IsNullOrWhiteSpace(UserLogin))
            {
                return false;
            }
            using (var fc = GetContext())
            {
                var user = await fc.Users.FirstOrDefaultAsync(u => u.Name.ToLower() == UserLogin.ToLower() && u.IsDeleted == false);
                if (user != null)
                {
                    user.SmsNotify = EnableSms;
                    fc.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Установка кода подтверждения email
        /// </summary>
        public bool SetConfirmationEmailCode(long userId, string code)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var user = fc.Users.FirstOrDefault(c => c.Id == userId && c.IsDeleted == false);

                    if (user != null)
                    {
                        user.EmailConfirmationCode = code;
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

        public bool DeleteConfirmationEmailCodeByUserId(long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var user = fc.Users.FirstOrDefault(c => c.Id == userId && c.IsDeleted == false);

                    if (user != null)
                    {
                        user.EmailConfirmationCode = null;
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

        #endregion
    }
}
