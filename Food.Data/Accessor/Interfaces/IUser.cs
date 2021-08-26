using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IUser
    {
        #region User

        /// <summary>
        /// Добавляем пользователя
        /// </summary>
        long AddUser(User user);

        //TODO: Авторизация через соцсети. Удалить
        //bool AddExternalLogin(UserExternalLogin login);

        /// <summary>
        /// Проверяет уникальное ли email у пользователя
        /// </summary>
        bool CheckUniqueEmail(string email, long userId = -1);

        /// <summary>
        /// Вернуть юзера по логину
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        User GetUserByLogin(string login);

        /// <summary>
        /// Get user by e-mail
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        User GetUserByEmail(string email);

        /// <summary>
        /// Получение пользователя по номеру телефона
        /// (не удален и не заблокирован);
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        User GetUserByPhone(string phone);

        //TODO: Авторизация через соцсети. Удалить
        /// <summary>
        /// Получение пользователя по externalLoginInfo
        /// </summary>
        //User GetUserByExternalLoginInfo(string loginProvider, string providerKey);

        /// <summary>
        /// Вурнуть юзера по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        User GetUserById(long id);

        /// <summary>
        /// Получает список пользователей, которые имеют определенную роль.
        /// </summary>
        /// <param name="roleId">Идентификатор роли.</param>
        /// <returns>Список пользователей.</returns>
        List<User> GetUserByRoleId(Int64 roleId);

        /// <summary>
        /// Получает список пользователей, привязанных к определенному кафе/компании.
        /// </summary>
        /// <param name="id">Идентификатор организации</param>
        /// <returns>Список пользователей.</returns>
        List<User> GetUsersByCafe(long id);

        /// <summary>
        /// Получает пользователя по device_uuid
        /// </summary>
        User GetUserByDeviceUuid(string deviceUuid);

        /// <summary>
        /// Получает список пользователей без кураторов, которым назначено кафе
        /// </summary>
        /// <returns></returns>
        List<User> GetUsersWithoutCurators();

        /// <summary>
        /// Получение полного списка пользователей.
        /// </summary>
        /// <returns>Список пользователей.</returns>
        List<User> GetFullListOfUsers();
        
        /// <summary>
        /// Изменение данных пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <returns>true - успешно, false - ошибка изменения.</returns>
        bool EditUser(User user);

        /// <summary>
        /// Установка значения для подтверждения email
        /// </summary>
        bool SetConfirmEmail(long userId, bool emailConfirmed = false);

        /// <summary>
        /// Начисление/списание баллов пользователю по логину
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="typePoints">Тип баллов: личные или от рефералов</param>
        /// <param name="points">количество баллов: при начисление число > 0, при списании число меньше 0</param>
        /// <returns></returns>
        bool EditUserPointsByLogin(string login, long typePoints, double points);

        /// <summary>
        /// Начисление/списание баллов пользователю по логину, получаемых от суммы заказа
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="totalPrice">сумма заказа</param>
        /// <returns></returns>
        bool EditUserPointsByLoginAndTotalPrice(string login, double totalPrice);

        /// <summary>
        /// Получение баллов пользователя через логин
        /// </summary>
        /// <param name="login">логин</param>
        /// <returns></returns>
        double GetUserPointsByLogin(string login);

        User GetUserByReferralLink(string referralLink);

        /// <summary>
        /// DANGEROUS, NOT FOR USE
        /// </summary>
        /// <returns></returns>
        bool GenerateReferralLinksForUsers();

        /// <summary>
        /// Изменить пароль для пользователя
        /// </summary>
        bool ResetUserPassword(long userId, string code, string passwordHash, long authorId);

        /// <summary>
        /// Установка флага СМС-оповещений для пользователя о заказах
        /// </summary>
        /// <param name="UserLogin">Логин пользователя, для которого надо включить-отключить оповещения</param>
        /// <param name="EnableSms">true - включает СМС-оповещения, false - отключает</param>
        /// <returns>Возвращает true, если удалось успешно сохранить изменения в БД, false - если пользователь не найден</returns>
        Task<bool> UserSetSmsNotifications(string UserLogin, bool EnableSms);

        #endregion
    }
}
