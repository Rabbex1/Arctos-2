namespace Arctos.Wpf.Inputs
{
    public class InputEnumVM(string description, Enum value, Action<Enum> onChange)
        : DataInputWrapperVM<Enum>(description, value, onChange);
}
