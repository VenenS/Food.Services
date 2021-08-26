using ITWebNet.Food.Core.DataContracts.Common;

namespace ITWebNet.Food.Controllers
{
    public abstract class ReportCreatorBase
    {
        /// <summary>
        ///     Поле тела отчёт
        /// </summary>
        protected ReportBase Report;

        /// <summary>
        ///     Формирование тела отчёта
        /// </summary>
        /// <param name="reportBody"></param>
        public abstract void FormFile(ReportBase reportBody);

        /// <summary>
        ///     Формирование отчёта
        /// </summary>
        /// <returns></returns>
        public abstract ReportModel GetFileBody();

    }
}