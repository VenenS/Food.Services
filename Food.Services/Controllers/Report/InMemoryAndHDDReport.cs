using ITWebNet.Food.Core.DataContracts.Common;
using System;

namespace ITWebNet.Food.Controllers
{
    public class InMemoryAndHddReport : ReportCreatorBase
    {
        public override void FormFile(ReportBase report)
        {
            throw new NotImplementedException();
        }

        public override ReportModel GetFileBody()
        {
            throw new NotImplementedException();
        }
    }
}