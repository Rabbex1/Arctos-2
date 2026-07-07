using System.Collections.ObjectModel;

namespace Arctos.Wpf.Inputs
{
    public class InputObjectComboVM : DataInputWrapperVM<object>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<object> Options { get; set; } = null!;

        #region DisplayMember
        private string _displayMember = string.Empty;
        public string DisplayMember
        {
            get => _displayMember;
            set => SetProperty(ref _displayMember, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputObjectComboVM(string description, object value, IEnumerable<object> options, string displayMember, Action<object?> onChange)
            : base(description, value, onChange)
        {
            Options = new ObservableCollection<object>(options);
            DisplayMember = displayMember;
        }

        public InputObjectComboVM(string description, object value, IEnumerable<object> options, string displayMember, Action<DataInputWrapperVM<object>, object?> onChange)
            : base(description, value, onChange)
        {
            Options = new ObservableCollection<object>(options);
            DisplayMember = displayMember;
        }

        #endregion Constructors
    }
}
