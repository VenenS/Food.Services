using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public sealed class AllowNullAttribute : Attribute
    {

    }
}
