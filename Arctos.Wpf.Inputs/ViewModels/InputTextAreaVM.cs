namespace Arctos.Wpf.Inputs
{
    public sealed class InputTextAreaVM(string description, string? value, Action<string?>? onChange)
        : DataInputWrapperVM<string>(description, value, onChange)
    {
    }
}
