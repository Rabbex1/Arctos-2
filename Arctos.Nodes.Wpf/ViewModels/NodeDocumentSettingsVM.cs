using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Wraps graph document settings for editing in a document toolbox.
/// </summary>
public class NodeDocumentSettingsVM : ObservableObject
{
    #region Fields

    private readonly NodeDocumentSettings _model;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the wrapped graph document settings model.
    /// </summary>
    public NodeDocumentSettings Model => _model;

    /// <summary>
    /// Gets or sets the persisted document format version.
    /// </summary>
    public int FormatVersion
    {
        get => _model.FormatVersion;
        set
        {
            if (_model.FormatVersion == value)
                return;

            _model.FormatVersion = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the document contains node graph content.
    /// </summary>
    public bool ContainsGraph
    {
        get => _model.ContainsGraph;
        set
        {
            if (_model.ContainsGraph == value)
                return;

            _model.ContainsGraph = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the preferred startup mode.
    /// </summary>
    public OperationMode StartupMode
    {
        get => _model.StartupMode;
        set
        {
            if (_model.StartupMode == value)
                return;

            _model.StartupMode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the canvas background color.
    /// </summary>
    public NodeColor CanvasColor
    {
        get => _model.CanvasColor;
        set
        {
            if (_model.CanvasColor == value)
                return;

            _model.CanvasColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the document name.
    /// </summary>
    public string? Name
    {
        get => _model.Name;
        set
        {
            if (_model.Name == value)
                return;

            _model.Name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the application-defined document type.
    /// </summary>
    public string? DocumentType
    {
        get => _model.DocumentType;
        set
        {
            if (_model.DocumentType == value)
                return;

            _model.DocumentType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether library editing is allowed.
    /// </summary>
    public bool AllowLibraryEdit
    {
        get => _model.AllowLibraryEdit;
        set
        {
            if (_model.AllowLibraryEdit == value)
                return;

            _model.AllowLibraryEdit = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether graph editing is allowed.
    /// </summary>
    public bool AllowGraphEdit
    {
        get => _model.AllowGraphEdit;
        set
        {
            if (_model.AllowGraphEdit == value)
                return;

            _model.AllowGraphEdit = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new graph document settings VM.
    /// </summary>
    /// <param name="model">The wrapped graph document settings model.</param>
    public NodeDocumentSettingsVM(NodeDocumentSettings model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    #endregion Constructors
}
