using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(20200821131111, "AddResetPasswordKeysTableToUser Добавление новой таблицы для хранения ключей восстановления пароля")]
    public class _20200821131111_AddResetPasswordKeysTableToUser : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"CREATE TABLE public.user_password_reset_keys 
(id serial NOT NULL PRIMARY KEY, 
user_id bigint NOT NULL, 
key text NOT NULL, 
issued_to timestamp without time zone NOT NULL);");
            Execute.Sql("ALTER TABLE ONLY public.user_password_reset_keys ADD CONSTRAINT passwordreset_user_fk FOREIGN KEY (user_id) REFERENCES public.user(id);");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE public.user_password_reset_keys");
        }
    }
}
