using Food.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services
{
    public class CheckUserInBasePermission : IPermission
    {
        public virtual void Demand()
        {
            var user = Thread.CurrentPrincipal.Identity.GetUserById();

            if (
                user.Lockout
                && user.LockoutEnddate.HasValue
                && user.LockoutEnddate > DateTime.Now
                || user.IsDeleted
                )
            {
                throw new SecurityException("Пользователь заблокирован");
            }
        }

        public virtual IPermission Intersect(IPermission target)
        {
            if (target == null) return null;

            return (CheckUserInBasePermission)Clone();
        }

        public virtual bool IsSubsetOf(IPermission target)
        {
            if (target == null) return false;

            return true;
        }

        public virtual IPermission Copy()
        {
            return (IPermission)Clone();
        }

        public virtual IPermission Union(IPermission target)
        {
            if (target == null) return Copy();

            return (IPermission)Clone();
        }

        public virtual void FromXml(SecurityElement e)
        {
        }

        public virtual SecurityElement ToXml()
        {
            var e = new SecurityElement("IPermission");
            e.AddAttribute("class", GetType().AssemblyQualifiedName.Replace('\"', '\''));
            e.AddAttribute("version", "1");

            return e;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
