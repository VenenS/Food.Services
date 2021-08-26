using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202010301533, "Способы оплаты в кафе")]
    public class _202010301533_Add_Info_To_Cafe : Migration
    {
        public override void Up()
        {
            Execute.Sql("ALTER TABLE public.cafe ADD COLUMN payment_method smallint not null DEFAULT 1;");
            Execute.Sql("update cafe set payment_method=3 where online_payment_sign = true;");
            Execute.Sql("update cafe set payment_method=1 where online_payment_sign = false or online_payment_sign is null;");
            Execute.Sql("ALTER TABLE public.cafe DROP COLUMN online_payment_sign");
            Execute.Sql("ALTER TABLE public.cafe ADD COLUMN delivery_regions character varying(256);");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE public.cafe DROP COLUMN delivery_regions;");
            Execute.Sql("ALTER TABLE public.cafe DROP COLUMN payment_method;");
            Execute.Sql("update cafe set online_payment_sign = (payment_method | 2 = payment_method)");
            Execute.Sql("ALTER TABLE public.cafe ADD COLUMN online_payment_sign bool");
        }
    }
}
