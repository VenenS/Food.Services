using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет отклонение рабочего времени.
    /// </summary>
    [XmlRoot("businessHoursDeparture")]
    public class BusinessHoursDeparture
    {
        /// <summary>
        /// Возвращает или задает дату.
        /// </summary>
        [XmlAttribute("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Возвращает или задает значение, которое указывает, является ли день выходным.
        /// </summary>
        [XmlAttribute("isDayOff")]
        public bool IsDayOff { get; set; }

        /// <summary>
        /// Возвращает или задает список элементов рабочего времени.
        /// </summary>
        [XmlArray("items")]
        [XmlArrayItem("item")]
        public List<BusinessHoursItem> Items { get; set; }
    }
}
