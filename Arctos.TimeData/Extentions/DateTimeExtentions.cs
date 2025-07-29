using System.Globalization;

namespace Arctos.TimeData
{
    public static class DateTimeExtenstion
    {
        public static bool IsBefore(this DateTime dateTime, Period period)
        {
            return dateTime < period.StartTime;
        }

        public static bool IsAfter(this DateTime dateTime, Period period)
        {
            return dateTime > period.EndTime;
        }
    }
}