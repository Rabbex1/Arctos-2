namespace Arctos.Wpf.Inputs
{
    public class InputStringVM(string description, string value, Action<string>? onChange)
        : DataInputWrapperVM<string>(description, value, onChange)
    {
    }
}
