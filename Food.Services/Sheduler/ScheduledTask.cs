using System;

namespace Food.Services
{
    /// <summary>
    /// </summary>
    public abstract class ScheduledTask
    {
        /// <summary>
        ///     Запланированная задача.
        /// </summary>
        /// <param name="executeTime">Назначеное время выполнения задачи.</param>
        /// <param name="isRepeatable">Тип повторяемости задачи</param>
        /// <param name="isDeleted">Статус активности задачи.</param>
        public ScheduledTask(DateTime? executeTime, long isRepeatable, bool isDeleted)
        {
            _executeTime = executeTime;
            _isDeleted = isDeleted;
        }

        /// <summary>
        ///     Запуск задачи - должно быть реализованно с помощью конкретного класса.
        /// </summary>
        public abstract void Run();

        /// <summary>
        ///     Запись в лог - должно быть реализованно с помощью конкретного класса.
        /// </summary>
        protected abstract void WriteLog();

        /// <summary>
        ///     Отмена задачи - должно быть реализованно с помощью конкретного класса.
        /// </summary>
        protected abstract void CancelTask(long taskId);

        #region Fields

        /// <summary>
        ///     Дата выполнения задачи.
        /// </summary>
        private DateTime? _executeTime;

        /// <summary>
        ///     Тип повторяемости задачи
        /// </summary>
        private long _isRepeatable;

        /// <summary>
        ///     Статус задачи.
        /// </summary>
        private bool _isDeleted;

        #endregion
    }
}
