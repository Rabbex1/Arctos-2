using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public interface IDataProvider
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    string Name { get; }
    bool IsWaitingData { get; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    object? GetDataObject(string source, object value);
    void ForAxisRange(Axis axis, Action<object?> action);
    bool GetMinMax(string source, out object? min, out object? max);
    bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max);
    object? LastAtOrBefore(object? value);
    object? FirstAtOrAfter(object? value);
    PropertyInfo? PropertyInfo(string name);

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    event EventHandler<DataChangedEventArgs> DataChanged;

    #endregion Events

  }
}
