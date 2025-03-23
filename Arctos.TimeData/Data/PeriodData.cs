using System;

namespace Arctos.TimeData
{
    public class PeriodData<T> : Period, IPeriodData<T>
    {
        public T Value { get; }

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public PeriodData(T value)
        {
            Value = value;
        }

        public PeriodData(DateTime start, DateTime end, T value)
            : base(start, end)
        {
            Value = value;
        }

        public PeriodData(DateTime start, TimeSpan span, T value)
            : base(start, span)
        {
            Value = value;
        }

        public PeriodData(TimeSpan span, DateTime end, T value)
            : base(span, end)
        {
            Value = value;
        }

        #endregion Constructors

    }
}