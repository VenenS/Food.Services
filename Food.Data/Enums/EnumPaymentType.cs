using System;
using System.ComponentModel;

namespace Food.Data.Enums
{
    [Flags]
    public enum EnumPaymentType : short
    {
        None = 0,

        [Description("По умолчанию")]
        Default = CashOnly,

        /// <summary>
        /// Наличными (1)
        /// </summary>
        [Description("Наличными")]
        CashOnly = 0x_0001,

        /// <summary>
        /// Картой на сайте (2)
        /// </summary>
        [Description("Картой на сайте")]
        OnlineOnly = 0x_0010,

        ///// <summary>
        ///// Наличными/Картой на сайте (3)
        ///// </summary>
        //[Description("Наличными/Картой на сайте")]
        //CashAndOnline = CashOnly | OnlineOnly,

        /// <summary>
        /// Картой курьеру (4)
        /// </summary>
        [Description("Картой курьеру")]
        ByCardToTheCourierOnly = 0x_0100,

        ///// <summary>
        ///// Наличными/Картой курьеру (5)
        ///// </summary>
        //[Description("Наличными/Картой курьеру")]
        //CourierOnly = CashOnly | ByCardToTheCourierOnly,

        ///// <summary>
        ///// Картой на сайте/Картой курьеру (6)
        ///// </summary>
        //[Description("Картой на сайте/Картой курьеру")]
        //CardOnly = OnlineOnly | ByCardToTheCourierOnly,

        ///// <summary>
        ///// Наличными/Картой курьеру/Картой на сайте (7)
        ///// </summary>
        //[Description("Наличными/Картой курьеру/Картой на сайте")]
        //All = CashOnly | ByCardToTheCourierOnly | OnlineOnly,
    }
}
