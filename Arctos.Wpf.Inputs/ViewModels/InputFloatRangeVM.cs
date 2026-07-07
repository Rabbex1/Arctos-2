namespace Arctos.Wpf.Inputs
{
    public class InputFloatRangeVM : DataInputWrapperVM<FloatRangeInput>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputFloatRangeVM(string description, FloatRangeInput value, Action<FloatRangeInput?> onChange)
            : base(description, value, onChange)
        {
            value.Wrapper = this;
        }

        public InputFloatRangeVM(string description, FloatRangeInput value, Action<DataInputWrapperVM<FloatRangeInput>, FloatRangeInput?> onChange)
            : base(description, value, onChange)
        {
            value.Wrapper = this;
            OnChange = input => OnChangeWithSender?.Invoke(this, input);
        }

        #endregion Constructors

        public void UpdateRange(FloatRangeInput value)
        {
            value.Wrapper = this;
            UpdateValue(value);
        }
    }
}
