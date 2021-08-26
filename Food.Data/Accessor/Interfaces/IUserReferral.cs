using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IUserReferral
    {
        bool AddUserReferralLink(long parentId, long referralId);

        List<UserReferral> GetUserReferrals(long userId, int[] level);

        void AddPointsToReferrals(long userId, int depth, double sum);
    }
}
