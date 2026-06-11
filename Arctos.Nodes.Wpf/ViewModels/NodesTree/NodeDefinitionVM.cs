using System.Collections.Generic;
using Arctos.Nodes.Models;
using Arctos.Wpf;
using Arctos.Wpf.Inputs;
using Arctos.Wpf.Utilities;
using System.Windows.Input;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Editable tree view model wrapper over a <see cref="NodeDefinition"/>.
/// </summary>
public sealed class NodeDefinitionVM : NodeTreeItemVM<INodeTreeItemVM>, IEditableProperties, IModelHost<NodeDefinition>
{
    private readonly NodeDefinition _definition;
    private ICommand? _deleteCommand;

    public NodeDefinitionVM(NodeDefinition definition, INodeTreeItemVM? parent = null, bool isExternal = false)
        : base(parent)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        IsExternal = isExternal;
        DisplayName = _definition.DisplayName ?? _definition.TypeName;
        PropertiesForm.OnInputChanged = NotifyDocumentChanged;
        MakePropertiesForm();
    }

    public NodeDefinition Model => _definition;

    object IModelHost.Model => Model;

    string IModelHost.DisplayName => DisplayName ?? string.Empty;

    bool IModelHost.HasDetails => HasDetails;


    public bool IsExternal { get; }

    public string TypeName => _definition.TypeName;

    public string DisplayNameText
    {
        get => _definition.DisplayName ?? string.Empty;
        set
        {
            if (string.Equals(_definition.DisplayName, value, StringComparison.Ordinal))
                return;

            _definition.DisplayName = value;
            DisplayName = value;
            OnPropertyChanged();
        }
    }

    public string DomainType
    {
        get => _definition.DomainType;
        set
        {
            if (string.Equals(_definition.DomainType, value, StringComparison.Ordinal))
                return;

            _definition.DomainType = value;
            OnPropertyChanged();
        }
    }

    public NodeShape NodeShape
    {
        get => _definition.NodeShape;
        set
        {
            if (_definition.NodeShape == value)
                return;

            _definition.NodeShape = value;
            OnPropertyChanged();
        }
    }

    public bool HasDetails
    {
        get => _definition.HasDetails;
        set
        {
            if (_definition.HasDetails == value)
                return;

            _definition.HasDetails = value;
            OnPropertyChanged();
        }
    }

    public InputFormVM PropertiesForm { get; } = new();

    public Action? OnDocumentChanged { get; private set; }

    public ICommand DeleteCommand => _deleteCommand ??= new RelayCommand(
        _ => Delete(),
        _ => !IsExternal && Parent is NodeLibraryVM);

    private void Delete()
    {
        if (IsExternal)
            return;

        if (Parent is NodeLibraryVM library)
            library.RemoveDefinition(this);
    }

    public void SetDocumentChangedCallback(Action? onDocumentChanged)
    {
        OnDocumentChanged = onDocumentChanged;
    }

    private void NotifyDocumentChanged()
    {
        OnDocumentChanged?.Invoke();
    }

    private void MakePropertiesForm()
    {
        PropertiesForm.Inputs.Add(new InputGroupVM("General", ("General", CreateGeneralInputs()), collapsable: false));
        PropertiesForm.Inputs.Add(new InputGroupVM("Style", ("Style", CreateStyleInputs()), collapsable: false));
    }

    private List<DataInputWrapperVM> CreateGeneralInputs()
    {
        return
        [
            new InputStringVM("Display Name:", _definition.DisplayName, displayName =>
            {
                DisplayNameText = displayName ?? string.Empty;
            }),

            new InputStringVM("Domain Type:", _definition.DomainType, domainType =>
            {
                DomainType = domainType ?? string.Empty;
            }),

            new InputEnumVM("Node Shape:", _definition.NodeShape, shape =>
            {
                if (shape == null)
                    return;

                NodeShape = (NodeShape)shape;
            }),

            new InputBoolVM("Has Details:", _definition.HasDetails, hasDetails =>
            {
                HasDetails = hasDetails;
            }),

        ];
    }

    private List<DataInputWrapperVM> CreateStyleInputs()
    {
        return
        [
            new InputColorVM("Background Color:", ShapeStyle.BackgroundColor.ToMediaColor(), color =>
            {
                ShapeStyle.BackgroundColor = color.ToNodeColor();
            }),

            new InputColorVM("Border Color:", ShapeStyle.BorderColor.ToMediaColor(), color =>
            {
                ShapeStyle.BorderColor = color.ToNodeColor();
            }),

            new InputFloatVM("Border Thickness:", ShapeStyle.BorderThickness, thickness =>
            {
                ShapeStyle.BorderThickness = thickness;
            }),

            new InputFloatVM("Font Size:", ShapeStyle.FontSize, fontSize =>
            {
                ShapeStyle.FontSize = fontSize;
            }),

            new InputColorVM("Font Color:", ShapeStyle.FontColor.ToMediaColor(), color =>
            {
                ShapeStyle.FontColor = color.ToNodeColor();
            }),

            new InputStringVM("Font Family:", ShapeStyle.FontFamily, fontFamily =>
            {
                ShapeStyle.FontFamily = fontFamily;
            }),

            new InputEnumVM("Font Weight:", ShapeStyle.FontWeight, fontWeight =>
            {
                if (fontWeight == null)
                    return;

                ShapeStyle.FontWeight = (NodeFontWeight)fontWeight;
            }),
        ];
    }

    private NodeShapeStyle ShapeStyle
    {
        get
        {
            _definition.Style ??= new NodeStyle();
            return _definition.Style.ShapeStyle;
        }
    }
}
