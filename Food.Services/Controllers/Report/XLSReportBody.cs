using ITWebNet.FoodService.Food.DbAccessor;
using System;

namespace ITWebNet.Food.Controllers
{
    public class XlsReportBody : ReportBase
    {
        private readonly ReportData _data;

        private Exception _formingException;

        public XlsReportBody(ReportData data)
        {
            _data = data;
            ReportInputData = new ReportInputData
            {
                NameTemplate = "Отчет" + $" {data?.Cafe?.Name ?? string.Empty}"
            };
        }

        public override string ReportExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(reportExtension))
                    reportExtension = "xlsx";
                return reportExtension;
            }
        }

        public override void FormReportBody(ReportInputData reportInputData)
        {
            throw new NotImplementedException();
        }

        public override byte[] GetReportBody()
        {
            byte[] bytes = null;

            try
            {
                bytes = new CompanyOrderXls().GetXlsBytes(_data);
            }
            catch (Exception exc)
            {
                _formingException = exc;
            }

            return bytes;
        }

        public override Exception GetFormingException()
        {
            return _formingException;
        }
    }
}