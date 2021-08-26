using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202010011540, "Изменение таблицы order добавление колонок manager_comment и pay_type")]
    public class _202010011540_EditOrderTable : Migration
    {
        public override void Down()
        {
            Execute.Sql("ALTER TABLE public.\"order\" DROP COLUMN manager_comment; ALTER TABLE public.\"order\" DROP COLUMN pay_type;");
        }

        public override void Up()
        {
            Execute.Sql("ALTER TABLE public.\"order\" ADD manager_comment varchar NULL;" +
                        " ALTER TABLE public.\"order\" ADD pay_type varchar NOT NULL DEFAULT 'CourierCash'::character varying; " +
                        "COMMENT ON COLUMN public.\"order\".pay_type IS 'Способ оплаты';");
        }
    }
}