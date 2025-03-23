using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public abstract class DataProvider : IDataProvider
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public string Name { get; set; } = string.Empty;
    public virtual bool IsWaitingData => false;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public abstract object? GetDataObject(string source, object value);
    public abstract void ForAxisRange(Axis axis, Action<object?> action);
    public abstract bool GetMinMax(string source, out object? min, out object? max);
    public abstract bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max);
    public abstract object? LastAtOrBefore(object? value);
    public abstract object? FirstAtOrAfter(object? value);
    public abstract PropertyInfo? PropertyInfo(string name);

    #endregion Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events

    public event EventHandler<DataChangedEventArgs>? DataChanged;

    protected void RaiseDataChanged(DataChangedEventArgs args)
    {
      DataChanged?.Invoke(this, args);
    }

    #endregion Events

  }
}
