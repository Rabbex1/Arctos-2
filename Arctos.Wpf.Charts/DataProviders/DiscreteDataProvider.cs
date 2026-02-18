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
            if (TryConvert(value, typeof(TKey), out var convertedKey) && convertedKey is TKey key && Elements.Contains(key))
                return Elements[key];

            var prop = PropertyInfo(source);
            if (prop == null)
                return null;

            foreach (DictionaryEntry entry in Elements)
            {
                var candidate = prop.GetValue(entry.Value);
                if (TryCompare(candidate, value, out var compare) && compare == 0)
                    return entry.Value;
            }

            return null;
        }

        public override void ForAxisRange(Axis axis, Action<object?> action)
        {
            foreach (DictionaryEntry entry in Elements)
                action(entry.Value);
        }

        public override bool GetMinMax(string source, out object? min, out object? max)
        {
            min = null;
            max = null;
            if (Elements.Count == 0)
                return false;

            var prop = PropertyInfo(source);
            object? currentMin = null;
            object? currentMax = null;
            var found = false;

            foreach (DictionaryEntry entry in Elements)
            {
                var candidate = prop != null ? prop.GetValue(entry.Value) : entry.Key;
                if (candidate == null)
                    continue;

                if (!found)
                {
                    currentMin = candidate;
                    currentMax = candidate;
                    found = true;
                    continue;
                }

                if (TryCompare(candidate, currentMin, out var minCompare) && minCompare < 0)
                    currentMin = candidate;
                if (TryCompare(candidate, currentMax, out var maxCompare) && maxCompare > 0)
                    currentMax = candidate;
            }

            if (!found)
                return false;

            min = currentMin;
            max = currentMax;
            return true;
        }

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object? min, out object? max)
        {
            min = null;
            max = null;
            if (Elements.Count == 0 || range.Minimum == null || range.Maximum == null)
                return false;

            var rangeProp = PropertyInfo(rangeSource);
            var sourceProp = PropertyInfo(source);
            object? currentMin = null;
            object? currentMax = null;
            var found = false;

            foreach (DictionaryEntry entry in Elements)
            {
                var rangeValue = rangeProp != null ? rangeProp.GetValue(entry.Value) : entry.Key;
                if (rangeValue == null)
                    continue;
                if (!TryCompare(rangeValue, range.Minimum, out var lower) || !TryCompare(rangeValue, range.Maximum, out var upper))
                    continue;
                if (lower < 0 || upper > 0)
                    continue;

                var sourceValue = sourceProp != null ? sourceProp.GetValue(entry.Value) : entry.Key;
                if (sourceValue == null)
                    continue;

                if (!found)
                {
                    currentMin = sourceValue;
                    currentMax = sourceValue;
                    found = true;
                    continue;
                }

                if (TryCompare(sourceValue, currentMin, out var minCompare) && minCompare < 0)
                    currentMin = sourceValue;
                if (TryCompare(sourceValue, currentMax, out var maxCompare) && maxCompare > 0)
                    currentMax = sourceValue;
            }

            if (!found)
                return false;

            min = currentMin;
            max = currentMax;
            return true;
        }

        public override object? LastAtOrBefore(object? value)
        {
            if (value == null || Elements.Count == 0)
                return null;

            object? best = null;
            foreach (DictionaryEntry entry in Elements)
            {
                var key = entry.Key;
                if (!TryCompare(key, value, out var keyToTarget) || keyToTarget > 0)
                    continue;
                if (best == null || (TryCompare(key, best, out var keyToBest) && keyToBest > 0))
                    best = key;
            }
            return best;
        }

        public override object? FirstAtOrAfter(object? value)
        {
            if (value == null || Elements.Count == 0)
                return null;

            object? best = null;
            foreach (DictionaryEntry entry in Elements)
            {
                var key = entry.Key;
                if (!TryCompare(key, value, out var keyToTarget) || keyToTarget < 0)
                    continue;
                if (best == null || (TryCompare(key, best, out var keyToBest) && keyToBest < 0))
                    best = key;
            }
            return best;
        }

        public override PropertyInfo? PropertyInfo(string name)
        {
            return typeof(TValue).GetProperty(name);
        }

        #endregion Methods

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
