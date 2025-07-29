namespace Arctos.Wpf.Inputs
{
    public class InputUnsignedIntegerVM(string description, uint value, Action<uint> onChange)
        : DataInputWrapperVM<uint>(description, value, onChange)
    {
    }
}
