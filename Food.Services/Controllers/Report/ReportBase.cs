using System;
using System.Globalization;

namespace ITWebNet.Food.Controllers
{
    public abstract class ReportBase
    {
        /// <summary>
        ///     Расширение итогового файла
        /// </summary>
        protected string reportExtension;

        /// <summary>
        ///     Имя итогового файла
        /// </summary>
        private string _reportFileName;

        /// <summary>
        ///     Имя итогового файла без расширения
        /// </summary>
        private string _reportFileNameWithoutExtension;

        /// <summary>
        ///     Входные данные для формирования отчёта
        /// </summary>
        protected ReportInputData ReportInputData;

        /// <summary>
        ///     Имя итогового файла без расширения
        /// </summary>
        protected string ReportFileNameWithoutExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_reportFileNameWithoutExtension))
                    if (ReportInputData != null
                        && !string.IsNullOrWhiteSpace(ReportInputData.NameTemplate))
                        _reportFileNameWithoutExtension =
                            GetNameByTemplate(ReportInputData.NameTemplate);
                    else
                        _reportFileNameWithoutExtension =
                            Guid.NewGuid().ToString();

                return _reportFileNameWithoutExtension;
            }
        }

        /// <summary>
        ///     Получение расширения итогового файла
        /// </summary>
        public abstract string ReportExtension { get; }

        /// <summary>
        ///     Полное имя итогового файла
        /// </summary>
        public string ReportFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_reportFileName))
                    _reportFileName = string.Format(
                        "{0}.{1}",
                        ReportFileNameWithoutExtension,
                        ReportExtension
                    );

                return _reportFileName;
            }
        }

        /// <summary>
        ///     Функция получения итогового имени без расширения из шаблона.
        ///     Заменяем типовые значения необходимыми значениями.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string GetNameByTemplate(string name)
        {
            var finalName = ReportInputData.NameTemplate;

            finalName =
                finalName.Replace(
                    "{Date}",
                    DateTime.Now.ToShortDateString()
                );

            finalName =
                finalName.Replace(
                    "{DateTime}",
                    DateTime.Now.ToString(CultureInfo.InvariantCulture)
                );

            finalName =
                finalName.Replace(
                    "{Time}",
                    DateTime.Now.ToShortTimeString()
                );

            return finalName;
        }

        /// <summary>
        ///     Формирование тела отчёта
        /// </summary>
        /// <param name="reportInputData"></param>
        public abstract void FormReportBody(ReportInputData reportInputData);

        /// <summary>
        ///     Получение тела отчёта
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetReportBody();

        /// <summary>
        ///     Получение ошибки при формировании отчёта
        /// </summary>
        /// <returns></returns>
        public abstract Exception GetFormingException();
    }
}