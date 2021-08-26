namespace ITWebNet.Food.Controllers
{
    public class ReportInputData
    {
        /// <summary>
        ///     Исходные данные отчёта
        /// </summary>
        public string InitialInfo { get; set; }

        /// <summary>
        ///     XSLT преобразование, если оно необходимо
        /// </summary>
        public string XsltTransform { get; set; }

        /// <summary>
        ///     Шаблон имени файла
        /// </summary>
        public string NameTemplate { get; set; }
    }
}