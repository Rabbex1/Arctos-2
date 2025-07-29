namespace Arctos.Wpf.Inputs
{
    public class InputIntegerVM(string description, int value, Action<int> onChange)
        : DataInputWrapperVM<int>(description, value, onChange)
    {
    }
}
