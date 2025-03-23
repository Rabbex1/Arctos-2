namespace Arctos.TimeData
{
    public interface IPeriodData<out T> : IPeriod
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        T Value { get; }

        #endregion Properties
    }
}