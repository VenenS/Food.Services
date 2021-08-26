using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Extensions.Schedule
{
    public static class ScheduleExtensions
    {
        public static ScheduleModel GetContract(this Data.Entities.DishInMenu schedule)
        {
            return schedule == null
                ? null
                : new ScheduleModel
                {
                    Id = schedule.Id,
                    BeginDate = schedule.BeginDate,
                    EndDate = schedule.EndDate,
                    MonthDays = schedule.MonthDays,
                    OneDate = schedule.OneDate,
                    WeekDays = schedule.WeekDays,
                    Price = schedule.Price,
                    Type = schedule.Type,
                    DishID = schedule.DishId,
                    CreateDate = schedule.CreateDate,
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

        public static Data.Entities.DishInMenu GetEntity(this ScheduleModel schedule)
        {
            return schedule == null
                ? new Data.Entities.DishInMenu()
                : new Data.Entities.DishInMenu
                {
                    Id = schedule.Id,
                    BeginDate = schedule.BeginDate,
                    EndDate = schedule.EndDate,
                    MonthDays = schedule.MonthDays,
                    OneDate = schedule.OneDate,
                    WeekDays = schedule.WeekDays,
                    Price = schedule.Price,
                    Type = schedule.Type,
                    DishId = schedule.DishID
                };
        }
    }
}
