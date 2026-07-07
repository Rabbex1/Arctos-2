namespace Arctos.Wpf.Inputs
{
    public class InputUnsignedIntegerVM(string description, uint value, Action<uint> onChange)
        : DataInputWrapperVM<uint>(description, value, onChange)
    {
        public InputUnsignedIntegerVM(string description, uint value, Action<DataInputWrapperVM<uint>, uint> onChange)
            : this(description, value, _ => { })
        {
            OnChangeWithSender = onChange;
            OnChange = null;
        }
    }
}
