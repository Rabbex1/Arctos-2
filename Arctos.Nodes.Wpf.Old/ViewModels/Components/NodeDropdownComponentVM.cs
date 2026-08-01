using Arctos.Nodes.Models;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a dropdown input component with dedicated selector-state editing.
/// </summary>
public sealed class NodeDropdownComponentVM : NodeInputComponentVM
{
    private readonly record struct DropdownState(StringSelectionValue Selection);
    private InputStringComboVM? _defaultOptionInput;

    /// <summary>
    /// Creates a new dropdown component view model.
    /// </summary>
    public NodeDropdownComponentVM(NodeInput? model = null, InputTypeRegistry? registry = null)
        : base(model ?? new NodeInput
        {
            Label = "Dropdown",
            DisplayName = "Dropdown",
            TypeId = InputTypeIds.StringSelection,
        }, registry)
    {
        if (InputModel.TypeId != InputTypeIds.StringSelection)
            InputModel.TypeId = InputTypeIds.StringSelection;

        if (InputModel.GetDefaultValue(Registry) is not StringSelectionValue)
            InputModel.SetDefaultValue(new StringSelectionValue(), Registry);

        RebuildEditTabs();
    }

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        var selection = GetDropdownSelection();
        _defaultOptionInput = null;

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

            new InputStringComboVM("Source:", selection.Source.ToString(), Enum.GetNames<StringSelectionSource>(), (_, selectedSource) =>
            {
                if (!Enum.TryParse<StringSelectionSource>(selectedSource, out var source))
                    return;

                var previousState = CaptureDropdownState();
                var nextSelection = CloneSelection(previousState.Selection);
                nextSelection.Source = source;
                NormalizeSelection(nextSelection);
                var nextState = new DropdownState(nextSelection);

                ApplyComponentChange(
                    "Change dropdown source",
                    previousState,
                    nextState,
                    ApplyDropdownStateAndRebuild);
            }),

            new InputStringVM("Options:", GetOptionsText(selection.Options), (wrapper, optionsText) =>
            {
                var previousState = CaptureDropdownState();
                var nextSelection = CloneSelection(previousState.Selection);
                nextSelection.Options = ParseOptionsText(optionsText);
                NormalizeSelection(nextSelection);
                var nextState = new DropdownState(nextSelection);

                ApplyComponentChange(
                    "Change dropdown options",
                    previousState,
                    nextState,
                    ApplyDropdownStateAndRebuild);
            })
            {
                IsMultiLine = true,
                ShowMaxLines = 5,
                Hide = selection.Source == StringSelectionSource.External,
            },

            new InputLabelVM(string.Empty, "Enter one option per line (Shift + Enter).", "Center")

            {
                Hide = selection.Source == StringSelectionSource.External,
            },
        ];

        _defaultOptionInput = new InputStringComboVM("Default Option:", selection.Value, selection.Options, (_, selectedValue) =>
        {
            var previousState = CaptureDropdownState();
            var nextSelection = CloneSelection(previousState.Selection);
            nextSelection.Value = selectedValue ?? string.Empty;
            NormalizeSelection(nextSelection);
            var nextState = new DropdownState(nextSelection);

            ApplyComponentChange(
                "Change dropdown default option",
                previousState,
                nextState,
                state => ApplyDropdownSelection(state.Selection, _defaultOptionInput));
        })
        {
            Hide = selection.Source == StringSelectionSource.External,
        };
        inputs.Add(_defaultOptionInput);

        return inputs;
    }

    private DropdownState CaptureDropdownState()
    {
        return new DropdownState(CloneSelection(GetDropdownSelection()));
    }

    private void ApplyDropdownStateAndRebuild(DropdownState state)
    {
        ApplyDropdownSelection(state.Selection);
        RebuildEditTabs();
    }

    private void ApplyDropdownSelection(StringSelectionValue selection, InputStringComboVM? defaultOptionInput = null)
    {
        InputModel.SetDefaultValue(CloneSelection(selection), Registry);

        if (defaultOptionInput != null)
        {
            var targetOptions = selection.Options ?? [];
            if (defaultOptionInput.Options.SequenceEqual(targetOptions))
            {
                defaultOptionInput.UpdateValue(selection.Value);
            }
            else
            {
                defaultOptionInput.UpdateState(targetOptions, selection.Value);
            }
        }

        OnPropertyChanged(nameof(DefaultValue));
    }

    private StringSelectionValue GetDropdownSelection()
    {
        return DefaultValue as StringSelectionValue ?? new StringSelectionValue();
    }

    private static StringSelectionValue CloneSelection(StringSelectionValue selection)
    {
        return new StringSelectionValue
        {
            Source = selection.Source,
            Options = [.. selection.Options ?? []],
            Value = selection.Value,
        };
    }

    private static void NormalizeSelection(StringSelectionValue selection)
    {
        if (selection.Source == StringSelectionSource.External)
            return;

        selection.Options ??= [];

        if (selection.Options.Count == 0)
        {
            selection.Value = string.Empty;
            return;
        }

        if (selection.Value == null || !selection.Options.Contains(selection.Value))
            selection.Value = selection.Options.First();
    }

    private static string GetOptionsText(IEnumerable<string>? options)
    {
        return string.Join(Environment.NewLine, options ?? []);
    }

    private static List<string> ParseOptionsText(string? optionsText)
    {
        return (optionsText ?? string.Empty)
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(option => option.Trim())
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .ToList();
    }
}
