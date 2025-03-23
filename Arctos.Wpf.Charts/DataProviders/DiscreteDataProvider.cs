using System.Collections;
using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public class DiscreteDataProvider<TKey, TValue> : DataProvider, IDiscreteDataProvider
        where TKey : notnull
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        public IDictionary Elements { get; } = new Dictionary<TKey, TValue>();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override object? GetDataObject(string source, object value)
        {
            throw new NotImplementedException();
        }

        public override void ForAxisRange(Axis axis, Action<object?> action)
        {
            throw new NotImplementedException();
        }

        public override bool GetMinMax(string source, out object? min, out object? max)
        {
            throw new NotImplementedException();
        }

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max)
        {
            throw new NotImplementedException();
        }

        public override object? LastAtOrBefore(object? value)
        {
            throw new NotImplementedException();
        }

        public override object? FirstAtOrAfter(object? value)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo? PropertyInfo(string name)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
