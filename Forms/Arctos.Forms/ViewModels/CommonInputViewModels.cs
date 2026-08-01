using Arctos;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Arctos.Forms;

public sealed class DisplayObjectVM : DataInputWrapperVM<object>
{
    private string _displayMember = string.Empty;

    public DisplayObjectVM(string description, object value, string displayMember, Action<object?> onChange)
        : base(description, value, onChange)
    {
        DisplayMember = displayMember;
    }

    public string DisplayMember
    {
        get => _displayMember;
        set => SetProperty(ref _displayMember, value);
    }
}

public sealed class InputBoolVM : DataInputWrapperVM<bool>
{
    public InputBoolVM(string description, bool value, Action<bool>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputBoolVM(string description, bool value, Action<DataInputWrapperVM<bool>, bool> onChange)
        : base(description, value, onChange)
    {
    }
}

public sealed class InputDateTimeVM : DataInputWrapperVM<DateTime>
{
    private static readonly string[] Formats =
    [
        "d/M/yyyy h:mm tt",
        "dd/M/yyyy h:mm tt",
        "d/MM/yyyy h:mm tt",
        "dd/MM/yyyy h:mm tt",
        "d/M/yyyy h:mm:ss tt",
        "dd/M/yyyy h:mm:ss tt",
        "d/MM/yyyy h:mm:ss tt",
        "dd/MM/yyyy h:mm:ss tt"
    ];

    private readonly DateTimeKind _kind;
    private string _dateTimeString = string.Empty;

    public InputDateTimeVM(string description, DateTime value, DateTimeKind kind, Action<DateTime>? onChange)
        : base(description, value, onChange)
    {
        _kind = kind;
        _value = DateTime.SpecifyKind(value, _kind);
        _dateTimeString = _value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }

    public InputDateTimeVM(string description, DateTime value, DateTimeKind kind, Action<DataInputWrapperVM<DateTime>, DateTime> onChange)
        : base(description, value, (Action<DateTime>?)null)
    {
        _kind = kind;
        OnChangeWithSender = onChange;
        _value = DateTime.SpecifyKind(value, _kind);
        _dateTimeString = _value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
    }

    public string DateTimeString
    {
        get => _dateTimeString;
        set
        {
            if (_dateTimeString == value)
                return;

            var normalized = value.Replace(":00:00 ", ":00 ", StringComparison.Ordinal);
            SetProperty(ref _dateTimeString, normalized);

            if (!DateTime.TryParseExact(
                    normalized,
                    Formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var parsedDateTime))
            {
                return;
            }

            Value = DateTime.SpecifyKind(parsedDateTime, _kind);
        }
    }

    public override DateTime Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<DateTime>.Default.Equals(_value, value))
                return;

            _value = value;
            SetProperty(ref _dateTimeString, value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture), nameof(DateTimeString));
            OnPropertyChanged();
            OnChange?.Invoke(value);
            OnChangeWithSender?.Invoke(this, value);
            NotifyInputChanged();
        }
    }

    public void UpdateDateTime(DateTime value)
    {
        UpdateValue(value);
        SetProperty(ref _dateTimeString, value.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture), nameof(DateTimeString));
    }
}

public sealed class InputDoubleVM : DataInputWrapperVM<double>
{
    public InputDoubleVM(string description, double value, Action<double>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputDoubleVM(string description, double value, Action<DataInputWrapperVM<double>, double> onChange)
        : base(description, value, onChange)
    {
    }
}

public sealed class InputEnumVM : DataInputWrapperVM<Enum>
{
    public InputEnumVM(string description, Enum value, Action<Enum?>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputEnumVM(string description, Enum value, Action<DataInputWrapperVM<Enum>, Enum?> onChange)
        : base(description, value, onChange)
    {
    }
}

public sealed class InputFloatVM : DataInputWrapperVM<float>
{
    public InputFloatVM(string description, float value, Action<float>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputFloatVM(string description, float value, Action<DataInputWrapperVM<float>, float> onChange)
        : base(description, value, onChange)
    {
    }
}

public sealed class InputFloatRangeVM : DataInputWrapperVM<FloatRangeInput>
{
    private string _stringValue = string.Empty;

    public InputFloatRangeVM(string description, FloatRangeInput value, Action<FloatRangeInput?>? onChange)
        : base(description, value, (Action<FloatRangeInput?>?)null)
    {
        value.Wrapper = this;
        _stringValue = value.Value.ToString(CultureInfo.CurrentCulture);
        OnChange = input =>
        {
            SyncStringValue(input);
            onChange?.Invoke(input);
        };
    }

    public InputFloatRangeVM(string description, FloatRangeInput value, Action<DataInputWrapperVM<FloatRangeInput>, FloatRangeInput?> onChange)
        : base(description, value, (Action<FloatRangeInput>?)null)
    {
        value.Wrapper = this;
        _stringValue = value.Value.ToString(CultureInfo.CurrentCulture);
        OnChange = input =>
        {
            SyncStringValue(input);
            OnChangeWithSender?.Invoke(this, input);
        };
        OnChangeWithSender = onChange;
    }

    public string StringValue
    {
        get => _stringValue;
        set
        {
            if (_stringValue == value)
                return;

            if (Value != null
                && float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsedValue)
                && ValidateValue(parsedValue).IsValid)
            {
                Value.Value = parsedValue;
            }

            SetProperty(ref _stringValue, value);
        }
    }

    public void UpdateRange(FloatRangeInput value)
    {
        value.Wrapper = this;
        UpdateValue(value);
        SetProperty(ref _stringValue, value.Value.ToString(CultureInfo.CurrentCulture), nameof(StringValue));
    }

    public InputValidationResult ValidateValue(float value)
    {
        if (Value == null)
            return InputValidationResult.Invalid("Unable to validate the float range.");

        return InputValidators.ValidateFloatRange(value, Value.Min, Value.Max);
    }

    public InputValidationResult ValidateRange()
    {
        return InputValidators.ValidateFloatRange(Value);
    }

    public InputValidationResult ValidateText(string? text)
    {
        if (Value == null)
            return InputValidationResult.Invalid("Unable to validate the float range.");

        return InputValidators.ValidateFloatRange(text, Value.Min, Value.Max, CultureInfo.CurrentCulture);
    }

    private void SyncStringValue(FloatRangeInput? input)
    {
        if (input == null)
            return;

        SetProperty(ref _stringValue, input.Value.ToString(CultureInfo.CurrentCulture), nameof(StringValue));
    }
}

public sealed class InputInfinityIntVM : DataInputWrapperVM<int>
{
    private string _stringValue = string.Empty;

    public InputInfinityIntVM(string description, int value, Action<int>? onChange)
        : base(description, value, onChange)
    {
        StringValue = value switch
        {
            int.MaxValue => "+i",
            int.MinValue => "-i",
            _ => value.ToString(CultureInfo.InvariantCulture)
        };
    }

    public string StringValue
    {
        get => _stringValue;
        set
        {
            var normalized = value ?? string.Empty;
            if (_stringValue == normalized)
                return;

            switch (normalized)
            {
                case "i":
                case "+i":
                case "+\u221E":
                    Value = int.MaxValue;
                    normalized = "+\u221E";
                    break;
                case "-i":
                case "-\u221E":
                    Value = int.MinValue;
                    normalized = "-\u221E";
                    break;
                case "+":
                case "-":
                    break;
                default:
                    if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue))
                        Value = parsedValue;
                    break;
            }

            SetProperty(ref _stringValue, normalized);
        }
    }

    public InputValidationResult ValidateText(string? text)
    {
        return InputValidators.ValidateInfinityInteger(text, CultureInfo.InvariantCulture);
    }
}

public sealed class InputIntegerVM : DataInputWrapperVM<int>
{
    public InputIntegerVM(string description, int value, Action<int>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputIntegerVM(string description, int value, Action<DataInputWrapperVM<int>, int> onChange)
        : base(description, value, onChange)
    {
    }
}

public sealed class InputIntRangeExtraVM : DataInputWrapperVM<string>
{
    private readonly Dictionary<char, string> _specialValues;
    private string _stringValue = string.Empty;

    public InputIntRangeExtraVM(string description, string? value, int min, int max, Action<string?>? onChange, params (char Key, string Text)[] specialValues)
        : base(description, null, onChange)
    {
        Min = Math.Min(min, max);
        Max = Math.Max(min, max);
        _specialValues = BuildSpecialValueMap(specialValues);
        SpecialValues = _specialValues.Select(pair => (pair.Key, pair.Value)).ToList();

        var initialValue = InputValidators.NormalizeIntegerRangeExtraInitialValue(value, Min, Max, SpecialValues);
        _value = initialValue;
        _stringValue = initialValue;
    }

    public InputIntRangeExtraVM(string description, string? value, int min, int max, Action<DataInputWrapperVM<string>, string?> onChange, params (char Key, string Text)[] specialValues)
        : this(description, value, min, max, (Action<string?>?)null, specialValues)
    {
        OnChangeWithSender = onChange;
    }

    public int Min { get; }

    public int Max { get; }

    public IReadOnlyList<(char Key, string Text)> SpecialValues { get; }

    public string AllowedValuesDescription
    {
        get => InputValidators.GetIntegerRangeExtraDescription(Min, Max, SpecialValues);
    }

    public string StringValue
    {
        get => _stringValue;
        set
        {
            var normalized = InputValidators.NormalizeIntegerRangeExtraInput(value, SpecialValues);
            if (_stringValue == normalized)
                return;

            if (InputValidators.TryGetIntegerRangeExtraCommittedValue(normalized, Min, Max, SpecialValues, out var committedValue))
                Value = committedValue;

            SetProperty(ref _stringValue, normalized);
        }
    }

    public bool IsValidText(string? text, out string error)
    {
        var result = InputValidators.ValidateIntegerRangeExtra(text, Min, Max, SpecialValues);
        if (result.IsValid)
        {
            error = string.Empty;
            return true;
        }

        error = result.FirstError ?? AllowedValuesDescription;
        return false;
    }

    public void UpdateText(string? value)
    {
        var normalized = InputValidators.NormalizeIntegerRangeExtraInput(value, SpecialValues);
        if (InputValidators.TryGetIntegerRangeExtraCommittedValue(normalized, Min, Max, SpecialValues, out var committedValue))
            UpdateValue(committedValue);

        SetProperty(ref _stringValue, normalized, nameof(StringValue));
    }

    public InputValidationResult ValidateText(string? text)
    {
        return InputValidators.ValidateIntegerRangeExtra(text, Min, Max, SpecialValues);
    }

    private static Dictionary<char, string> BuildSpecialValueMap(IEnumerable<(char Key, string Text)> specialValues)
    {
        var result = new Dictionary<char, string>();

        foreach (var (key, text) in specialValues)
        {
            if (char.IsWhiteSpace(key) || text == null)
                continue;

            result[char.ToLowerInvariant(key)] = text;
        }

        return result;
    }

}

public sealed class InputIntRangeVM : DataInputWrapperVM<IntRangeInput>
{
    private string _stringValue = string.Empty;

    public InputIntRangeVM(string description, IntRangeInput value, Action<IntRangeInput?>? onChange)
        : base(description, value, onChange)
    {
        value.Wrapper = this;
        StringValue = value.Value.ToString(CultureInfo.InvariantCulture);
    }

    public InputIntRangeVM(string description, IntRangeInput value, Action<DataInputWrapperVM<IntRangeInput>, IntRangeInput?> onChange)
        : base(description, value, onChange)
    {
        value.Wrapper = this;
        OnChange = input => OnChangeWithSender?.Invoke(this, input);
        StringValue = value.Value.ToString(CultureInfo.InvariantCulture);
    }

    public string StringValue
    {
        get => _stringValue;
        set
        {
            if (_stringValue == value)
                return;

            if (Value != null
                && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue)
                && parsedValue >= Value.Min
                && parsedValue <= Value.Max)
            {
                Value.Value = parsedValue;
            }

            SetProperty(ref _stringValue, value);
        }
    }

    public void UpdateRange(IntRangeInput value)
    {
        value.Wrapper = this;
        UpdateValue(value);
        SetProperty(ref _stringValue, value.Value.ToString(CultureInfo.InvariantCulture), nameof(StringValue));
    }

    public InputValidationResult ValidateText(string? text)
    {
        if (Value == null)
            return InputValidationResult.Invalid("Unable to validate the integer range.");

        return InputValidators.ValidateIntegerRange(text, Value.Min, Value.Max, CultureInfo.InvariantCulture);
    }
}

public sealed class InputLabelVM : DataInputWrapperVM<string>
{
    public InputLabelVM(string description, string? value, string justify = "Left")
        : base(description, value, (Action<string?>?)null)
    {
        Justify = justify;
    }

    public string Justify { get; set; }
}

public interface IInputMatrixVM
{
    int Rows { get; }

    int Columns { get; }

    ObservableCollection<InputMatrixCellVM> Cells { get; }
}

public sealed class InputMatrixVM<T> : DataInputWrapperVM<T[]>, IInputMatrixVM
{
    private readonly Func<T?, string> _formatter;
    private readonly Func<string, (bool success, T? value)> _parser;
    private bool _isSynchronizingCells;

    public InputMatrixVM(string description, int rows, int columns, T[]? data, Action<T[]?>? onChange)
        : this(description, rows, columns, data, onChange, null, null)
    {
    }

    public InputMatrixVM(string description, int rows, int columns, T[]? data, Action<DataInputWrapperVM<T[]>, T[]?> onChange)
        : this(description, rows, columns, data, (Action<T[]?>?)null, null, null)
    {
        OnChangeWithSender = onChange;
    }

    public InputMatrixVM(
        string description,
        int rows,
        int columns,
        T[]? data,
        Action<T[]?>? onChange,
        Func<T?, string>? formatter,
        Func<string, (bool success, T? value)>? parser)
        : base(description, Normalize(data, rows, columns), onChange)
    {
        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "Rows must be greater than zero.");

        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns), columns, "Columns must be greater than zero.");

        Rows = rows;
        Columns = columns;
        _formatter = formatter ?? GetDefaultFormatter();
        _parser = parser ?? GetDefaultParser();
        BuildCells();
    }

    public int Rows { get; }

    public int Columns { get; }

    public ObservableCollection<InputMatrixCellVM> Cells { get; } = [];

    public override T[]? Value
    {
        get => base.Value;
        set
        {
            var normalized = Normalize(value, Rows, Columns);
            base.Value = normalized;

            if (_isSynchronizingCells)
                return;

            SyncCellsFromValue(normalized);
        }
    }

    public void UpdateMatrix(T[]? values)
    {
        var normalized = Normalize(values, Rows, Columns);
        UpdateValue(normalized);
        SyncCellsFromValue(normalized);
    }

    private void BuildCells()
    {
        Cells.Clear();

        var values = Value ?? Normalize(null, Rows, Columns);
        for (var row = 0; row < Rows; row++)
        {
            for (var column = 0; column < Columns; column++)
            {
                var index = ToIndex(row, column);
                Cells.Add(new InputMatrixCellVM(row, column, _formatter(values[index]), OnCellTextChanged));
            }
        }
    }

    private void OnCellTextChanged(InputMatrixCellVM cell, string text)
    {
        if (_isSynchronizingCells)
            return;

        var (success, value) = _parser(text);
        if (!success)
        {
            var existingValues = Value ?? Normalize(null, Rows, Columns);
            cell.SetTextSilently(_formatter(existingValues[ToIndex(cell.Row, cell.Column)]));
            return;
        }

        var values = Value is { Length: > 0 }
            ? [.. Value]
            : Normalize(null, Rows, Columns);

        values[ToIndex(cell.Row, cell.Column)] = value!;

        _isSynchronizingCells = true;
        base.Value = values;
        _isSynchronizingCells = false;
    }

    private void SyncCellsFromValue(T[] values)
    {
        _isSynchronizingCells = true;

        for (var index = 0; index < Math.Min(values.Length, Cells.Count); index++)
            Cells[index].SetTextSilently(_formatter(values[index]));

        _isSynchronizingCells = false;
    }

    private int ToIndex(int row, int column)
    {
        return (row * Columns) + column;
    }

    private static T[] Normalize(T[]? value, int rows, int columns)
    {
        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, "Rows must be greater than zero.");

        if (columns <= 0)
            throw new ArgumentOutOfRangeException(nameof(columns), columns, "Columns must be greater than zero.");

        var length = rows * columns;
        if (value == null || value.Length == 0)
            return new T[length];

        if (value.Length == length)
            return [.. value];

        return value
            .Concat(Enumerable.Repeat(default(T)!, Math.Max(0, length - value.Length)))
            .Take(length)
            .ToArray();
    }

    private static Func<T?, string> GetDefaultFormatter()
    {
        return typeof(T) switch
        {
            var type when type == typeof(double) => value => value is null ? string.Empty : ((double)(object)value).ToString("N4", CultureInfo.CurrentCulture),
            var type when type == typeof(float) => value => value is null ? string.Empty : ((float)(object)value).ToString("N4", CultureInfo.CurrentCulture),
            var type when type == typeof(int) => value => value?.ToString() ?? string.Empty,
            var type when type == typeof(uint) => value => value?.ToString() ?? string.Empty,
            var type when type == typeof(string) => value => value as string ?? string.Empty,
            _ => value => value?.ToString() ?? string.Empty,
        };
    }

    private static Func<string, (bool success, T? value)> GetDefaultParser()
    {
        return typeof(T) switch
        {
            var type when type == typeof(double) => text => double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsedValue)
                ? (true, (T?)(object?)parsedValue)
                : (false, default),
            var type when type == typeof(float) => text => float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var parsedFloat)
                ? (true, (T?)(object?)parsedFloat)
                : (false, default),
            var type when type == typeof(int) => text => int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedInt)
                ? (true, (T?)(object?)parsedInt)
                : (false, default),
            var type when type == typeof(uint) => text => uint.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedUInt)
                ? (true, (T?)(object?)parsedUInt)
                : (false, default),
            var type when type == typeof(string) => text => (true, (T?)(object?)text),
            _ => throw new NotSupportedException($"InputMatrixVM does not provide a default parser for '{typeof(T).Name}'. Supply custom formatter and parser delegates."),
        };
    }
}

public sealed class InputMatrixCellVM : BaseVM
{
    private readonly Action<InputMatrixCellVM, string> _onChange;
    private bool _suppressChange;
    private string _text = string.Empty;

    public InputMatrixCellVM(int row, int column, string text, Action<InputMatrixCellVM, string> onChange)
    {
        Row = row;
        Column = column;
        _text = text ?? string.Empty;
        _onChange = onChange ?? throw new ArgumentNullException(nameof(onChange));
    }

    public int Row { get; }

    public int Column { get; }

    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value ?? string.Empty))
                return;

            if (_suppressChange)
                return;

            _onChange(this, _text);
        }
    }

    internal void SetTextSilently(string text)
    {
        _suppressChange = true;
        Text = text ?? string.Empty;
        _suppressChange = false;
    }
}

public sealed class InputObjectComboVM : DataInputWrapperVM<object>
{
    private string _displayMember = string.Empty;

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

    public ObservableCollection<object> Options { get; set; }

    public string DisplayMember
    {
        get => _displayMember;
        set => SetProperty(ref _displayMember, value);
    }
}

public sealed class InputStringComboVM : DataInputWrapperVM<string>
{
    private bool _suppressNotifications;

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

    public ObservableCollection<string> Options { get; set; }

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
}

public sealed class InputStringListBoxVM : DataInputWrapperVM<string>
{
    public InputStringListBoxVM(string description, string? value, IEnumerable<string> options, Action<string?>? onChange)
        : base(description, value, onChange)
    {
        Options = options;
    }

    public IEnumerable<string> Options { get; set; }
}

public sealed class InputStringListVM : DataInputWrapperVM<List<string>>
{
    public sealed class StringListItemVM(Action onChanged) : BaseVM
    {
        private string _value = string.Empty;
        private bool _suppressChange;

        public string Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    if (!_suppressChange)
                        onChanged();
                }
            }
        }

        public void SetValueSilently(string? value)
        {
            _suppressChange = true;
            Value = value ?? string.Empty;
            _suppressChange = false;
        }
    }

    private StringListItemVM? _selectedItem;
    private readonly Action<string?>? _onSelectedItemChanged;
    private bool _suppressNotifications;
    private RelayCommand? _addCommand;
    private RelayCommand? _removeCommand;
    private RelayCommand? _moveUpCommand;
    private RelayCommand? _moveDownCommand;

    public InputStringListVM(string description, IEnumerable<string>? value, string? selectedValue, Action<List<string>?>? onChange, Action<string?>? onSelectedItemChanged = null)
        : base(description, value?.ToList() ?? [], onChange)
    {
        _onSelectedItemChanged = onSelectedItemChanged;
        _suppressNotifications = true;

        foreach (var item in Value ?? [])
            Items.Add(CreateItem(item));

        SelectedItem = Items.FirstOrDefault(item => item.Value == (selectedValue ?? string.Empty)) ?? Items.FirstOrDefault();
        _suppressNotifications = false;
    }

    public ObservableCollection<StringListItemVM> Items { get; } = [];

    public StringListItemVM? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (!SetProperty(ref _selectedItem, value))
                return;

            if (!_suppressNotifications)
                _onSelectedItemChanged?.Invoke(value?.Value);

            RaisePropertyChanged(nameof(CanRemove));
            RaisePropertyChanged(nameof(CanMoveUp));
            RaisePropertyChanged(nameof(CanMoveDown));
        }
    }

    public bool CanRemove => SelectedItem != null;

    public bool CanMoveUp => SelectedItem != null && Items.IndexOf(SelectedItem) > 0;

    public bool CanMoveDown => SelectedItem != null && Items.IndexOf(SelectedItem) >= 0 && Items.IndexOf(SelectedItem) < Items.Count - 1;

    public RelayCommand AddCommand => _addCommand ??= new RelayCommand(AddItem);

    public RelayCommand RemoveCommand => _removeCommand ??= new RelayCommand(RemoveSelectedItem);

    public RelayCommand MoveUpCommand => _moveUpCommand ??= new RelayCommand(MoveSelectedItemUp);

    public RelayCommand MoveDownCommand => _moveDownCommand ??= new RelayCommand(MoveSelectedItemDown);

    public void UpdateItems(IEnumerable<string>? items, string? selectedValue)
    {
        _suppressNotifications = true;
        Items.Clear();

        foreach (var item in items ?? [])
            Items.Add(CreateItem(item));

        UpdateValue(Items.Select(item => item.Value).ToList());
        SelectedItem = Items.FirstOrDefault(item => item.Value == (selectedValue ?? string.Empty)) ?? Items.FirstOrDefault();

        _suppressNotifications = false;
        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private void AddItem()
    {
        var item = CreateItem(string.Empty);
        Items.Add(item);

        _suppressNotifications = true;
        SelectedItem = item;
        _suppressNotifications = false;

        SyncValueFromItems();
    }

    private void RemoveSelectedItem()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0)
            return;

        _suppressNotifications = true;
        Items.RemoveAt(index);
        SelectedItem = Items.Count == 0 ? null : index <= 0 ? Items[0] : Items[index - 1];
        _suppressNotifications = false;
        SyncValueFromItems();
    }

    private void MoveSelectedItemUp()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index <= 0)
            return;

        _suppressNotifications = true;
        Items.Move(index, index - 1);
        _suppressNotifications = false;
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private void MoveSelectedItemDown()
    {
        if (SelectedItem == null)
            return;

        var index = Items.IndexOf(SelectedItem);
        if (index < 0 || index >= Items.Count - 1)
            return;

        _suppressNotifications = true;
        Items.Move(index, index + 1);
        _suppressNotifications = false;
        SyncValueFromItems();
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }

    private StringListItemVM CreateItem(string? value)
    {
        var item = new StringListItemVM(SyncValueFromItems);
        item.SetValueSilently(value);
        return item;
    }

    private void SyncValueFromItems()
    {
        if (!_suppressNotifications)
            _onSelectedItemChanged?.Invoke(SelectedItem?.Value);

        Value = [.. Items.Select(item => item.Value)];
        RaisePropertyChanged(nameof(CanRemove));
        RaisePropertyChanged(nameof(CanMoveUp));
        RaisePropertyChanged(nameof(CanMoveDown));
    }
}

public sealed class InputStringVM : DataInputWrapperVM<string>
{
    private bool _isMultiLine;
    private int _showMaxLines = 1;

    public InputStringVM(string description, string? value, Action<string?>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputStringVM(string description, string? value, Action<DataInputWrapperVM<string>, string?>? onChange)
        : base(description, value, onChange)
    {
    }

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
}

public sealed class InputTextAreaVM : DataInputWrapperVM<string>
{
    public InputTextAreaVM(string description, string? value, Action<string?>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputTextAreaVM(string description, string? value, Action<DataInputWrapperVM<string>, string?> onChange)
        : this(description, value, (Action<string?>?)null)
    {
        OnChangeWithSender = onChange;
    }
}

public sealed class InputTimeSpanVM : DataInputWrapperVM<TimeSpan>
{
    public InputTimeSpanVM(string description, TimeSpan value, Action<TimeSpan>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputTimeSpanVM(string description, TimeSpan value, Action<DataInputWrapperVM<TimeSpan>, TimeSpan> onChange)
        : base(description, value, onChange)
    {
    }

    public InputValidationResult ValidateText(string? text)
    {
        return InputValidators.ValidateTimeSpan(text, CultureInfo.InvariantCulture);
    }
}

public sealed class InputUnsignedIntegerVM : DataInputWrapperVM<uint>
{
    public InputUnsignedIntegerVM(string description, uint value, Action<uint>? onChange)
        : base(description, value, onChange)
    {
    }

    public InputUnsignedIntegerVM(string description, uint value, Action<DataInputWrapperVM<uint>, uint> onChange)
        : base(description, value, onChange)
    {
    }
}
