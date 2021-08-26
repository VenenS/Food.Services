using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(201912170937, "Создание пустой БД на основе дампа")]
    public class _201912170937_CreateSchema : Migration
    {
        public override void Up()
        {
            Execute.EmbeddedScript("SqlScripts.201912170937_CreateSchema");
            Execute.EmbeddedScript("SqlScripts.201912170937_Populate");
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}
