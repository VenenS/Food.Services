using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202011112024, "Удаляет значение -1 в таблице кафе для полей \"среднее время доставки\" и \"минимальная сумма заказа\". Добавляет комментарий к способам оплыты")]
    public class _202011112024_RemoveMinusOneValue_Cafe : Migration
    {
        public override void Up()
        {
            Execute.Sql("update public.cafe set average_delivery_time = null where average_delivery_time = -1");
            Execute.Sql("update public.cafe set minimum_order_rub = null where minimum_order_rub = -1");
            Execute.Sql("comment on column public.cafe.payment_method is 'Битовое перечисление: 1 - наличные; 2 - картой на сайте; 4 - Картой курьеру. Значения 3, 5 и 7 являются комбинацией первых (результат применения логического или |)'");
        }

        public override void Down()
        {
            Execute.Sql("update public.cafe set average_delivery_time = -1 where average_delivery_time = null");
            Execute.Sql("update public.cafe set minimum_order_rub = -1 where minimum_order_rub = null");
            Execute.Sql("comment on column public.cafe.payment_method is null");
        }
    }
}
