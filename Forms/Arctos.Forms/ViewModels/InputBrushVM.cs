using System.Collections.ObjectModel;

namespace Arctos.Forms;

/// <summary>
/// A transactional string-valued brush input. Draft edits remain local until <see cref="CommitEdit"/>.
/// </summary>
public sealed class InputBrushVM : DataInputWrapperVM<string>
{
    private string _draftValue = string.Empty;
    private string _validationError = string.Empty;
    private bool _isEditing;

    public InputBrushVM(
        string description,
        string value,
        IEnumerable<InputBrushPaletteEntry>? palette,
        InputBrushKinds allowedKinds,
        Action<string?>? onChange,
        bool allowClear = false,
        BrushInputOptions? options = null)
        : base(description, value, onChange)
    {
        AllowedKinds = allowedKinds;
        AllowClear = allowClear;
        Options = options ?? new();
        ReplacePalette(palette ?? []);
        _draftValue = value;
        ValidateDraft();
    }

    public ObservableCollection<InputBrushPaletteEntry> Palette { get; } = [];

    public InputBrushKinds AllowedKinds { get; }

    public bool AllowClear { get; }

    public BrushInputOptions Options { get; }

    /// <summary>Application-supplied editor colours; the editor has no light/dark mode.</summary>
    public InputColor EditorBackground { get; set; } = new(247, 246, 242);
    public InputColor EditorForeground { get; set; } = new(32, 32, 32);

    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }

    public string DraftValue
    {
        get => _draftValue;
        set
        {
            if (!SetProperty(ref _draftValue, value ?? string.Empty))
                return;
            ValidateDraft();
            OnPropertyChanged(nameof(DraftDescription));
        }
    }

    public string ValidationError
    {
        get => _validationError;
        private set
        {
            if (!SetProperty(ref _validationError, value))
                return;
            OnPropertyChanged(nameof(CanCommit));
        }
    }

    public bool CanCommit => IsEnabled && !IsReadOnly && string.IsNullOrEmpty(ValidationError);

    public string DisplayDescription => Describe(Value);

    public string DraftDescription => Describe(DraftValue);

    public void BeginEdit()
    {
        DraftValue = Value ?? string.Empty;
        IsEditing = true;
        OnPropertyChanged(nameof(CanCommit));
    }

    public bool CommitEdit()
    {
        ValidateDraft();
        if (!CanCommit || !InputBrushFormat.TryParse(DraftValue, out var brush, out _))
            return false;

        var canonical = InputBrushFormat.Format(brush!);
        if (!string.Equals(Value, canonical, StringComparison.Ordinal))
            Value = canonical;
        IsEditing = false;
        DraftValue = Value ?? string.Empty;
        OnPropertyChanged(nameof(DisplayDescription));
        return true;
    }

    public void CancelEdit()
    {
        DraftValue = Value ?? string.Empty;
        IsEditing = false;
    }

    public bool CommitClear()
    {
        if (!AllowClear || IsReadOnly || !IsEnabled)
            return false;
        if (!string.IsNullOrEmpty(Value))
            Value = string.Empty;
        IsEditing = false;
        DraftValue = string.Empty;
        OnPropertyChanged(nameof(DisplayDescription));
        return true;
    }

    /// <summary>Refreshes resolved palette previews without changing or committing the authored value.</summary>
    public void ReplacePalette(IEnumerable<InputBrushPaletteEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var replacement = entries.ToArray();
        if (replacement.Any(entry => string.IsNullOrWhiteSpace(entry.Name)))
            throw new ArgumentException("Palette names cannot be empty.", nameof(entries));
        if (replacement.Select(entry => entry.Name).Distinct(StringComparer.Ordinal).Count() != replacement.Length)
            throw new ArgumentException("Palette names must be unique using ordinal, case-sensitive comparison.", nameof(entries));

        Palette.Clear();
        foreach (var entry in replacement)
            Palette.Add(entry);
        ValidateDraft();
        OnPropertyChanged(nameof(DisplayDescription));
        OnPropertyChanged(nameof(DraftDescription));
    }

    public InputValidationResult Validate(string? value)
    {
        if (AllowClear && string.IsNullOrEmpty(value))
            return InputValidationResult.Success;
        if (!InputBrushFormat.TryParse(value, out var brush, out var error))
            return InputValidationResult.Invalid(error);

        if (brush is InputNamedBrushDescription named)
        {
            if (!AllowedKinds.HasFlag(InputBrushKinds.Named))
                return InputValidationResult.Invalid("Named brushes are not permitted for this property.");
            var entry = Palette.FirstOrDefault(candidate => string.Equals(candidate.Name, named.Name, StringComparison.Ordinal));
            if (entry is null)
                return InputValidationResult.Invalid($"The palette does not contain a brush named '{named.Name}'.");
            if (!InputBrushFormat.TryParse(entry.Description, out var resolved, out var paletteError))
                return InputValidationResult.Invalid($"Palette brush '{named.Name}' is malformed: {paletteError}");
            if (resolved is InputNamedBrushDescription)
                return InputValidationResult.Invalid($"Palette brush '{named.Name}' cannot reference another named brush.");
            if (!AllowedKinds.HasFlag(InputBrushFormat.ToFlag(resolved!.Kind)))
                return InputValidationResult.Invalid($"Palette brush '{named.Name}' resolves to a disallowed {resolved.Kind.ToString().ToLowerInvariant()} brush.");
            return InputValidationResult.Success;
        }

        return AllowedKinds.HasFlag(InputBrushFormat.ToFlag(brush!.Kind))
            ? InputValidationResult.Success
            : InputValidationResult.Invalid($"{brush.Kind} brushes are not permitted for this property.");
    }

    public bool TryResolve(string? value, out InputBrushDescription? brush, out string error)
    {
        brush = null;
        var validation = Validate(value);
        if (!validation.IsValid)
        {
            error = validation.FirstError ?? "Invalid brush.";
            return false;
        }
        if (!InputBrushFormat.TryParse(value, out brush, out error))
            return false;
        if (brush is not InputNamedBrushDescription named)
            return true;
        var entry = Palette.First(candidate => string.Equals(candidate.Name, named.Name, StringComparison.Ordinal));
        return InputBrushFormat.TryParse(entry.Description, out brush, out error);
    }

    private void ValidateDraft()
    {
        var validation = Validate(DraftValue);
        ValidationError = validation.IsValid ? string.Empty : validation.FirstError ?? "Invalid brush.";
    }

    private string Describe(string? value)
    {
        if (AllowClear && string.IsNullOrEmpty(value))
            return "Inherit / cleared";
        if (!InputBrushFormat.TryParse(value, out var brush, out _))
            return "Invalid brush";
        return brush switch
        {
            InputNamedBrushDescription named => named.Name,
            InputSolidBrushDescription solid => InputBrushFormat.FormatColor(solid.Color),
            InputGradientBrushDescription gradient => $"{gradient.Kind} gradient · {gradient.Stops.Count} stops",
            _ => brush!.Kind.ToString()
        };
    }
}
