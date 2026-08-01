using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf.ViewModels;

/// <summary>
/// Represents a single edit tab shown in the shared edit toolbox.
/// </summary>
public sealed class EditTabVM : BaseVM
{
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    /// <summary>
    /// Gets the tab header text.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets the input form shown inside the tab.
    /// </summary>
    public InputFormVM Form { get; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    /// <summary>
    /// Creates a new edit tab view model.
    /// </summary>
    /// <param name="header">The tab header text.</param>
    /// <param name="form">The input form shown inside the tab.</param>
    public EditTabVM(string header, InputFormVM form)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentNullException.ThrowIfNull(form);

        Header = header;
        Form = form;
        DisplayName = header;
    }

    #endregion Constructors
}
