using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public class CollectionDataProvider : DataProvider
    {
        private IEnumerable? _data;

        //---------------------------------------------------------------------
        #region Public Methods

        public void Hook(INotifyCollectionChanged? newCollection)
        {
            if (newCollection != null)
                newCollection.CollectionChanged += OnCollectionChanged;
            _data = newCollection as IEnumerable;
        }

        public void UnHook(INotifyCollectionChanged? oldCollection)
        {
            if (oldCollection == null)
                return;
            oldCollection.CollectionChanged -= OnCollectionChanged;
            _data = null;
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region IDataProvider Members

        public override object GetDataObject(string source, object? value)
        {
            throw new NotImplementedException();
        }

        public override void ForAxisRange(Axis axis, Action<object> action)
        {
            if (_data != null)
                foreach (var obj in _data)
                    action(obj);
        }

        public override bool GetMinMax(string source, out object? min, out object? max)
        {
            if (_data == null)
            {
                min = max = null;
                return false;
            }
            var iterator = _data.GetEnumerator();
            if (!iterator.MoveNext())
            {
                min = null;
                max = null;
                return false;
            }
            var prop = iterator.Current?.GetType().GetProperty(source);
            var enumerable = _data as object[] ?? _data.Cast<object>().ToArray();
            min = prop?.GetValue(enumerable.MinBy(i => prop.GetValue(i)));
            max = prop?.GetValue(enumerable.MaxBy(i => prop.GetValue(i)));
            return true;
        }

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object min, out object max)
        {
            throw new NotImplementedException();
        }

        public override object LastAtOrBefore(object? value)
        {
            throw new NotImplementedException();
        }

        public override object FirstAtOrAfter(object? value)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo? PropertyInfo(string name)
        {
            var type = _data?.GetType().GetGenericArguments()[0];
            return type?.GetProperty(name);
        }

        #endregion IDataProvider Members


        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseDataChanged(new DataChangedEventArgs(Name));
        }
    }
}
