namespace Arctos.Wpf.Inputs
{
    public class InputBoolVM(string description, bool value, Action<bool> onChange)
        : DataInputWrapperVM<bool>(description, value, onChange)
    {
    }
}
