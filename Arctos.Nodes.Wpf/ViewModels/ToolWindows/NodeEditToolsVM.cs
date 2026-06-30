using System.ComponentModel;
using Arctos.Shell.ToolWindows;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.DragImage;
using Arctos.Nodes.Wpf.Rendering;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Nodes.Wpf.ViewModels.Documents;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Minimal shell tool-window VM for node editing tools.
/// </summary>
public sealed class NodeEditToolsVM : ToolWindowVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly IReadOnlyList<PaletteGroupVM> _paletteGroups;
    private readonly DragImageRenderer _dragImageRenderer = new();
    private INotifyPropertyChanged? _activeNodeDocumentNotifier;

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
        HookActiveNodeDocument(Session.ActiveNodeDocument);
        RefreshPaletteDragImages();
        Session.PropertyChanged += OnSessionPropertyChanged;
    }

    #endregion Constructors

    #region Private Methods

    private IReadOnlyList<PaletteGroupVM> CreatePaletteGroups()
    {
        return
        [
            new PaletteGroupVM("Components",
            [
                new PaletteItemVM(PaletteItemKind.Text, "Text", () => new NodeLabelComponentVM()),
                new PaletteItemVM(PaletteItemKind.Input, "Input", () => new NodeInputComponentVM()),
                new PaletteItemVM(PaletteItemKind.ValueDisplay, "Value Display", () => new NodeValueDisplayComponentVM()),
                new PaletteItemVM(PaletteItemKind.GridInput, "Grid Input", () => new NodeGridInputComponentVM()),
                new PaletteItemVM(PaletteItemKind.Button, "Button", () => new NodeButtonComponentVM()),
                new PaletteItemVM(PaletteItemKind.IconButton, "Icon Button", () => new NodeIconButtonComponentVM()),
                new PaletteItemVM(PaletteItemKind.Indicator, "Indicator", () => new NodeIndicatorComponentVM()),
                new PaletteItemVM(PaletteItemKind.Column, "Column", () => new NodeLabelComponentVM(new NodeText { Label = "Column" })),
                new PaletteItemVM(PaletteItemKind.Separator, "Separator", () => new NodeSeparatorComponentVM()),
                new PaletteItemVM(PaletteItemKind.Spacer, "Spacer", () => new NodeSpacerComponentVM()),
            ]),

            new PaletteGroupVM("Ports",
            [
                new PaletteItemVM(PaletteItemKind.Port, "Port", CreatePalettePortComponent),
            ]),
        ];
    }

    private NodePortComponentVM CreatePalettePortComponent()
    {
        return new NodePortComponentVM(new NodePort
        {
            Label = "Port",
            Side = NodePortSide.Left,
            Shape = NodePortShape.Circle,
            FillColor = CreateNeutralColor(GetCurrentNodeBorderColor()),
        });
    }

    private NodeColor GetCurrentNodeBorderColor()
    {
        return ActiveNodeDocument?.SelectedHost is NodeDefinitionVM definitionHost
            ? definitionHost.Model.Style?.ShapeStyle.BorderColor ?? new NodeStyle().ShapeStyle.BorderColor
            : new NodeStyle().ShapeStyle.BorderColor;
    }

    private static NodeColor CreateNeutralColor(NodeColor color)
    {
        var grayscale = (byte)Math.Clamp(
            MathF.Round((color.Red * 0.299f) + (color.Green * 0.587f) + (color.Blue * 0.114f)),
            0.0f,
            255.0f);
        return new NodeColor(grayscale, grayscale, grayscale, color.Alpha);
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NodeSession.ActiveNodeDocument))
        {
            HookActiveNodeDocument(Session.ActiveNodeDocument);
            RefreshPaletteDragImages();
            OnPropertyChanged(nameof(ActiveNodeDocument));
        }
    }

    private void HookActiveNodeDocument(INodeDocumentVM? document)
    {
        if (_activeNodeDocumentNotifier != null)
            _activeNodeDocumentNotifier.PropertyChanged -= OnActiveNodeDocumentPropertyChanged;

        _activeNodeDocumentNotifier = document as INotifyPropertyChanged;

        if (_activeNodeDocumentNotifier != null)
            _activeNodeDocumentNotifier.PropertyChanged += OnActiveNodeDocumentPropertyChanged;
    }

    private void OnActiveNodeDocumentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.PropertyName)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasFontFamily)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasFontColor)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasFontSize)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasMinimumColumnWidth)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasPortSize)
            || e.PropertyName == nameof(NodeDocumentVM.CanvasPortPlacement)
            || e.PropertyName == nameof(NodeDocumentVM.SelectedHost))
        {
            RefreshPaletteDragImages();
        }
    }

    private void RefreshPaletteDragImages()
    {
        var styleContext = CreateDragImageStyleContext();
        var nodeStyle = CreateDragImageNodeStyle();

        foreach (var item in _paletteGroups.SelectMany(group => group.Items))
        {
            var component = item.CreateComponent();
            item.DragImage = _dragImageRenderer.RenderComponent(component, styleContext, nodeStyle);
        }
    }

    private NodeCanvasStyleContext CreateDragImageStyleContext()
    {
        if (ActiveNodeDocument?.CurrentCanvas is { } canvas)
        {
            return new NodeCanvasStyleContext(
                canvas.FontFamily,
                canvas.FontSize,
                canvas.HeaderFontSize,
                canvas.FontColor,
                canvas.LayoutPadding,
                canvas.RowSpacing,
                canvas.ColumnSpacing,
                canvas.RoundedCornerRadius,
                canvas.ControlCornerRadius,
                canvas.MinimumColumnWidth,
                canvas.PortSize,
                canvas.PortPlacement);
        }

        return new NodeCanvasStyleContext(
            FontFamily: null,
            FontSize: 12.0f,
            HeaderFontSize: 14.0f,
            FontColor: NodeColor.Black,
            Padding: 1.0f,
            RowSpacing: 0.0f,
            ColumnSpacing: 1.0f,
            RoundedCornerRadius: 2.0f,
            ControlCornerRadius: 1.0f,
            MinimumColumnWidth: 24.0f,
            PortSize: 1.5f,
            PortPlacement: NodePortPlacement.Over);
    }

    private NodeStyle CreateDragImageNodeStyle()
    {
        if (ActiveNodeDocument?.SelectedHost is NodeDefinitionVM definitionHost && definitionHost.Model.Style != null)
            return definitionHost.Model.Style;

        return new NodeStyle();
    }

    #endregion Private Methods
}
