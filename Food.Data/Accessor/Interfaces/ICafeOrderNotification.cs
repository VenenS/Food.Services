using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafeOrderNotification
    {
        /// <summary>
        /// Get caffee id with new unwatched orders for user
        /// </summary>
        /// <param name="userId">Manager of caffee id</param>
        /// <returns></returns>
        long GetCafeIdWithNewOrders(long userId);

        /// <summary>
        /// User doesn't want to be notified about orders that already done
        /// </summary>
        /// <param name="userId">Identifier of a cafe manager</param>
        void StopNotifyUser(long userId);

        /// <summary>
        /// One of the managers had already watched the orders of the caffee, remove unneccessary notifications
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        void SetOrdersOfCafeViewed(long cafeId);

        /// <summary>
        /// New caffe order was created, all managers should know about that!
        /// </summary>
        /// <param name="cafeId">Caffee identifier</param>
        /// <param name="deliveryDate"></param>
        List<string> NewOrderForNotification(long cafeId, DateTime deliveryDate);
    }
}
