using System;
using System.Collections;
using System.Collections.Generic;

namespace Arctos.TimeData
{
    public interface ISequence<T> : IPeriod, ICollection<T>
    {
        bool IsEmpty { get; }

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        T? FindByTime(DateTime time, Relationship relationship);

        void ForPeriod(IPeriod period, Topology topology, Action<object?> action);

        void Trim(IPeriod period);

        IEnumerable<T?>? Range(IPeriod timeRange, Topology topology);

        #endregion Methods
    }
}