using Food.Data.Entities;
using System;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        public bool PostOrderInfo(OrderInfo orderInfo)
        {
            try
            {
                using (var fc = GetContext())
                {
                    orderInfo.CreateDate = DateTime.Now;

                    fc.OrderInfo.Add(orderInfo);
                    fc.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
