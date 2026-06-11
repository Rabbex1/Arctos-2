using Arctos.Wpf.Inputs;

namespace Arctos.Nodes.Wpf;

/// <summary>
/// Exposes an editable properties form for the shared properties toolbox.
/// </summary>
public interface IEditableProperties
{
    InputFormVM PropertiesForm { get; }
}
