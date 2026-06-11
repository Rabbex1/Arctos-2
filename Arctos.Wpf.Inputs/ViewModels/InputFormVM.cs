using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Arctos.Wpf.Inputs
{
    public class InputFormVM : BaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        public Action? OnInputChanged { get; set; }

        #region Inputs
        private ObservableCollection<DataInputWrapperVM> _inputs = [];
        public ObservableCollection<DataInputWrapperVM> Inputs
        {
            get => _inputs;
            set
            {
                if (ReferenceEquals(_inputs, value))
                    return;

                _inputs.CollectionChanged -= OnInputsCollectionChanged;
                foreach (var input in _inputs)
                    DetachInput(input);

                SetProperty(ref _inputs, value);

                _inputs.CollectionChanged += OnInputsCollectionChanged;
                foreach (var input in _inputs)
                    AttachInput(input);
            }
        }
        #endregion

        #region SelectedInput
        private DataInputWrapperVM? _selectedInput;
        public DataInputWrapperVM? SelectedInput
        {
            get => _selectedInput;
            set => SetProperty(ref _selectedInput, value);
        }
        #endregion

        #endregion Properties

        public InputFormVM()
        {
            _inputs.CollectionChanged += OnInputsCollectionChanged;
        }

        private void OnInputsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (DataInputWrapperVM input in e.OldItems)
                    DetachInput(input);
            }

            if (e.NewItems != null)
            {
                foreach (DataInputWrapperVM input in e.NewItems)
                    AttachInput(input);
            }
        }

        private void AttachInput(DataInputWrapperVM input)
        {
            input.OnInputChanged += RaiseInputChanged;

            if (input is InputGroupVM group)
                group.PropertiesForm.OnInputChanged += RaiseInputChanged;
        }

        private void DetachInput(DataInputWrapperVM input)
        {
            input.OnInputChanged -= RaiseInputChanged;

            if (input is InputGroupVM group)
                group.PropertiesForm.OnInputChanged -= RaiseInputChanged;
        }

        private void RaiseInputChanged()
        {
            OnInputChanged?.Invoke();
        }
    }
}
