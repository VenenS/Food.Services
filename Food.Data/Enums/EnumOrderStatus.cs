using System.ComponentModel;

namespace Food.Data.Entities
{
    /// <summary>
    /// Типы статусов заказа пользователя
    /// </summary>
    public enum EnumOrderStatus : long
    {
        /// <summary>
        /// Заказ создан
        /// </summary>
        [Description("Новый заказ")]
        Created = 1,
        /// <summary>
        /// Заказ принят
        /// </summary>
        [Description("Заказ принят кафе")]
        Accepted = 2,
        /// <summary>
        /// Заказ в процессе доставки
        /// </summary>
        [Description("Заказ в процессе доставки")]
        Delivery = 3,
        /// <summary>
        /// Заказ доставлен. Итоговая стадия заказа
        /// </summary>
        [Description("Заказ успешно доставлен")]
        Delivered = 4,
        /// <summary>
        /// Заказ отменен
        /// </summary>
        [Description("Заказ отменён")]
        Abort = 5,
        /// <summary>
        /// Корзина с сайта
        /// </summary>
        [Description("Unknown")]
        Cart = 6
    }
}
