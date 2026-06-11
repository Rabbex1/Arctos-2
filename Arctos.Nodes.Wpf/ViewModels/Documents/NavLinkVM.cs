using Arctos;

namespace Arctos.Nodes.Wpf.ViewModels.Documents;

/// <summary>
/// Represents a linked navigation step for the current document view path.
/// </summary>
public sealed class NavLinkVM : BaseVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private NavLinkVM? _child;
    private bool _isCurrent;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>Gets the parent navigation step, or <see langword="null"/> for the root.</summary>
    public NavLinkVM? Parent { get; }

    /// <summary>Gets or sets the child navigation step.</summary>
    public NavLinkVM? Child
    {
        get => _child;
        set => SetProperty(ref _child, value);
    }

    /// <summary>Gets the hosted model for this navigation step.</summary>
    public IModelHost ModelHost { get; }

    /// <summary>Gets the label shown in the document header.</summary>
    public string Label => ModelHost.DisplayName;

    /// <summary>Gets a value indicating whether this link is the root of the current path.</summary>
    public bool IsRoot => Parent == null;

    /// <summary>Gets or sets a value indicating whether this link is the active step.</summary>
    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetProperty(ref _isCurrent, value);
    }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new navigation link for the given model host.
    /// </summary>
    /// <param name="modelHost">The hosted model shown for this navigation step.</param>
    /// <param name="parent">The parent navigation link.</param>
    public NavLinkVM(IModelHost modelHost, NavLinkVM? parent = null)
    {
        ModelHost = modelHost ?? throw new ArgumentNullException(nameof(modelHost));
        Parent = parent;
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    /// <summary>
    /// Refreshes the displayed label after the hosted model display name changes.
    /// </summary>
    public void RefreshLabel()
    {
        OnPropertyChanged(nameof(Label));
    }

    #endregion Public Methods
}
