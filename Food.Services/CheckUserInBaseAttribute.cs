using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Security;
using System.Threading.Tasks;

namespace Food.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class CheckUserInBaseAttribute : CodeAccessSecurityAttribute
    {
        public CheckUserInBaseAttribute(SecurityAction action)
            : base(action)
        {
        }

        public override IPermission CreatePermission()
        {
            return new CheckUserInBasePermission();
        }
    }
}
