namespace Arctos.Wpf.Inputs
{
    public sealed class InputTextAreaVM(string description, string? value, Action<string?>? onChange)
        : DataInputWrapperVM<string>(description, value, onChange)
    {
        public InputTextAreaVM(string description, string? value, Action<DataInputWrapperVM<string>, string?> onChange)
            : this(description, value, (Action<string?>?)null)
        {
            OnChangeWithSender = onChange;
        }
    }
}
