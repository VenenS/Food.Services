using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    //не работает
    class UserReferralFactory
    {
        public static UserReferral CreateReferral(User user)
        {
            var referral = new UserReferral()
            {
                IsActive = true,
                IsDeleted = false

            };
            ContextManager.Get().UserReferral.Add(referral);
            return referral;
        }
    }
}
