using Food.Services.Tests.Context;
using ITWebNet.FoodService.Food.DbAccessor;

namespace Food.Services.Tests.Tools
{
    public static class AccessorManager
    {
        public static ITWebNet.FoodService.Food.DbAccessor.Accessor Accessor;
        public static FakeContext Context;
        static AccessorManager()
        {
            Accessor = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance;
            Context = new FakeContext();
            ContextManager.Set(Context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(Context);
        }
    }
}
