using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202027101217, "Добавление в таблицу address колонок entrance, floor, intercom")]
    public class _202027101217_AddColumnsToAddressTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"ALTER TABLE ONLY public.address ADD entrance varchar;");
            Execute.Sql(@"ALTER TABLE ONLY public.address ADD storey varchar;");
            Execute.Sql(@"ALTER TABLE ONLY public.address ADD intercom varchar;");
        }
        public override void Down()
        {
            Execute.Sql(@"ALTER TABLE public.address DROP COLUMN entrance;");
            Execute.Sql(@"ALTER TABLE public.address DROP COLUMN storey;");
            Execute.Sql(@"ALTER TABLE public.address DROP COLUMN intercom;");
        }
    }
}
