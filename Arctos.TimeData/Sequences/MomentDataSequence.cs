using System;

namespace Arctos.TimeData
{
    public class MomentDataSequence<TData> : Sequence<TData>
      where TData : class, IMoment, new()
    {
        //---------------------------------------------------------------------
        #region SequenceData Overrides

        public override void Add(TData? item)
        {
            if (item == null)
                return;
            if (InternalData.Count == 0)
            {
                InternalData.Add(item);
                ChangeTime(new Period(item.Time, TimeSpan.Zero));
                return;
            }

            //todo: this may cause problems with binary search was '<='
            if (item.Time < EndTime)
                throw new ArgumentException("Item time must be at or after EndTime.");
            InternalData.Add(item);
            EndTime = item.Time;
        }

        protected override BeforeAtAfterIndices? GetBeforeAtAfterIndices(DateTime time)
        {
            var results = new BeforeAtAfterIndices();

            if (InternalData.Count == 0)
                return null;

            if (InternalData[0] is IMoment first)
            {
                if (time <= first.Time)
                {
                    if (time == first.Time)
                    {
                        results.At = 0;
                        if (InternalData.Count > 1)
                            results.After = 1;
                    }
                    else
                        results.After = 0;

                    return results;
                }
            }

            if(InternalData[^1] is IMoment last)
            {
                if (time >= last.Time)
                {
                    if (time == last.Time)
                    {
                        results.At = InternalData.Count - 1;
                        if (InternalData.Count > 1)
                            results.Before = InternalData.Count - 2;
                    }
                    else
                        results.Before = InternalData.Count - 1;
                    return results;
                }
            }

            var index = InternalData.BinarySearch(new TData { Time = time });
            if (index < 0)
            {
                var nextIndex = ~index;
                if (nextIndex == InternalData.Count)
                    return results;
                results.After = nextIndex;
                results.Before = nextIndex > 1 ? nextIndex - 1 : -1;
                return results;
            }
            results.Before = index - 1 >= 0 ? index - 1 : -1;
            results.At = index;
            results.After = index + 1 <= InternalData.Count - 1 ? index + 1 : -1;
            return results;
        }

        public static MomentDataSequence<TData> Empty(IPeriod period)
        {
            var empty = new MomentDataSequence<TData>();
            empty.ChangeTime(period);
            return empty;

        }

        public override void ToUniversalTime()
        {
            base.ToUniversalTime();
            foreach (var data in InternalData)
            {
                if (data != null) 
                    data.Time = data.Time.ToUniversalTime();
            }
        }

        public override void ToLocalTime()
        {
            base.ToLocalTime();
            foreach (var data in InternalData)
            {
                if (data != null) 
                    data.Time = data.Time.ToLocalTime();
            }
        }

        #endregion SequenceData Overrides

    }
}
