using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Arctos.Wpf.Charts
{
    public interface IChartHostVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        ObservableCollection<IChartVM> Charts { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        void AddChart(ITimeChartVM chartToAdd, int index);
        void RemoveChart(ITimeChartVM chartToRemove);

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        event NotifyCollectionChangedEventHandler ChartsChanged;

        #endregion Events

    }
}