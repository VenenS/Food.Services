using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;

namespace Food.Services
{
    public static class IdentityExtensions
    {
        public static User GetUserById(this IIdentity identity)
        {
            var ci =
                identity as ClaimsIdentity;

            if (ci == null)
            {
                throw new SecurityException("Пользователь не авторизован");
            }

            var userId = ci.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;

            return Accessor.Instance.GetUserById(long.Parse(userId));
        }

        public static long GetUserId(this IIdentity identity)
        {
            var ci =
                identity as ClaimsIdentity;

            var claim = ci?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);

            if (claim == null)
            {
                throw new SecurityException("Пользователь не авторизован");
            }
            var userId = claim.Value;

            return long.Parse(userId);
        }
    }
}