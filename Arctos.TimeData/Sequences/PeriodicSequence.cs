using System;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace Arctos.TimeData
{
    public class PeriodicSequence<TPeriod> : Sequence<TPeriod>, IPeriodicSequence<TPeriod>
      where TPeriod : class, IPeriod, new()
    {

        //---------------------------------------------------------------------
        #region Methods

        public PeriodicSequence<Period> Gaps()
        {
            IPeriod? prevPeriod = null;
            var gaps = new PeriodicSequence<Period>();

            foreach (var period in InternalData)
            {
                if (period == null)
                    continue;
                if (prevPeriod == null)
                {
                    prevPeriod = period;
                    continue;
                }

                if (!prevPeriod.IsLeading(period))
                {
                    var gap = new Period(new DateTime(prevPeriod.EndTime.Ticks + 1), new DateTime(period.StartTime.Ticks - 1));
                    if(gap.Duration != TimeSpan.Zero)
                        gaps.Add(gap);
                }

                prevPeriod = period;
            }
            return gaps;
        }

        public PeriodicSequence<Period> Gaps(Period totalPeriod)
        {
            PeriodicSequence<Period> gaps;

            if (Count == 0)
            {
                gaps = new PeriodicSequence<Period> { totalPeriod };
                return gaps;
            }

            gaps = Gaps();
            if (totalPeriod.StartTime < StartTime)
                gaps.Insert(new Period(totalPeriod.StartTime, new DateTime(StartTime.Ticks - 1)));
            if (totalPeriod.EndTime > EndTime)
                gaps.Add(new Period(new DateTime(EndTime.Ticks + 1), totalPeriod.EndTime));
            return gaps;
        }

        public void Insert(TPeriod item)
        {
            //if currently empty
            if (InternalData.Count == 0)
            {
                ChangeTime(item);
                InternalData.Add(item);
                return;
            }

            if (InternalData.Any(p => p != null && p.Intersects(item)))
                throw new InvalidOperationException("TimeSequence cannot contain overlapping TimePeriods.");

            //if new period is at the end
            if (item.IsTrailing(InternalData.Last()) || item.IsAfter(InternalData.Last()))
            {
                InternalData.Add(item);
                EndTime = item.EndTime;
                return;
            }
            var index = InternalData.BinarySearch(item);
            if (index < 0)
                index = ~index;
            InternalData.Insert(index, item);
            if (index == 0)
                StartTime = item.StartTime;
        }

        public ISequence<TPeriod>? Zip(PeriodicSequence<TPeriod> other)
        {
            var thisType = GetType();
            var otherType = other.GetType();
            if (thisType != otherType)
                throw new ArgumentException("Cannot zip type " + thisType + " with type " + otherType);
            if (Count == 0)
                return other.Copy() as PeriodicSequence<TPeriod>;
            if (other.Count == 0)
                return Copy() as PeriodicSequence<TPeriod>;

            var result = Activator.CreateInstance(thisType) as ISequence<TPeriod>;//()new PeriodicSequence<TPeriod>();
            if (result == null)
                return result;

            using var thisEnumerator = GetEnumerator();
            using var otherEnumerator = other.GetEnumerator();
            var thisDone = false;
            var otherDone = false;
            thisEnumerator.MoveNext();
            otherEnumerator.MoveNext();

            while (true)
            {
                if (thisDone)
                {
                    result.Add(otherEnumerator.Current);
                    if (!otherEnumerator.MoveNext())
                        break;
                }
                else if (otherDone)
                {
                    result.Add(thisEnumerator.Current);
                    if (!thisEnumerator.MoveNext())
                        break;
                }
                else if (thisEnumerator.Current.StartTime < otherEnumerator.Current.StartTime)
                {
                    result.Add(thisEnumerator.Current);
                    if (!thisEnumerator.MoveNext())
                        thisDone = true;
                }
                else
                {
                    result.Add(otherEnumerator.Current);
                    if (!otherEnumerator.MoveNext())
                        otherDone = true;
                }
            }

            return result;
        }

        public override void Trim(IPeriod item)
        {
            var discard = new List<TPeriod>();

            if (StartTime > item.StartTime && StartTime < item.EndTime)
                StartTime = item.EndTime.AddTicks(1);
            else if (EndTime > item.StartTime && EndTime < item.EndTime)
                EndTime = item.StartTime.AddTicks(-1);

            foreach (var data in this)
            {
                if(data.IsBefore(StartTime) ||  data.IsAfter(EndTime))
                    discard.Add(data);
            }

            foreach (var data in discard)
                Remove(data);
        }

        #endregion Methods

        //---------------------------------------------------------------------
        #region Sequence Overrides

        public override void Add(TPeriod item)
        {
            if (InternalData.Count == 0)
            {
                InternalData.Add(item);
                ChangeTime(item);
                return;
            }

            var startIndices = GetBeforeAtAfterIndices(item.StartTime);
            var endIndices = GetBeforeAtAfterIndices(item.EndTime);

            var overlapped = InternalData.Where(d => d is not null && d.Intersects(item));
            if (overlapped.Any())
                if (startIndices.At != -1 || endIndices.At != -1)
                    throw new InvalidOperationException("Cannot add data as it overlaps with existing data");

            if (startIndices.After != endIndices.After || startIndices.Before != endIndices.Before)
                throw new InvalidOperationException("WTF in PeriodicSequence.Add");
            var index = startIndices.After == -1 ? startIndices.Before + 1 : startIndices.After;

            InternalData.Insert(index, item);
            Unite(item);
        }

        protected override BeforeAtAfterIndices GetBeforeAtAfterIndices(DateTime time)
        {
            var results = new BeforeAtAfterIndices();

            var first = InternalData[0];
            if( first != null )
            {
                if (first.IsAfter(time) || first.Encompasses(time))
                {
                    if (first.Encompasses(time))
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

            var last = InternalData[^1];
            if (last != null)
            {
                if (last.IsBefore(time) || last.Encompasses(time))
                {
                    if (last.Encompasses(time))
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

            var searchItem = new TPeriod();
            searchItem.ChangeTime(new Period(time, TimeSpan.Zero));
            var index = InternalData.BinarySearch(searchItem);
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

        #endregion Sequence Overrides

    }
}
