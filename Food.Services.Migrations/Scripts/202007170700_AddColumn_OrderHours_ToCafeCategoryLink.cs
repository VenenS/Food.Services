using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202007170700, "OrderHours содержит расписание активности категории блюда")]
    public class _202007170700_AddColumn_OrderHours_ToCafeCategoryLink : Migration
    {
        public override void Up()
        {
            Execute.Sql(
               @"ALTER TABLE public.cafe_category_link ADD COLUMN order_hours character varying(8192);"
           );
        }

        public override void Down()
        {
            Execute.Sql(@"ALTER TABLE public.cafe_category_link DROP COLUMN order_hours;");
        }
    }
}
