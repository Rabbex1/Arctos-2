using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;
using System.Linq;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a portable input component inside a node.
/// </summary>
public class NodeInputComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly InputTypeRegistry _registry;

    private readonly record struct InputState(InputTypeId TypeId, object? DefaultValue, DateTimeDisplayMode DateTimeDisplayMode);
    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the registered input type identifier used by this component.
    /// </summary>
    public InputTypeId TypeId
    {
        get => InputModel.TypeId;
        set
        {
            if (InputModel.TypeId == value)
                return;

            if (!_registry.TryGetValue(value, out var definition) || definition == null)
                return;

            var previousState = CaptureState();
            var nextState = new InputState(value, CloneInputValue(CreateDefaultValue(definition)), DateTimeDisplayMode);

            ApplyComponentChange(
                "Change input type",
                previousState,
                nextState,
                ApplyState);
        }
    }

    /// <summary>
    /// Gets the current default value stored by the component.
    /// </summary>
    public object? DefaultValue => InputModel.GetDefaultValue(_registry);

    /// <summary>
    /// Gets or sets how DateTime inputs are displayed.
    /// </summary>
    public DateTimeDisplayMode DateTimeDisplayMode
    {
        get => InputModel.DateTimeDisplayMode;
        set
        {
            if (InputModel.DateTimeDisplayMode == value)
                return;

            ApplyComponentChange(
                "Change date time display mode",
                InputModel.DateTimeDisplayMode,
                value,
                displayMode =>
                {
                    InputModel.DateTimeDisplayMode = displayMode;
                    OnPropertyChanged();
                    RebuildEditTabs();
                });
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node input component view model.
    /// </summary>
    /// <param name="model">The persisted input model to wrap.</param>
    /// <param name="registry">The input type registry used to resolve the component type.</param>
    public NodeInputComponentVM(NodeInput? model = null, InputTypeRegistry? registry = null)
        : base(model ?? new NodeInput
        {
            Label = "Input",
            DisplayName = "Input",
            TypeId = InputTypeIds.String,
        }, PaletteItemKind.Input, "Input", supportsId: true)
    {
        _registry = registry ?? InputTypeRegistry.Default;

        if (InputModel.TypeId == default)
            InputModel.TypeId = InputTypeIds.String;

        if (InputModel.DefaultValue == null)
            InputModel.SetDefaultValue(CreateDefaultValue(CurrentInputType), _registry);

        RebuildEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        List<DataInputWrapperVM> inputs =
        [
            new InputStringVM("Label:", Label, (wrapper, label) =>
            {
                var value = label ?? string.Empty;
                ApplyComponentChange(
                    "Change component label",
                    Model.Label,
                    value,
                    resolvedLabel =>
                    {
                        Model.Label = resolvedLabel;
                        wrapper.UpdateValue(resolvedLabel);
                        DisplayName = GetDisplayName();
                        OnPropertyChanged(nameof(Label));
                    });
            }),

            new InputObjectComboVM(
                "Input Type:",
                CurrentInputType,
                GetSelectableInputTypes().ToList(),
                nameof(InputTypeDefinition.DisplayName),
                (_, selectedType) =>
                {
                    if (selectedType is not InputTypeDefinition inputType)
                        return;

                    TypeId = inputType.Id;
                }),
        ];

        if (TypeId == InputTypeIds.DateTime)
        {
            inputs.Add(new InputStringComboVM(
                "Display Mode:",
                GetDateTimeDisplayModeLabel(DateTimeDisplayMode),
                GetDateTimeDisplayModeLabels(),
                (_, selectedMode) =>
                {
                    if (TryParseDateTimeDisplayMode(selectedMode, out var displayMode))
                        DateTimeDisplayMode = displayMode;
                }));
        }

        var defaultValueWrapper = DataInputWrapperVM.CreateTypeWrapper("Default Value:", ConvertToEditorValue(DefaultValue), (wrapper, value) =>
        {
            var oldValue = CloneInputValue(DefaultValue);
            var newValue = CloneInputValue(ConvertFromEditorValue(value));

            if (InputValuesEqual(oldValue, newValue))
                return;

            ApplyComponentChange(
                "Change input default value",
                oldValue,
                newValue,
                currentValue =>
                {
                    InputModel.SetDefaultValue(CloneInputValue(currentValue), _registry);
                    UpdateEditorWrapper(wrapper, ConvertToEditorValue(currentValue));
                    OnPropertyChanged(nameof(DefaultValue));
                });
        });

        if (defaultValueWrapper != null)
            inputs.Add(defaultValueWrapper);

        return inputs;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    protected InputTypeRegistry Registry => _registry;

    protected virtual IEnumerable<object> GetSelectableInputTypes()
    {
        return _registry
            .Where(definition => definition.Id != InputTypeIds.StringSelection)
            .Cast<object>();
    }

    protected InputTypeDefinition CurrentInputType
    {
        get
        {
            if (_registry.TryGetValue(TypeId, out var definition) && definition != null)
                return definition;

            throw new InvalidOperationException($"The input type '{TypeId}' has not been registered.");
        }
    }

    protected static object CreateDefaultValue(InputTypeDefinition definition)
    {
        return definition.ValueType switch
        {
            var valueType when valueType == typeof(bool) => false,
            var valueType when valueType == typeof(int) => 0,
            var valueType when valueType == typeof(uint) => 0U,
            var valueType when valueType == typeof(float) => 0F,
            var valueType when valueType == typeof(double) => 0D,
            var valueType when valueType == typeof(string) => string.Empty,
            var valueType when valueType == typeof(DateTime) => DateTime.Now,
            var valueType when valueType == typeof(TimeSpan) => TimeSpan.Zero,
            var valueType when valueType == typeof(NodeColor) => NodeColor.White,
            var valueType when valueType == typeof(StringSelectionValue) => new StringSelectionValue(),
            var valueType when valueType == typeof(FloatRangeValue) => new FloatRangeValue(),
            var valueType when valueType == typeof(IntRangeValue) => new IntRangeValue(),
            _ => Activator.CreateInstance(definition.ValueType)
                 ?? throw new InvalidOperationException($"Unable to create a default value for '{definition.ValueType.Name}'."),
        };
    }

    protected object? ConvertFromEditorValue(object? value)
    {
        return value switch
        {
            StringSelection selection => new StringSelectionValue
            {
                Source = StringSelectionSource.Internal,
                Options = [.. selection.Options],
                Value = selection.Value,
            },
            FloatRangeInput range => new FloatRangeValue
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            IntRangeInput range => new IntRangeValue
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            _ => value,
        };
    }

    protected object? ConvertToEditorValue(object? value)
    {
        return value switch
        {
            StringSelectionValue selection => new StringSelection
            {
                Options = [.. selection.Options],
                Value = selection.Value ?? string.Empty,
            },
            FloatRangeValue range => new FloatRangeInput
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            IntRangeValue range => new IntRangeInput
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            _ => value,
        };
    }

    protected void SetDefaultValue(object? value)
    {
        InputModel.SetDefaultValue(value, _registry);
        OnPropertyChanged(nameof(DefaultValue));
        NotifyComponentChanged();
    }

    private InputState CaptureState()
    {
        return new InputState(TypeId, CloneInputValue(DefaultValue), DateTimeDisplayMode);
    }

    private void ApplyState(InputState state)
    {
        InputModel.TypeId = state.TypeId;
        InputModel.SetDefaultValue(CloneInputValue(state.DefaultValue), _registry);
        InputModel.DateTimeDisplayMode = state.DateTimeDisplayMode;
        OnPropertyChanged(nameof(TypeId));
        OnPropertyChanged(nameof(DefaultValue));
        OnPropertyChanged(nameof(DateTimeDisplayMode));
        RebuildEditTabs();
    }

    protected static object? CloneInputValue(object? value)
    {
        return value switch
        {
            null => null,
            StringSelectionValue selection => new StringSelectionValue
            {
                Source = selection.Source,
                Options = [.. selection.Options ?? []],
                Value = selection.Value,
            },
            FloatRangeValue range => new FloatRangeValue
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            IntRangeValue range => new IntRangeValue
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            StringSelection selection => new StringSelection
            {
                Options = [.. selection.Options],
                Value = selection.Value,
            },
            FloatRangeInput range => new FloatRangeInput
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            IntRangeInput range => new IntRangeInput
            {
                Min = range.Min,
                Max = range.Max,
                Value = range.Value,
            },
            _ => value,
        };
    }

    protected static bool InputValuesEqual(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left == null || right == null)
            return false;

        return (left, right) switch
        {
            (StringSelectionValue a, StringSelectionValue b) => a.Source == b.Source
                && string.Equals(a.Value, b.Value, StringComparison.Ordinal)
                && (a.Options ?? []).SequenceEqual(b.Options ?? []),
            (FloatRangeValue a, FloatRangeValue b) => a.Min.Equals(b.Min)
                && a.Max.Equals(b.Max)
                && a.Value.Equals(b.Value),
            (IntRangeValue a, IntRangeValue b) => a.Min == b.Min
                && a.Max == b.Max
                && a.Value == b.Value,
            (StringSelection a, StringSelection b) => string.Equals(a.Value, b.Value, StringComparison.Ordinal)
                && a.Options.SequenceEqual(b.Options),
            (FloatRangeInput a, FloatRangeInput b) => a.Min.Equals(b.Min)
                && a.Max.Equals(b.Max)
                && a.Value.Equals(b.Value),
            (IntRangeInput a, IntRangeInput b) => a.Min == b.Min
                && a.Max == b.Max
                && a.Value == b.Value,
            _ => Equals(left, right),
        };
    }

    protected static void UpdateEditorWrapper(DataInputWrapperVM wrapper, object? value)
    {
        switch (wrapper)
        {
            case InputBoolVM boolWrapper when value is bool boolValue:
                boolWrapper.UpdateValue(boolValue);
                break;

            case InputColorVM colorWrapper when value is System.Windows.Media.Color colorValue:
                colorWrapper.UpdateColor(colorValue);
                break;

            case InputDateTimeVM dateTimeWrapper when value is DateTime dateTimeValue:
                dateTimeWrapper.UpdateDateTime(dateTimeValue);
                break;

            case InputDoubleVM doubleWrapper when value is double doubleValue:
                doubleWrapper.UpdateValue(doubleValue);
                break;

            case InputEnumVM enumWrapper when value is Enum enumValue:
                enumWrapper.UpdateValue(enumValue);
                break;

            case InputFloatRangeVM floatRangeWrapper when value is FloatRangeInput floatRangeValue:
                floatRangeWrapper.UpdateRange(floatRangeValue);
                break;

            case InputFloatVM floatWrapper when value is float floatValue:
                floatWrapper.UpdateValue(floatValue);
                break;

            case InputIntegerVM integerWrapper when value is int intValue:
                integerWrapper.UpdateValue(intValue);
                break;

            case InputIntRangeVM intRangeWrapper when value is IntRangeInput intRangeValue:
                intRangeWrapper.UpdateRange(intRangeValue);
                break;

            case InputObjectComboVM objectWrapper:
                objectWrapper.UpdateValue(value);
                break;

            case InputStringComboVM stringComboWrapper when value is string stringComboValue:
                stringComboWrapper.UpdateValue(stringComboValue);
                break;

            case InputStringVM stringWrapper when value is string stringValue:
                stringWrapper.UpdateValue(stringValue);
                break;

            case InputTimeSpanVM timeSpanWrapper when value is TimeSpan timeSpanValue:
                timeSpanWrapper.UpdateValue(timeSpanValue);
                break;

            case InputUnsignedIntegerVM unsignedIntegerWrapper when value is uint uintValue:
                unsignedIntegerWrapper.UpdateValue(uintValue);
                break;
        }
    }

    private static IEnumerable<string> GetDateTimeDisplayModeLabels()
    {
        return
        [
            "Date Only",
            "Time Only",
            "Both",
        ];
    }

    private static string GetDateTimeDisplayModeLabel(DateTimeDisplayMode mode)
    {
        return mode switch
        {
            DateTimeDisplayMode.DateOnly => "Date Only",
            DateTimeDisplayMode.TimeOnly => "Time Only",
            _ => "Both",
        };
    }

    private static bool TryParseDateTimeDisplayMode(string? label, out DateTimeDisplayMode mode)
    {
        switch (label)
        {
            case "Date Only":
                mode = DateTimeDisplayMode.DateOnly;
                return true;

            case "Time Only":
                mode = DateTimeDisplayMode.TimeOnly;
                return true;

            case "Both":
                mode = DateTimeDisplayMode.Both;
                return true;

            default:
                mode = DateTimeDisplayMode.Both;
                return false;
        }
    }

    protected NodeInput InputModel => (NodeInput)Model;

    #endregion Private Methods
}
