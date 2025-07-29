namespace Arctos.Wpf.Inputs
{
    public class InputStringListBoxVM : DataInputWrapperVM<string>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public IEnumerable<string> Options { get; set; } = null!;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructors

        public InputStringListBoxVM(string description, string value, IEnumerable<string> options, Action<string?> onChange)
            : base(description, value, onChange)
        {
            Options = options;
        }

        #endregion Constructors
    }
}