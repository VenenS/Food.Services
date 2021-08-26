using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Food.Data.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region SmsCode

        /// <summary>
        /// Получение кода по Id
        /// </summary>
        /// <param name="Id">идентификатор смс-кода</param>
        /// <returns></returns>
        public virtual async Task<SmsCode> GetSmsCodeById(long Id)
        {
            return await GetContext().SmsCodes.FirstOrDefaultAsync(sc => sc.Id == Id);
        }

        /// <summary>
        /// Получение сущности Смс кода по телефону и коду
        /// </summary>
        /// <param name="phone">Телефон</param>
        /// <param name="code">Смс код</param>
        /// <returns></returns>
        public virtual async Task<SmsCode> GetSmsCode(string phone, string code)
        {
            phone = new string(phone.Where(char.IsDigit).ToArray());

            return await GetContext().SmsCodes.Include(e => e.User).FirstOrDefaultAsync(
                sc => sc.IsActive
                && !sc.IsDeleted
                && sc.ValidTime >= DateTime.Now
                && sc.Code == code
                && sc.Phone == phone);
        }

        /// <summary>
        /// Добавление смс-кода в БД
        /// </summary>
        /// <param name="code">смс-код (сущность)</param>
        /// <returns>Возвращает идентификатор кода, или -1, если код добавить не удалось</returns>
        public virtual async Task<long> AddSmsCode(SmsCode code)
        {
            try
            {
                using (var fc = GetContext())
                {
                    SmsCode oldCode = await fc.SmsCodes.FirstOrDefaultAsync(sc => sc.UserId == code.UserId);
                    long ResultId;
                    if (oldCode != null)
                    {
                        oldCode.Phone = new string(code.Phone.Where(u => char.IsDigit(u)).ToArray());
                        oldCode.Code = code.Code;
                        oldCode.CreationTime = code.CreationTime;
                        oldCode.ValidTime = code.ValidTime;
                        oldCode.IsActive = code.IsActive;
                        oldCode.IsDeleted = false;
                        fc.SaveChanges();
                        ResultId = oldCode.Id;
                    }
                    else
                    {
                        fc.SmsCodes.Add(code);
                        fc.SaveChanges();
                        ResultId = code.Id;
                    }
                    return ResultId;
                }
            }
            catch //(Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// Изменение кода
        /// </summary>
        /// <param name="cafe">смс-код (сущность))</param>
        /// <returns></returns>
        public virtual async Task<long> EditSmsCode(SmsCode code)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldCode = await fc.SmsCodes.FirstOrDefaultAsync(sc => sc.Id == code.Id);

                    if (oldCode != null)
                    {
                        oldCode.UserId = code.UserId;
                        oldCode.Phone = new string(code.Phone.Where(u => char.IsDigit(u)).ToArray());
                        oldCode.Code = code.Code;
                        oldCode.CreationTime = code.CreationTime;
                        oldCode.ValidTime = code.ValidTime;
                        oldCode.IsActive = code.IsActive;

                        fc.SaveChanges();
                        return oldCode.Id;
                    }
                    else
                    {
                        return -1;
                    }

                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Удаление кода
        /// </summary>
        /// <param name="cafeId">Идентификатор кода</param>
        /// <returns></returns>
        public virtual async Task<bool> RemoveSmsCode(long Id)
        {
            try
            {
                using (var fc = GetContext())
                {
                    SmsCode сode = await fc.SmsCodes.FirstOrDefaultAsync(sc => sc.Id == Id);

                    if (сode != null)
                    {
                        fc.SmsCodes.Remove(сode);
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
        /// Блокировка СМС-кода (прекращение действия)
        /// </summary>
        /// <param name="cafeId">Идентификатор кода</param>
        /// <returns></returns>
        public virtual async Task<bool> BlockSmsCode(long Id)
        {
            try
            {
                using (var fc = GetContext())
                {
                    SmsCode oldCode = await fc.SmsCodes.FirstOrDefaultAsync(sc => sc.Id == Id);

                    if (oldCode != null)
                    {
                        oldCode.IsActive = false;
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
        /// Получение самого последнего сгенерированного кода для указанного пользователя
        /// </summary>
        /// <param name="UserId">Идентификатор ползователя, для которого надо получить код</param>
        /// <returns></returns>
        public virtual async Task<SmsCode> GetLastUserCode(long UserId)
        {
            SmsCode code = null;
            try
            {
                using (var fc = GetContext())
                {
                    code = await fc.SmsCodes.Where(sc => sc.UserId == UserId)
                        .OrderByDescending(sc => sc.CreationTime)
                        .FirstOrDefaultAsync();
                    return code;
                }
            }
            catch (Exception)
            {
                return code;
            }
        }

        #endregion
    }
}
