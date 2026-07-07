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

        public InputIntRangeVM(string description, IntRangeInput value, Action<DataInputWrapperVM<IntRangeInput>, IntRangeInput?> onChange)
            : base(description, value, onChange)
        {
            value.Wrapper = this;
            OnChange = input => OnChangeWithSender?.Invoke(this, input);
            StringValue = value.Value.ToString();
        }

        #endregion Constructors

        public void UpdateRange(IntRangeInput value)
        {
            value.Wrapper = this;
            UpdateValue(value);
            SetProperty(ref _stringValue, value.Value.ToString(), nameof(StringValue));
        }
    }
}
