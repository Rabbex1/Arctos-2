using System.Windows;
using System.Windows.Controls;

namespace Arctos.Nodes.Wpf.Selectors;

/// <summary>
/// Selects the appropriate structure template for the active node artifact document.
/// </summary>
public sealed class NodeStructureTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NodeMapDocumentTemplate { get; set; }

    public DataTemplate? NodeLibraryDocumentTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            INodeMapDocumentVM => NodeMapDocumentTemplate,
            INodeLibraryDocumentVM => NodeLibraryDocumentTemplate,
            _ => base.SelectTemplate(item, container),
        };
    }
}
