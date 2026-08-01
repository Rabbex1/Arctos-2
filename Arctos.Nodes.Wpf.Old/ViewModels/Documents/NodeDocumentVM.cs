using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Arctos;
using Arctos.History;
using Arctos.Nodes;
using Arctos.Nodes.Models;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Shell.Documents;
using Arctos.Wpf.Utilities;

namespace Arctos.Nodes.Wpf.ViewModels.Documents;

/// <summary>
/// Shared shell document wrapper surface for node artifacts.
/// </summary>
public abstract class NodeDocumentVM : FileDocumentVM, INodeDocumentVM, IHistory
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly SkiaHostVM _skiaHost = new();
    private readonly IHistoryService _historyService;
    private bool _historyReleased;
    private IModelHost? _navLinkSource;
    private INotifyPropertyChanged? _navLinkSourceNotifier;
    private NavLinkVM? _currentNavLink;
    private EditTabVM? _selectedEditTab;
    private NodeColor _canvasBackgroundColor = NodeColor.Neutral.Surface;
    private NodeColor _canvasFontColor = NodeColor.Black;
    private string? _canvasFontFamily;
    private float _canvasFontSize = 12.0f;
    private float _canvasHeaderFontSize = 14.0f;
    private float _canvasPadding = 1.0f;
    private float _canvasRowSpacing;
    private float _canvasColumnSpacing = 1.0f;
    private float _canvasRoundedCornerRadius = 2.0f;
    private float _canvasControlCornerRadius = 1.0f;
    private float _canvasMinimumColumnWidth = 24.0f;
    private float _canvasPortSize = 1.5f;
    private NodePortPlacement _canvasPortPlacement = NodePortPlacement.Over;
    private ICommand? _toggleSurfaceModeCommand;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the owning node session.
    /// </summary>
    public NodeSession Session { get; }

    /// <summary>
    /// Gets the active document surface currently presented by this shell document.
    /// </summary>
    public virtual INodeDocumentVM PresentationDocument => this;

    /// <summary>
    /// Gets a value indicating whether this shell document is currently presenting its design surface.
    /// </summary>
    public virtual bool IsDesignMode => true;

    /// <summary>
    /// Gets a value indicating whether this shell document is currently presenting its transient test surface.
    /// </summary>
    public bool IsTestMode => !IsDesignMode;

    /// <summary>
    /// Gets a value indicating whether this document can switch between design and test surfaces.
    /// </summary>
    public virtual bool CanToggleSurfaceMode => false;

    /// <summary>
    /// Gets the user-facing surface mode text.
    /// </summary>
    public string SurfaceMode => IsDesignMode ? "Design" : "Test";

    /// <summary>
    /// Gets the user-facing command text for switching surface modes.
    /// </summary>
    public string ToggleSurfaceModeText => IsDesignMode ? "Enter Test" : "Return Design";

    /// <summary>
    /// Gets the command that toggles between the design and test surfaces when supported.
    /// </summary>
    public ICommand ToggleSurfaceModeCommand => _toggleSurfaceModeCommand ??= new RelayCommand(
        _ => ToggleSurfaceMode(),
        _ => CanToggleSurfaceMode);

    /// <summary>
    /// Gets the document-scoped history context.
    /// </summary>
    public IHistoryContext History { get; }

    /// <summary>
    /// Gets or sets the root navigation link for the current document path.
    /// </summary>
    #region NavLinkRoot
    private NavLinkVM? _navLinkRoot;
    public NavLinkVM? NavLinkRoot
    {
        get => _navLinkRoot;
        protected set
        {
            if (!SetProperty(ref _navLinkRoot, value))
                return;

            OnPropertyChanged(nameof(NavLinks));
        }
    }
    #endregion

    /// <summary>
    /// Gets or sets the current navigation link for the active document path.
    /// </summary>
    public NavLinkVM? CurrentNavLink
    {
        get => _currentNavLink;
        protected set
        {
            if (ReferenceEquals(_currentNavLink, value))
                return;

            if (_currentNavLink != null)
                _currentNavLink.IsCurrent = false;

            _currentNavLink = value;

            if (_currentNavLink != null)
                _currentNavLink.IsCurrent = true;

            OnPropertyChanged();
            OnPropertyChanged(nameof(NavLinks));
            OnPropertyChanged(nameof(CurrentCanvas));
            OnPropertyChanged(nameof(ShowOverviewToggle));
        }
    }

    /// <summary>
    /// Gets the current navigation chain from root to leaf.
    /// </summary>
    public IEnumerable<NavLinkVM> NavLinks => EnumerateNavLinks();

    /// <summary>
    /// Gets the single shared Skia host for the document.
    /// </summary>
    public SkiaHostVM CurrentCanvas => _skiaHost;

    /// <summary>
    /// Gets or sets the resolved canvas background color for the current document context.
    /// </summary>
    public NodeColor CanvasBackgroundColor
    {
        get => _canvasBackgroundColor;
        protected set
        {
            if (_canvasBackgroundColor == value)
            {
                CurrentCanvas.BackgroundColor = value;
                return;
            }

            _canvasBackgroundColor = value;
            CurrentCanvas.BackgroundColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved canvas font color for the current document context.
    /// </summary>
    public NodeColor CanvasFontColor
    {
        get => _canvasFontColor;
        protected set
        {
            if (_canvasFontColor == value)
            {
                CurrentCanvas.FontColor = value;
                return;
            }

            _canvasFontColor = value;
            CurrentCanvas.FontColor = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved canvas font family for the current document context.
    /// </summary>
    public string? CanvasFontFamily
    {
        get => _canvasFontFamily;
        protected set
        {
            if (_canvasFontFamily == value)
            {
                CurrentCanvas.FontFamily = value;
                return;
            }

            _canvasFontFamily = value;
            CurrentCanvas.FontFamily = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved canvas font size for the current document context.
    /// </summary>
    public float CanvasFontSize
    {
        get => _canvasFontSize;
        protected set
        {
            if (Math.Abs(_canvasFontSize - value) < float.Epsilon)
            {
                CurrentCanvas.FontSize = value;
                return;
            }

            _canvasFontSize = value;
            CurrentCanvas.FontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved canvas header font size for the current document context.
    /// </summary>
    public float CanvasHeaderFontSize
    {
        get => _canvasHeaderFontSize;
        protected set
        {
            if (Math.Abs(_canvasHeaderFontSize - value) < float.Epsilon)
            {
                CurrentCanvas.HeaderFontSize = value;
                return;
            }

            _canvasHeaderFontSize = value;
            CurrentCanvas.HeaderFontSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved detailed node padding for the current document context.
    /// </summary>
    public float CanvasPadding
    {
        get => _canvasPadding;
        protected set
        {
            if (Math.Abs(_canvasPadding - value) < float.Epsilon)
            {
                CurrentCanvas.LayoutPadding = value;
                return;
            }

            _canvasPadding = value;
            CurrentCanvas.LayoutPadding = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved detailed node row spacing for the current document context.
    /// </summary>
    public float CanvasRowSpacing
    {
        get => _canvasRowSpacing;
        protected set
        {
            if (Math.Abs(_canvasRowSpacing - value) < float.Epsilon)
            {
                CurrentCanvas.RowSpacing = value;
                return;
            }

            _canvasRowSpacing = value;
            CurrentCanvas.RowSpacing = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved detailed node column spacing for the current document context.
    /// </summary>
    public float CanvasColumnSpacing
    {
        get => _canvasColumnSpacing;
        protected set
        {
            if (Math.Abs(_canvasColumnSpacing - value) < float.Epsilon)
            {
                CurrentCanvas.ColumnSpacing = value;
                return;
            }

            _canvasColumnSpacing = value;
            CurrentCanvas.ColumnSpacing = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved rounded rectangle corner radius for the current document context.
    /// </summary>
    public float CanvasRoundedCornerRadius
    {
        get => _canvasRoundedCornerRadius;
        protected set
        {
            if (Math.Abs(_canvasRoundedCornerRadius - value) < float.Epsilon)
            {
                CurrentCanvas.RoundedCornerRadius = value;
                return;
            }

            _canvasRoundedCornerRadius = value;
            CurrentCanvas.RoundedCornerRadius = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved control corner radius for the current document context.
    /// </summary>
    public float CanvasControlCornerRadius
    {
        get => _canvasControlCornerRadius;
        protected set
        {
            if (Math.Abs(_canvasControlCornerRadius - value) < float.Epsilon)
            {
                CurrentCanvas.ControlCornerRadius = value;
                return;
            }

            _canvasControlCornerRadius = value;
            CurrentCanvas.ControlCornerRadius = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved minimum width used by a single detailed-layout content column for the current document context.
    /// </summary>
    public float CanvasMinimumColumnWidth
    {
        get => _canvasMinimumColumnWidth;
        protected set
        {
            if (Math.Abs(_canvasMinimumColumnWidth - value) < float.Epsilon)
            {
                CurrentCanvas.MinimumColumnWidth = value;
                return;
            }

            _canvasMinimumColumnWidth = value;
            CurrentCanvas.MinimumColumnWidth = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved shared port glyph size for the current document context.
    /// </summary>
    public float CanvasPortSize
    {
        get => _canvasPortSize;
        protected set
        {
            if (Math.Abs(_canvasPortSize - value) < float.Epsilon)
            {
                CurrentCanvas.PortSize = value;
                return;
            }

            _canvasPortSize = value;
            CurrentCanvas.PortSize = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the resolved shared port placement for the current document context.
    /// </summary>
    public NodePortPlacement CanvasPortPlacement
    {
        get => _canvasPortPlacement;
        protected set
        {
            if (_canvasPortPlacement == value)
            {
                CurrentCanvas.PortPlacement = value;
                return;
            }

            _canvasPortPlacement = value;
            CurrentCanvas.PortPlacement = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the persisted document identifier when one exists.
    /// </summary>
    public abstract Guid? DocumentId { get; }

    /// <summary>
    /// Gets the current document display name.
    /// </summary>
    public abstract string DocumentName { get; }

    /// <summary>
    /// Gets the total number of definitions exposed by the document.
    /// </summary>
    public abstract int DefinitionCount { get; }

    /// <summary>
    /// Gets the currently selected navigation host.
    /// </summary>
    public abstract IModelHost? SelectedHost { get; }

    /// <summary>
    /// Gets the edit tabs for the current selection when it is editable.
    /// </summary>
    public IReadOnlyList<EditTabVM>? EditTabs => (SelectedHost as IEditTabs)?.EditTabs;

    /// <summary>
    /// Gets the display name for the current edit target shown in the edit tools.
    /// </summary>
    public string CurrentEditTargetDisplayName => SelectedHost is NodeDefinitionVM definitionHost
        ? definitionHost.CurrentEditTargetDisplayName
        : SelectedHost?.DisplayName ?? string.Empty;

    /// <summary>
    /// Gets or sets the currently selected edit tab for the active selection.
    /// </summary>
    public EditTabVM? SelectedEditTab
    {
        get => _selectedEditTab;
        set => SetProperty(ref _selectedEditTab, value);
    }

    /// <summary>
    /// Gets a value indicating whether the overview toggle should be shown for the current selection.
    /// </summary>
    public bool ShowOverviewToggle => CurrentNavLink?.ModelHost.HasOverview == true && CurrentCanvas.ShowDrawModeToggle;

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new node document wrapper.
    /// </summary>
    /// <param name="session">The owning node session.</param>
    protected NodeDocumentVM(NodeSession session, IHistoryService historyService, IHistoryContext history)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        History = history ?? throw new ArgumentNullException(nameof(history));
        CurrentCanvas.HistoryContext = History;
        CurrentCanvas.BackgroundColor = _canvasBackgroundColor;
        CurrentCanvas.FontColor = _canvasFontColor;
        CurrentCanvas.FontFamily = _canvasFontFamily;
        CurrentCanvas.FontSize = _canvasFontSize;
        CurrentCanvas.HeaderFontSize = _canvasHeaderFontSize;
        CurrentCanvas.LayoutPadding = _canvasPadding;
        CurrentCanvas.RowSpacing = _canvasRowSpacing;
        CurrentCanvas.ColumnSpacing = _canvasColumnSpacing;
        CurrentCanvas.RoundedCornerRadius = _canvasRoundedCornerRadius;
        CurrentCanvas.ControlCornerRadius = _canvasControlCornerRadius;
        CurrentCanvas.MinimumColumnWidth = _canvasMinimumColumnWidth;
        CurrentCanvas.PortSize = _canvasPortSize;
        CurrentCanvas.PortPlacement = _canvasPortPlacement;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Protected Methods

    /// <inheritdoc />
    public void ReleaseHistoryContext()
    {
        if (_historyReleased)
            return;

        _historyReleased = true;
        _historyService.RemoveContext(History.Id);
    }

    /// <summary>
    /// Updates the current navigation source from the provided selected host.
    /// </summary>
    /// <param name="selectedHost">The host to use as the navigation source.</param>
    protected void UpdateNavLinksFromSelection(IModelHost? selectedHost)
    {
        if (ReferenceEquals(_navLinkSource, selectedHost))
        {
            RefreshNavigationState();
            return;
        }

        if (_navLinkSourceNotifier != null)
            _navLinkSourceNotifier.PropertyChanged -= OnNavLinkSourcePropertyChanged;

        _navLinkSource = selectedHost;
        _navLinkSourceNotifier = selectedHost as INotifyPropertyChanged;

        if (_navLinkSourceNotifier != null)
            _navLinkSourceNotifier.PropertyChanged += OnNavLinkSourcePropertyChanged;

        RefreshNavigationState();
    }

    /// <summary>
    /// Refreshes the current document canvas background color.
    /// </summary>
    protected void RefreshCanvasBackgroundColor()
    {
        CanvasBackgroundColor = ResolveCanvasBackgroundColor();
    }

    /// <summary>
    /// Refreshes the current document canvas typography settings.
    /// </summary>
    protected void RefreshCanvasTypography()
    {
        CanvasFontColor = ResolveCanvasFontColor();
        CanvasFontFamily = ResolveCanvasFontFamily();
        CanvasFontSize = ResolveCanvasFontSize();
        CanvasHeaderFontSize = ResolveCanvasHeaderFontSize();
    }

    /// <summary>
    /// Refreshes the current document node layout settings.
    /// </summary>
    protected void RefreshCanvasLayout()
    {
        CanvasPadding = ResolveCanvasPadding();
        CanvasRowSpacing = ResolveCanvasRowSpacing();
        CanvasColumnSpacing = ResolveCanvasColumnSpacing();
        CanvasRoundedCornerRadius = ResolveCanvasRoundedCornerRadius();
        CanvasControlCornerRadius = ResolveCanvasControlCornerRadius();
        CanvasMinimumColumnWidth = ResolveCanvasMinimumColumnWidth();
        CanvasPortSize = ResolveCanvasPortSize();
        CanvasPortPlacement = ResolveCanvasPortPlacement();
    }

    /// <summary>
    /// Marks the current document as changed and rebuilds the active visual tree.
    /// </summary>
    protected void MarkChanged()
    {
        IsChanged = true;
        RefreshCanvasBackgroundColor();
        RefreshCanvasTypography();
        RefreshCanvasLayout();
        CurrentCanvas.RebuildVisualTree();
    }

    /// <summary>
    /// Refreshes the current canvas surface mode flags.
    /// </summary>
    protected void RefreshCanvasSurfaceMode()
    {
        CurrentCanvas.IsDesignMode = IsDesignMode;
    }

    /// <summary>
    /// Creates a recorder that writes actions into the current document history and keeps the canvas up to date.
    /// </summary>
    /// <returns>The recorder callback used by child view models.</returns>
    protected Action<string, Action, Action> CreateHistoryRecorder()
    {
        return (description, redo, undo) =>
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description);
            ArgumentNullException.ThrowIfNull(redo);
            ArgumentNullException.ThrowIfNull(undo);

            History.Execute(
                description,
                () =>
                {
                    redo();
                    MarkChanged();
                },
                () =>
                {
                    undo();
                    MarkChanged();
                });
        };
    }

    /// <summary>
    /// Gets the history service used by this document and any transient child surfaces.
    /// </summary>
    protected IHistoryService HistoryService => _historyService;

    /// <summary>
    /// Toggles the current surface mode when supported by the derived document type.
    /// </summary>
    protected virtual void ToggleSurfaceMode()
    {
    }

    /// <summary>
    /// Resolves the effective canvas background color for the current document context.
    /// </summary>
    /// <returns>The effective canvas background color.</returns>
    protected virtual NodeColor ResolveCanvasBackgroundColor()
    {
        return NodeColor.Neutral.Surface;
    }

    /// <summary>
    /// Validates component UIDs in the raw file and optionally auto-fixes the file in place before loading continues.
    /// </summary>
    protected bool ValidateAndAutoFixComponentUids<TFile>(
        string filePath,
        Func<string, TFile?> deserialize,
        Action<TFile> save)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(deserialize);
        ArgumentNullException.ThrowIfNull(save);

        var json = File.ReadAllText(filePath);
        var validation = NodeComponentUidValidator.ValidateJson(json);
        if (validation.IsValid)
            return true;

        var promptText =
            $"Invalid file:{Environment.NewLine}{Environment.NewLine}" +
            $"Missing component UIDs: {validation.MissingUidCount}{Environment.NewLine}" +
            $"Duplicate component UIDs: {validation.DuplicateUidCount}{Environment.NewLine}{Environment.NewLine}" +
            "Auto fix - Yes/No";

        var result = ShowLoadValidationMessageBox(promptText, "Invalid File");
        if (result != MessageBoxResult.Yes)
        {
            AbortLoad();
            return false;
        }

        var file = deserialize(json)
            ?? throw new InvalidOperationException($"The file '{filePath}' could not be deserialized.");
        _ = file switch
        {
            NodeMapFile mapFile => NodeComponentUidValidator.AutoFix(mapFile),
            NodeLibraryFile libraryFile => NodeComponentUidValidator.AutoFix(libraryFile),
            _ => throw new NotSupportedException($"Unsupported file validation type '{typeof(TFile).Name}'."),
        };
        save(file);
        return true;
    }

    /// <summary>
    /// Resolves the effective canvas font family for the current document context.
    /// </summary>
    /// <returns>The effective canvas font family, or <see langword="null"/> to use the host default font.</returns>
    protected virtual string? ResolveCanvasFontFamily()
    {
        return null;
    }

    /// <summary>
    /// Resolves the effective canvas font color for the current document context.
    /// </summary>
    /// <returns>The effective canvas font color.</returns>
    protected virtual NodeColor ResolveCanvasFontColor()
    {
        return NodeColor.Black;
    }

    /// <summary>
    /// Resolves the effective canvas font size for the current document context.
    /// </summary>
    /// <returns>The effective canvas font size.</returns>
    protected virtual float ResolveCanvasFontSize()
    {
        return 12.0f;
    }

    /// <summary>
    /// Resolves the effective canvas header font size for the current document context.
    /// </summary>
    /// <returns>The effective canvas header font size.</returns>
    protected virtual float ResolveCanvasHeaderFontSize()
    {
        return 14.0f;
    }

    /// <summary>
    /// Resolves the effective detailed node padding for the current document context.
    /// </summary>
    /// <returns>The effective detailed node padding in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasPadding()
    {
        return 1.0f;
    }

    /// <summary>
    /// Resolves the effective detailed node row spacing for the current document context.
    /// </summary>
    /// <returns>The effective detailed node row spacing in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasRowSpacing()
    {
        return 0.0f;
    }

    /// <summary>
    /// Resolves the effective detailed node column spacing for the current document context.
    /// </summary>
    /// <returns>The effective detailed node column spacing in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasColumnSpacing()
    {
        return 1.0f;
    }

    /// <summary>
    /// Resolves the effective rounded rectangle corner radius for the current document context.
    /// </summary>
    /// <returns>The effective rounded rectangle corner radius in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasRoundedCornerRadius()
    {
        return 2.0f;
    }

    /// <summary>
    /// Resolves the effective control corner radius for the current document context.
    /// </summary>
    /// <returns>The effective control corner radius in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasControlCornerRadius()
    {
        return 1.0f;
    }

    /// <summary>
    /// Resolves the effective minimum width used by a single detailed-layout content column for the current document context.
    /// </summary>
    /// <returns>The effective minimum column width in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasMinimumColumnWidth()
    {
        return 24.0f;
    }

    /// <summary>
    /// Resolves the effective shared port glyph size for the current document context.
    /// </summary>
    /// <returns>The effective port size in <c>nu</c>.</returns>
    protected virtual float ResolveCanvasPortSize()
    {
        return 1.5f;
    }

    /// <summary>
    /// Resolves the effective shared port placement for the current document context.
    /// </summary>
    /// <returns>The effective port placement.</returns>
    protected virtual NodePortPlacement ResolveCanvasPortPlacement()
    {
        return NodePortPlacement.Over;
    }

    #endregion Protected Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void OnNavLinkSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!ShouldRefreshNavigationState(e.PropertyName))
            return;

        RefreshNavigationState();
    }

    private void RefreshNavigationState()
    {
        RefreshCanvasBackgroundColor();
        RefreshCanvasTypography();
        RefreshCanvasLayout();

        if (_navLinkSource == null)
        {
            NavLinkRoot = null;
            CurrentNavLink = null;
            CurrentCanvas.Host = null;
            CurrentCanvas.Overview = false;
            SelectedEditTab = null;
            OnPropertyChanged(nameof(EditTabs));
            OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
            return;
        }

        if (CurrentNavLink != null && ReferenceEquals(CurrentNavLink.ModelHost, _navLinkSource))
        {
            CurrentNavLink.RefreshLabel();
            CurrentCanvas.Host = _navLinkSource;

            if (!_navLinkSource.HasOverview)
                CurrentCanvas.Overview = false;

            EnsureSelectedEditTab();
            OnPropertyChanged(nameof(EditTabs));
            OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
            OnPropertyChanged(nameof(ShowOverviewToggle));
            return;
        }

        CurrentCanvas.Host = _navLinkSource;

        if (!_navLinkSource.HasOverview)
            CurrentCanvas.Overview = false;

        var rootLink = new NavLinkVM(_navLinkSource)
        {
            IsCurrent = true,
        };

        NavLinkRoot = rootLink;
        CurrentNavLink = rootLink;
        EnsureSelectedEditTab();
        OnPropertyChanged(nameof(EditTabs));
        OnPropertyChanged(nameof(CurrentEditTargetDisplayName));
    }

    private IEnumerable<NavLinkVM> EnumerateNavLinks()
    {
        for (var navLink = NavLinkRoot; navLink != null; navLink = navLink.Child)
            yield return navLink;
    }

    private static MessageBoxResult ShowLoadValidationMessageBox(string message, string caption)
    {
        if (Application.Current?.Dispatcher == null || Application.Current.Dispatcher.CheckAccess())
            return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        return Application.Current.Dispatcher.Invoke(
            () => MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning));
    }

    private static bool ShouldRefreshNavigationState(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
               || propertyName == nameof(BaseVM.DisplayName)
               || propertyName == nameof(NodeDefinitionVM.HasOverview)
               || propertyName == nameof(NodeDefinitionVM.DisplayNameText)
               || propertyName == nameof(NodeDefinitionVM.SelectedComponent)
               || propertyName == nameof(NodeDefinitionVM.EditTabs)
               || propertyName == nameof(NodeDefinitionVM.CurrentEditTargetDisplayName)
               || propertyName == nameof(NodeInstanceVM.NodeDefinitionName);
    }

    private void EnsureSelectedEditTab()
    {
        var editTabs = EditTabs;

        if (editTabs == null || editTabs.Count == 0)
        {
            SelectedEditTab = null;
            return;
        }

        if (SelectedEditTab != null && editTabs.Contains(SelectedEditTab))
            return;

        SelectedEditTab = editTabs[0];
    }

    #endregion Private Methods
}
