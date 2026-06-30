using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf;
using Arctos.Nodes.Wpf.ViewModels.ToolWindows;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Components;

/// <summary>
/// Represents a port component inside a node.
/// </summary>
public sealed class NodePortComponentVM : NodeComponentVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private NodePortSide _side;
    private NodePortShape _shape;
    private NodeColor _fillColor;
    private NodePoint _attach;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets or sets the side the port anchors to.
    /// </summary>
    public NodePortSide Side
    {
        get => _side;
        set
        {
            ApplyComponentChange(
                "Change port side",
                _side,
                value,
                side =>
                {
                    _side = side;
                    PortModel.Side = side;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets the preset port shape.
    /// </summary>
    public NodePortShape Shape
    {
        get => _shape;
        set
        {
            ApplyComponentChange(
                "Change port shape",
                _shape,
                value,
                shape =>
                {
                    _shape = shape;
                    PortModel.Shape = shape;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets or sets the authored fill color for the port glyph.
    /// </summary>
    public NodeColor FillColor
    {
        get => _fillColor;
        set
        {
            ApplyComponentChange(
                "Change port fill color",
                _fillColor,
                value,
                fillColor =>
                {
                    _fillColor = fillColor;
                    PortModel.FillColor = fillColor;
                    OnPropertyChanged();
                });
        }
    }

    /// <summary>
    /// Gets the resolved border-centered connection anchor in node-space world units.
    /// </summary>
    public NodePoint Attach
    {
        get => _attach;
        internal set => SetProperty(ref _attach, value);
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node port component view model.
    /// </summary>
    /// <param name="model">The persisted port model to wrap.</param>
    public NodePortComponentVM(NodePort? model = null)
        : base(model ?? new NodePort
        {
            Label = "Port",
            Side = NodePortSide.Left,
            Shape = NodePortShape.Circle,
            FillColor = NodeColor.White,
        }, PaletteItemKind.Port, "Port", supportsId: true)
    {
        _side = PortModel.Side;
        _shape = PortModel.Shape;
        _fillColor = PortModel.FillColor;
        _attach = default;
        RebuildEditTabs();
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    protected override List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Label:", Label, label =>
            {
                Label = label ?? string.Empty;
            }),

            new InputEnumVM("Glyph Position:", Side, side =>
            {
                if (side == null)
                    return;

                Side = (NodePortSide)side;
            }),

            new InputEnumVM("Shape:", Shape, shape =>
            {
                if (shape == null)
                    return;

                Shape = (NodePortShape)shape;
            }),

            new InputColorVM("Fill Color:", FillColor.ToMediaColor(), color =>
            {
                FillColor = color.ToNodeColor();
            }),
        ];
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Properties

    private NodePort PortModel => (NodePort)Model;

    #endregion Private Properties
}
