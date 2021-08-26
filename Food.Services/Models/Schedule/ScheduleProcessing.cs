using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using ITWebNet.FoodServiceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Food.Services.Models.Schedule
{
    public class ScheduleProcessing
    {
        #region Members

        #region Private Members

        private readonly User _currentUser;

        #endregion

        #region Public Members

        /// <summary>
        ///     Итоговое расписание, после проверок и изменений
        /// </summary>
        public ScheduleModel FinalSchedule { get; }

        #endregion

        #endregion

        #region Methods

        #region Private Methods

        /// <summary>
        ///     Сравнение со старым расписанием (действует только для тех расписаний, которые необходимо изменить)
        ///     При несовпадении типов расписании возникает ошибка
        /// </summary>
        /// <returns></returns>
        private bool CompareWithOldSchedule()
        {
            var oldSchedule =
                Accessor.Instance.GetScheduleById(FinalSchedule.Id);

            if (oldSchedule == null) return false;

            if (oldSchedule.Type != FinalSchedule.Type)
            {
                if (!Accessor.Instance.IsUserManagerOfCafe(_currentUser.Id, 
                    oldSchedule.Dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId))
                    throw new SecurityException("Attempt of unauthorized access");

                return false;
            }

            return true;
        }

        /// <summary>
        ///     Проверка временных периодов расписания
        ///     Ошибка возникает в следующих случаях:
        ///     1. При отсутствующих периодах
        ///     2. Если начало периода больше его конца
        ///     Если дата окончания пуста, то ставится сегодняшняя дата + 100 лет
        ///     Обнуляются поля, ненужные для определённых типов расписаний
        /// </summary>
        private void CheckTimePeriods()
        {
            if (FinalSchedule.OneDate == null
                && FinalSchedule.BeginDate == null)
                throw new Exception("Wrong time period.");

            if (FinalSchedule.OneDate == null
                && FinalSchedule.BeginDate != null
                && FinalSchedule.EndDate != null
                && FinalSchedule.BeginDate > FinalSchedule.EndDate)
                throw new Exception("Wrong time period");

            if (FinalSchedule.BeginDate != null
                && FinalSchedule.EndDate == null)
                FinalSchedule.EndDate = DateTime.Now.AddYears(100);

            if (FinalSchedule.BeginDate != null
                && FinalSchedule.EndDate != null
                && FinalSchedule.BeginDate == FinalSchedule.EndDate
                && FinalSchedule.Type != ((char)ScheduleTypeEnum.Daily).ToString())
            {
                FinalSchedule.OneDate = FinalSchedule.BeginDate;
                FinalSchedule.BeginDate = null;
                FinalSchedule.EndDate = null;
            }

            if (FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Simply).ToString())
            {
                FinalSchedule.BeginDate = null;
                FinalSchedule.EndDate = null;
            }

            if (FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Daily).ToString())
                if (FinalSchedule.BeginDate == null
                    && FinalSchedule.OneDate != null)
                    FinalSchedule.Type =
                        ((char)ScheduleTypeEnum.Simply).ToString();
        }

        /// <summary>
        ///     Работа с полями, необходимыми для определённых типов расписаний.
        ///     Заполнение полей, в случае их отсутствия, изменение в случае необходимости.
        ///     1. Если для Simply отсутствует OneDate, то ставится текущая дата
        ///     2. Если Exclude, то цена зануляется. Если есть дата начала периода, то зануляется OneDate.
        ///     3. Для типов Daily, Weekly, Monthly нет возможности создать OneDate, только период
        /// </summary>
        private void WorkWithScheduleType()
        {
            if (FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Simply).ToString())
                FinalSchedule.OneDate = FinalSchedule.OneDate ?? DateTime.Now;

            if (FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Exclude).ToString())
            {
                FinalSchedule.Price = 0;
                if (FinalSchedule.BeginDate != null)
                {
                    FinalSchedule.OneDate = null;
                }
                else
                {
                    FinalSchedule.OneDate = FinalSchedule.OneDate ?? DateTime.Now;
                    FinalSchedule.BeginDate = null;
                    FinalSchedule.EndDate = null;
                }
            }

            if (
                FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Daily).ToString()
                || FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Weekly).ToString()
                || FinalSchedule.Type ==
                ((char)ScheduleTypeEnum.Monthly).ToString())
            {
                if (FinalSchedule.BeginDate == null)
                {
                    FinalSchedule.BeginDate
                        = FinalSchedule.OneDate ?? DateTime.Now;

                    FinalSchedule.EndDate
                        = FinalSchedule.OneDate ?? DateTime.Now;
                }

                FinalSchedule.OneDate = null;
            }
        }

        /// <summary>
        ///     Проверка блюда, к которому пытается привязаться расписание.
        ///     Проверяется, существует ли блюдо
        ///     Идёт проверка, может ли пользователь работать с данным блюдом.
        ///     Дальше проверяется, активно ли блюдо на добавляемое расписание
        /// </summary>
        /// <returns></returns>
        private bool CheckDish()
        {
            var dish =
                Accessor.Instance.GetFoodDishesById(new List<long>
                {
                    FinalSchedule.DishID
                }.ToArray()).FirstOrDefault();

            if (dish == null) return false;

            if (!Accessor.Instance.IsUserManagerOfCafe(_currentUser.Id, 
                dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId))
                throw new SecurityException("Attempt of unauthorized access");

            if (FinalSchedule.OneDate == null)
            {
                //if (!(dish.VersionFrom <= schedule.BeginDate && ((dish.VersionTo == null && (schedule.EndDate ?? DateTime.Now.AddYears(100)) >= dish.VersionFrom)
                //            || (dish.VersionTo != null && (schedule.EndDate ?? DateTime.Now.AddYears(100)) <= dish.VersionTo))))
                //{
                //    return false;
                //}
            }
            else
            {
                if (!(dish.VersionFrom <= FinalSchedule.OneDate
                      &&
                      (dish.VersionTo
                       ?? DateTime.Now.AddYears(100)
                      ) >= FinalSchedule.OneDate
                    )
                )
                    return false;
            }

            return true;
        }

        /// <summary>
        ///     Работа с расписанием типа Simply
        ///     Если на эту дату есть Exclude, то
        ///     1. Если Exclude расписание на эту дату типа OneDate, то оно удаляется
        ///     2. Если Exclude расписание типа периода, то из этого периода исключается необходимый день
        /// </summary>
        /// <returns></returns>
        private bool WorkWithSimplyType()
        {
            var currentSchedule =
                Accessor.Instance.GetScheduleActiveByDate(
                    (DateTime)FinalSchedule.OneDate, FinalSchedule.DishID
                ).Where(s => s.Id != FinalSchedule.Id).ToList();

            var excludeSchedule =
                currentSchedule.FirstOrDefault(
                    s => s.Type == ((char)ScheduleTypeEnum.Exclude).ToString()
                );

            if (excludeSchedule != null)
                if (excludeSchedule.OneDate != null)
                {
                    Accessor.Instance.DeleteSchedule(excludeSchedule.Id, _currentUser.Id);
                }
                else
                {
                    var leftPeriodStart = (DateTime)excludeSchedule.BeginDate;
                    var leftPeriodEnd = ((DateTime)FinalSchedule.OneDate).AddDays(-1);
                    var rightPeriodStart = ((DateTime)FinalSchedule.OneDate).AddDays(+1);
                    var rightPeriodEnd = excludeSchedule.EndDate;

                    if (leftPeriodEnd > leftPeriodStart)
                    {
                        Accessor.Instance.AddSchedule(
                            excludeSchedule.DishId,
                            'E',
                            leftPeriodStart,
                            leftPeriodEnd,
                            null,
                            null,
                            null,
                            0,
                            _currentUser.Id
                        );
                    }
                    else
                    {
                        if (leftPeriodEnd == leftPeriodStart)
                            Accessor.Instance.AddSchedule(
                                excludeSchedule.DishId,
                                (char)ScheduleTypeEnum.Exclude,
                                null,
                                null,
                                leftPeriodStart,
                                null,
                                null,
                                0,
                                _currentUser.Id
                            );
                    }

                    if (rightPeriodEnd == null
                        || rightPeriodStart < rightPeriodEnd)
                    {
                        Accessor.Instance.AddSchedule(
                            excludeSchedule.DishId,
                            (char)ScheduleTypeEnum.Exclude,
                            rightPeriodStart,
                            rightPeriodEnd,
                            null,
                            null,
                            null,
                            0,
                            _currentUser.Id
                        );
                    }
                    else
                    {
                        if (rightPeriodStart == rightPeriodEnd)
                            Accessor.Instance.AddSchedule(
                                excludeSchedule.DishId,
                                (char)ScheduleTypeEnum.Exclude,
                                null,
                                null,
                                rightPeriodStart,
                                null,
                                null,
                                0,
                                _currentUser.Id
                            );
                    }

                    Accessor.Instance.DeleteSchedule(excludeSchedule.Id, _currentUser.Id);

                    currentSchedule.Remove(excludeSchedule);
                }

            var simplySchedules =
                currentSchedule.Where(
                    s => s.Type == ((char)ScheduleTypeEnum.Simply).ToString()
                ).ToList();

            foreach (var simplySchedule in simplySchedules)
                Accessor.Instance.RemoveSchedule(simplySchedule.Id, _currentUser.Id);

            return true;
        }

        public static bool CheckDishExistingInOrders(long dishId, DateTime? wantedDate, DateTime? beginDate,
            DateTime? endDate)
        {
            return Accessor.Instance.CheckExistingDishInOrders(dishId, wantedDate, beginDate, endDate);
        }

        /// <summary>
        ///     Работа с расписанием типа Exclude.
        ///     Если блюдо уже есть в чьих-то заказах, то нельзя добавить исключение (нет).
        ///     Если уже есть Exclude, пересекающийся с этим периодом\датой в новом Exclude, то блокируется добавление
        /// </summary>
        /// <returns></returns>
        private bool WorkWithExclude()
        {
            List<DishInMenu> currentSchedule;

            if (FinalSchedule.OneDate != null)
            {
                if (CheckDishExistingInOrders(
                    FinalSchedule.DishID,
                    FinalSchedule.OneDate,
                    null,
                    null)
                )
                    return false;
                currentSchedule =
                    Accessor.Instance.GetScheduleActiveByDate(
                        (DateTime)FinalSchedule.OneDate, FinalSchedule.DishID
                    ).Where(s => s.Id != FinalSchedule.Id).ToList();
            }
            else
            {
                if (CheckDishExistingInOrders(
                    FinalSchedule.DishID,
                    null,
                    FinalSchedule.BeginDate,
                    FinalSchedule.EndDate)
                )
                    return false;
                currentSchedule =
                    Accessor.Instance.GetScheduleActiveByDateRange(
                        (DateTime)FinalSchedule.BeginDate,
                        FinalSchedule.EndDate ?? DateTime.Now.AddYears(100),
                        FinalSchedule.DishID
                    ).Where(s =>
                        s.Id != FinalSchedule.Id
                    ).ToList();
            }

            //if (currentSchedule.Where(
            //            s =>
            //                s.Type == ((char)ScheduleTypeEnum.Exclude).ToString()
            //                && schedule.Id != s.Id
            //        ).Count() > 0)
            //{
            //    return false;
            //}
            var excludeSchedule =
                currentSchedule.FirstOrDefault(
                    s => s.Type == ((char)ScheduleTypeEnum.Simply).ToString()
                );
            if (excludeSchedule != null) Accessor.Instance.RemoveSchedule(excludeSchedule.Id, _currentUser.Id);

            var dailySchedule =
                currentSchedule.FirstOrDefault(
                    s => s.Type == ((char)ScheduleTypeEnum.Daily).ToString()
                );
            if (dailySchedule != null) Accessor.Instance.DeleteSchedule(dailySchedule.Id, _currentUser.Id);
            return true;
        }

        /// <summary>
        ///     Работа с расписанием типа Daily
        ///     Удаляются все расписания, которые пересекаются с периодом добавляемого расписания
        /// </summary>
        /// <returns></returns>
        private bool WorkWithDaily()
        {
            var currentSchedule =
                Accessor.Instance.GetScheduleActiveByDateRange(
                        (DateTime)FinalSchedule.BeginDate,
                        FinalSchedule.EndDate ?? DateTime.Now.AddYears(100),
                        FinalSchedule.DishID
                    ).Where(s => s.Id != FinalSchedule.Id)
                    .ToList();

            for (var i = 0; i < currentSchedule.Count; i++)
                if (currentSchedule[i].OneDate != null)
                {
                    Accessor.Instance.RemoveSchedule(currentSchedule[i].Id, _currentUser.Id);
                    currentSchedule.RemoveAt(i);
                }
                else
                {
                    if (currentSchedule[i].BeginDate >= FinalSchedule.BeginDate
                        &&
                        (currentSchedule[i].EndDate ?? DateTime.Now.AddYears(100))
                        <= (FinalSchedule.EndDate ?? DateTime.Now.AddYears(100))
                    )
                    {
                        Accessor.Instance.RemoveSchedule(currentSchedule[i].Id, _currentUser.Id);
                        currentSchedule.RemoveAt(i);
                    }
                    else
                    {
                        if (currentSchedule[i].BeginDate < FinalSchedule.BeginDate)
                        {
                            if (
                                (FinalSchedule.EndDate ?? DateTime.Now.AddYears(100))
                                < (currentSchedule[i].EndDate ?? DateTime.Now.AddYears(100))
                            )
                                Accessor.Instance.AddSchedule(
                                    currentSchedule[i].DishId,
                                    currentSchedule[i].Type[0],
                                    FinalSchedule.EndDate.Value.AddDays(+1),
                                    currentSchedule[i].EndDate,
                                    currentSchedule[i].OneDate,
                                    currentSchedule[i].MonthDays,
                                    currentSchedule[i].WeekDays,
                                    currentSchedule[i].Price,
                                    _currentUser.Id
                                );
                            currentSchedule[i].EndDate =
                                FinalSchedule.BeginDate.Value.AddDays(-1);
                        }
                        else
                        {
                            currentSchedule[i].BeginDate =
                                FinalSchedule.EndDate.Value.AddDays(+1);
                        }

                        if (currentSchedule[i].BeginDate <=
                            (currentSchedule[i].EndDate ?? DateTime.Now.AddYears(100)))
                            Accessor.Instance.UpdateSchedule(currentSchedule[i], _currentUser.Id);
                    }
                }

            return true;
        }

        #endregion

        #region Public Methods

        public ScheduleProcessing(ScheduleModel schedule, User currentUser)
        {
            FinalSchedule = schedule;
            FinalSchedule.Type = FinalSchedule.Type[0].ToString();
            _currentUser = currentUser;
        }

        public ScheduleProcessing(
            long dishId,
            ScheduleTypeEnum scheduleType,
            DateTime? beginDate,
            DateTime? endDate,
            DateTime? oneDay,
            string monthDays,
            string weekDays,
            double? price,
            User currentUser
        )
        {
            FinalSchedule = new ScheduleModel
            {
                BeginDate = beginDate,
                DishID = dishId,
                EndDate = endDate,
                Id = -1,
                MonthDays = monthDays,
                OneDate = oneDay,
                Price = price,
                Type = ((char)scheduleType).ToString(),
                WeekDays = weekDays
            };
            _currentUser = currentUser;
        }

        /// <summary>
        ///     Функция проверки расписания на то, что оно может быть добавлено.
        ///     При этом проводятся сразу все необходимые изменения в информации от пользователя.
        /// </summary>
        /// <returns></returns>
        public bool CheckScheduleOnStartInformation()
        {
            if (FinalSchedule.Id > 0)
                if (!CompareWithOldSchedule())
                    return false;

            WorkWithScheduleType();

            CheckTimePeriods();

            if (!CheckDish())
                return false;

            switch (FinalSchedule.Type[0])
            {
                case (char)ScheduleTypeEnum.Simply:
                    {
                        return WorkWithSimplyType();
                    }
                case (char)ScheduleTypeEnum.Exclude:
                    {
                        return WorkWithExclude();
                    }
                case (char)ScheduleTypeEnum.Daily:
                    {
                        return WorkWithDaily();
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// Функция для удаления расписания с такими же параметрами, как добавляемое.
        /// Нужна, чтобы расписания не задваивались при добавлении новых
        /// </summary>
        public void DisableOldSheduleWithSameParams()
        {
            Accessor accessor = Accessor.Instance;
            // Нужно найти расписание с таким же блюдом, типом и датами:
            var oldShedule = accessor.GetSheduleByParams(FinalSchedule.DishID, FinalSchedule.Type, FinalSchedule.BeginDate, FinalSchedule.EndDate, FinalSchedule.OneDate);
            // Если старого расписания нет - то и делать с ним ничего не нужно:
            if (oldShedule == null || oldShedule.Count <= 0)
                return;
            // Cтарое расписание есть - надо его отключить:
            foreach (DishInMenu shedule in oldShedule)
                accessor.RemoveSchedule(shedule.Id, _currentUser.Id);
        }

        #endregion

        #endregion
    }
}
