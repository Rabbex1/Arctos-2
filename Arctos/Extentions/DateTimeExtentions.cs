using System.Globalization;

namespace Arctos
{
    public static class DateTimeExtenstion
    {
        public static DateTime GetStartOfWeek(this DateTime dt)
        {
            DateTime ndt = dt.Subtract(TimeSpan.FromDays((int)dt.DayOfWeek));
            return new DateTime(ndt.Year, ndt.Month, ndt.Day, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfWeek(this DateTime dt, int year, int week)
        {
            DateTime dateTime = new DateTime(year, 1, 1);
            while (dateTime.DayOfWeekName() != "Sunday")
                dateTime = dateTime.AddDays(1);
            return new DateTime(dateTime.Ticks + (TimeSpan.FromDays(7).Ticks * (week - 1)));
        }

        public static DateTime FirstDayOfOfWeek(this DateTime dt)
        {
            DateTime ndt = dt.GetStartOfWeek().AddDays(6);
            return new DateTime(ndt.Year, ndt.Month, ndt.Day);
        }

        public static DateTime GetEndOfWeek(this DateTime dt, int year, int week)
        {
            return new DateTime(dt.GetStartOfWeek(year, week).Ticks + (TimeSpan.FromDays(7).Ticks - 1));
        }

        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var delta = (d.Ticks - (dt.Ticks % d.Ticks)) % d.Ticks;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;

            return roundUp ? dt.RoundUp(d) : dt.RoundDown(d);
        }

        public static string MonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ShortMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        public static string DayOfWeekName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)dateTime.DayOfWeek];
        }

        public static string ShortDayOfWeekName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[(int)dateTime.DayOfWeek];
        }

        public static int Week(this DateTime dateTime)
        {
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
        }

        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime FirstDayOfYear(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1);
        }

        public static DateTime FirstDayOfDecade(this DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public static DateTime FirstDayOfCentury(this DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public static long ToJsTimeStamp(this DateTime dateTime)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = dateTime.Ticks - origin.Ticks;
            return diff / 10000000;
        }
        public static DateTime FromJsTimeStamp(this DateTime dateTime, long timeStamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var result =  origin.AddSeconds(timeStamp);
            return result;
        }


    }
}