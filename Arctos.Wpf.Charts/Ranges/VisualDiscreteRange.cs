namespace Arctos.Wpf.Charts
{
    class VisualDiscreteRange : VisualRange<int>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public int ElementCount { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructors

        public VisualDiscreteRange(int minimumRank, int maximumRank) 
            : base(minimumRank, maximumRank)
        {
        }

        public VisualDiscreteRange()
            : base(1, 1)
        {
        }


        #endregion Constructors

        public override bool IsValid()
        {
            return !double.IsNaN(VisualScale);
        }

        public override void SetVisualScale(double length)
        {
            VisualLength = length;
            VisualScale = length / (ElementCount * 2);
        }

        public override int FractionalToValue(double fraction)
        {
            throw new NotImplementedException();
            //return (int)(Maximum - (Maximum - Minimum) * fraction);
        }

        public override double ValueToFractional(int value)
        {
            throw new NotImplementedException();
            //return Math.Abs(Maximum - value) / Math.Abs(Maximum - Minimum);
        }


        public override double ValueToVisual(int value)
        {
            //todo: should the result really be rounded
            return Orientation switch
            {
                PartOrientation.None => throw new InvalidOperationException(
                    "The Orientation must be set for visual conversions."),
                PartOrientation.Horizontal => ((double) value * 2 + 1) * VisualScale,
                PartOrientation.Vertical => VisualLength - ((double) value * 2 + 1) * VisualScale,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

    }
}
