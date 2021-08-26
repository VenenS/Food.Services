using Food.Data;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    public class ContextableApiController : ControllerBase
    { 
        protected IFoodContext Context;
        protected bool TestMode;
        protected Accessor Accessor;
        protected ILogger Logger;

        public ContextableApiController()
        {
            Logger = Log.Logger; 
        }

        /// <summary>
        ///     Обеспечение автотестирования
        /// </summary>
        /// <returns>IDataContext</returns>
        protected Accessor GetAccessor()
        {
            if (!TestMode) return Accessor.Instance;
            Accessor.SetTestingModeOn(Context);
            return Accessor;
        }
    }
}
