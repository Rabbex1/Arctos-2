namespace Arctos.Wpf.Charts
{
    public interface IRange
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        object? Maximum { get; set; }
        object? Minimum { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        object? FractionalToValue(double fraction);
        double ValueToFractional(object? value);
        int Closest(object value, object a, object b);

        #endregion Methods

    }
}