using System;
using System.IO;
using System.Text;
using System.Web.Http;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace ITWebNet.Food.Controllers
{
    public class ReportBodyPdf : ReportBase
    {
        private IConverter _converter;
        private Exception _formingException;
        private string _finalReportData;

        //Конструктор исключительно для PDF отчетов.
        public ReportBodyPdf(IConverter converter)
        {
            _converter = converter;
        }

        public override string ReportExtension
        {
            get
            {
                if (string.IsNullOrWhiteSpace(reportExtension))
                    reportExtension = "pdf";
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
            byte[] res = null;
            using (MemoryStream ms = new MemoryStream())
            {
                //Старый сервис pdf IronPdf
                //var htmlToPdf = new IronPdf.HtmlToPdf();
                //res = htmlToPdf.RenderHtmlAsPdf(_finalReportData).BinaryData;
                
                //Новый pdf сервис.
                //Тут задаются глобальные настройки файла
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings {Top = 10},
                    DocumentTitle = "Отчет PDF"
                };

                //Настройка самого файла + добавляем контент в файл
                var objectSetting = new ObjectSettings
                {

                    PagesCount = true,
                    HtmlContent = _finalReportData,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    HeaderSettings =
                    {
                        FontName = "Arial",
                        FontSize = 9,
                        Right = "Страница [page] из [toPage]",
                        Line = true
                    },
                    FooterSettings =
                    {
                        FontName = "Arial",
                        FontSize = 9,
                        Line = true,
                    }
                };

                
                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = {objectSetting}
                };

                res = _converter.Convert(pdf);

                //По всей видимости это еще более древний Сервис для pdf файлов чем IronPDF
                //res = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(_finalReportData);
                //удалить pdfSharp из nuget
                //var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(_finalReportData.ToString(), PdfSharp.PageSize.A4);
                //pdf.Save(ms);
                //res = ms.ToArray();
            }
            return res;
            
        }

        public override Exception GetFormingException()
        {
            return _formingException;
        }
    }
}