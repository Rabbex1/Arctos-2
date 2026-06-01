using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Wraps a node header style for editing.
/// </summary>
public class NodeHeaderStyleVM : ObservableObject
{
    #region Fields

    private readonly NodeHeaderStyle _model;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets or sets the header background color.
    /// </summary>
    public NodeColor BackgroundColor
    {
        get => _model.BackgroundColor;
        set
        {
            if (_model.BackgroundColor == value)
                return;

            _model.BackgroundColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the header font color.
    /// </summary>
    public NodeColor FontColor
    {
        get => _model.FontColor;
        set
        {
            if (_model.FontColor == value)
                return;

            _model.FontColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the header font size.
    /// </summary>
    public float FontSize
    {
        get => _model.FontSize;
        set
        {
            if (Math.Abs(_model.FontSize - value) < float.Epsilon)
                return;

            _model.FontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the preferred header font family.
    /// </summary>
    public string? FontFamily
    {
        get => _model.FontFamily;
        set
        {
            if (_model.FontFamily == value)
                return;

            _model.FontFamily = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the preferred header font weight.
    /// </summary>
    public NodeFontWeight FontWeight
    {
        get => _model.FontWeight;
        set
        {
            if (_model.FontWeight == value)
                return;

            _model.FontWeight = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node header style VM.
    /// </summary>
    /// <param name="model">The wrapped node header style.</param>
    public NodeHeaderStyleVM(NodeHeaderStyle model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    #endregion Constructors
}
