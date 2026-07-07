namespace Arctos.Wpf.Inputs
{
    public class InputFloatVM : DataInputWrapperVM<float>
    {
        public InputFloatVM(string description, float value, Action<float> onChange)
            : base(description, value, onChange)
        {
        }

        public InputFloatVM(string description, float value, Action<DataInputWrapperVM<float>, float> onChange)
        {
            Description = description;
            OnChangeWithSender = onChange;
            _value = value;
        }
    }
}
