using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class XsltExtensions
    {
        public static XSLTModel GetContract(this ReportStylesheet xslt)
        {
            return xslt == null
                ? null
                : new XSLTModel
                {
                    CafeId = xslt.CafeId,
                    Description = xslt.Description,
                    ExtId = xslt.ExtId,
                    Id = xslt.Id,
                    Name = xslt.Name,
                    Transformation = xslt.Transformation,
                    IsCommon = xslt.IsCommon
                };
        }

        public static ReportStylesheet GetEntity(this XSLTModel xslt)
        {
            return xslt == null
                ? new ReportStylesheet()
                : new ReportStylesheet
                {
                    CafeId = xslt.CafeId,
                    Description = xslt.Description,
                    ExtId = xslt.ExtId,
                    Id = xslt.Id,
                    Name = xslt.Name,
                    Transformation = xslt.Transformation
                };
        }
    }
}
