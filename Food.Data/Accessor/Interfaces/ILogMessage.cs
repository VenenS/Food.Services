using Food.Data;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    interface ILogMessage
    {
        /// <summary>
        /// Логирование сообщения в БД.
        /// </summary>
        /// <param name="severity">Уровень критичности сообщения (см. <see cref="EnumSeverity"/>)</param>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        void LogMessage(string severity, string source, string message);

        /// <summary>
        /// Логирование информационного сообщения в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        void LogInfo(string source, string message);

        /// <summary>
        /// Логирование предупреждения в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        void LogWarning(string source, string message);

        /// <summary>
        /// Логирование ошибки в БД.
        /// </summary>
        /// <param name="source">Источник сообщения (например, название модуля/класса)</param>
        /// <param name="message">Текст сообщения</param>
        void LogError(string source, string message);

        /// <summary>
        /// Записать ошибку в log_message
        /// </summary>
        /// <param name="message">Текс ошибки</param>
        void WriteShedulerError(string message);
    }
}
