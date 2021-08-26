using Food.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services
{
    public class NotificationHelper
    {
        private static Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //для создания кода из цифр и букв
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // для создания кода из букв
        const string numeric = "0123456789"; //только цифры

        public static string GenerateCode(int length, SMSCodeTypeEnum type)
        {
            var code = "";
            switch (type)
            {
                case SMSCodeTypeEnum.Alphabet:
                    code = new string(Enumerable.Repeat(alphabet, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    break;
                case SMSCodeTypeEnum.Mixed:
                    code = new string(Enumerable.Repeat(chars, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    break;
                case SMSCodeTypeEnum.Numeric:
                default:
                    code = new string(Enumerable.Repeat(numeric, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    break;
            }
            return code;
        }

        /// <summary>
        /// Функция для получения кода с заданными параметрами, сохранение его в базу с привязкой к пользователю
        /// </summary>
        /// <param name="length"> длина кода </param>
        /// <param name="type"> тип кода </param>
        /// <returns></returns>
        public string GetRandomCode(int length, SMSCodeTypeEnum type)
        {
            var code = GenerateCode(length, type);
            return code;
        }
    }
}
