namespace Arctos.Wpf.Inputs
{
    public class InputStringVM : DataInputWrapperVM<string>
    {
        private bool _isMultiLine;
        private int _showMaxLines = 1;

        public bool IsMultiLine
        {
            get => _isMultiLine;
            set => SetProperty(ref _isMultiLine, value);
        }

        public int ShowMaxLines
        {
            get => _showMaxLines;
            set => SetProperty(ref _showMaxLines, Math.Max(1, value));
        }

        public InputStringVM(string description, string? value, Action<string?>? onChange)
            : base(description, value, onChange)
        {
        }

        public InputStringVM(string description, string? value, Action<DataInputWrapperVM<string>, string?>? onChange)
            : base(description, value, onChange)
        {
        }
    }
}
