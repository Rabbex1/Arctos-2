using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Arctos.Wpf.Charts
{
  public interface IPresenterHost
  {
    ObservableCollection<Axis> Axes { get; }
    ObservableCollection<AxisMarker> Markers { get; }
    void Refresh();
  }
}
