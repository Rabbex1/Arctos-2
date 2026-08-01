using System.Windows;
using System.Windows.Controls;

namespace Arctos.Forms.Wpf.Selectors;

public sealed class ParameterTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not DataInputWrapperVM input)
            return null;

        return input switch
        {
            IInputMatrixVM => Application.Current.Resources["ParameterMatrixInput"] as DataTemplate,
            InputGroupVM => Application.Current.Resources["ParameterGroupInput"] as DataTemplate,
            InputLabelVM => Application.Current.Resources["ParameterLabelInput"] as DataTemplate,
            _ => Application.Current.Resources["ParameterInput"] as DataTemplate
        };
    }
}
