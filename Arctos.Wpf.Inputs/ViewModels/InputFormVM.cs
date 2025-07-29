using System.Collections.ObjectModel;

namespace Arctos.Wpf.Inputs
{
    public class InputFormVM : BaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        #region Inputs
        private ObservableCollection<DataInputWrapperVM> _inputs = [];
        public ObservableCollection<DataInputWrapperVM> Inputs
        {
            get => _inputs;
            set => SetProperty(ref _inputs, value);
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
    }
}
