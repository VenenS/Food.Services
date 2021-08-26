using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FromUnixTime(long seconds)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(seconds);
            return dtDateTime;
        }
    }
}
