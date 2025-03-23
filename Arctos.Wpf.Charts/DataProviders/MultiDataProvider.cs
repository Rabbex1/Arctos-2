using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public class MultiDataProvider : DataProvider
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private IDataProvider? _selectedProvider;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Dictionary<object, IDataProvider> Providers { get; } = new();
        public override bool IsWaitingData => Providers.Values.Any(p => p.IsWaitingData);

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void AddProvider(string name, IDataProvider provider)
        {
            Providers.Add(name, provider);
            provider.DataChanged += OnProviderDataChanged;
        }

        public void RemoveProvider(string name)
        {
            if (!Providers.TryGetValue(name, out IDataProvider? value))
                return;
            if (_selectedProvider?.Name == name)
                _selectedProvider = null;
            value.DataChanged -= OnProviderDataChanged;
            Providers.Remove(name);
        }

        public void SelectProvider(string name)
        {
            if (Providers.TryGetValue(name, out var value))
                _selectedProvider = value;
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region DataProvider Method Overrides

        public override object? GetDataObject(string source, object value)
        {
            return _selectedProvider?.GetDataObject(source, value);
        }

        public override void ForAxisRange(Axis axis, Action<object?> action)
        {
            _selectedProvider?.ForAxisRange(axis, action);
        }

        public override bool GetMinMax(string source, out object? min, out object? max)
        {
            min = null;
            max = null;
            return _selectedProvider != null && _selectedProvider.GetMinMax(source, out min, out max);
        }

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max)
        {
            min = null;
            max = null;
            return _selectedProvider != null && _selectedProvider.GetRangeMinMax(rangeSource, range, source, out min, out max);
        }

        public override object? LastAtOrBefore(object? value)
        {
            return _selectedProvider?.LastAtOrBefore(value);
        }

        public override object? FirstAtOrAfter(object? value)
        {
            return _selectedProvider?.FirstAtOrAfter(value);
        }

        public override PropertyInfo? PropertyInfo(string name)
        {
            return _selectedProvider?.PropertyInfo(name);
        }

        #endregion DataProvider Method Overrides


        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnProviderDataChanged(object? sender, DataChangedEventArgs e)
        {
            RaiseDataChanged(e);
        }

        #endregion Event Handlers
    }
}
