using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Wraps a node component for editing.
/// </summary>
public class NodeComponentVM : ObservableObject
{
    #region Fields

    private readonly NodeComponent _model;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the wrapped node component model.
    /// </summary>
    public NodeComponent Model => _model;

    /// <summary>
    /// Gets or sets the stable component identifier.
    /// </summary>
    public string Id
    {
        get => _model.Id;
        set
        {
            if (_model.Id == value)
                return;

            _model.Id = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string? DisplayName
    {
        get => _model.DisplayName;
        set
        {
            if (_model.DisplayName == value)
                return;

            _model.DisplayName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the optional component description.
    /// </summary>
    public string? Description
    {
        get => _model.Description;
        set
        {
            if (_model.Description == value)
                return;

            _model.Description = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the registered component type identifier.
    /// </summary>
    public InputTypeId TypeId
    {
        get => _model.TypeId;
        set
        {
            if (_model.TypeId == value)
                return;

            _model.TypeId = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the component is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => _model.IsReadOnly;
        set
        {
            if (_model.IsReadOnly == value)
                return;

            _model.IsReadOnly = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the component is visible by default.
    /// </summary>
    public bool IsVisible
    {
        get => _model.IsVisible;
        set
        {
            if (_model.IsVisible == value)
                return;

            _model.IsVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the optional logical group identifier.
    /// </summary>
    public string? GroupId
    {
        get => _model.GroupId;
        set
        {
            if (_model.GroupId == value)
                return;

            _model.GroupId = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node component VM.
    /// </summary>
    /// <param name="model">The wrapped node component.</param>
    public NodeComponentVM(NodeComponent model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    #endregion Constructors
}
