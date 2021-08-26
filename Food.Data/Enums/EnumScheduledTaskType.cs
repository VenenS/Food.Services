namespace Food.Data.Entities
{
    public enum EnumScheduledTaskType
    {
        /// <summary>
        /// Отправить уведомление для кафе
        /// </summary>
        NotificationCafe = 0,
        /// <summary>
        /// Создать компаниейские заказы по расписанию кураторов
        /// </summary>
        CreateCompanyOrders = 1,
        /// <summary>
        /// Удаление из DishInMenu раз в месяц
        /// </summary>
        DeleteFromDishInMenu = 2
    }
}
