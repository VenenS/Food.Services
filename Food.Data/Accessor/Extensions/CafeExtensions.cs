using System;
using System.Linq;
using Food.Data.Entities;
using System.IO;
using System.Xml.Serialization;
using accessor = ITWebNet.FoodService.Food.DbAccessor.Accessor;
using System.Collections.Generic;
using System.Diagnostics;

namespace ITWebNet.FoodService.Food.Data.Accessor.Extensions
{
    public static class CafeExtensions
    {

        public static BusinessHours GetBusinessHours(string businessHours)
        {
            if (!string.IsNullOrWhiteSpace(businessHours))
            {
                try
                {
                    using (StringReader reader = new StringReader(businessHours))
                    {
                        XmlSerializer serialiser = new XmlSerializer(typeof(BusinessHours));
                        return (BusinessHours)serialiser.Deserialize(reader);
                    }
                }
                catch
                {
                    return new BusinessHours();
                }
            }
            return new BusinessHours();
        }

        /// <summary>
        /// Рабочее время для кафе
        /// </summary>
        public static TimeSpan? GetWorkingTime(BusinessHours cafeBusinessHours, bool isWorkFrom = false, DateTime? date = null)
        {
            TimeSpan? getWorkingTime = null;

            try
            {
                DateTime workingTime = date ?? DateTime.Now;

                bool isDepartures = false;
                var workingToday = cafeBusinessHours.Departures?.FirstOrDefault(o => o.Date == workingTime.Date);
                if (workingToday != null)
                {
                    isDepartures = true;
                    if (workingToday.IsDayOff == false && workingToday.Items != null)
                    {
                        getWorkingTime = isWorkFrom ? TimeSpan.FromTicks(workingToday.Items.First().OpeningTime.Ticks) : TimeSpan.FromTicks(workingToday.Items.First().ClosingTime.Ticks);
                    }
                    else
                    {
                        return null;
                    }
                }

                if (isDepartures == false)
                {
                    var workingHours = new List<List<BusinessHoursItem>>
                    {
                        cafeBusinessHours.Sunday,
                        cafeBusinessHours.Monday,
                        cafeBusinessHours.Tuesday,
                        cafeBusinessHours.Wednesday,
                        cafeBusinessHours.Thursday,
                        cafeBusinessHours.Friday,
                        cafeBusinessHours.Saturday
                    };
                    var workingHoursToday = workingHours[(int)workingTime.DayOfWeek];
                    if (workingHoursToday == null || !workingHoursToday.Any())
                        return null;
                    getWorkingTime = isWorkFrom
                        ? TimeSpan.FromTicks(workingHoursToday[0].OpeningTime.Ticks)
                        : TimeSpan.FromTicks(workingHoursToday[0].ClosingTime.Ticks);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return new TimeSpan(getWorkingTime.Value.Hours, getWorkingTime.Value.Minutes, 0);
        }

        public static bool IsRest(BusinessHours cafeBusinessHours, DateTime? date = null)
        {
            DateTime workingTime = DateTime.Now;
            if (date != null)
                workingTime = date.Value;
            bool isDayOff = false;
            if (cafeBusinessHours.Departures != null)
            {
                string dayOfWeek = DateTime.Now.DayOfWeek.ToString();
                if (dayOfWeek == "Monday" && cafeBusinessHours.Monday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Saturday" && cafeBusinessHours.Saturday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Sunday" && cafeBusinessHours.Sunday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Thursday" && cafeBusinessHours.Thursday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Tuesday" && cafeBusinessHours.Tuesday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Wednesday" && cafeBusinessHours.Wednesday.Count == 0) isDayOff = true;
                if (dayOfWeek == "Friday" && cafeBusinessHours.Friday.Count == 0) isDayOff = true;

                var workingToday = cafeBusinessHours.Departures.FirstOrDefault(o => o.Date == workingTime.Date);
                if (workingToday != null)
                {
                    return workingToday.IsDayOff;
                }
            }

            return isDayOff;
        }

        /// <summary>
        /// Рабочее время для кафе
        /// </summary>
        public static TimeSpan? GetWorkingTime(long cafeId, bool isWorkFrom = false, DateTime? date = null)
        {
            try
            {
                BusinessHours cafeBusinessHours = accessor.Instance.GetCafeBusinessHours(cafeId);
                return GetWorkingTime(cafeBusinessHours, isWorkFrom, date);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool IsRest(long cafeId, DateTime? date = null)
        {
            return IsRest(accessor.Instance.GetCafeBusinessHours(cafeId), date);
        }

        public static DateTime GetWorkingTimeToday(TimeSpan? WorkingSpan)
        {
            return WorkingSpan.HasValue
                        ? DateTime.Now.Date.Add(WorkingSpan.Value) : DateTime.Now;
        }

    }
}
