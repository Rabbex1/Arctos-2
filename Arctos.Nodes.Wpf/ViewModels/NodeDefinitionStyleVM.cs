using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Wraps a node definition style for editing.
/// </summary>
public class NodeDefinitionStyleVM : ObservableObject
{
    #region Fields

    private readonly NodeDefinition _definition;
    private NodeHeaderStyleVM? _headerStyle;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets a value indicating whether the definition has a local style override.
    /// </summary>
    public bool HasStyle
    {
        get => _definition.Style != null;
        set
        {
            if (HasStyle == value)
                return;

            _definition.Style = value ? new NodeStyle() : null;
            _headerStyle = null;
            RaiseAllPropertiesChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node background color.
    /// </summary>
    public NodeColor BackgroundColor
    {
        get => EnsureStyle().ShapeStyle.BackgroundColor;
        set
        {
            if (EnsureStyle().ShapeStyle.BackgroundColor == value)
                return;

            EnsureStyle().ShapeStyle.BackgroundColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node border color.
    /// </summary>
    public NodeColor BorderColor
    {
        get => EnsureStyle().ShapeStyle.BorderColor;
        set
        {
            if (EnsureStyle().ShapeStyle.BorderColor == value)
                return;

            EnsureStyle().ShapeStyle.BorderColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node border thickness.
    /// </summary>
    public float BorderThickness
    {
        get => EnsureStyle().ShapeStyle.BorderThickness;
        set
        {
            if (Math.Abs(EnsureStyle().ShapeStyle.BorderThickness - value) < float.Epsilon)
                return;

            EnsureStyle().ShapeStyle.BorderThickness = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node font size.
    /// </summary>
    public float FontSize
    {
        get => EnsureStyle().ShapeStyle.FontSize;
        set
        {
            if (Math.Abs(EnsureStyle().ShapeStyle.FontSize - value) < float.Epsilon)
                return;

            EnsureStyle().ShapeStyle.FontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node font color.
    /// </summary>
    public NodeColor FontColor
    {
        get => EnsureStyle().ShapeStyle.FontColor;
        set
        {
            if (EnsureStyle().ShapeStyle.FontColor == value)
                return;

            EnsureStyle().ShapeStyle.FontColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the preferred node font family.
    /// </summary>
    public string? FontFamily
    {
        get => EnsureStyle().ShapeStyle.FontFamily;
        set
        {
            if (EnsureStyle().ShapeStyle.FontFamily == value)
                return;

            EnsureStyle().ShapeStyle.FontFamily = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the preferred node font weight.
    /// </summary>
    public NodeFontWeight FontWeight
    {
        get => EnsureStyle().ShapeStyle.FontWeight;
        set
        {
            if (EnsureStyle().ShapeStyle.FontWeight == value)
                return;

            EnsureStyle().ShapeStyle.FontWeight = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether node style overrides are allowed.
    /// </summary>
    public bool AllowStyleOverrides
    {
        get => EnsureStyle().OverrideRules.AllowStyleOverrides;
        set
        {
            if (EnsureStyle().OverrideRules.AllowStyleOverrides == value)
                return;

            EnsureStyle().OverrideRules.AllowStyleOverrides = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether node border overrides are allowed.
    /// </summary>
    public bool AllowBorderOverrides
    {
        get => EnsureStyle().OverrideRules.AllowBorderOverrides;
        set
        {
            if (EnsureStyle().OverrideRules.AllowBorderOverrides == value)
                return;

            EnsureStyle().OverrideRules.AllowBorderOverrides = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether node font overrides are allowed.
    /// </summary>
    public bool AllowFontOverrides
    {
        get => EnsureStyle().OverrideRules.AllowFontOverrides;
        set
        {
            if (EnsureStyle().OverrideRules.AllowFontOverrides == value)
                return;

            EnsureStyle().OverrideRules.AllowFontOverrides = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether node header overrides are allowed.
    /// </summary>
    public bool AllowHeaderOverrides
    {
        get => EnsureStyle().OverrideRules.AllowHeaderOverrides;
        set
        {
            if (EnsureStyle().OverrideRules.AllowHeaderOverrides == value)
                return;

            EnsureStyle().OverrideRules.AllowHeaderOverrides = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the definition has a header by default.
    /// </summary>
    public bool HasHeader
    {
        get => EnsureStyle().HeaderStyle != null;
        set
        {
            if (HasHeader == value)
                return;

            EnsureStyle().HeaderStyle = value ? new NodeHeaderStyle() : null;
            _headerStyle = value && EnsureStyle().HeaderStyle != null ? new NodeHeaderStyleVM(EnsureStyle().HeaderStyle!) : null;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HeaderStyle));
        }
    }

    /// <summary>
    /// Gets the header style VM when the node definition has a header.
    /// </summary>
    public NodeHeaderStyleVM? HeaderStyle
    {
        get
        {
            if (EnsureStyle().HeaderStyle == null)
                return null;

            return _headerStyle ??= new NodeHeaderStyleVM(EnsureStyle().HeaderStyle!);
        }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node definition style VM.
    /// </summary>
    /// <param name="definition">The owning node definition.</param>
    public NodeDefinitionStyleVM(NodeDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    #endregion Constructors

    #region Private Methods

    private NodeStyle EnsureStyle()
    {
        return _definition.Style ??= new NodeStyle();
    }

    private void RaiseAllPropertiesChanged()
    {
        OnPropertyChanged(nameof(HasStyle));
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(BorderColor));
        OnPropertyChanged(nameof(BorderThickness));
        OnPropertyChanged(nameof(FontSize));
        OnPropertyChanged(nameof(FontColor));
        OnPropertyChanged(nameof(FontFamily));
        OnPropertyChanged(nameof(FontWeight));
        OnPropertyChanged(nameof(AllowStyleOverrides));
        OnPropertyChanged(nameof(AllowBorderOverrides));
        OnPropertyChanged(nameof(AllowFontOverrides));
        OnPropertyChanged(nameof(AllowHeaderOverrides));
        OnPropertyChanged(nameof(HasHeader));
        OnPropertyChanged(nameof(HeaderStyle));
    }

    #endregion Private Methods
}
