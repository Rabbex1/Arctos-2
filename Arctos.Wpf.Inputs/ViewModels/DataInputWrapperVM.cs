using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        #region Id
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        #region IsEnabled
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        #endregion

        private static readonly List<Func<string, object?, Action<object>?, DataInputWrapperVM?>> _extensions = [];

        public int ValueColumnSpan => Icon != null ? 1 : 2;

        #endregion Properties

        public static void RegisterExtension(Func<string, object?, Action<object>?, DataInputWrapperVM?> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);
            _extensions.Add(factory);
        }

        public static DataInputWrapperVM? CreateTypeWrapper<T>(string description, T value, Action<object?>? action = null)
        {
            DataInputWrapperVM? wrapper = value switch
            {
                bool b => new InputBoolVM(description, b, v => action?.Invoke(v)),
                Color c => new InputColorVM(description, c, v => action?.Invoke(v)),
                double d => new InputDoubleVM(description, d, v => action?.Invoke(v)),
                Enum e => new InputEnumVM(description, e, v => action?.Invoke(v)),
                float f => new InputFloatVM(description, f, v => action?.Invoke(v)),
                int i => new InputIntegerVM(description, i, v => action?.Invoke(v)),
                uint u => new InputUnsignedIntegerVM(description, u, v => action?.Invoke(v)),
                string s => new InputStringVM(description, s, v => action?.Invoke(v)),
                DateTime dt => new InputDateTimeVM(description, dt, dt.Kind, v => action?.Invoke(v)),
                TimeSpan ts => new InputTimeSpanVM(description, ts, v => action?.Invoke(v)),
                FloatRangeInput fr => new InputFloatRangeVM(description, fr, v => action?.Invoke(v)),
                StringSelection ss => new InputStringComboVM(description, ss.Value, ss.Options, v =>
                {
                    ss.Value = v ?? string.Empty;
                    action?.Invoke(ss);
                }),
                DirectoryInfo di => new InputFolderVM(description, di, v => action?.Invoke(v)),
                _ => null
            };

            if (wrapper != null)
                return wrapper;

            return _extensions
                .Select(ext => ext(description, value, action))
                .FirstOrDefault(result => result != null);
        }

    }

    public interface IDataInputWrapperVM
    {
        object? UntypedValue { get; set; }
    }

    public class DataInputWrapperVM<T> : DataInputWrapperVM, IDataInputWrapperVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Action<T?>? OnChange { get; set; }

        #region Value
        // ReSharper disable once InconsistentNaming
        protected T? _value;
        public virtual T? Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                OnChange?.Invoke(value);
            }
        }
        #endregion

        public object? UntypedValue
        {
            get => Value;
            set
            {
                if (value is null)
                {
                    Value = default!;
                    return;
                }

                if (value is T t)
                {
                    Value = t;
                    return;
                }

                Value = (T)Convert.ChangeType(value, typeof(T));
            }
        }

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

        public DataInputWrapperVM()
        {
            _value = default!;
        }

        public DataInputWrapperVM(string description, T? value, Action<T?>? onChange)
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
