using System.ComponentModel;
using Arctos.Shell.ToolWindows;
using Arctos.Nodes.Wpf.ViewModels.Components;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Minimal shell tool-window VM for node editing tools.
/// </summary>
public sealed class NodeEditToolsVM : ToolWindowVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly IReadOnlyList<PaletteGroupVM> _paletteGroups;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the current node session.
    /// </summary>
    public NodeSession Session { get; }

    /// <summary>
    /// Gets the currently active node document.
    /// </summary>
    public INodeDocumentVM? ActiveNodeDocument => Session.ActiveNodeDocument;

    /// <summary>
    /// Gets the system-defined palette groups shown in the lower palette section.
    /// </summary>
    public IReadOnlyList<PaletteGroupVM> PaletteGroups => _paletteGroups;

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Creates a new node edit tool-window VM.
    /// </summary>
    /// <param name="session">The shared node session.</param>
    public NodeEditToolsVM(NodeSession session)
        : base("Node Edit Tools")
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        _paletteGroups = CreatePaletteGroups();
        Session.PropertyChanged += OnSessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private static IReadOnlyList<PaletteGroupVM> CreatePaletteGroups()
    {
        return
        [
            new PaletteGroupVM("Components",
            [
                new PaletteItemVM(PaletteItemKind.Text, "Text", () => new NodeLabelComponentVM()),
                new PaletteItemVM(PaletteItemKind.Input, "Input", () => new NodeInputComponentVM()),
                new PaletteItemVM(PaletteItemKind.Button, "Button", () => new NodeButtonComponentVM()),
                new PaletteItemVM(PaletteItemKind.Separator, "Separator", () => new NodeSeparatorComponentVM()),
                new PaletteItemVM(PaletteItemKind.Spacer, "Spacer", () => new NodeSpacerComponentVM()),
            ]),

            new PaletteGroupVM("Ports",
            [
                new PaletteItemVM(PaletteItemKind.Port, "Port", () => new NodePortComponentVM()),
            ]),
        ];
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeSession.ActiveNodeDocument))
            OnPropertyChanged(nameof(ActiveNodeDocument));
    }

    #endregion Private Methods
}
