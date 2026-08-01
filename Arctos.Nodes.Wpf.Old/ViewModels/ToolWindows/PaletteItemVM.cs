using Arctos;
using Arctos.Nodes.Wpf.DragImage;
using Arctos.Nodes.Wpf.ViewModels.Components;
using Arctos.Wpf.DragDrop;
using DragImageArtifact = Arctos.Nodes.Wpf.DragImage.DragImage;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Arctos.Nodes.Wpf.ViewModels.ToolWindows;

/// <summary>
/// Represents a single system-defined palette item that may be dragged into a node.
/// </summary>
public sealed class PaletteItemVM : BaseVM, IDragable, IDragFeedbackState
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the core palette item kind represented by this entry.
    /// </summary>
    public PaletteItemKind Kind { get; }

    /// <summary>
    /// Gets the user-facing label shown for this palette item.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the factory used to create a component instance from this palette item.
    /// </summary>
    public Func<NodeComponentVM> CreateComponent { get; }

    /// <summary>
    /// Gets or sets the cached drag image seed for this palette item.
    /// </summary>
    #region DragImage
    private DragImageArtifact? _dragImage;
    public DragImageArtifact? DragImage
    {
        get => _dragImage;
        set
        {
            if (!SetProperty(ref _dragImage, value))
                return;

            CursorImageSource = CreateCursorImageSource(value);
        }
    }
    #endregion

    /// <summary>
    /// Gets the WPF image source used by the custom drag cursor.
    /// </summary>
    #region CursorImageSource
    private ImageSource? _cursorImageSource;
    public ImageSource? CursorImageSource
    {
        get => _cursorImageSource;
        private set => SetProperty(ref _cursorImageSource, value);
    }
    #endregion

    /// <summary>
    /// Gets the drag/drop payload type exposed by this palette item.
    /// </summary>
    public Type DataType => typeof(PaletteItemVM);

    /// <summary>
    /// Gets the drag/drop payload object exposed by this palette item.
    /// </summary>
    public object DataObject => this;

    /// <summary>
    /// Gets or sets an optional override for the currently displayed drag feedback effect.
    /// </summary>
    public System.Windows.DragDropEffects? DragFeedbackEffectOverride { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new palette item view model.
    /// </summary>
    /// <param name="kind">The represented palette item kind.</param>
    /// <param name="label">The label shown for the palette item.</param>
    /// <param name="createComponent">The callback used to create a component from this palette item.</param>
    public PaletteItemVM(PaletteItemKind kind, string label, Func<NodeComponentVM> createComponent)
    {
        Kind = kind;
        Label = label ?? throw new ArgumentNullException(nameof(label));
        CreateComponent = createComponent ?? throw new ArgumentNullException(nameof(createComponent));
        DisplayName = label;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Removes the supplied dragged object from its owner. Palette items are immutable drag sources, so this is a no-op.
    /// </summary>
    /// <param name="i">The dragged object.</param>
    public void Remove(object i)
    {
        _ = i;
    }

    private static ImageSource? CreateCursorImageSource(DragImageArtifact? dragImage)
    {
        if (dragImage == null || dragImage.PngData.Length == 0)
            return null;

        using var stream = new MemoryStream(dragImage.PngData, writable: false);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    #endregion Public Methods
}
