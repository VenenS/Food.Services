using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202007291256, "EmailConfirmationCode Добавление столбца для кодов подтверждения E-mail")]
    public class _202007291256_AddColumn_EmailConfirmationCode_ToUser : Migration
    {
        public override void Up()
        {
            Execute.Sql("ALTER TABLE public.user ADD COLUMN user_email_confirmation_code character varying(36);");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE public.user DROP COLUMN user_email_confirmation_code;");
        }
    }
}
