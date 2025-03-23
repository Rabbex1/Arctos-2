
namespace Arctos.Wpf.Charts
{
    public interface ITimeChartVM : IChartVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        //PriceBarSpan PriceBarSpan { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        DateTime? CurrentTime { get; set; }
        decimal? MinValue { get; set; }
        decimal? MaxValue { get; set; }
        decimal? CurrentValue { get; set; }

        #endregion Properties

    }
}