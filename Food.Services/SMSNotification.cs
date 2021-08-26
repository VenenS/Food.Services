using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services
{
    public class SmsNotification : NotificationBase
    {
        public override void FormNotification(NotificationBodyBase notification)
        {
            throw new NotImplementedException();
        }

        public override Task SendNotificationCurator((Dictionary<string, string>, string) address)
        {
            throw new NotImplementedException();
        }

        protected override Task SendNotification()
        {
            throw new NotImplementedException();
        }
    }
}
