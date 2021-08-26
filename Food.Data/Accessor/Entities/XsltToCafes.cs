using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        public List<XsltToCafe> GetCafesToXslt(long id)
        {
            var cafes = new List<XsltToCafe>();
            using (var fc = GetContext())
            {
                var query =
                    from xslt in fc.XsltToCafe.AsNoTracking()
                    where (
                        xslt.IsDeleted == false && xslt.XsltId == id
                    )
                    select xslt;
                cafes = query.ToList();
                return cafes;
            }
        }

        public bool IsUniqueNameXslt(string name)
        {
            using (var fc = GetContext())
            {
                var query =
                    from xslt in fc.ReportStylesheets.AsNoTracking()
                    where (
                        xslt.IsDeleted == false && xslt.Name == name
                    )
                    select xslt;
                if (query.ToList().Count == 0)
                {
                    return true;
                }
                else return false;

            }
        }

        public long AddCafeToXslt(XsltToCafe model)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.XsltToCafe.Add(model);
                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return model.Id;
        }

        public XsltToCafe GetXsltToCafeById(long id)
        {
            using (var fc = GetContext())
            {
                return fc.XsltToCafe.AsNoTracking().FirstOrDefault(xslt => xslt.Id == id);
            }
        }

        public bool RemoveXsltToCafe(long layoutId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldLayout = fc.XsltToCafe.FirstOrDefault(e => e.Id == layoutId && e.IsDeleted == false);
                    if (oldLayout != null)
                    {
                        oldLayout.IsDeleted = true;
                        oldLayout.LastUpdateByUserId = userId;
                        oldLayout.LastUpdDate = DateTime.Now;
                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
