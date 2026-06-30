namespace Arctos.Wpf.Inputs
{
    public class InputLabelVM(string description, string? value, string justify = "Left")
        : DataInputWrapperVM<string>(description, value, (Action<string?>?)null)
    {
        public string Justify { get; set; } = justify;
    }
}
