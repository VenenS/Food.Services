using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;

namespace ITWebNet.Food.Controllers
{
    public class InMemoryReport : ReportCreatorBase
    {
        private MemoryMappedFile _file;

        public override void FormFile(ReportBase report)
        {
            Report = report;
        }

        public override ReportModel GetFileBody()
        {
            var report = new ReportModel();

            var fileBody = Report.GetReportBody();

            var fileName = Report.ReportFileName;

            if (Report.GetFormingException() == null)
                try
                {
                    using (_file = MemoryMappedFile.CreateNew(fileName, fileBody.Length))
                    {
                        using (var memoryMappedViewAccessor =
                            _file.CreateViewAccessor()
                        )
                        {
                            memoryMappedViewAccessor.WriteArray(
                                0,
                                fileBody,
                                0,
                                fileBody.Length
                            );
                        }

                        using (var stream = _file.CreateViewStream())
                        {
                            using (var binReader = new BinaryReader(stream))
                            {
                                report.FileBody =
                                    binReader.ReadBytes(fileBody.Length);

                                using (var md5Hash =
                                    MD5.Create()
                                )
                                {
                                    report.Hash =
                                        md5Hash.ComputeHash(report.FileBody);
                                }

                                report.FileName = fileName;

                                return report;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            
            return null;
        }
    }
}