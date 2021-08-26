using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202010141505, "Изменение таблицы bankets добавление колонок name, status и url")]
    public class _202010141505_EditBanketsTable : Migration
    {
        public override void Down()
        {
            Execute.Sql("ALTER TABLE public.bankets DROP COLUMN name; " +
                "ALTER TABLE public.bankets DROP COLUMN status; " +
                "ALTER TABLE public.bankets DROP COLUMN url; ");
        }

        public override void Up()
        {
            Execute.Sql("ALTER TABLE public.bankets ADD COLUMN name character varying NOT NULL DEFAULT ''; "+
                "ALTER TABLE public.bankets ALTER COLUMN name DROP DEFAULT; " +
                "ALTER TABLE public.bankets ADD COLUMN status integer NOT NULL DEFAULT 4; " +
                "ALTER TABLE public.bankets ALTER COLUMN status DROP DEFAULT; " +
                "ALTER TABLE public.bankets ADD COLUMN url varchar NOT NULL DEFAULT ''; " +
                "ALTER TABLE public.bankets ALTER COLUMN url DROP DEFAULT; ");
        }
    }
}
