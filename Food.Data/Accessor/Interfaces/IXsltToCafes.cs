using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IXsltToCafes
    {
        List<XsltToCafe> GetCafesToXslt(long id);

        bool IsUniqueNameXslt(string name);

        long AddCafeToXslt(XsltToCafe model);

        XsltToCafe GetXsltToCafeById(long id);

        bool RemoveXsltToCafe(long layoutId, long userId);
    }
}
