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

        #endregion Constructors
    }
}
