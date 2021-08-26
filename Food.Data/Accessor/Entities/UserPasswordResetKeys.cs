using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Account;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Добавление нового ключа восстановления пароля
        /// <param name="newKey"></param>
        /// </summary>
        public bool AddUserResetPasswordKey(UserPasswordResetKey newKey)
        {
            try
            {
                using (var fc = GetContext())
                {
                    // Если для пользователя установлен код, изменяем его
                    if (fc.UserPasswordResetKeys.FirstOrDefault(k=>k.UserId==newKey.UserId) is UserPasswordResetKey old)
                    {
                        old.Key = newKey.Key;
                        old.IssuedTo = newKey.IssuedTo;
                    }
                    // Если кода нет добавляем новый
                    else
                    {
                        fc.UserPasswordResetKeys.Add(newKey);
                    }

                    fc.SaveChanges();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Валидация ключа восстановления пароля
        /// <param name="keyId"></param>
        /// <returns>
        /// Возвращает модель восстановления пароля если ключ валидный
        /// </returns>
        /// </summary>
        public ResetPasswordModel ValidatePasswordResetKey(long userId, string key)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var userPasswordResetKeys = fc.UserPasswordResetKeys
                        .FirstOrDefault(r => r.UserId == userId && r.Key == key && r.IssuedTo > DateTime.Now);

                    if (userPasswordResetKeys != null)
                    {
                        var resetPasswordModel = new ResetPasswordModel
                        {
                            Email = userPasswordResetKeys.User.Email,
                            Code = key
                        };

                        return resetPasswordModel;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
