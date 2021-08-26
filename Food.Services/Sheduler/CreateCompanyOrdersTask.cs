using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services
{
    /// <summary>
    /// Отвечает за создание компанейских заказов по расписаниям кураторов
    /// </summary>
    class CreateCompanyOrdersTask
    {
        /// Cоздание компанейских заказов в полночь по расписаниям кураторов
        /// и создание задачи на следующий день в sheduled_tasks
        public async Task Run()
        {
            try
            {
                var companyOrdersShedule = Accessor.Instance.GetAllCompanyOrderScheduleByDate(DateTime.Now);
                //TODO: поправить дату в GetAllCompanyOrdersGreaterDate
                var companyOrders = Accessor.Instance.GetAllCompanyOrdersGreaterDate(DateTime.Now);
                foreach (var itemShedule in companyOrdersShedule)
                {
                    var date = DateTime.Now;
                    if (itemShedule.Cafe.WeekMenuIsActive)
                    {
                        //если вкл. заказы на неделю, то создаем 7 компанейских заказов
                        for (var day = 0; day < 7; day++)
                        {
                            date = date.AddDays(day == 0 ? 0 : 1);
                            //проверяем существует ли на дату компанейский заказ
                            var companyOrderExists = companyOrders.FirstOrDefault(
                                o => itemShedule.CompanyId == o.CompanyId
                                && itemShedule.CafeId == o.CafeId
                                && o.OpenDate.Value.Date.ToShortDateString() == date.Date.ToShortDateString());
                            if (companyOrderExists != null)
                            {
                                //если существует, то меняем время и адрес доставки
                                EditCompanyOrder(companyOrderExists, date, itemShedule);
                            }
                            else
                            {
                                //если не существует, то создаем
                                AddCompanyOrder(date, itemShedule);
                            }
                        }
                    }
                    else
                    {
                        //создаем на сегодня
                        //проверяем существует ли на сегодня компанейский заказ
                        var companyOrderExists = companyOrders.FirstOrDefault(
                            o => itemShedule.CompanyId == o.CompanyId
                            && itemShedule.CafeId == o.CafeId
                            && o.OpenDate.Value.Date.ToShortDateString() == date.Date.ToShortDateString());

                        if (companyOrderExists != null)
                        {
                            //если существует, то меняем время и адрес доставки
                            EditCompanyOrder(companyOrderExists, date, itemShedule);
                        }
                        else
                        {
                            //если не существует, то создаем
                            AddCompanyOrder(date, itemShedule);
                        }
                    }
                }

                //Удаляем задачу по созданию корп. заказов на сегодня и создаем на следующий день
                var task = Accessor.Instance.GetTaskCreateCompanyOrderByDate();
                if (task != null)
                    Accessor.Instance.RemoveTask(task.Id);
                else
                {
                    //если на сегодня не создана задача, то создадим,
                    //чтобы scheduleTimerCheckStatusTask не запустил создание корп. заказов снова
                    Accessor.Instance.AddTask(new Data.Entities.ScheduledTask()
                    {
                        CreateDate = DateTime.Now,
                        CreatorId = 0,
                        ScheduledExecutionTime = DateTime.Now.Date,
                        IsRepeatable = 1,
                        IsDeleted = true,
                        TaskType = EnumScheduledTaskType.CreateCompanyOrders
                    });
                }

                //смотрим, есть ли на завтра задача
                var taskNextDay = Accessor.Instance.GetTaskCreateCompanyOrderByDate(DateTime.Now.AddDays(1).Date);
                if (taskNextDay == null || taskNextDay.IsDeleted)
                    Accessor.Instance.AddTask(new Data.Entities.ScheduledTask()
                    {
                        CreateDate = DateTime.Now,
                        CreatorId = 0,
                        ScheduledExecutionTime = DateTime.Now.AddDays(1).Date,
                        IsRepeatable = 1,
                        IsDeleted = false,
                        TaskType = EnumScheduledTaskType.CreateCompanyOrders
                    });
            }
            catch (Exception e)
            {
                Accessor.Instance.WriteShedulerError(e.Message + " " + e.StackTrace);
            }
        }

        /// <summary>
        /// Обновление компанейского заказа
        /// </summary>
        void EditCompanyOrder(CompanyOrder companyOrder, DateTime date, CompanyOrderSchedule companyOrderShedule)
        {
            companyOrder.OpenDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderStartTime);
            companyOrder.AutoCloseDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderStopTime);
            companyOrder.DeliveryDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderSendTime);
            companyOrder.DeliveryAddress = companyOrderShedule.CompanyDeliveryAdress;
            companyOrder.LastUpdate = DateTime.Now;
            Accessor.Instance.EditCompanyOrder(companyOrder);
        }

        /// <summary>
        /// Добавление компанейского заказа
        /// </summary>
        void AddCompanyOrder(DateTime date, CompanyOrderSchedule companyOrderShedule)
        {
            Accessor.Instance.AddCompanyOrder(new CompanyOrder()
            {
                AutoCloseDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderStopTime),
                CafeId = companyOrderShedule.CafeId,
                CompanyId = companyOrderShedule.CompanyId,
                CreationDate = DateTime.Now,
                CreatorId = 0,
                TotalPrice = 0,
                DeliveryAddress = companyOrderShedule.CompanyDeliveryAdress,
                DeliveryDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderSendTime),
                OpenDate = DateTime.Parse(date.Date.ToShortDateString() + " " + companyOrderShedule.OrderStartTime),
                OrderCreateDate = DateTime.Now,
                State = 0,
            });
        }
    }
}