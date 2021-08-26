using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class UserReferralExtensions
    {
        public static UserReferralModel GetContract(this UserReferral user)
        {
            return user == null
                ? null
                : new UserReferralModel
                {
                    CreateDate = user.CreateDate,
                    Id = user.Id,
                    EarnedPoints = user.EarnedPoints,
                    Parent = user.Parent.GetContract(),
                    Referral = user.Referral.GetContract(),
                    Level = user.Level
                };
        }

        public static UserReferral GetEntity(this UserReferralModel user)
        {
            return user == null
                ? new UserReferral()
                : new UserReferral
                {
                    CreateDate = user.CreateDate,
                    Id = user.Id,
                    EarnedPoints = user.EarnedPoints,
                    RefId = user.Referral.Id,
                    ParentId = user.Parent.Id,
                    Level = user.Level
                };
        }
    }
}
