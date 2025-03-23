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
            throw new NotImplementedException();
        }

        public virtual double ValueToFractional(T? value)
        {
            throw new NotImplementedException();
        }

        public virtual int Closest(T value, T a, T b)
        {
            throw new NotImplementedException();
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

    }
}
