namespace Arctos.TimeData
{
    public class Moment : IMoment
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties
        public DateTime Time { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public Moment() { }
        public Moment(DateTime time)
        {
            Time = time;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region ICompareable Methods

        public int CompareTo(IMoment? other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            return ReferenceEquals(null, other)
                ? 1
                : Time.CompareTo(other.Time);
        }

        #endregion ICompareable Methods

    }
}
