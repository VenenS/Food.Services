namespace Food.Data.Entities
{
    /// <summary>
    /// Тип пользователя. Кем является пользователь в компании. (UserInCompany)
    /// </summary>
    public static class EnumUserType
    {
        /// <summary>
        /// Менеджре кафе
        /// </summary>
        public const string Manager = "Manager";
        /// <summary>
        /// Куратор
        /// </summary>
        public const string Consolidator = "Consolidat";
        /// <summary>
        /// Не знаю кто это. Для тестов используется
        /// </summary>
        public const string SomeType = "SomeType";
    }
}
