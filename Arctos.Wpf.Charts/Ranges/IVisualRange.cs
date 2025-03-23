namespace Arctos.Wpf.Charts
{
    public interface IVisualRange : IRange
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        PartOrientation Orientation { get; set; }
        double VisualLength { get; }
        double VisualScale { get; }


        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        bool IsValid();
        object NextAtRank(object value, int rank);
        object PreviousAtRank(object value, int rank);
        object PreviousAtRankAndFrequency(object value, int rank, double frequency);
        object Round(object value, int rank);
        object AdjustByRank(object value, int rank, double quantity);
        object RankValue(int rank);
        void SetVisualScale(double length);
        void Move(double distance);
        void GetMovedMinMax(double distance, out object newMin, out object newMax);
        void Zoom(double value);
        void GetZoomedMinMax(double value, out object newMin, out object newMax);
        string ParseValue(object value, string format);
        double ValueToVisual(object? value);
        double ValueToVisualSpan(object value);
        object VisualToValue(double visual);
        double FractionalToVisual(double fraction);
        double VisualToFractional(double visual);
        double ValueToVisualNormal(object? value);
        double VisualDistanceAtRank(int rank);


        #endregion Methods
    }
}