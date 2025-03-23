namespace Arctos.Wpf.Charts
{
    public abstract class VisualRange<T> : Range<T>, IVisualRange where T : IComparable
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        protected readonly int MinimumRank;
        protected readonly int MaximumRank;

        private readonly Guid _guid = Guid.NewGuid();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public PartOrientation Orientation { get; set; } = PartOrientation.None;
        public double VisualLength { get; protected set; }
        public double VisualScale { get; protected set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected VisualRange(T minimum, T maximum, int minimumRank, int maximumRank)
          : this(minimumRank, maximumRank)
        {
            Minimum = minimum;
            Maximum = maximum;
            if (minimum.CompareTo(maximum) > 0)
                throw new ArgumentException("Minimum in Range must be less than or equal to maximum");
        }

        // ReSharper disable once ConvertToPrimaryConstructor
        protected VisualRange(int minimumRank, int maximumRank)
        {
            MinimumRank = minimumRank;
            MaximumRank = maximumRank;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Operator Methods

        public static bool operator == (VisualRange<T>? leftRange, VisualRange<T>? rightRange)
        {
            //if (leftRange == null || rightRange == null)
            //  return false;
            if (leftRange is null || (object?)rightRange is null)
            {
                return false;
            }
            return leftRange.Maximum != null && leftRange.Minimum != null 
                                             && leftRange.Minimum.Equals(rightRange.Minimum) 
                                             && leftRange.Maximum.Equals(rightRange.Maximum);
        }

        public static bool operator !=(VisualRange<T> leftRange, VisualRange<T> rightRange)
        {
            return !(leftRange == rightRange);
        }


        #endregion Operator Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public virtual bool IsValid()
        {
            throw new NotImplementedException();
        }

        public virtual T NextAtRank(T value, int rank)
        {
            throw new NotImplementedException();
        }

        public virtual T PreviousAtRank(T value, int rank)
        {
            throw new NotImplementedException();
        }

        public virtual T PreviousAtRankAndFrequency(T value, int rank, double frequency)
        {
            throw new NotImplementedException();
        }

        public virtual T Round(T value, int rank)
        {
            throw new NotImplementedException();
        }

        public virtual T AdjustByRank(T value, int rank, double quantity)
        {
            throw new NotImplementedException();
        }

        public virtual T RankValue(int rank)
        {
            throw new NotImplementedException();
        }

        public virtual void SetVisualScale(double length)
        {
            throw new NotImplementedException();
        }

        public virtual void Move(double distance)
        {
            throw new NotImplementedException();
        }

        public virtual void GetMovedMinMax(double distance, out T newMin, out T newMax)
        {
            throw new NotImplementedException();
        }

        public virtual void Zoom(double value)
        {
            throw new NotImplementedException();
        }

        public virtual void GetZoomedMinMax(double value, out T newMin, out T newMax)
        {
            throw new NotImplementedException();
        }

        public virtual string ParseValue(T value, string format)
        {
            throw new NotImplementedException();
        }

        public virtual double ValueToVisualNormal(T value)
        {
            throw new NotImplementedException();
        }

        public virtual double ValueToVisual(T value)
        {
            throw new NotImplementedException();
        }

        public virtual double ValueToVisualSpan(T value)
        {
            throw new NotImplementedException();
        }

        public virtual T VisualToValue(double visualUnits)
        {
            throw new NotImplementedException();
        }

        public virtual double VisualDistanceAtRank(int rank)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((VisualRange<T>)obj);
        }

        public double FractionalToVisual(double fraction)
        {
            switch (Orientation)
            {
                case PartOrientation.None:
                    throw new InvalidOperationException("The Orientation must be set for visual conversions.");
                case PartOrientation.Horizontal:
                    return VisualLength * fraction;
                case PartOrientation.Vertical:
                    return VisualLength - VisualLength * fraction;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public double VisualToFractional(double visual)
        {
            return Orientation switch
            {
                PartOrientation.None => throw new InvalidOperationException(
                    "The Orientation must be set for visual conversions."),
                PartOrientation.Horizontal => visual / VisualLength,
                PartOrientation.Vertical => (VisualLength - visual) / VisualLength,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected bool Equals(VisualRange<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Maximum, other.Maximum) && EqualityComparer<T>.Default.Equals(Minimum, other.Minimum);
        }

        protected void ValidateRank(int rank)
        {
            if (rank < MinimumRank || rank > MaximumRank)
                throw new ArgumentOutOfRangeException(nameof(rank), @"Avalible scles for " + GetType() + @" are " + MinimumRank + @" to " + MaximumRank);
        }

        #endregion Protected Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region IVisualRange Members

        bool IVisualRange.IsValid()
        {
            return IsValid();
        }

        object IVisualRange.NextAtRank(object value, int rank)
        {
            try
            {
                return NextAtRank((T)value, rank);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        object IVisualRange.PreviousAtRank(object value, int rank)
        {
            try
            {
                return PreviousAtRank((T)value, rank);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        object IVisualRange.PreviousAtRankAndFrequency(object value, int rank, double frequency)
        {
            try
            {
                return PreviousAtRankAndFrequency((T)value, rank, frequency);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        object IVisualRange.Round(object value, int rank)
        {
            try
            {
                return Round((T)value, rank);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        object IVisualRange.AdjustByRank(object value, int rank, double quantity)
        {
            try
            {
                return AdjustByRank((T)value, rank, quantity);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        object IVisualRange.RankValue(int rank)
        {
            try
            {
                return RankValue(rank);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        void IVisualRange.SetVisualScale(double length)
        {
            SetVisualScale(length);
        }

        void IVisualRange.Move(double distance)
        {
            Move(distance);
        }

        void IVisualRange.GetMovedMinMax(double distance, out object newMin, out object newMax)
        {
            GetMovedMinMax(distance, out var nMin, out var nMax);
            newMin = nMin;
            newMax = nMax;
        }

        void IVisualRange.Zoom(double value)
        {
            Zoom(value);
        }

        void IVisualRange.GetZoomedMinMax(double value, out object newMin, out object newMax)
        {
            GetZoomedMinMax(value, out var nMin, out var nMax);
            newMin = nMin;
            newMax = nMax;
        }

        string IVisualRange.ParseValue(object value, string format)
        {
            try
            {
                return ParseValue((T)value, format);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        double IVisualRange.ValueToVisual(object? value)
        {
            try
            {
                if (value == null)
                    return 0;
                var val = (T?)Convert.ChangeType(value, typeof(T));
                return val == null ? 0 : ValueToVisual(val);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid argument type", ex);
            }
        }

        double IVisualRange.ValueToVisualSpan(object? value)
        {
            try
            {
                if (value == null)
                    return 0;
                var val = (T?)Convert.ChangeType(value, typeof(T));
                return val == null ? 0 : ValueToVisualSpan(val);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid argument type", ex);
            }
        }

        object IVisualRange.VisualToValue(double visual)
        {
            try
            {
                return VisualToValue(visual);
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("Invalid argument type");
            }
        }

        double IVisualRange.FractionalToVisual(double fraction)
        {
            return FractionalToVisual(fraction);
        }

        double IVisualRange.VisualToFractional(double visual)
        {
            return VisualToFractional(visual);
        }

        double IVisualRange.ValueToVisualNormal(object? value)
        {
            try
            {
                if (value == null)
                    return 0;
                var val = (T?)Convert.ChangeType(value, typeof(T));
                return val == null ? 0 : ValueToVisualNormal(val);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid argument type", ex);
            }
        }

        double IVisualRange.VisualDistanceAtRank(int rank)
        {
            return VisualDistanceAtRank(rank);
        }

        #endregion IVisualRange Members

    }
}
