using System.Diagnostics;

namespace Arctos.Wpf.Charts
{
    public class Range<T> : IRange
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public T? Maximum 
        { 
            get; 
            protected set;
        }
        public T? Minimum { get; protected set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual T FractionalToValue(double fractional)
        {
            LogFallbackUsage(nameof(FractionalToValue));
            return Minimum is not null ? Minimum : default!;
        }

        public virtual double ValueToFractional(T? value)
        {
            LogFallbackUsage(nameof(ValueToFractional));
            return 0;
        }

        public virtual int Closest(T value, T a, T b)
        {
            LogFallbackUsage(nameof(Closest));
            return 0;
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region IRange Members

        dynamic? IRange.Maximum
        {
            get => Maximum;
            set
            {
                try
                {
                    Maximum = (T?)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Invalid argument type");
                }
            }
        }

        dynamic? IRange.Minimum
        {
            get => Minimum;
            set
            {
                try
                {
                    Minimum = (T?)value;
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException("Invalid argument type");
                }
            }
        }

        object? IRange.FractionalToValue(double fraction)
        {
            return FractionalToValue(fraction);
        }

        double IRange.ValueToFractional(object? value)
        {
            try
            {
                return ValueToFractional((T?)value);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        int IRange.Closest(object value, object a, object b)
        {
            try
            {
                return Closest((T)value, (T)a, (T)b);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        #endregion IRange Members

        [Conditional("DEBUG")]
        protected void LogFallbackUsage(string methodName)
        {
            Debug.WriteLine($"{GetType().Name}: using base fallback for {methodName}.");
        }
    }
}
