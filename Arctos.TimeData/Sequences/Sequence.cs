using System;
using System.Collections;
using System.Collections.Generic;

namespace Arctos.TimeData
{
    public abstract class Sequence<T> : Period, ISequence<T>
      where T : class, new()
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected readonly List<T?> InternalData = new();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public bool IsEmpty => InternalData.Count == 0 || StartTime == DateTime.MinValue && Duration == TimeSpan.Zero;

        public int Count => InternalData.Count;

        public bool IsReadOnly => false;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Classes

        protected class BeforeAtAfterIndices
        {
            public int Before { get; set; } = -1;
            public int At { get; set; } = -1;
            public int After { get; set; } = -1;
        }

        protected class StartEndIndices
        {
            public int Start { get; set; } = -1;
            public int End { get; set; } = -1;
        }

        #endregion Classes

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public T? this[int index] => InternalData[index];

        public void AddRange(Sequence<T>? range)
        {
            if (range == null || range.Count == 0)
                return;
            if (InternalData.Count == 0)
            {
                InternalData.AddRange(range);
                ChangeTime(range);
                return;
            }
            if (!range.IsAfter(EndTime))
                throw new ArgumentException("Appended block must start after EndTime.");
            InternalData.AddRange(range);
            EndTime = range.EndTime;
        }

        public int IndexOf(T item)
        {
            return InternalData.IndexOf(item);
        }

        public ISequence<T?>? Copy()
        {
            var copy = Activator.CreateInstance(GetType()) as ISequence<T?>;
            foreach (var item in InternalData)
                copy?.Add(item);
            return copy;
        }

        public static ISequence<T?>? Empty(Period period)
        {
            var sequenceType = typeof(Sequence<>).MakeGenericType(typeof(T));
            var empty = Activator.CreateInstance(sequenceType) as ISequence<T?>;
            empty?.ChangeTime(period);
            return empty;
        }


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region ISequence Methods

        public virtual T? FindByTime(DateTime time, Relationship relationship)
        {
            var indices = GetBeforeAtAfterIndices(time);
            if (indices != null)
            {
                return relationship switch
                {
                    Relationship.Before => InternalData[indices.Before],
                    Relationship.BeforeOrAt => InternalData[indices.At] ?? InternalData[indices.Before],
                    Relationship.At => InternalData[indices.At],
                    Relationship.AtOrAfter => InternalData[indices.At] ?? InternalData[indices.After],
                    Relationship.After => InternalData[indices.After],
                    Relationship.Unknown => throw new ArgumentException("Time relationship cannot be 'Unknown'."),
                    _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null)
                };
            }

            return null;
        }

        public virtual void ForPeriod(IPeriod period, Topology topology, Action<object?> action)
        {
            var indices = GetStartEndIndices(period, topology);
            if (indices == null)
                return;

            if (indices.Start == -1 || indices.End == -1)
                return;
            var index = indices.Start;
            while (index <= indices.End)
            {
                action.Invoke(InternalData[index]);
                index++;
            }
        }

        public virtual IEnumerable<T?>? Range(IPeriod period, Topology topology)
        {
            var indices = GetStartEndIndices(period, topology);
            if (indices == null)
                return null;
            var results = new List<T?>();
            var index = indices.Start;
            while (index <= indices.End)
            {
                results.Add(InternalData[index]);
                index++;
            }
            return results;
        }

        public virtual void Trim(IPeriod period)
        {
            var remove = new List<T>();
            var index = 0;

            if (typeof(IMoment).IsAssignableFrom(typeof(T)))
            {
                var current = InternalData[index] ;
                while (current != null && ((IMoment) current).Time < period.StartTime)
                {
                    remove.Add(current);
                    index++;
                    current = InternalData[index];
                }

                index = InternalData.Count - 1;
                current = InternalData[index];
                while (current != null && ((IMoment)current).Time > period.EndTime)
                {
                    remove.Add(current);
                    index--;
                    current = InternalData[index];
                }
                foreach (var old in remove)
                    InternalData.Remove(old);

                if (InternalData.Count == 0)
                {
                    StartTime = DateTime.MinValue;
                    EndTime = DateTime.MinValue;
                    return;
                }

                var first = InternalData[0] as IMoment;
                StartTime = first?.Time ?? DateTime.MinValue;
                var last = InternalData[^1] as IMoment;
                EndTime = last?.Time ?? DateTime.MinValue;
            }
        }


        #endregion ISequence Methods

        //---------------------------------------------------------------------
        #region ICollection Methods

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InternalData.GetEnumerator();
        }

        public abstract void Add(T item);

        public void Clear()
        {
            InternalData.Clear();
        }

        public bool Contains(T item)
        {
            return InternalData.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            InternalData.CopyTo(array, arrayIndex);
        }

        public bool Remove(T? item)
        {
            return InternalData.Remove(item);
            //todo should adjust time
        }

        #endregion ICollection Methods

        //---------------------------------------------------------------------
        #region Protected methods

        protected abstract BeforeAtAfterIndices? GetBeforeAtAfterIndices(DateTime time);

        protected StartEndIndices? GetStartEndIndices(IPeriod period, Topology topology)
        {
            if (InternalData.Count == 0)
                return null;
            var results = new StartEndIndices();
            if (topology == Topology.Coverage)
            {
                var indices = GetBeforeAtAfterIndices(period.StartTime);
                if(indices != null)
                    results.Start = indices.Before != -1 ? indices.Before : indices.At != -1 ? indices.At : indices.After;

                indices = GetBeforeAtAfterIndices(period.EndTime);
                if(indices != null)
                    results.End = indices.After != -1 ? indices.After : indices.At != -1 ? indices.At : indices.Before;
            }
            else
            {
                var indices = GetBeforeAtAfterIndices(period.StartTime);
                if (indices != null)
                    results.Start = indices.At != -1 ? indices.At : indices.After;

                indices = GetBeforeAtAfterIndices(period.EndTime);
                if (indices != null)
                    results.End = indices.At != -1 ? indices.At : indices.Before;

                if (results.Start == -1 || results.End == -1)
                    return null;
            }
            return results;
        }

        //object? ISequence.FindByTime(DateTime time, Relationship relationship)
        //{
        //    return FindByTime(time, relationship);
        //}

        //IEnumerable? ISequence.Range(IPeriod timeRange, Topology topology)
        //{
        //    return Range(timeRange, topology);
        //}

        #endregion Protected methods

        #endregion Methods

    }
}
