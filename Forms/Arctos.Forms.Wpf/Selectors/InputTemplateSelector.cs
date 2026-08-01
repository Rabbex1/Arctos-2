using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Forms.Wpf.Selectors;

public sealed class InputTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not DataInputWrapperVM input)
            return null;

        if (input is IInputMatrixVM)
            return Application.Current.Resources["InputMatrixTemplate"] as DataTemplate;

        var key = new DataTemplateKey(input.GetType());
        var template = Application.Current.TryFindResource(key) as DataTemplate;
        if (template != null)
            return template;

        Debug.WriteLine($"InputTemplateSelector cannot find DataTemplate with Key: {input}");
        return null;
    }
}
