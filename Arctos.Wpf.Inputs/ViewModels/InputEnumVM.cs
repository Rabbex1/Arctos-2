namespace Arctos.Wpf.Inputs
{
    public class InputEnumVM : DataInputWrapperVM<Enum>
    {
        public InputEnumVM(string description, Enum value, Action<Enum?> onChange)
            : base(description, value, onChange)
        {
        }

        public InputEnumVM(string description, Enum value, Action<DataInputWrapperVM<Enum>, Enum?> onChange)
            : base(description, value, onChange)
        {
        }
    }
}
