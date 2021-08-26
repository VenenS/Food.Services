using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет рабочее время.
    /// </summary>
    [XmlRoot("businessHours")]
    public class BusinessHours
    {
        /// <summary>
        /// Возвращает или задает список отклонений рабочего времени.
        /// </summary>
        [XmlArray("departures")]
        [XmlArrayItem("departure")]
        public List<BusinessHoursDeparture> Departures { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в пятницу.
        /// </summary>
        [XmlArray("friday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Friday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в понедельник.
        /// </summary>
        [XmlArray("monday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Monday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в субботу.
        /// </summary>
        [XmlArray("saturday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Saturday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в воскресенье.
        /// </summary>
        [XmlArray("sunday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Sunday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в четверг.
        /// </summary>
        [XmlArray("thursday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Thursday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время во вторник.
        /// </summary>
        [XmlArray("tuesday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Tuesday { get; set; }

        /// <summary>
        /// Возвращает или задает рабочее время в среду.
        /// </summary>
        [XmlArray("wednesday")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Wednesday { get; set; }

        /// <summary>
        /// Возвращает рабочие часы для указанного дня недели (не учитывает
        /// отклонения рабочего времени!).
        /// </summary>
        [XmlIgnore]
        public List<BusinessHoursItem> this[DayOfWeek dayOfWeek]
        {
            get
            {
                switch (dayOfWeek)
                {
                    case DayOfWeek.Monday: return Monday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Tuesday: return Tuesday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Wednesday: return Wednesday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Thursday: return Thursday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Friday: return Friday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Saturday: return Saturday ?? new List<BusinessHoursItem>();
                    case DayOfWeek.Sunday: return Sunday ?? new List<BusinessHoursItem>();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
                }
            }
        }

        /// <summary>
        /// Возвращает находится ли дата в диапазоне рабочего времени.
        /// </summary>
        public bool IsDateWithinBusinessHoursRange(DateTime when)
        {
            var hours = GetBusinessHours(when);
            var whenTod = when.TimeOfDay;
            return hours.Any(x => x.OpeningTime.TimeOfDay <= whenTod && whenTod <= x.ClosingTime.TimeOfDay);
        }

        /// <summary>
        /// Возвращает рабочие часы на указанную дату.
        /// </summary>
        public List<BusinessHoursItem> GetBusinessHours(DateTime when)
        {
            when = when.Date;

            var departure = Departures?.FirstOrDefault(x => x.Date.Date == when);
            if (departure != null)
            {
                return !departure.IsDayOff
                    ? departure.Items ?? new List<BusinessHoursItem>()
                    : new List<BusinessHoursItem>();
            }

            return this[when.DayOfWeek];
        }
    }
}
