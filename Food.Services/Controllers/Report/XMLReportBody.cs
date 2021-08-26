using System;

namespace ITWebNet.Food.Controllers
{
    public class XmlReportBody : ReportBase
    {
        private string _finalReportData;

        private Exception _formingException;

        public override string ReportExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(reportExtension))
                    reportExtension = "xml";
                return reportExtension;
            }
        }

        public override void FormReportBody(ReportInputData reportInputData)
        {
            if (reportInputData != null
                && !string.IsNullOrWhiteSpace(reportInputData.InitialInfo)
            )
                if (!string.IsNullOrWhiteSpace(reportInputData.XsltTransform))
                    try
                    {
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
                    _finalReportData = reportInputData.InitialInfo;
            else
                _formingException = new Exception("Lacks the necessary input data");
        }

        public override byte[] GetReportBody()
        {
            byte[] bytes = null;

            try
            {
                bytes = new byte[_finalReportData.Length * sizeof(char)];
                Buffer.BlockCopy(
                    _finalReportData.ToCharArray(),
                    0,
                    bytes,
                    0,
                    bytes.Length
                );
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