using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(201912181556, "OrderAbortTIme для конфигурирования времени отмены корпоративных заказов при недостаточной стоимости")]
    class _201912181556_AddColumnAbortTimeToCafeTable : Migration
    {
        public override void Down()
        {
            Execute.Sql("ALTER TABLE cafe DROP COLUMN order_abort_time;");
        }

        public override void Up()
        {
            Execute.Sql(
               @"ALTER TABLE cafe ADD COLUMN order_abort_time time(6);"
           );
        }
    }
}
