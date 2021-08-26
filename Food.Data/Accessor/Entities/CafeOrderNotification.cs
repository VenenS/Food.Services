using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Get caffee id with new unwatched orders for user
        /// </summary>
        /// <param name="userId">Manager of caffee id</param>
        /// <returns></returns>
        public long GetCafeIdWithNewOrders(long userId)
        {
            using (var context = GetContext())
            {
                return context.CafeOrderNotifications.AsNoTracking().FirstOrDefault(e => e.UserId == userId && e.DeliverDate <= DateTime.Now)?.CafeId ?? -1;
            }
        }

        /// <summary>
        /// User doesn't want to be notified about orders that already done
        /// </summary>
        /// <param name="userId">Identifier of a cafe manager</param>
        public void StopNotifyUser(long userId)
        {
            using (var context = GetContext())
            {
                var lastNotification = context.CafeOrderNotifications.FirstOrDefault(e => e.UserId == userId && e.DeliverDate <= DateTime.Now);
                if (lastNotification == null) return;
                var notifications = context.CafeOrderNotifications.Where(e => e.CafeId == lastNotification.CafeId)
                    .Where(e => e.UserId == userId && e.DeliverDate <= DateTime.Now).ToList();
                context.CafeOrderNotifications.RemoveRange(notifications);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// One of the managers had already watched the orders of the caffee, remove unneccessary notifications
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        public void SetOrdersOfCafeViewed(long cafeId)
        {
            using (var context = GetContext())
            {
                var notifications = context.CafeOrderNotifications.Where(e => e.CafeId == cafeId && e.DeliverDate <= DateTime.Now).ToList();
                if(!notifications.Any()) return;
                context.CafeOrderNotifications.RemoveRange(notifications);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// New caffe order was created, all managers should know about that!
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        /// <param name="deliveryDate"></param>
        public List<string> NewOrderForNotification(long cafeId, DateTime deliveryDate)
        {
            using (var context = GetContext())
            {
                try
                {
                    var listOfNames = new List<string>();
                    var managers = context.CafeManagers.Where(e => !e.IsDeleted && e.CafeId == cafeId).ToList();
                    foreach (var manager in managers)
                    {
                        listOfNames.Add(manager.User.Name);
                        context.CafeOrderNotifications.Add(new CafeOrderNotification()
                        {
                            CafeId = cafeId, UserId = manager.UserId, DeliverDate = deliveryDate
                        });
                    }
                    context.SaveChanges();
                    return listOfNames;
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
        }
    }
}
