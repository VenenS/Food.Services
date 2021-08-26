using Food.Data;
using Food.Data.Entities;
using System;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Логирование сообщения в БД.
        /// </summary>
        /// <param name="severity">Уровень критичности сообщения (см. <see cref="EnumSeverity"/>)</param>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        public void LogMessage(string severity, string source, string message)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.LogMessages.Add(new LogMessage()
                    {
                        Date = DateTime.Now,
                        Severity = severity,

                        // FIXME: (временно) сохранять название машины-источника сообщения
                        SourcePackage = $"[{Environment.MachineName}] {source}",
                        Text = message
                    });

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
        }

        /// <summary>
        /// Логирование информационного сообщения в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        public void LogInfo(string source, string message)
        {
            LogMessage(EnumSeverity.Info, source, message);
        }

        /// <summary>
        /// Логирование предупреждения в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        public void LogWarning(string source, string message)
        {
            LogMessage(EnumSeverity.Warning, source, message);
        }

        /// <summary>
        /// Логирование ошибки в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        public void LogError(string source, string message)
        {
            LogMessage(EnumSeverity.Error, source, message);
        }

        /// <summary>
        /// Записать ошибку в log_message
        /// </summary>
        /// <param name="message">Текс ошибки</param>
        public void WriteShedulerError(string message)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.LogMessages.Add(new LogMessage()
                    {
                        Date = DateTime.Now,
                        Code = "INFO",
                        Severity = "MAJOR",
                        SourcePackage = "Планировщик",
                        Text = message
                    });

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
        }
    }
}
