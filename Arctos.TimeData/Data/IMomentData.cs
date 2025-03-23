namespace Arctos.TimeData
{
    public interface IMomentData<T> : IMoment
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        T Data { get; set; }

        #endregion Properties
    }
}