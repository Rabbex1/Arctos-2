using Arctos;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Windows.Input;

namespace Arctos.Forms;

public class DataInputWrapperVM : BaseVM
{
    private static readonly List<Func<string, object?, Action<DataInputWrapperVM, object?>?, DataInputWrapperVM?>> Extensions = [];

    private string _description = string.Empty;
    private bool _isReadOnly;
    private string _id = string.Empty;
    private bool _isEnabled = true;
    private bool _hide;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    public int ValueColumnSpan => Icon != null ? 1 : 2;

    public Action? OnInputChanged { get; set; }

    public bool Hide
    {
        get => _hide;
        set => SetProperty(ref _hide, value);
    }

    public static void RegisterExtension(Func<string, object?, Action<DataInputWrapperVM, object?>?, DataInputWrapperVM?> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        Extensions.Add(factory);
    }

    public static DataInputWrapperVM? CreateTypeWrapper<T>(string description, T value, Action<object?>? action = null)
    {
        return CreateTypeWrapper(description, value, (_, changedValue) => action?.Invoke(changedValue));
    }

    public static DataInputWrapperVM? CreateTypeWrapper<T>(string description, T value, Action<DataInputWrapperVM, object?>? action)
    {
        DataInputWrapperVM? wrapper = value switch
        {
            bool booleanValue => new InputBoolVM(description, booleanValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            double doubleValue => new InputDoubleVM(description, doubleValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            Enum enumValue => new InputEnumVM(description, enumValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            float floatValue => new InputFloatVM(description, floatValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            int intValue => new InputIntegerVM(description, intValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            uint uintValue => new InputUnsignedIntegerVM(description, uintValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            string stringValue => new InputStringVM(description, stringValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            DateTime dateTimeValue => new InputDateTimeVM(description, dateTimeValue, dateTimeValue.Kind, (input, changedValue) => action?.Invoke(input, changedValue)),
            TimeSpan timeSpanValue => new InputTimeSpanVM(description, timeSpanValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            FloatRangeInput floatRangeValue => new InputFloatRangeVM(description, floatRangeValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            IntRangeInput intRangeValue => new InputIntRangeVM(description, intRangeValue, (input, changedValue) => action?.Invoke(input, changedValue)),
            StringSelection stringSelection => new InputStringComboVM(description, stringSelection.Value, stringSelection.Options, (input, changedValue) =>
            {
                stringSelection.Value = changedValue ?? string.Empty;
                action?.Invoke(input, stringSelection);
            }),
            _ => null
        };

        if (wrapper != null)
            return wrapper;

        return Extensions
            .Select(extension => extension(description, value, action))
            .FirstOrDefault(result => result != null);
    }
}

public interface IDataInputWrapperVM
{
    object? UntypedValue { get; set; }
}

public class DataInputWrapperVM<T> : DataInputWrapperVM, IDataInputWrapperVM
{
    protected T? _value;
    private ICommand? _changeButtonCommand;

    public DataInputWrapperVM()
    {
        _value = default;
    }

    public DataInputWrapperVM(string description, T? value, Action<T?>? onChange)
    {
        Description = description;
        OnChange = onChange;
        _value = value;
    }

    public DataInputWrapperVM(string description, T? value, Action<DataInputWrapperVM<T>, T?>? onChange)
    {
        Description = description;
        OnChangeWithSender = onChange;
        _value = value;
    }

    public Action<T?>? OnChange { get; set; }

    public Action<DataInputWrapperVM<T>, T?>? OnChangeWithSender { get; set; }

    public virtual T? Value
    {
        get => _value;
        set
        {
            if (!SetProperty(ref _value, value))
                return;

            OnChange?.Invoke(value);
            OnChangeWithSender?.Invoke(this, value);
            NotifyInputChanged();
        }
    }

    public object? UntypedValue
    {
        get => Value;
        set
        {
            if (value is null)
            {
                Value = default;
                return;
            }

            if (value is T typedValue)
            {
                Value = typedValue;
                return;
            }

            Value = (T)Convert.ChangeType(value, typeof(T), CultureInfo.CurrentCulture);
        }
    }

    public Action<DataInputWrapperVM<T>>? OnButtonClicked { get; set; }

    public ICommand ChangeButtonCommand => _changeButtonCommand ??= new RelayCommand(() => OnButtonClicked?.Invoke(this), () => true);

    public void UpdateValue(T? value)
    {
        SetProperty(ref _value, value, nameof(Value));
    }

    protected void NotifyInputChanged()
    {
        OnInputChanged?.Invoke();
    }
}

public sealed class InputFormVM : BaseVM
{
    private ObservableCollection<DataInputWrapperVM> _inputs = [];
    private DataInputWrapperVM? _selectedInput;

    public InputFormVM()
    {
        _inputs.CollectionChanged += OnInputsCollectionChanged;
    }

    public Action? OnInputChanged { get; set; }

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

    public DataInputWrapperVM? SelectedInput
    {
        get => _selectedInput;
        set => SetProperty(ref _selectedInput, value);
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

public sealed class InputGroupVM : DataInputWrapperVM<(string header, List<DataInputWrapperVM> wrappers)>
{
    private readonly Action? _extraAction;
    private string _header = string.Empty;
    private bool _collapsable;

    public InputGroupVM(string description, (string header, List<DataInputWrapperVM> wrappers) value, bool collapsable = false, Action? extraAction = null)
        : base(description, value, (Action<(string header, List<DataInputWrapperVM> wrappers)>?)null)
    {
        Header = value.header;
        Collapsable = collapsable;
        _extraAction = extraAction;
        PropertiesForm.Inputs = new ObservableCollection<DataInputWrapperVM>(value.wrappers);
    }

    public string Header
    {
        get => _header;
        set => SetProperty(ref _header, value);
    }

    public bool Collapsable
    {
        get => _collapsable;
        set => SetProperty(ref _collapsable, value);
    }

    public InputFormVM PropertiesForm { get; set; } = new();

    public void GetProperties()
    {
        SortedList<int, PropertyInfo> publicProperties = [];

        var propInfos = Value.GetType().GetProperties();
        foreach (var propInfo in propInfos)
        {
            if (propInfo.SetMethod == null || propInfo.GetMethod == null)
                continue;

            var arctosInputAttr = (ArctosInputAttribute[])propInfo.GetCustomAttributes(typeof(ArctosInputAttribute), false);
            if (arctosInputAttr.Length != 0)
            {
                publicProperties.Add(arctosInputAttr[0].Position, propInfo);
                continue;
            }

            var arctosSettingAttr = (ArctosSettingAttribute[])propInfo.GetCustomAttributes(typeof(ArctosSettingAttribute), false);
            if (arctosSettingAttr.Length != 0)
                publicProperties.Add(arctosSettingAttr[0].Order, propInfo);
        }

        foreach (var property in publicProperties.Values)
        {
            var wrapper = CreateTypeWrapper(property.Name.SplitCamelCase(), property.GetValue(Value), (input, changedValue) =>
            {
                property.SetValue(Value, changedValue);
                _extraAction?.Invoke();
            });

            if (wrapper == null)
                continue;

            wrapper.Description = property.Name.SplitCamelCase() + ":";
            if (wrapper.Description == "Name:")
                wrapper.IsReadOnly = true;

            PropertiesForm.Inputs.Add(wrapper);
        }
    }
}
