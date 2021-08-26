using System;
using System.Xml.Serialization;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет элемент рабочего времени.
    /// </summary>
    [XmlRoot("businessHoursItem")]
    public class BusinessHoursItem
    {
        /// <summary>
        /// Возвращает или задает время закрытия.
        /// </summary>
        [XmlAttribute("closingTime")]
        public DateTime ClosingTime { get; set; }

        /// <summary>
        /// Возвращает или задает время открытия.
        /// </summary>
        [XmlAttribute("openingTime")]
        public DateTime OpeningTime { get; set; }

        public override string ToString() => $"{OpeningTime:HH:mm}-{ClosingTime:HH:mm}";
    }
}
