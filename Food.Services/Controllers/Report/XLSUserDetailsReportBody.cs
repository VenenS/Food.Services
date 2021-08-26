using Food.Services.GenerateXLSX.Model;
using System;

namespace ITWebNet.Food.Controllers
{
    public class XLSUserDetailsReportBody : ReportBase
    {
        private readonly ReportUserOrders _data;
        private Exception _formingException;

        public XLSUserDetailsReportBody(ReportUserOrders data)
        {
            _data = data;
            ReportInputData = new ReportInputData
            {
                NameTemplate = "Отчет" + $" {data?.Orders[0]?.Id.ToString() ?? string.Empty}"
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
                bytes = new UserOrderDetailsXLS().GetXlsBytes(_data);
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