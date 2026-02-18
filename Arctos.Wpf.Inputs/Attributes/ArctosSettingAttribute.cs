namespace Arctos.Wpf.Inputs
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ArctosSettingAttribute : Attribute
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Order
        private int _order = -1;
        public int Order
        {
            get => _order;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Cannot be negative", nameof(Order));
                _order = value;
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ArctosSettingAttribute()
        {
        }

        #endregion Constructor
    }
}
