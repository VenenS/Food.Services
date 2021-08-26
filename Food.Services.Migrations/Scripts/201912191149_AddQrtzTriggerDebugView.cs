using FluentMigrator;

namespace Food.Services.Migrations.Scripts
{
    [Migration(201912191149, "View для помощи в отладке задач в планировщике")]
    public class _201912191149_AddQuartzTriggerDebugView : Migration
    {
        public override void Up()
        {
            Execute.Sql(
                @"CREATE OR REPLACE VIEW public.v_qrtz_trigger_schedule AS
                  SELECT job_name,
                     trigger_name,
                     to_timestamp(next_fire_time / 10000000 - 62135596800) + '3h' :: interval next_fire_time_msk,
                     to_timestamp(next_fire_time / 10000000 - 62135596800) next_fire_time_utc,
                     to_timestamp(prev_fire_time / 10000000 - 62135596800) + '3h' :: interval prev_fire_time_msk,
                     to_timestamp(prev_fire_time / 10000000 - 62135596800) prev_fire_time_utc
                  FROM public.qrtz_triggers
                  ORDER BY trigger_name;"
            );
        }

        public override void Down()
        {
            Execute.Sql("DROP VIEW IF EXISTS v_qrtz_trigger_schedule;");
        }
    }
}
