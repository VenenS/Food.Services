using Food.Data.Entities;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IOrderInfo
    {
        bool PostOrderInfo(OrderInfo orderInfo);
    }
}
