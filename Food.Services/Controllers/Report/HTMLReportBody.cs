using System;
using System.Text;

namespace ITWebNet.Food.Controllers
{
    public class HtmlReportBody : ReportBase
    {
        private string _finalReportData;

        private Exception _formingException;

        public override string ReportExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(reportExtension))
                    reportExtension = "html";
                return reportExtension;
            }
        }

        public override void FormReportBody(ReportInputData reportInputData)
        {
            if (reportInputData != null
                && !string.IsNullOrWhiteSpace(reportInputData.InitialInfo)
                && !string.IsNullOrWhiteSpace(reportInputData.XsltTransform)
            )
                try
                {
                    ReportInputData = reportInputData;

                    var xsltTransform = new XslttRansform();

                    _finalReportData = xsltTransform.XsltTransformation(
                        reportInputData.InitialInfo,
                        reportInputData.XsltTransform
                    );
                }
                catch (Exception exc)
                {
                    _formingException = exc;
                }
            else
                _formingException = new Exception("Lacks the necessary input data");
        }

        public override byte[] GetReportBody()
        {
            byte[] bytes = null;

            try
            {
                var encoding = new UTF8Encoding();

                bytes = new byte[_finalReportData.Length * sizeof(char)];

                bytes = encoding.GetBytes(_finalReportData);

                //System.Buffer.BlockCopy(
                //    finalReportData.ToCharArray(),
                //    0,
                //    bytes,
                //    0,
                //    bytes.Length
                //);
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