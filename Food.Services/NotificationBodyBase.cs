using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services
{
    /// <summary>
    ///     Базовый класс сообщения уведомления
    /// </summary>
    public abstract class NotificationBodyBase
    {
        /// <summary>
        ///     Получение тела сообщения уведомления
        /// </summary>
        /// <returns></returns>
        public abstract string GetMessageBody();

        /// <summary>
        ///     Получение списка адресов для отправки уведомления
        /// </summary>
        /// <returns></returns>
        public abstract List<string> GetReceiverAddress();

        /// <summary>
        ///     Формирование записи об отправке уведомления
        /// </summary>
        public abstract void FormLogInfo();

        /// <summary>
        ///     Выставление сообщения об ошибке при отправке уведомления
        /// </summary>
        /// <param name="exc"></param>
        public abstract void SetSendError(Exception exc);

        /// <summary>
        ///     Получение темы уведомления
        /// </summary>
        /// <returns></returns>
        public abstract string GetSubject();
    }
}
