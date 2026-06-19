using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;
using System.Linq;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a portable input component inside a node.
/// </summary>
public sealed class NodeInputComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly InputTypeRegistry _registry;
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

            InputModel.TypeId = value;
            InputModel.SetDefaultValue(CreateDefaultValue(CurrentInputType), _registry);
            OnPropertyChanged();
            OnPropertyChanged(nameof(DefaultValue));
            RebuildEditTabs();
            NotifyComponentChanged();
        }
    }

    /// <summary>
    /// Gets the current default value stored by the component.
    /// </summary>
    public object? DefaultValue => InputModel.GetDefaultValue(_registry);

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
            new InputStringVM("Label:", Label, label =>
            {
                Label = label ?? string.Empty;
            }),

            new InputObjectComboVM(
                "Input Type:",
                CurrentInputType,
                _registry.Cast<object>().ToList(),
                nameof(InputTypeDefinition.DisplayName),
                selectedType =>
                {
                    if (selectedType is not InputTypeDefinition inputType)
                        return;

                    TypeId = inputType.Id;
                }),
        ];

        if (TypeId == InputTypeIds.StringSelection)
        {
            AddStringSelectionInputs(inputs);
            return inputs;
        }

        var defaultValueWrapper = DataInputWrapperVM.CreateTypeWrapper("Default Value:", ConvertToEditorValue(DefaultValue), value =>
        {
            SetDefaultValue(ConvertFromEditorValue(value));
        });

        if (defaultValueWrapper != null)
            inputs.Add(defaultValueWrapper);

        return inputs;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private InputTypeDefinition CurrentInputType
    {
        get
        {
            if (_registry.TryGetValue(TypeId, out var definition) && definition != null)
                return definition;

            throw new InvalidOperationException($"The input type '{TypeId}' has not been registered.");
        }
    }

    private static object CreateDefaultValue(InputTypeDefinition definition)
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

    private object? ConvertFromEditorValue(object? value)
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

    private object? ConvertToEditorValue(object? value)
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

    private void AddStringSelectionInputs(List<DataInputWrapperVM> inputs)
    {
        var selection = GetStringSelectionValue();

        inputs.Add(new InputStringComboVM("Source:", selection.Source.ToString(), Enum.GetNames<StringSelectionSource>(), selectedSource =>
        {
            if (!Enum.TryParse<StringSelectionSource>(selectedSource, out var source))
                return;

            selection.Source = source;
            NormalizeStringSelectionValue(selection);
            SetDefaultValue(selection);
            RebuildEditTabs();
        }));

        var optionsInput = new InputStringListVM("Options:", selection.Options, selection.Value,
        options =>
        {
            selection.Options = options ?? [];
            NormalizeStringSelectionValue(selection);
            SetDefaultValue(selection);
        },
        selectedValue =>
        {
            selection.Value = selectedValue ?? string.Empty;
            SetDefaultValue(selection);
        })
        {
            Hide = selection.Source == StringSelectionSource.External,
        };
        inputs.Add(optionsInput);

        inputs.Add(new InputLabelVM(string.Empty, "Selected option is the default.", "Center")
        {
            Hide = selection.Source == StringSelectionSource.External,
        });
    }

    private void SetDefaultValue(object? value)
    {
        InputModel.SetDefaultValue(value, _registry);
        OnPropertyChanged(nameof(DefaultValue));
        NotifyComponentChanged();
    }

    private StringSelectionValue GetStringSelectionValue()
    {
        return DefaultValue as StringSelectionValue ?? new StringSelectionValue();
    }

    private static void NormalizeStringSelectionValue(StringSelectionValue selection)
    {
        if (selection.Source == StringSelectionSource.External)
            return;

        selection.Options ??= [];

        if (selection.Options.Count == 0)
        {
            selection.Value = string.Empty;
            return;
        }

        if (string.IsNullOrWhiteSpace(selection.Value) || !selection.Options.Contains(selection.Value))
            selection.Value = selection.Options.First();
    }

    private NodeInput InputModel => (NodeInput)Model;

    #endregion Private Methods
}
