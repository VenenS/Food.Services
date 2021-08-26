using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        public bool AddUserReferralLink(long parentId, long referralId)
        {
            using (var fc = GetContext())
            {
                try
                {
                    var link =
                        fc.UserReferral.FirstOrDefault(u => u.ParentId == parentId && u.RefId == referralId && u.IsActive && u.IsDeleted == false);
                    if (link != null)
                        return true;

                    var parent = fc.UserReferral.FirstOrDefault(p => p.RefId == parentId && p.IsActive && p.IsDeleted == false);
                    if (parent == null)
                    {
                        parent = new UserReferral
                        {
                            NumMapping = "0.0",
                            PathIndex = 0,
                            Level = 0,
                            ParentId = null,
                            RefId = parentId,
                            IsDeleted = false,
                            IsActive = true,
                            CreateDate = DateTime.Now,
                            RootId = parentId
                        };
                        fc.UserReferral.Add(parent);
                    }

                    var root = parent;
                    while (root?.ParentId != null)
                    {
                        root = fc.UserReferral.FirstOrDefault(p => p.RefId == root.ParentId && p.IsActive && p.IsDeleted == false);
                    }

                    var referral = new UserReferral
                    {
                        IsActive = true,
                        IsDeleted = false,
                        NumMapping = null,
                        ParentId = parentId,
                        RefId = referralId,
                        PathIndex = 0,
                        Level = 0,
                        CreateDate = DateTime.Now,
                        RootId = root.RefId
                    };

                    fc.UserReferral.Add(referral);

                    fc.SaveChanges();

                    var refPathIndex = (from u in fc.UserReferral
                        where u.ParentId.HasValue
                        orderby u.ParentId ascending
                        select new
                        {
                            u.ParentId,
                            PathIndex =
                                (from o in fc.UserReferral where o.ParentId == parentId select o)
                                    .Count()
                        }).FirstOrDefault();

                    referral.PathIndex = refPathIndex?.PathIndex ?? 0;

                    var depth = 1;
                    var numMap = parent.NumMapping != "0.0"
                        ? parent.NumMapping + "." + referral.PathIndex
                        : referral.PathIndex.ToString();
                    while (parent?.ParentId != null)
                    {
                        parent =
                            fc.UserReferral.FirstOrDefault(
                                p => p.RefId == parent.ParentId && p.IsActive && p.IsDeleted == false);
                        depth++;
                    }

                    referral.Level = depth;
                    referral.NumMapping = numMap;
                    fc.SaveChanges();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }


        public List<UserReferral> GetUserReferrals(long userId, int[] level)
        {
            var fc = GetContext();
            var referral =
                fc.UserReferral.AsNoTracking().FirstOrDefault(u => u.RefId == userId && u.IsDeleted == false && u.IsActive);
            if (referral != null)
            {
                var tree = fc.UserReferral.AsNoTracking().Where(u => u.RootId == referral.RootId && u.Id != referral.Id).ToList();

                if (referral.NumMapping == "0.0")
                {
                    if (level.Length == 0)
                        return tree;
                    else
                        return tree.Where(u => level.Contains(u.Level)).ToList();
                }
                else
                {
                    tree = tree.Where(u => u.NumMapping.StartsWith(referral.NumMapping)).ToList();
                    if (level.Length == 0)
                        return tree;
                    else
                    {
                        tree = tree.Select(u =>
                        {
                            u.Level = u.Level - referral.Level;
                            return u;
                        }).ToList();
                        return tree.Where(u => level.Contains((u.Level))).ToList();
                    }
                }
            }

            return new List<UserReferral>();
        }


        public void AddPointsToReferrals(long userId, int depth, double sum)
        {
            try
            {
                var fc = GetContext();

                var referral =
                    fc.UserReferral.FirstOrDefault(u => u.RefId == userId && u.IsDeleted == false && u.IsActive);

                if (referral != null)
                {
                    if(referral.Parent == null)
                        return;

                    //ищем узлы в дереве, которые выше юзера
                    var tree =
                        fc.UserReferral.Where(
                            u =>
                                u.RootId == referral.RootId && u.RefId != referral.RefId &&
                                u.Level < referral.Level).ToList();

                    tree = tree.Where(u => u.NumMapping.StartsWith(referral.NumMapping.Split('.')[0]) || u.RefId == referral.RootId).ToList();

                    //таблица с коэф
                    var coefficients = fc.ReferralCoef.OrderBy(k => k.Level).ToDictionary(k => k.Level, v => v.Coefficient);
                    var parents = new List<UserReferral>();

                    while (depth > 0 && referral != null)
                    {
                        if (referral.Parent != null)
                        {
                            parents.Add(referral);
                            referral = tree.FirstOrDefault(r => r.RefId == referral.Parent.Id);
                        }
                        else
                        {
                            referral = null;
                        }
                        depth--;
                    }

                    for (int i = 0; i < parents.Count; i++)
                    {
                        var points = (coefficients.ContainsKey(parents.Count - i)
                            ? coefficients[parents.Count - i]
                            : 0)*sum;
                        parents[i].Parent.ReferralPoints += points;
                        parents[i].EarnedPoints += points;
                    }

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
