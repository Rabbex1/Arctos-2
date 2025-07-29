using System.Windows.Forms.VisualStyles;

namespace Arctos.Wpf.Charts
{
    public class VisualTimeRange : VisualRange<DateTime>
    {
        private readonly long[] _ticksPerRank =
        [
            10,                         //uS
            10000,                      //mS
            10000000,                   //S          
            600000000,                  //m
            36000000000,                //H
            864000000000,               //D  
            6048000000000               //W
        ];

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public VisualTimeRange(DateTime minimum, DateTime maximum)
          : base(minimum, maximum, 0, 10)
        {
        }

        public VisualTimeRange()
          : base(0, 10)
        {
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override double ValueToVisualNormal(DateTime value)
        {
            throw new NotImplementedException();
        }

        public override bool IsValid()
        {
            return !double.IsNaN(VisualScale);
        }

        public override DateTime NextAtRank(DateTime value, int rank)
        {
            if (value == DateTime.MaxValue)
                return value;
            ValidateRank(rank);
            switch (rank)
            {
                case TimeRank.Century:
                    return value.AddYears(100);
                case TimeRank.Decade:
                    return value.AddYears(10);
                case TimeRank.Year:
                    return value.AddYears(1);
                case TimeRank.Month:
                    return value.AddMonths(1);
                case TimeRank.Week:
                    return value.AddDays(7);
                default:
                    var nearest = Round(value, rank);
                    return nearest <= value ? AdjustByRank(nearest, rank, 1) : nearest;
            }
        }

        public override DateTime PreviousAtRank(DateTime value, int rank)
        {
            if (value == DateTime.MinValue)
                return value;
            ValidateRank(rank);
            switch (rank)
            {
                case TimeRank.Century:
                    return value.AddYears(-100);
                case TimeRank.Decade:
                    return value.AddYears(-10);
                case TimeRank.Year:
                    return value.AddYears(-1);
                case TimeRank.Month:
                    return value.AddMonths(-1);
                case TimeRank.Week:
                    return value.AddDays(-7);
                default:
                    var nearest = Round(value, rank);
                    return nearest >= value ? AdjustByRank(nearest, rank, -1) : nearest;
            }

        }

        public override DateTime PreviousAtRankAndFrequency(DateTime value, int rank, double frequency)
        {
            if (value == DateTime.MinValue)
                return value;
            ValidateRank(rank);
            int mod = 0;
            switch (rank)
            {
                case TimeRank.Century:
                    var startOfCentury = value.FirstDayOfCentury();
                    return frequency > 1 ? startOfCentury.AddYears((int)-frequency * 100) : startOfCentury;
                case TimeRank.Decade:
                    var startOfDecade = value.FirstDayOfDecade();
                    return frequency > 1 ? startOfDecade.AddYears((int)-frequency * 10) : startOfDecade;
                case TimeRank.Year:
                    var startOfYear = value.FirstDayOfYear();
                    return frequency > 1 ? startOfYear.AddYears((int)-frequency) : startOfYear;
                case TimeRank.Month:
                    var startOfMonth = value.FirstDayOfMonth();
                    mod = (startOfMonth.Month - 1) % (int)frequency;
                    return frequency > 1 ? startOfMonth.AddMonths(-mod) : startOfMonth;
                case TimeRank.Week:
                    var startOfWeek = value.FirstDayOfOfWeek();
                    return frequency > 1 ? startOfWeek.AddDays(-7 * (int)frequency) : startOfWeek;
                case TimeRank.Day:
                    mod = frequency > 1 ? (value.Day - 1) % (int)frequency : 1;
                    return new DateTime(value.Year, value.Month, value.Day).AddDays(-mod);

                default:
                    var trueFreq = _ticksPerRank[rank] * frequency;
                    return new DateTime((long)(Math.Floor(value.Ticks / trueFreq) * trueFreq));
            }
        }

        public override DateTime Round(DateTime value, int rank)
        {
            switch (rank)
            {
                case 10:
                    break;
                case 9:
                    break;
                case 8:
                    break;
                case 7:
                    break;
                case 6:
                    break;
                default:
                    return new DateTime(value.AddTicks(-(value.Ticks % _ticksPerRank[rank])).Ticks);
            }
            return value; //dummy code
        }

        public override DateTime AdjustByRank(DateTime value, int rank, double frequency)
        {

            if (value == DateTime.MinValue)
                return value;
            ValidateRank(rank);
            switch (rank)
            {
                case TimeRank.Century:
                    return value.AddYears((int)frequency * 100);
                case TimeRank.Decade:
                    return value.AddYears((int)frequency * 10);
                case TimeRank.Year:
                    return value.AddYears((int)frequency);
                case TimeRank.Month:
                    return value.AddMonths((int)frequency);
                case TimeRank.Week:
                    return value.AddDays(7 * (int)frequency);
                case TimeRank.Day:
                    var day = value.AddDays((int) frequency);
                    if (day.Day < (int) frequency)
                        day = day.FirstDayOfMonth();
                    return day;

                default:
                    return value.AddTicks((long)(RankValue(rank).Ticks * frequency));
            }
        }

        public override DateTime RankValue(int rank)
        {
            if (rank > 5)
                throw new ArgumentOutOfRangeException(nameof(rank), @"The rank must be <= 5");
            return new DateTime(_ticksPerRank[rank]);
        }

        public override void SetVisualScale(double length)
        {
            VisualLength = length;
            VisualScale = length / Math.Abs(Maximum.Ticks - Minimum.Ticks);
        }

        public override void Move(double distance)
        {
            var adjust = (long)(distance / VisualScale);
            Minimum = Minimum.AddTicks(adjust);
            Maximum = Maximum.AddTicks(adjust);
        }

        public override void GetMovedMinMax(double distance, out DateTime newMin, out DateTime newMax)
        {
            var adjust = (long)(distance / VisualScale);
            newMin = Minimum.AddTicks(adjust);
            newMax = Maximum.AddTicks(adjust);
        }

        public override void Zoom(double value)
        {
            var adjust = (long)((Maximum.Ticks - Minimum.Ticks) * (value / 100.0));
            Minimum = Minimum.AddTicks(adjust);
            Maximum = Maximum.AddTicks(-adjust);
            VisualScale = VisualLength / Math.Abs(Maximum.Ticks - Minimum.Ticks);
        }

        public override void GetZoomedMinMax(double value, out DateTime newMin, out DateTime newMax)
        {
            var adjust = (long)((Maximum.Ticks - Minimum.Ticks) * (value / 100.0));
            newMin = Minimum.AddTicks(adjust);
            newMax = Maximum.AddTicks(-adjust);
        }

        public override string ParseValue(DateTime value, string format)
        {
            if (format.Contains("|"))
            {
                var fmts = format.Split('|');
                var result = "";
                var i = 0;
                for (; i < fmts.Length - 1; i++)
                {
                    if (string.IsNullOrEmpty(fmts[i]))
                        continue;
                    result += value.ToString(fmts[i]) + Environment.NewLine;
                }
                result += string.IsNullOrEmpty(fmts[i]) ? "" : value.ToString(fmts[i]);
                return result;
            }
            return value.ToString(format);
        }

        public override int Closest(DateTime value, DateTime a, DateTime b)
        {
            var diffA = Math.Abs(value.Ticks - a.Ticks);
            var diffB = Math.Abs(value.Ticks - b.Ticks);
            if (diffA == diffB) return 0;
            return diffA < diffB ? -1 : 1;
        }

        public override DateTime FractionalToValue(double fraction)
        {
            return new DateTime(Minimum.Ticks + (long)((Maximum.Ticks - Minimum.Ticks) * fraction));
        }

        public override double ValueToFractional(DateTime value)
        {
            throw new NotImplementedException();
        }

        public override double ValueToVisual(DateTime value)
        {
            //todo: shold the result really be rounded
            switch (Orientation)
            {
                case PartOrientation.None:
                    throw new InvalidOperationException("The Orientation must be set for visual conversions.");
                case PartOrientation.Horizontal:
                    return (value.Ticks - Minimum.Ticks) * VisualScale;
                case PartOrientation.Vertical:
                    return VisualLength - (value.Ticks - Minimum.Ticks) * VisualScale;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override double ValueToVisualSpan(DateTime value)
        {
            throw new NotImplementedException();
        }


        public override DateTime VisualToValue(double visual)
        {
            switch (Orientation)
            {
                case PartOrientation.None:
                    throw new InvalidOperationException("The Orientation must be set for visual conversions.");
                case PartOrientation.Horizontal:
                    return new DateTime(Minimum.Ticks + (long)(visual / VisualScale));
                case PartOrientation.Vertical:
                    return new DateTime(Minimum.Ticks + (long)((VisualLength - visual) / VisualScale));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override double VisualDistanceAtRank(int rank)
        {
            if (double.IsInfinity(VisualScale))
                return double.NaN;
            var first = Minimum;
            var second = NextAtRank(first, rank);
            var third = NextAtRank(second, rank);
            var result = ValueToVisual(third) - ValueToVisual(second);
            return result;
        }

        #endregion Methods
    }
}
