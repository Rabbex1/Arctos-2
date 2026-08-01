using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;
using System.Globalization;
using System.Linq;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a repeated numeric or integer cell grid inside a node.
/// </summary>
public sealed class NodeGridInputComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly InputTypeRegistry _registry;
    private readonly record struct GridState(int Rows, int Columns, InputTypeId CellTypeId, float CellWidth, double MinValue, double MaxValue, double[] DefaultValues);

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public int Rows
    {
        get => GridInputModel.Rows;
        set
        {
            var rows = NodeGridInput.RowRange.Clamp(value);
            if (GridInputModel.Rows == rows)
                return;

            ResizeGrid(rows, Columns);
        }
    }

    public int Columns
    {
        get => GridInputModel.Columns;
        set
        {
            var columns = NodeGridInput.ColumnRange.Clamp(value);
            if (GridInputModel.Columns == columns)
                return;

            ResizeGrid(Rows, columns);
        }
    }

    public InputTypeId CellTypeId
    {
        get => GridInputModel.CellTypeId;
        set
        {
            if (GridInputModel.CellTypeId == value)
                return;

            GridInputModel.CellTypeId = value;
            ApplyDefaultRangeForCellType();
            NormalizeDefaultValues();
            OnPropertyChanged();
            RebuildEditTabs();
            NotifyComponentChanged();
        }
    }

    public float CellWidth
    {
        get => GridInputModel.CellWidth;
        set
        {
            var cellWidth = MathF.Max(NodeGridInput.MinimumCellWidth, value);
            if (Math.Abs(GridInputModel.CellWidth - cellWidth) < 0.0001f)
                return;

            GridInputModel.CellWidth = cellWidth;
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    public double MinValue
    {
        get => GridInputModel.MinValue;
        set
        {
            if (Math.Abs(GridInputModel.MinValue - value) < 0.000001d)
                return;

            GridInputModel.MinValue = value;
            if (GridInputModel.MaxValue < GridInputModel.MinValue)
            {
                GridInputModel.MaxValue = GridInputModel.MinValue;
                OnPropertyChanged(nameof(MaxValue));
            }

            NormalizeDefaultValues();
            OnPropertyChanged();
            RebuildEditTabs();
            NotifyComponentChanged();
        }
    }

    public double MaxValue
    {
        get => GridInputModel.MaxValue;
        set
        {
            var maxValue = Math.Max(value, GridInputModel.MinValue);
            if (Math.Abs(GridInputModel.MaxValue - maxValue) < 0.000001d)
                return;

            GridInputModel.MaxValue = maxValue;
            NormalizeDefaultValues();
            OnPropertyChanged();
            RebuildEditTabs();
            NotifyComponentChanged();
        }
    }

    public double[] DefaultValues
    {
        get => NormalizeDefaultValues(GridInputModel.DefaultValues, Rows, Columns);
        set
        {
            GridInputModel.DefaultValues = NormalizeAndClampValues(value, Rows, Columns);
            OnPropertyChanged();
            NotifyComponentChanged();
        }
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public NodeGridInputComponentVM(NodeGridInput? model = null, InputTypeRegistry? registry = null)
        : base(model ?? new NodeGridInput
        {
            Label = "Grid Input",
            CellTypeId = InputTypeIds.Double,
        }, PaletteItemKind.GridInput, "Grid Input", supportsId: true)
    {
        _registry = registry ?? InputTypeRegistry.Default;

        if (GridInputModel.CellTypeId == default)
            GridInputModel.CellTypeId = InputTypeIds.Double;

        GridInputModel.Rows = NodeGridInput.RowRange.Clamp(GridInputModel.Rows);
        GridInputModel.Columns = NodeGridInput.ColumnRange.Clamp(GridInputModel.Columns);
        GridInputModel.CellWidth = MathF.Max(NodeGridInput.MinimumCellWidth, GridInputModel.CellWidth <= 0.0f ? NodeGridInput.DefaultCellWidth : GridInputModel.CellWidth);
        ApplyDefaultRangeForCellTypeIfUninitialized();
        NormalizeDefaultValues();
        RebuildEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        var rowInput = new IntRangeInput
        {
            Min = NodeGridInput.RowRange.Minimum,
            Max = NodeGridInput.RowRange.Maximum,
            Value = Rows,
        };
        var columnInput = new IntRangeInput
        {
            Min = NodeGridInput.ColumnRange.Minimum,
            Max = NodeGridInput.ColumnRange.Maximum,
            Value = Columns,
        };

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

            new InputIntRangeVM("Rows:", rowInput, (_, input) =>
            {
                if (input == null || input.Value == Rows)
                    return;

                var previousState = CaptureState();
                var nextState = new GridState(
                    input.Value,
                    previousState.Columns,
                    previousState.CellTypeId,
                    previousState.CellWidth,
                    previousState.MinValue,
                    previousState.MaxValue,
                    ResizePreservingOverlap(previousState.DefaultValues, previousState.Rows, previousState.Columns, input.Value, previousState.Columns));

                ApplyComponentChange("Change grid rows", previousState, nextState, ApplyState);
            }),

            new InputIntRangeVM("Columns:", columnInput, (_, input) =>
            {
                if (input == null || input.Value == Columns)
                    return;

                var previousState = CaptureState();
                var nextState = new GridState(
                    previousState.Rows,
                    input.Value,
                    previousState.CellTypeId,
                    previousState.CellWidth,
                    previousState.MinValue,
                    previousState.MaxValue,
                    ResizePreservingOverlap(previousState.DefaultValues, previousState.Rows, previousState.Columns, previousState.Rows, input.Value));

                ApplyComponentChange("Change grid columns", previousState, nextState, ApplyState);
            }),

            new InputFloatVM("Cell Width (nu):", CellWidth, (_, width) =>
            {
                var cellWidth = MathF.Max(NodeGridInput.MinimumCellWidth, width);
                if (Math.Abs(CellWidth - cellWidth) < 0.0001f)
                    return;

                var previousState = CaptureState();
                var nextState = previousState with { CellWidth = cellWidth };
                ApplyComponentChange("Change grid cell width", previousState, nextState, ApplyState);
            }),

            new InputObjectComboVM(
                "Cell Type:",
                CurrentCellType,
                AllowedCellTypes.Cast<object>().ToList(),
                nameof(InputTypeDefinition.DisplayName),
                (_, selectedType) =>
                {
                    if (selectedType is not InputTypeDefinition inputType || inputType.Id == CellTypeId)
                        return;

                    var previousState = CaptureState();
                    var nextState = previousState with
                    {
                        CellTypeId = inputType.Id,
                        MinValue = inputType.Id == InputTypeIds.Integer ? 0.0d : 0.0d,
                        MaxValue = inputType.Id == InputTypeIds.Integer ? 255.0d : 1.0d,
                    };
                    nextState = nextState with
                    {
                        DefaultValues = NormalizeAndClampValues(previousState.DefaultValues, nextState.Rows, nextState.Columns, nextState.CellTypeId, nextState.MinValue, nextState.MaxValue),
                    };

                    ApplyComponentChange("Change grid cell type", previousState, nextState, ApplyState);
                }),
        ];

        if (IsIntegerCellType)
        {
            inputs.Add(new InputIntegerVM("Min:", (int)Math.Round(MinValue), (_, min) =>
            {
                var nextMin = min;
                var nextMax = Math.Max(nextMin, MaxValue);
                if (Math.Abs(MinValue - nextMin) < 0.000001d && Math.Abs(MaxValue - nextMax) < 0.000001d)
                    return;

                var previousState = CaptureState();
                var nextState = previousState with
                {
                    MinValue = nextMin,
                    MaxValue = nextMax,
                    DefaultValues = NormalizeAndClampValues(previousState.DefaultValues, previousState.Rows, previousState.Columns, previousState.CellTypeId, nextMin, nextMax),
                };
                ApplyComponentChange("Change grid minimum", previousState, nextState, ApplyState);
            }));

            inputs.Add(new InputIntegerVM("Max:", (int)Math.Round(MaxValue), (_, max) =>
            {
                var nextMax = Math.Max(max, MinValue);
                if (Math.Abs(MaxValue - nextMax) < 0.000001d)
                    return;

                var previousState = CaptureState();
                var nextState = previousState with
                {
                    MaxValue = nextMax,
                    DefaultValues = NormalizeAndClampValues(previousState.DefaultValues, previousState.Rows, previousState.Columns, previousState.CellTypeId, previousState.MinValue, nextMax),
                };
                ApplyComponentChange("Change grid maximum", previousState, nextState, ApplyState);
            }));

            inputs.Add(new InputMatrixVM<int>("Default Values:", Rows, Columns, BuildIntegerValues(), (_, values) =>
            {
                if (values == null)
                    return;

                var previousState = CaptureState();
                var nextState = previousState with
                {
                    DefaultValues = NormalizeAndClampValues(values.Select(value => (double)value).ToArray(), previousState.Rows, previousState.Columns, previousState.CellTypeId, previousState.MinValue, previousState.MaxValue),
                };
                ApplyComponentChange("Change grid default values", previousState, nextState, ApplyState);
            }));
            return inputs;
        }

        inputs.Add(new InputDoubleVM("Min:", MinValue, (_, min) =>
        {
            var nextMin = min;
            var nextMax = Math.Max(nextMin, MaxValue);
            if (Math.Abs(MinValue - nextMin) < 0.000001d && Math.Abs(MaxValue - nextMax) < 0.000001d)
                return;

            var previousState = CaptureState();
            var nextState = previousState with
            {
                MinValue = nextMin,
                MaxValue = nextMax,
                DefaultValues = NormalizeAndClampValues(previousState.DefaultValues, previousState.Rows, previousState.Columns, previousState.CellTypeId, nextMin, nextMax),
            };
            ApplyComponentChange("Change grid minimum", previousState, nextState, ApplyState);
        }));

        inputs.Add(new InputDoubleVM("Max:", MaxValue, (_, max) =>
        {
            var nextMax = Math.Max(max, MinValue);
            if (Math.Abs(MaxValue - nextMax) < 0.000001d)
                return;

            var previousState = CaptureState();
            var nextState = previousState with
            {
                MaxValue = nextMax,
                DefaultValues = NormalizeAndClampValues(previousState.DefaultValues, previousState.Rows, previousState.Columns, previousState.CellTypeId, previousState.MinValue, nextMax),
            };
            ApplyComponentChange("Change grid maximum", previousState, nextState, ApplyState);
        }));

        inputs.Add(new InputMatrixVM<double>("Default Values:", Rows, Columns, BuildNumericValues(), (_, values) =>
        {
            if (values == null)
                return;

            var previousState = CaptureState();
            var nextState = previousState with
            {
                DefaultValues = NormalizeAndClampValues(values, previousState.Rows, previousState.Columns, previousState.CellTypeId, previousState.MinValue, previousState.MaxValue),
            };
            ApplyComponentChange("Change grid default values", previousState, nextState, ApplyState);
        }));
        return inputs;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private IReadOnlyList<InputTypeDefinition> AllowedCellTypes => _registry
        .Where(definition => definition.Id == InputTypeIds.Double || definition.Id == InputTypeIds.Integer)
        .ToList();

    private InputTypeDefinition CurrentCellType
    {
        get
        {
            if (_registry.TryGetValue(CellTypeId, out var definition) && definition != null)
                return definition;

            throw new InvalidOperationException($"The input type '{CellTypeId}' has not been registered.");
        }
    }

    private bool IsIntegerCellType => CellTypeId == InputTypeIds.Integer;

    private GridState CaptureState()
    {
        return new GridState(Rows, Columns, CellTypeId, CellWidth, MinValue, MaxValue, [.. DefaultValues]);
    }

    private void ApplyState(GridState state)
    {
        GridInputModel.Rows = NodeGridInput.RowRange.Clamp(state.Rows);
        GridInputModel.Columns = NodeGridInput.ColumnRange.Clamp(state.Columns);
        GridInputModel.CellTypeId = state.CellTypeId;
        GridInputModel.CellWidth = MathF.Max(NodeGridInput.MinimumCellWidth, state.CellWidth);
        GridInputModel.MinValue = state.MinValue;
        GridInputModel.MaxValue = Math.Max(state.MinValue, state.MaxValue);
        GridInputModel.DefaultValues = NormalizeAndClampValues(
            state.DefaultValues,
            GridInputModel.Rows,
            GridInputModel.Columns,
            GridInputModel.CellTypeId,
            GridInputModel.MinValue,
            GridInputModel.MaxValue);

        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(Columns));
        OnPropertyChanged(nameof(CellTypeId));
        OnPropertyChanged(nameof(CellWidth));
        OnPropertyChanged(nameof(MinValue));
        OnPropertyChanged(nameof(MaxValue));
        OnPropertyChanged(nameof(DefaultValues));
        RebuildEditTabs();
    }

    private void ResizeGrid(int newRows, int newColumns)
    {
        var resized = ResizePreservingOverlap(DefaultValues, Rows, Columns, newRows, newColumns);
        GridInputModel.Rows = newRows;
        GridInputModel.Columns = newColumns;
        GridInputModel.DefaultValues = NormalizeAndClampValues(resized, newRows, newColumns);
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(Columns));
        OnPropertyChanged(nameof(DefaultValues));
        RebuildEditTabs();
        NotifyComponentChanged();
    }

    private void NormalizeDefaultValues()
    {
        GridInputModel.DefaultValues = NormalizeAndClampValues(GridInputModel.DefaultValues, Rows, Columns);
        OnPropertyChanged(nameof(DefaultValues));
    }

    private double[] NormalizeAndClampValues(double[]? values, int rows, int columns)
    {
        return NormalizeAndClampValues(values, rows, columns, CellTypeId, MinValue, MaxValue);
    }

    private static double[] NormalizeAndClampValues(double[]? values, int rows, int columns, InputTypeId cellTypeId, double minValue, double maxValue)
    {
        var normalized = NormalizeDefaultValues(values, rows, columns);
        for (var index = 0; index < normalized.Length; index++)
            normalized[index] = ClampToCellType(normalized[index], cellTypeId, minValue, maxValue);

        return normalized;
    }

    private static double[] NormalizeDefaultValues(double[]? values, int rows, int columns)
    {
        var length = rows * columns;
        if (values == null || values.Length == 0)
            return new double[length];

        if (values.Length == length)
            return [.. values];

        return values
            .Concat(Enumerable.Repeat(0.0d, Math.Max(0, length - values.Length)))
            .Take(length)
            .ToArray();
    }

    private double[] ResizePreservingOverlap(double[] values, int oldRows, int oldColumns, int newRows, int newColumns)
    {
        var source = NormalizeDefaultValues(values, oldRows, oldColumns);
        var resized = new double[newRows * newColumns];
        var rowsToCopy = Math.Min(oldRows, newRows);
        var columnsToCopy = Math.Min(oldColumns, newColumns);

        for (var row = 0; row < rowsToCopy; row++)
        {
            for (var column = 0; column < columnsToCopy; column++)
            {
                resized[(row * newColumns) + column] = source[(row * oldColumns) + column];
            }
        }

        return resized;
    }

    private void ApplyDefaultRangeForCellType()
    {
        if (CellTypeId == InputTypeIds.Integer)
        {
            GridInputModel.MinValue = 0.0d;
            GridInputModel.MaxValue = 255.0d;
        }
        else
        {
            GridInputModel.MinValue = 0.0d;
            GridInputModel.MaxValue = 1.0d;
        }

        OnPropertyChanged(nameof(MinValue));
        OnPropertyChanged(nameof(MaxValue));
    }

    private void ApplyDefaultRangeForCellTypeIfUninitialized()
    {
        if (!(Math.Abs(GridInputModel.MinValue) < 0.000001d && Math.Abs(GridInputModel.MaxValue) < 0.000001d))
            return;

        ApplyDefaultRangeForCellType();
    }

    private double ClampToCellType(double value)
    {
        return ClampToCellType(value, CellTypeId, MinValue, MaxValue);
    }

    private static double ClampToCellType(double value, InputTypeId cellTypeId, double minValue, double maxValue)
    {
        var clamped = Math.Clamp(value, minValue, maxValue);
        return cellTypeId == InputTypeIds.Integer
            ? Math.Round(clamped, MidpointRounding.AwayFromZero)
            : clamped;
    }

    private int[] BuildIntegerValues()
    {
        return DefaultValues
            .Select(value => (int)Math.Round(ClampToCellType(value), MidpointRounding.AwayFromZero))
            .ToArray();
    }

    private double[] BuildNumericValues()
    {
        return DefaultValues
            .Select(ClampToCellType)
            .ToArray();
    }

    private NodeGridInput GridInputModel => (NodeGridInput)Model;

    #endregion Private Methods
}
