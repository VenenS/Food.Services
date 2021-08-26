﻿namespace Food.Data.Entities
{
    public enum EnumTaskRepeatable
    {
        /// <summary>
        ///     Выполняется один раз
        /// </summary>
        Once = 1,

        /// <summary>
        ///     Выполняется ежедневно
        /// </summary>
        Dayly = 2,

        /// <summary>
        ///     Выполняется еженедельно
        /// </summary>
        Weekly = 3,

        /// <summary>
        ///     Выполняется ежемесячно
        /// </summary>
        Monthly = 4,

        /// <summary>
        ///     Выполнять через заданный промежуток времени
        /// </summary>
        Custom = 0
    }
}