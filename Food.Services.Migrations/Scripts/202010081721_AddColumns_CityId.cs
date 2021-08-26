using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(202010081721, "CityId добавление колонки в таблицы address, cafe, company, order, order_info")]
    public class _202010081721_AddColumns_CityId : Migration
    {
        public override void Up()
        {
            Execute.Sql("ALTER TABLE ONLY public.cafe ADD city_id int8 NOT NULL DEFAULT 517 REFERENCES public.city(id);");
            Execute.Sql("ALTER TABLE ONLY public.company ADD city_id int8 NOT NULL DEFAULT 517 REFERENCES public.city(id);");
            Execute.Sql("ALTER TABLE ONLY public.\"order\" ADD city_id int8 NOT NULL DEFAULT 517 REFERENCES public.city(id);");
            Execute.Sql("ALTER TABLE ONLY public.order_info ADD city_id int8 NOT NULL DEFAULT 517 REFERENCES public.city(id);");
            Execute.Sql("UPDATE public.address SET city_id = 517 WHERE city_id = 0 or city_id = -1 or city_id = 45824 OR city_id = 45470;");
            Execute.Sql("ALTER TABLE ONLY public.address ALTER COLUMN city_id SET DEFAULT 517;");
            Execute.Sql("ALTER TABLE ONLY public.address ALTER COLUMN city_id SET NOT NULL;");
            Execute.Sql("ALTER TABLE ONLY public.address ADD CONSTRAINT address_city_id_fkey FOREIGN KEY(city_id) REFERENCES city(id);");
            Execute.Sql("ALTER TABLE ONLY public.city ADD is_deleted boolean DEFAULT false;");
    }

        public override void Down()
        {
            Execute.Sql(@"ALTER TABLE public.cafe DROP COLUMN city_id;");
            Execute.Sql(@"ALTER TABLE public.company DROP COLUMN city_id;");
            Execute.Sql(@"ALTER TABLE public.""order"" DROP COLUMN city_id;");
            Execute.Sql(@"ALTER TABLE public.order_info DROP COLUMN city_id;");
            Execute.Sql(@"ALTER TABLE public.order_info DROP COLUMN city_id;");
            Execute.Sql(@"ALTER TABLE public.address ALTER COLUMN city_id DROP DEFAULT");
            Execute.Sql(@"ALTER TABLE public.address ALTER COLUMN city_id DROP NOT NULL");
            Execute.Sql(@"ALTER TABLE public.city ALTER COLUMN is_deleted DROP DEFAULT");
            Execute.Sql(@"ALTER TABLE public.city DROP COLUMN is_deleted;");
        }
    }
}
