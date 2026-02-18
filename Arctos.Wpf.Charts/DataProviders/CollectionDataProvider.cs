using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public class CollectionDataProvider : DataProvider
    {
        private IEnumerable? _data;
        private string? _indexSource;

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

        public override object? GetDataObject(string source, object? value)
        {
            _indexSource = source;
            if (_data == null || value == null)
                return null;

            var prop = PropertyInfo(source);
            if (prop == null)
                return null;

            foreach (var item in _data)
            {
                var current = prop.GetValue(item);
                if (TryCompare(current, value, out var compare) && compare == 0)
                    return item;
            }

            return null;
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

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max)
        {
            min = null;
            max = null;

            if (_data == null || range.Minimum == null || range.Maximum == null)
                return false;

            var rangeProp = PropertyInfo(rangeSource);
            var valueProp = PropertyInfo(source);
            if (rangeProp == null || valueProp == null)
                return false;

            _indexSource = rangeSource;
            object? currentMin = null;
            object? currentMax = null;
            var found = false;

            foreach (var item in _data)
            {
                var rangeValue = rangeProp.GetValue(item);
                var value = valueProp.GetValue(item);
                if (rangeValue == null || value == null)
                    continue;

                if (!TryCompare(rangeValue, range.Minimum, out var lower) || !TryCompare(rangeValue, range.Maximum, out var upper))
                    continue;
                if (lower < 0 || upper > 0)
                    continue;

                if (!found)
                {
                    currentMin = value;
                    currentMax = value;
                    found = true;
                    continue;
                }

                if (TryCompare(value, currentMin, out var minCompare) && minCompare < 0)
                    currentMin = value;
                if (TryCompare(value, currentMax, out var maxCompare) && maxCompare > 0)
                    currentMax = value;
            }

            if (!found)
                return false;

            min = currentMin;
            max = currentMax;
            return true;
        }

        public override object? LastAtOrBefore(object? value)
        {
            if (_data == null || value == null)
                return null;

            var prop = ResolveIndexProperty(value);
            if (prop == null)
                return null;

            object? best = null;
            foreach (var item in _data)
            {
                var current = prop.GetValue(item);
                if (current == null)
                    continue;
                if (!TryCompare(current, value, out var currentToTarget) || currentToTarget > 0)
                    continue;
                if (best == null || (TryCompare(current, best, out var currentToBest) && currentToBest > 0))
                    best = current;
            }

            return best;
        }

        public override object? FirstAtOrAfter(object? value)
        {
            if (_data == null || value == null)
                return null;

            var prop = ResolveIndexProperty(value);
            if (prop == null)
                return null;

            object? best = null;
            foreach (var item in _data)
            {
                var current = prop.GetValue(item);
                if (current == null)
                    continue;
                if (!TryCompare(current, value, out var currentToTarget) || currentToTarget < 0)
                    continue;
                if (best == null || (TryCompare(current, best, out var currentToBest) && currentToBest < 0))
                    best = current;
            }

            return best;
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

        private PropertyInfo? ResolveIndexProperty(object value)
        {
            if (!string.IsNullOrWhiteSpace(_indexSource))
            {
                var preferred = PropertyInfo(_indexSource);
                if (preferred != null)
                    return preferred;
            }

            var type = _data?.GetType().GetGenericArguments()[0];
            if (type == null)
                return null;

            var valueType = value.GetType();
            return type.GetProperties()
              .FirstOrDefault(p => p.PropertyType.IsAssignableFrom(valueType))
              ?? type.GetProperties().FirstOrDefault(p => typeof(IComparable).IsAssignableFrom(p.PropertyType));
        }

        private static bool TryCompare(object? left, object? right, out int result)
        {
            result = 0;
            if (left == null || right == null)
                return false;

            if (left is IComparable comparableLeft)
            {
                if (TryConvert(right, left.GetType(), out var converted))
                {
                    result = comparableLeft.CompareTo(converted);
                    return true;
                }
            }

            if (right is IComparable comparableRight && TryConvert(left, right.GetType(), out var convertedLeft))
            {
                result = -comparableRight.CompareTo(convertedLeft);
                return true;
            }

            return false;
        }

        private static bool TryConvert(object value, Type targetType, out object? converted)
        {
            converted = null;
            try
            {
                var nullableType = Nullable.GetUnderlyingType(targetType);
                var actualType = nullableType ?? targetType;
                if (actualType.IsInstanceOfType(value))
                {
                    converted = value;
                    return true;
                }
                converted = Convert.ChangeType(value, actualType, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
