namespace Arctos.Wpf.Inputs
{
    public class InputDateTimeVM : DataInputWrapperVM<DateTime>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        #region DateTimeString

        private string _dateTimeString = string.Empty;

        public string DateTimeString
        {
            get => _dateTimeString;
            set 
            {
                if (_dateTimeString == value) return;
                _dateTimeString = value;
                Value = DateTime.Parse(value);
                SetProperty(ref _dateTimeString, value);
            }
        }

        #endregion

        public InputDateTimeVM(string description, DateTime value, Action<DateTime> onChange)
            : base(description, value, onChange)
        {
            DateTimeString = value.ToString("dd/MM/yyyy hh:mm tt");
        }

        #endregion Constructor
    }
}
