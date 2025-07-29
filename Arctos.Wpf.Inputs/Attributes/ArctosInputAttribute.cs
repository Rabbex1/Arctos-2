namespace Arctos.Wpf.Inputs
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArctosInputAttribute : Attribute
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Order
        private int _position = -1;
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Cannot be negative", nameof(Position));
                _position = value;
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ArctosInputAttribute()
        { }

        #endregion Constructor
    }
}
