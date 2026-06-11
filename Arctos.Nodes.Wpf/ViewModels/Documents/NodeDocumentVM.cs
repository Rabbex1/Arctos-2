using System.ComponentModel;
using System.Collections.Generic;
using Arctos;
using Arctos.Nodes;
using Arctos.Nodes.Wpf.ViewModels;
using Arctos.Shell.Documents;
using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels.Documents;

/// <summary>
/// Shared shell document wrapper surface for node artifacts.
/// </summary>
public abstract class NodeDocumentVM : FileDocumentVM, INodeDocumentVM
{
    private readonly SkiaHostVM _skiaHost = new();
    private IModelHost? _navLinkSource;
    private INotifyPropertyChanged? _navLinkSourceNotifier;
    private NavLinkVM? _navLinkRoot;
    private NavLinkVM? _currentNavLink;
    private NodeColor _canvasBackgroundColor = NodeColor.Neutral.Surface;

    protected NodeDocumentVM(NodeSession session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        CurrentCanvas.BackgroundColor = _canvasBackgroundColor;
    }

    public NodeSession Session { get; }

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
            OnPropertyChanged(nameof(ShowDetailedViewToggle));
        }
    }

    public IEnumerable<NavLinkVM> NavLinks => EnumerateNavLinks();

    public SkiaHostVM CurrentCanvas => _skiaHost;

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

    public abstract Guid? DocumentId { get; }

    public abstract string DocumentName { get; }

    public abstract int DefinitionCount { get; }

    public abstract IModelHost? SelectedHost { get; }

    public InputFormVM? PropertiesForm => (SelectedHost as IEditableProperties)?.PropertiesForm;

    public bool ShowDetailedViewToggle => CurrentNavLink?.ModelHost.HasDetails == true && CurrentCanvas.ShowDrawModeToggle;

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

    protected void RefreshCanvasBackgroundColor()
    {
        CanvasBackgroundColor = ResolveCanvasBackgroundColor();
    }

    /// <summary>
    /// Marks the current document as changed and rebuilds the active visual tree.
    /// </summary>
    protected void MarkChanged()
    {
        IsChanged = true;
        RefreshCanvasBackgroundColor();
        CurrentCanvas.RebuildVisualTree();
    }

    private void OnNavLinkSourcePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!ShouldRefreshNavigationState(e.PropertyName))
            return;

        RefreshNavigationState();
    }

    private void RefreshNavigationState()
    {
        RefreshCanvasBackgroundColor();

        if (_navLinkSource == null)
        {
            NavLinkRoot = null;
            CurrentNavLink = null;
            CurrentCanvas.Host = null;
            CurrentCanvas.DetailedView = false;
            return;
        }

        if (CurrentNavLink != null && ReferenceEquals(CurrentNavLink.ModelHost, _navLinkSource))
        {
            CurrentNavLink.RefreshLabel();
            CurrentCanvas.Host = _navLinkSource;

            if (!_navLinkSource.HasDetails)
                CurrentCanvas.DetailedView = false;

            OnPropertyChanged(nameof(ShowDetailedViewToggle));
            return;
        }

        CurrentCanvas.Host = _navLinkSource;

        if (!_navLinkSource.HasDetails)
            CurrentCanvas.DetailedView = false;

        var rootLink = new NavLinkVM(_navLinkSource)
        {
            IsCurrent = true,
        };

        NavLinkRoot = rootLink;
        CurrentNavLink = rootLink;
    }

    private IEnumerable<NavLinkVM> EnumerateNavLinks()
    {
        for (var navLink = NavLinkRoot; navLink != null; navLink = navLink.Child)
            yield return navLink;
    }

    private static bool ShouldRefreshNavigationState(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
               || propertyName == nameof(BaseVM.DisplayName)
               || propertyName == nameof(NodeDefinitionVM.HasDetails)
               || propertyName == nameof(NodeDefinitionVM.DisplayNameText)
               || propertyName == nameof(NodeInstanceVM.NodeDefinitionName);
    }

    protected virtual NodeColor ResolveCanvasBackgroundColor()
    {
        return NodeColor.Neutral.Surface;
    }
}
