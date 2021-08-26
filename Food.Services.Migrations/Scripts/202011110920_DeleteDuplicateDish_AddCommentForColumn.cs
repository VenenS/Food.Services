using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    namespace Food.Services.Migrations.Scripts
    {
        [Migration(202011110920, "Удаление дублирующихся блюд для определенной категории меню \"Банкет\". Добавление комментария для столбца status сущности bankets.")]
        public class _202011110920_DeleteDuplicateDish_AddCommentForColumn : Migration
        {
            public override void Down()
            {
                Execute.Sql("INSERT INTO public.cafe_menu_patterns_dishes(is_deleted, pattern_id, dish_id, price, name) " +
                            "VALUES (false, 119, 2213, 400, 'Щука по-варшавски'), (false, 119, 2213, 400, 'Щука по-варшавски');" +
                            "COMMENT ON COLUMN public.bankets.status IS NULL;");
            }

            public override void Up()
            {
                Execute.Sql("DELETE FROM public.cafe_menu_patterns_dishes WHERE id IN(SELECT id FROM cafe_menu_patterns_dishes " +
                            "WHERE pattern_id = 119 AND dish_id = 2213 " +
                            "LIMIT 2);" +
                            "COMMENT ON COLUMN public.bankets.status IS '1=PROJECTED; 2=FORMED; 3=PREPARING; 4=CLOSED;';");
            }

        }
    }
}
