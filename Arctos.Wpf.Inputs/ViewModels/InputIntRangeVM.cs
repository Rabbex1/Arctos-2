namespace Arctos.Wpf.Inputs
{
    public class InputIntRangeVM : DataInputWrapperVM<IntRangeInput>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        private string _stringValue = string.Empty;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                if (_stringValue == value)
                    return;

                if (Value != null
                    && int.TryParse(value, out var parsed)
                    && parsed >= Value.Min
                    && parsed <= Value.Max)
                {
                    Value.Value = parsed;
                }

                if (!SetProperty(ref _stringValue, value))
                    return;
            }
        }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputIntRangeVM(string description, IntRangeInput value, Action<IntRangeInput?> onChange)
            : base(description, value, onChange)
        {
            value.Wrapper = this;
            StringValue = value.Value.ToString();
        }

        #endregion Constructors
    }
}
