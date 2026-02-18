using System;

namespace Arctos.Wpf.Charts
{
    public class VisualNumericRange : VisualRange<double>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public VisualNumericRange(double minimum, double maximum)
          : base(minimum, maximum, -324, 308)
        {
        }

        public VisualNumericRange()
          : base(-324, 308)
        {
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override bool IsValid()
        {
            return !double.IsInfinity(VisualScale);
        }

        public override double NextAtRank(double value, int rank)
        {
            ValidateRank(rank);
            var nearest = Round(value, rank);
            return nearest <= value ? AdjustByRank(nearest, rank, 1) : nearest;
        }

        public override double PreviousAtRank(double value, int rank)
        {
            ValidateRank(rank);
            var nearest = Round(value, rank);
            return nearest >= value ? AdjustByRank(nearest, rank, -1) : nearest;
        }

        public override double PreviousAtRankAndFrequency(double value, int rank, double frequency)
        {
            ValidateRank(rank);
            var trueFreq = Math.Pow(10, rank) * frequency;
            return Math.Floor(value / trueFreq) * trueFreq;
        }

        public override double Round(double value, int rank)
        {
            var nearest = Math.Pow(10.0, rank);
            return Math.Ceiling(value / nearest) * nearest;
        }

        public override double AdjustByRank(double value, int rank, double quantity)
        {
            return value + RankValue(rank) * quantity;
        }

        public override double RankValue(int rank)
        {
            return Math.Pow(10.0, rank);
        }

        public override void SetVisualScale(double length)
        {
            VisualLength = length;
            VisualScale = length / Math.Abs(Maximum - Minimum);
        }

        public override void Move(double distance)
        {
            var adjust = distance / VisualScale;
            if (Orientation == PartOrientation.Horizontal)
                adjust *= -1;  //Todo: 
            Minimum -= adjust;
            Maximum -= adjust;
        }

        public override void GetMovedMinMax(double distance, out double newMin, out double newMax)
        {
            var adjust = distance / VisualScale;
            newMin = Minimum - adjust;
            newMax = Maximum - adjust;
        }

        public override void Zoom(double value)
        {
            var adjust = (Maximum - Minimum) * (value / 100.0);
            Minimum += adjust;
            Maximum -= adjust;
            VisualScale = VisualLength / Math.Abs(Maximum - Minimum);
        }

        public override void GetZoomedMinMax(double value, out double newMin, out double newMax)
        {
            var adjust = (Maximum - Minimum) * (value / 100.0);
            newMin = Minimum + adjust;
            newMax = Maximum - adjust;
        }

        public override string ParseValue(double value, string format)
        {
            return value.ToString(format);
        }

        public override int Closest(double value, double a, double b)
        {
            var diffA = Math.Abs(value - a);
            var diffB = Math.Abs(value - b);
            if (Math.Abs(diffA - diffB) < 10e-300) return 0;
            return (diffA < diffB) ? -1 : 1;
        }

        public override double FractionalToValue(double fraction)
        {
            return Maximum - (Maximum - Minimum) * fraction;
        }

        public override double ValueToFractional(double value)
        {
            return Math.Abs(Maximum - value) / Math.Abs(Maximum - Minimum);
        }

        public override double ValueToVisual(double value)
        {
            //todo: should the result really be rounded
            return Orientation switch
            {
                PartOrientation.None => throw new InvalidOperationException(
                    "The Orientation must be set for visual conversions."),
                PartOrientation.Horizontal => (value - Minimum) * VisualScale,
                PartOrientation.Vertical => VisualLength - (value - Minimum) * VisualScale,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public override double ValueToVisualSpan(double value)
        {
            return Orientation switch
            {
                PartOrientation.None => throw new InvalidOperationException(
                    "The Orientation must be set for visual conversions."),
                PartOrientation.Horizontal or PartOrientation.Vertical => value * VisualScale,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override double VisualToValue(double visual)
        {
            return Orientation switch
            {
                PartOrientation.None => throw new InvalidOperationException(
                    "The Orientation must be set for visual conversions."),
                PartOrientation.Horizontal => Minimum + visual / VisualScale,
                PartOrientation.Vertical => Minimum + (VisualLength - visual) / VisualScale,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override double ValueToVisualNormal(double value)
        {
            return value / (Maximum - Minimum);
        }

        public override double VisualDistanceAtRank(int rank)
        {
            var first = Minimum;
            var second = NextAtRank(first, rank);
            var third = NextAtRank(second, rank);
            var result = ValueToVisual(third) - ValueToVisual(second);
            return result;
        }


        #endregion Methods
    }
}
