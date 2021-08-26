using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class CafeServiceHelper
    {
        public static bool IsNewUserCafeLinkAvailable(CafeManager cafeManager)
        {
            if (cafeManager == null)
                return false;

            var user =
                Accessor.Instance.GetUserById(cafeManager.UserId);

            var cafe =
                Accessor.Instance.GetCafeById(cafeManager.CafeId);

            if (user != null && cafe != null)
            {
                var userCafeList =
                    Accessor.Instance.GetListOfCafeByUserId(user.Id);

                var isExist =
                    userCafeList.All(r => r.Id != cafe.Id);

                if (!isExist)
                    throw new Exception("Уже существует данная привязка к кафе для данного пользователя.");

                return true;
            }
            else
            {
                throw new Exception("Отсутствуют кафе или пользователь.");
            }
        }

        public static bool IsNewCafeNameAvailable(Cafe cafe)
        {
            if (cafe != null
                && !string.IsNullOrWhiteSpace(cafe.CafeName)
                && !string.IsNullOrWhiteSpace(cafe.CafeFullName)
            )
            {
                var cafes =
                    Accessor.Instance.GetCafes();

                var isExist =
                    !cafes
                    .Any(
                        c =>
                            ((
                                String.IsNullOrWhiteSpace(c.CafeFullName)
                                || c.CafeFullName.ToLower().Equals(cafe.CafeFullName.ToLower())
                            )
                            ||
                            (
                                String.IsNullOrWhiteSpace(c.CafeName)
                                || c.CafeName.ToLower().Equals(cafe.CafeName.ToLower())
                            ))
                            && c.Id != cafe.Id
                    );

                if (!isExist)
                    throw new Exception("Уже существует кафе с данным именем.");

                return true;
            }

            throw new Exception("Отсутствует кафе для работы с ним.");
        }
    }
}
