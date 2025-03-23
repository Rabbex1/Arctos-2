namespace Arctos.Wpf.Charts
{
    public interface ICanvasHost
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        IDataProvider? DataProvider { get; }
        Axis? XAxis { get; set; }
        Axis? YAxis { get; set; }
        Axis? PrimaryAxis { get; set; }

        #endregion Properties
    }
}