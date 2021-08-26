using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202009241210, "Добавление множественной привязки блюд к категориям")]
    public class _202009241210_AddTale_DishCafeCategoryLink : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"CREATE TABLE dish_cafe_category_link ( 
	                id SERIAL,
	                dish_id bigint references dish(id) not null,
	                cafe_category_link_id bigint references cafe_category_link(id) not null,
                    food_dish_index int,
	                create_date timestamp without time zone DEFAULT now(),
	                created_by bigint,
	                last_upd_date timestamp without time zone,
	                last_upd_by bigint,
                    is_active bool,
	                is_deleted bool not null
                );");
            Execute.Sql(
                @"DO $$
                DECLARE 
	                caf_cat_id int;
	                cur record;
                BEGIN
	                FOR cur IN SELECT * FROM cafe_category_link
	                LOOP
		                INSERT INTO dish_cafe_category_link (dish_id, cafe_category_link_id, food_dish_index, create_date, created_by, last_upd_date, last_upd_by, is_active, is_deleted)
		                SELECT id, cur.id, food_dish_index, cur.create_date, cur.created_by, cur.last_upd_date, cur.last_upd_by, cur.is_active, cur.is_deleted
		                FROM dish WHERE cafe_category_link_id = cur.id;
	                END LOOP;
                END $$;");
            Execute.Sql("ALTER TABLE dish DROP CONSTRAINT food_dish_cafe_category_link_id_fkey;");
            Execute.Sql("ALTER TABLE dish DROP COLUMN food_dish_index;");
            Execute.Sql("DROP INDEX \"food_dish_IX_food_category_id\";");

            // Столбец нельзя удалить из-за Materialized View - v_today_cafe_menu (возможно его можно удалить)
            //Execute.Sql("ALTER TABLE dish DROP COLUMN cafe_category_link_id;");

            Execute.Sql("ALTER TABLE dish ALTER COLUMN cafe_category_link_id DROP NOT NULL;");
            Execute.Sql("UPDATE dish SET cafe_category_link_id = null;");
        }

        public override void Down()
        {
            Execute.Sql("ALTER TABLE dish ADD food_dish_index int;");
            //Execute.Sql("ALTER TABLE dish ADD cafe_category_link_id bigint;");
            //Восстанавливает исходные данные
            Execute.Sql(
                @"DO $$
                DECLARE 
	                caf_cat_id int;
	                cur record;
                BEGIN
	                FOR cur IN SELECT * FROM cafe_category_link
	                LOOP
		                UPDATE dish SET cafe_category_link_id = cur.id 
                        WHERE dish.id in (select dish_id from dish_cafe_category_link WHERE dish_cafe_category_link.cafe_category_link_id = cur.id);
	                END LOOP;
                END $$;");
            Execute.Sql("ALTER TABLE dish ALTER COLUMN cafe_category_link_id SET NOT NULL;");
            Execute.Sql("CREATE INDEX \"food_dish_IX_food_category_id\" ON public.dish USING btree (cafe_category_link_id);");
            Execute.Sql("ALTER TABLE public.dish ADD CONSTRAINT food_dish_cafe_category_link_id_fkey FOREIGN KEY (cafe_category_link_id) REFERENCES cafe_category_link(id)");
            Execute.Sql("DROP TABLE dish_cafe_category_link");
        }
}
}
