namespace Arctos.Wpf.Charts
{
    public interface IChartVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        IChartHostVM? Host { get; set; }
        IDataProvider? DataProvider { get; set; }
        object? DataTemplateKey { get; }

        #endregion Properties

    }
}