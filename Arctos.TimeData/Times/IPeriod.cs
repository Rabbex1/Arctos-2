using System;

namespace Arctos.TimeData
{
    public interface IPeriod
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        TimeSpan Duration { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        IPeriod? Sum(IPeriod other);

        IPeriod?[]? Difference(IPeriod other);

        IPeriod? Intersection(IPeriod other);

        void Unite(IPeriod other);

        bool IsBefore(IPeriod other);

        bool IsBefore(DateTime time);

        bool IsAfter(IPeriod? other);

        bool IsAfter(DateTime time);

        bool IsLeading(IPeriod other);

        bool IsLeading(DateTime time);

        bool IsTrailing(IPeriod? other);

        bool IsTrailing(DateTime time);

        bool IsEncompassedBy(IPeriod other);

        bool Encompasses(IPeriod other);

        bool Encompasses(DateTime time);

        bool Intersects(IPeriod other);

        bool IntersectsStart(IPeriod other);

        bool IntersectsEnd(IPeriod other);

        IPeriod Scale(double factor);

        Period? GetGap(DateTime time);

        Period? GetGap(IPeriod other);

        void ChangeTime(IPeriod other);

        #endregion Methods

    }
}