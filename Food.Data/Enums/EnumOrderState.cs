namespace Food.Data.Entities
{
    /// <summary>
    /// Определяет состояние заказа.
    /// </summary>
    public enum EnumOrderState
    {
        /// <summary>
        /// Заказ создан
        /// </summary>
        Created = 1,
        /// <summary>
        /// Заказ принят
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// Заказ в процессе доставки
        /// </summary>
        Delivery = 3,
        /// <summary>
        /// Заказ доставлен. Итоговая стадия заказа
        /// </summary>
        Delivered = 4,
        /// <summary>
        /// Заказ отменен. Итоговая стадия заказа
        /// </summary>
        Abort = 5,
        /// <summary>
        /// Корзина с сайта
        /// </summary>
        Cart = 6
    }
}
