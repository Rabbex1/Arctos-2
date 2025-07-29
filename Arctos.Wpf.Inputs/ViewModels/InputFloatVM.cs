namespace Arctos.Wpf.Inputs
{
    public class InputFloatVM(string description, float value, Action<float> onChange)
        : DataInputWrapperVM<float>(description, value, onChange)
    {
    }
}

