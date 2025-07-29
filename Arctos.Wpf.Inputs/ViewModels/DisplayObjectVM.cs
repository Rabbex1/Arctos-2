namespace Arctos.Wpf.Inputs
{
    public class DisplayObjectVM : DataInputWrapperVM<object>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        #region DisplayMember
        private string _displayMember = string.Empty;
        public string DisplayMember
        {
            get => _displayMember;
            set => SetProperty(ref _displayMember, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public DisplayObjectVM(string description, object value, string displayMember, Action<object> onChange)
            : base(description, value, onChange)
        {
            DisplayMember = displayMember;
        }

        #endregion Constructors

    }
}
