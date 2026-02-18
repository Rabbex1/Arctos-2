using System.Collections.ObjectModel;

namespace Arctos.Wpf.Inputs
{
    public class InputStringComboVM : DataInputWrapperVM<string>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<string> Options { get; set; } = null!;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputStringComboVM(string description, string? value, IEnumerable<string> options, Action<string?>? onChange)
            : base(description, value, onChange)
        {
            Options = new ObservableCollection<string>(options);
        }


        #endregion Constructors
    }
}
