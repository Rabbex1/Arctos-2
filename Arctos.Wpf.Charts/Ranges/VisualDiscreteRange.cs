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
            VisualScale = ElementCount <= 0 ? 0 : length / (ElementCount * 2.0);
        }

        public override int FractionalToValue(double fraction)
        {
            if (ElementCount <= 0)
                return 0;
            var clamped = Math.Max(0.0, Math.Min(1.0, fraction));
            return Math.Min(ElementCount - 1, (int)Math.Floor(clamped * ElementCount));
        }

        public override double ValueToFractional(int value)
        {
            if (ElementCount <= 0)
                return 0;
            var clamped = Math.Max(0, Math.Min(ElementCount - 1, value));
            return ElementCount == 1 ? 0 : (double)clamped / (ElementCount - 1);
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
