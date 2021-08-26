using System.ComponentModel;

namespace Food.Data.Entities
{
    public enum EnumSearchType
    {
        [Description("имени заказчика")]
        SearchByName,
        [Description("телефону заказчика")]
        SearchByPhone,
        [Description("названию заказанного блюда")]
        SearchByDish,
        [Description("номеру заказа")]
        SearchByOrderNumber,
        [Description("названию кафе")]
        SearchByCafe,
    }
}
