using System;
using System.Collections.Generic;
using System.Linq;

namespace Arctos.TimeData
{
    public class CompoundSequence<TSequence, TData> : PeriodicSequence<TSequence>
        where TSequence : class, ISequence<TData>, new()
    {
        public override void ForPeriod(IPeriod period, Topology topology, Action<object?> action)
        {
            var indices = GetStartEndIndices(period, topology);
            if (indices == null)
                return;
            var index = indices.Start;
            while (index <= indices.End)
            {
                InternalData[index]?.ForPeriod(period, topology, action);
                index++;
            }
        }

        public IEnumerable<TData?>? DataRange(IPeriod period, Topology topology)
        {
            var indices = GetStartEndIndices(period, topology);
            if (indices == null)
                return null;
            var result = new List<TData?>();
            var index = indices.Start;
            while (index <= indices.End)
            {
                if (InternalData[index] is { } sequence)
                {
                    var range = sequence.Range(period, topology);
                    if (range != null)
                        result.AddRange(range);
                }
                index++;
            }
            return result;
        }

        public override void Trim(IPeriod period)
        {
            var removable = new List<TSequence?>();
            removable.AddRange(InternalData.Where(s => s != null && s.IsBefore(period.StartTime)));
            removable.AddRange(InternalData.Where(s => s != null && s.IsAfter(period.EndTime)));
            foreach (var old in removable)
                Remove(old);

            var trimmable = InternalData.Where(s => s != null && s.Intersects(period)).ToList();
            foreach (var sequence in trimmable)
                sequence?.Trim(period);

            if (InternalData.Count == 0)
            {
                StartTime = DateTime.MinValue;
                EndTime = DateTime.MinValue;
                return;
            }
            StartTime = InternalData[0]?.StartTime ?? DateTime.MinValue;
            EndTime = InternalData[^1]?.EndTime ?? DateTime.MinValue;
        }
    }
}
