namespace Arctos.Wpf.Inputs
{
    public class InputStringVM : DataInputWrapperVM<string>
    { 
        public InputStringVM(string description, string? value, Action<string?>? onChange)
            : base (description, value, onChange)
        { }

        public InputStringVM(string description, string? value, Action<DataInputWrapperVM<string>, string?>? onChange)
            : base(description, value, onChange)
        { }

    }

}
