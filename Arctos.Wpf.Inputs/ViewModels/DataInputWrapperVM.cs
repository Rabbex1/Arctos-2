using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Wpf.Inputs
{
    public class DataInputWrapperVM : BaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Description
        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        #endregion

        #region IsReadOnly
        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }
        #endregion

        public int ValueColumnSpan => Icon != null ? 1 : 2;

        #endregion Properties

        public static DataInputWrapperVM? CreateTypeWrapper<T>(string description, T value, Action<object> action)
        {
            return value switch
            {
                bool b => new InputBoolVM(description, b, v => action.Invoke(v)),
                Color c => new InputColorVM(description, c, v => action.Invoke(v)),
                double d => new InputDoubleVM(description, d, v => action.Invoke(v)),
                Enum e => new InputEnumVM(description, e, v => action.Invoke(v)),
                float f => new InputFloatVM(description, f, v => action.Invoke(v)),
                int i => new InputIntegerVM(description, i, v => action.Invoke(v)),
                uint u => new InputUnsignedIntegerVM(description, u, v => action.Invoke(v)),
                string s => new InputStringVM(description, s, v => action.Invoke(v)),
                DateTime dt => new InputDateTimeVM(description, dt, v => action.Invoke(v)),
                TimeSpan ts => new InputTimeSpanVM(description, ts, v => action.Invoke(v)),
                FloatRangeInput fr => new InputFloatRangeVM(description, fr, v => action.Invoke(v)),
                StringSelection ss => new InputStringComboVM(description, ss.Value, ss.List, v => ss.Action?.Invoke(v)),
                DirectoryInfo di => new InputFolderVM(description, di, v => action.Invoke(v)),
                _ => null
            };
        }


        //------------------------------------------------------------------------------------------------------------------------------------------
    }

    public class DataInputWrapperVM<T> : DataInputWrapperVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Action<T>? OnChange { get; set; }

        #region Value
        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                OnChange.Invoke(value);
            }
        }
        #endregion

        #region Hide
        private bool _hide;
        public bool Hide
        {
            get => _hide;
            set => SetProperty(ref _hide, value);
        }
        #endregion

        public Action<DataInputWrapperVM<T>>? OnButtonClicked { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public DataInputWrapperVM(string description, T value, Action<T>? onChange)
        {
            Description = description;
            OnChange = onChange;
            _value = value;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        #region DialogCommand
        private ICommand? _changeButtonCommand;
        public ICommand ChangeButtonCommand
        {
            get { return _changeButtonCommand ??= new RelayCommand(() => OnButtonClicked?.Invoke(this), () => true); }
        }
        #endregion

        #endregion Commands


    }
}
