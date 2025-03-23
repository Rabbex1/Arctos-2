namespace Arctos.Wpf.Charts
{
    public class DataChangedEventArgs : EventArgs
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string DataId { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public DataChangedEventArgs(string dataId)
        {
            DataId = dataId;
        }

        #endregion Constructors

    }
}
