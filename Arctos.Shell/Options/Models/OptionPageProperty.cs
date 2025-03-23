using System.Collections;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Shell.Options
{
    public class OptionPageProperty : ObservableObject
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Guid _guid = Guid.NewGuid();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string Name { get; set; }

        #region Value
        private object? _value;
        public object? Value
        {
            get => _value;
            set
            {
                //var val = value;
                if (value != null && value.GetType() != Type)
                {
                    if (value is string)
                    {
                        var converter = TypeDescriptor.GetConverter(Type);
                        value = converter.ConvertFrom(value);
                    }
                    else if (Type != null && !Type.IsInstanceOfType(value))
                        throw new InvalidOperationException("Cannot set Value of type:" + Type + " with:" + value.GetType());
                }

                if (SetProperty(ref _value, value))
                    Changed = true;
            }
        }
        #endregion

        public Type Type { get; }
        public bool Changed { get; set; }

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionPageProperty(string name, object value, Type type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public OptionPageProperty(string name, Type type)
        {
            Name = name;
            ////Todo: need a better way to do this to allow arrays
            //Value = type.IsValueType || typeof(IList).IsAssignableFrom(type) ? Activator.CreateInstance(type) : null;
            Type = type;
        }

        public OptionPageProperty(OptionPageProperty source)
        {
            _guid = source._guid;
            _value = source._value;
            Name = source.Name;
            Type = source.Type;
            Changed = source.Changed;
        }


        #endregion Properties

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override bool Equals(object? obj)
        {
            return Equals(obj as OptionPageProperty);
        }

        protected bool Equals(OptionPageProperty? other)
        {
            // If parameter is null, return false.
            if (other is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name &&
                   Type == other.Type &&
                   ValuesEqual(_value, other.Value, Type);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        private static bool ValuesEqual(object? v1, object? v2, Type? type)
        {
            if (v1 == null || v2 == null || type == null)
                return false;
            //change type if object is a SubClass of type
            if (v1.GetType() != type && v1.GetType().IsSubclassOf(type))
                type = v1.GetType();

            //Quick check for IEquatable and enums
            if (type.GetInterfaces().Any(x =>
                  x.IsGenericType &&
                  x.GetGenericTypeDefinition() == typeof(IEquatable<>)) ||
                type.IsEnum)
            {
                if (v1 is not string && v1 is IEnumerable && v2 is IEnumerable)
                    return false;
                return v1.Equals(v2);
            }

            //Check ILists
            if (v1 is IList vList)
                return ListEqual(vList, v2 as IList);

            //Otherwise check all public properties
            IEnumerable<PropertyInfo> properties1 = type.GetProperties();
            IEnumerable<PropertyInfo> properties2 = type.GetProperties();
            using var enumerator1 = properties1.GetEnumerator();
            using var enumerator2 = properties2.GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                if (enumerator1.Current == null && enumerator2.Current == null)
                    continue;
                if (enumerator1.Current == null || enumerator2.Current == null)
                    throw new InvalidOperationException("PropertyInfo should never be null");
                //Ignore properties that won't be saved in config file
                if (!Attribute.IsDefined(enumerator1.Current, typeof(UserOptionAttribute)))
                    continue;

                var v1P = enumerator1.Current.GetValue(v1);
                var v2P = enumerator2.Current.GetValue(v2);
                var vt = enumerator1.Current.PropertyType;
                if (!ValuesEqual(v1P, v2P, vt))
                    return false;
            }
            return true;
        }

        private static bool ListEqual(ICollection? v1, ICollection? v2)
        {
            if (v1 != null && v2 != null && v1.Count != v2.Count)
                return false;

            var enumerator1 = v1?.GetEnumerator();
            var enumerator2 = v2?.GetEnumerator();

            if (enumerator1 != null && enumerator2 != null)
            {
                while (enumerator1.MoveNext() && enumerator2.MoveNext())
                {
                    if (enumerator1.Current == null && enumerator2.Current == null)
                        continue;
                    if (enumerator1.Current != null && !ValuesEqual(enumerator1.Current, enumerator2.Current, enumerator1.Current.GetType()))
                        return false;
                }
            }

            return true;
        }

        #endregion Methods
    }
}
