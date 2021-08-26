using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.DishInMenu
{
    public static class DishInMenuExtensions
    {
        public static DishInMenuHistoryModel GetContract(this DishInMenuHistory schedule)
        {
            return schedule == null
                ? null
                : new DishInMenuHistoryModel
                {
                    Id = schedule.Id,
                    Price = schedule.Price,
                    Type = schedule.Type,
                    DishID = schedule.DishId,
                    LastUpdDate = schedule.LastUpdDate,
                    Creator = schedule.Creator == null
                        ? null
                        : new UserAdminModel
                        {
                            AccessFailedCount = schedule.Creator.AccessFailedCount ?? 0,
                            CreateDate = schedule.Creator.CreationDate,
                            CreatedBy = schedule.Creator.CreatorId,
                            Email = schedule.Creator.Email,
                            EmailConfirmed = schedule.Creator.EmailConfirmed,
                            Id = (int)schedule.Creator.Id,
                            IsDeleted = schedule.Creator.IsDeleted,
                            LastUpdateBy = schedule.Creator.LastUpdateByUserId,
                            LastUpdateDate = schedule.Creator.LastUpdDate,
                            LockoutEnabled = schedule.Creator.Lockout,
                            LockoutEndDateUtc = schedule.Creator.LockoutEnddate,
                            PhoneNumber = schedule.Creator.PhoneNumber,
                            PhoneNumberConfirmed = schedule.Creator.PhoneNumberConfirmed,
                            TwoFactorEnabled = schedule.Creator.TwoFactor,
                            UserFullName = schedule.Creator.FullName
                        }
                };
        }

        public static DishInMenuHistory GetEntity(this DishInMenuHistoryModel schedule)
        {
            return schedule == null
                ? new DishInMenuHistory()
                : new DishInMenuHistory
                {
                    Id = schedule.Id,
                    LastUpdDate = schedule.LastUpdDate ?? System.DateTime.Now,
                    Price = schedule.Price ?? 0,
                    Type = schedule.Type,
                    DishId = schedule.DishID
                };
        }
    }
}
