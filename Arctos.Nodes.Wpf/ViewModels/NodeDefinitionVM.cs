using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Wraps a node definition for library editing and preview.
/// </summary>
public class NodeDefinitionVM : ObservableObject
{
    #region Fields

    private readonly NodeDefinition _model;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the wrapped node definition model.
    /// </summary>
    public NodeDefinition Model => _model;

    /// <summary>
    /// Gets or sets the stable node definition name.
    /// </summary>
    public string TypeName
    {
        get => _model.TypeName;
        set
        {
            if (_model.TypeName == value)
                return;

            _model.TypeName = value;
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
    /// Gets or sets the domain type.
    /// </summary>
    public string DomainType
    {
        get => _model.DomainType;
        set
        {
            if (_model.DomainType == value)
                return;

            _model.DomainType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the node shape.
    /// </summary>
    public NodeShape NodeShape
    {
        get => _model.NodeShape;
        set
        {
            if (_model.NodeShape == value)
                return;

            _model.NodeShape = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the optional SVG icon.
    /// </summary>
    public string? IconSvg
    {
        get => _model.IconSvg;
        set
        {
            if (_model.IconSvg == value)
                return;

            _model.IconSvg = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the wrapped style VM.
    /// </summary>
    public NodeDefinitionStyleVM Style { get; }

    /// <summary>
    /// Gets the wrapped component VMs.
    /// </summary>
    public IReadOnlyList<NodeComponentVM> Components => _model.Components.Select(component => new NodeComponentVM(component)).ToList();

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node definition VM.
    /// </summary>
    /// <param name="model">The wrapped node definition.</param>
    public NodeDefinitionVM(NodeDefinition model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        Style = new NodeDefinitionStyleVM(_model);
    }

    #endregion Constructors
}
