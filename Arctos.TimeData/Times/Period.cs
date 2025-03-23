using System;

namespace Arctos.TimeData
{
    public class Period : IPeriod, IComparable<IPeriod>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Fields

        private readonly Guid _guid = Guid.NewGuid();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        //---------------------------------------------------------------------
        #region IPeriod Properties

        #region StartTime
        protected DateTime _startTime = DateTime.MinValue;
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (value > _endTime)
                    _endTime = value;
                _startTime = value;
                Duration = EndTime - value;
            }
        }
        #endregion

        #region EndTime
        protected DateTime _endTime = DateTime.MaxValue;
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                if (value < StartTime)
                    StartTime = value;
                _endTime = value;
                Duration = value - StartTime;
            }
        }
        #endregion

        public TimeSpan Duration { get; protected set; }

        #endregion IPeriod Properties

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public Period()
        {
        }

        public Period(DateTime start, DateTime end)
        {
            //set backing fields to avoid exception
            _startTime = start;
            _endTime = end;
            Duration = end - start;
        }

        public Period(DateTime start, TimeSpan duration)
        {
            //set backing fields to avoid exception
            _startTime = start;
            Duration = duration;
            _endTime = start + duration;
        }

        public Period(TimeSpan duration, DateTime end)
        {
            //set backing fields to avoid exception
            _startTime = end - duration;
            Duration = duration;
            _endTime = end;
        }

        public Period(IPeriod other)
          : this()
        {
            //set backing fields to avoid exception
            _startTime = new DateTime(other.StartTime.Ticks);
            _endTime = new DateTime(other.EndTime.Ticks);
            Duration = EndTime - StartTime;
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region IPeriod Methods

        public IPeriod? Sum(IPeriod other)
        {
            if (Intersects(other))
                return new Period
                (
                  StartTime < other.StartTime ? StartTime : other.StartTime,
                  EndTime > other.EndTime ? EndTime : other.EndTime
                );
            if (IsTrailing(other))
                return new Period(other.StartTime, EndTime);
            return IsLeading(other) ? new Period(StartTime, other.EndTime) : null;
        }

        public IPeriod?[]? Difference(IPeriod other)
        {
            if (!Intersects(other) || Equals(other))
                return null;

            var result = new IPeriod?[2];
            if (Encompasses(other))
            {
                if (StartTime == other.StartTime)
                {
                    result[0] = new Period(new DateTime(other.EndTime.Ticks + 1), EndTime);
                    result[1] = null;
                    return result;
                }
                if (EndTime == other.EndTime)
                {
                    result[0] = new Period(StartTime, new DateTime(other.StartTime.Ticks - 1));
                    result[1] = null;
                    return result;
                }
                result[0] = new Period(StartTime, new DateTime(other.StartTime.Ticks - 1));
                result[1] = new Period(new DateTime(other.EndTime.Ticks + 1), EndTime);
                return result;

            }

            if (IntersectsEnd(other))
            {
                result[0] = result[0] = EndTime == other.EndTime
                  ? null
                  : new Period(new DateTime(other.EndTime.Ticks + 1), EndTime);
                result[1] = null;
                return result[0] == null ? null : result;
            }
            if (IntersectsStart(other))
            {
                result[0] = StartTime == other.StartTime
                  ? null
                  : new Period(StartTime, new DateTime(other.StartTime.Ticks - 1));
                result[1] = null;
                return result[0] == null ? null : result;
            }
            return null;
        }

        public IPeriod? Intersection(IPeriod other)
        {
            if (!Intersects(other))
                return null;
            if (Equals(other))
                return this;

            if (Encompasses(other))
                return other;
            if (IntersectsStart(other))
                return new Period(other.StartTime, EndTime);
            if (IntersectsEnd(other))
                return new Period(StartTime, other.EndTime);
            return IsEncompassedBy(other) ? this : null;
        }

        public void Unite(IPeriod other)
        {
            if (other.EndTime > EndTime)
                EndTime = other.EndTime;
            if (other.StartTime < StartTime || StartTime == DateTime.MinValue)
                StartTime = other.StartTime;
            Duration = EndTime - StartTime;
        }

        public bool IsBefore(IPeriod other)
        {
            return EndTime < other.StartTime;
        }

        public bool IsBefore(DateTime time)
        {
            return EndTime < time;
        }

        public bool IsAfter(IPeriod? other)
        {
            return StartTime > other?.EndTime;
        }

        public bool IsAfter(DateTime time)
        {
            return time < StartTime;
        }

        public bool IsLeading(IPeriod other)
        {
            return EndTime.Ticks == other.StartTime.Ticks - 1;
        }

        public bool IsLeading(DateTime time)
        {
            return EndTime.Ticks == time.Ticks - 1;
        }

        public bool IsTrailing(IPeriod? other)
        {
            return StartTime.Ticks == other?.EndTime.Ticks + 1;
        }

        public bool IsTrailing(DateTime time)
        {
            return StartTime.Ticks == time.Ticks + 1;
        }

        public bool IsEncompassedBy(IPeriod other)
        {
            return other.StartTime <= StartTime && other.EndTime >= EndTime;
        }

        public bool Encompasses(IPeriod other)
        {
            return other.StartTime >= StartTime && other.EndTime <= EndTime;
        }

        public bool Encompasses(DateTime time)
        {
            return time >= StartTime && time <= EndTime;
        }

        public bool Intersects(IPeriod other)
        {
            return Encompasses(other.StartTime) || Encompasses(other.EndTime) ||
                   other.Encompasses(StartTime) || other.Encompasses(EndTime);
        }

        public bool IntersectsStart(IPeriod other)
        {
            return Encompasses(other.StartTime) && EndTime < other.EndTime;
        }

        public bool IntersectsEnd(IPeriod other)
        {
            return StartTime > other.StartTime && Encompasses(other.EndTime);
        }

        public Period? GetGap(DateTime time)
        {
            if (time < StartTime)
                return new Period(time, new DateTime(StartTime.Ticks - 1));
            return time > EndTime ? new Period(new DateTime(EndTime.Ticks + 1), time) : null;
        }

        public Period? GetGap(IPeriod other)
        {
            if (IsBefore(other))
                return new Period(new DateTime(EndTime.Ticks + 1), new DateTime(other.StartTime.Ticks - 1));
            return IsAfter(other) ? new Period(new DateTime(other.EndTime.Ticks + 1), new DateTime(StartTime.Ticks - 1)) : null;
        }

        public void ChangeTime(IPeriod other)
        {
            //set backing field to avoid exception
            _startTime = other.StartTime;
            EndTime = other.EndTime;
            Duration = EndTime - StartTime;
        }

        public IPeriod Scale(double factor)
        {
            var newSpan = new TimeSpan((long)(Duration.Ticks * factor));
            return factor > 1.0
              ? new Period(new DateTime(StartTime.Ticks - newSpan.Ticks / 4), newSpan)
              : new Period(new DateTime(StartTime.Ticks + newSpan.Ticks / 2), newSpan);
        }

        public virtual void ToUniversalTime()
        {
            //set backing field to avoid exception
            _startTime = _startTime.ToUniversalTime();
            _endTime = _endTime.ToUniversalTime();
            Duration = EndTime - StartTime;
        }

        public virtual void ToLocalTime()
        {
            //set backing field to avoid exception
            _startTime = _startTime.ToLocalTime();
            _endTime = _endTime.ToLocalTime();
            Duration = EndTime - StartTime;
        }

        protected bool Equals(IPeriod other)
        {
            return _startTime.Equals(other.StartTime) && _endTime.Equals(other.EndTime);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion IPeriod Methods

        //---------------------------------------------------------------------
        #region IComparable Methods

        public int CompareTo(IPeriod? other)
        {
            return StartTime.CompareTo(other?.StartTime);
        }

        #endregion IComparable Methods

        public override bool Equals(object? obj)
        {
            if (obj is not Period other)
                return false;
            return StartTime.Equals(other.StartTime) && EndTime.Equals(other.EndTime);
        }

        #endregion Methods
    }
}
