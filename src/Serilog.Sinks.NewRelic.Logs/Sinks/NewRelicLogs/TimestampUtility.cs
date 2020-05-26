using System;

namespace Serilog.Sinks.NewRelic.Logs.Sinks.NewRelicLogs
{
    public static class TimestampUtility
    {
        public static long ToUnixTimestamp(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            if (date == DateTime.MinValue)
            {
                return 0;
            }

            return (long)(date - epoch).TotalMilliseconds;
        }
    }
}
