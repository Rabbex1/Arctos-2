using System.Collections.ObjectModel;

namespace Arctos.Wpf.Inputs
{
    public class InputStringComboVM : DataInputWrapperVM<string>
    {
        private bool _suppressNotifications;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<string> Options { get; set; } = null!;

        public override string? Value
        {
            get => _value;
            set
            {
                if (!SetProperty(ref _value, value))
                    return;

                if (_suppressNotifications)
                    return;

                OnChange?.Invoke(value);
                OnChangeWithSender?.Invoke(this, value);
                NotifyInputChanged();
            }
        }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputStringComboVM(string description, string? value, IEnumerable<string> options, Action<string?>? onChange)
            : base(description, value, onChange)
        {
            Options = new ObservableCollection<string>(options);
        }

        public InputStringComboVM(string description, string? value, IEnumerable<string> options, Action<DataInputWrapperVM<string>, string?> onChange)
            : base(description, value, onChange)
        {
            Options = new ObservableCollection<string>(options);
        }

        public void UpdateOptions(IEnumerable<string>? options)
        {
            _suppressNotifications = true;
            Options.Clear();

            foreach (var option in options ?? [])
                Options.Add(option);
            _suppressNotifications = false;
        }

        public void UpdateState(IEnumerable<string>? options, string? selectedValue)
        {
            _suppressNotifications = true;
            Options.Clear();

            foreach (var option in options ?? [])
                Options.Add(option);

            UpdateValue(selectedValue);
            _suppressNotifications = false;
        }


        #endregion Constructors
    }
}
