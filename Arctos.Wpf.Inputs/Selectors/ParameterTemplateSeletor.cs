using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Inputs.Selectors
{
    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not DataInputWrapperVM input)
                return null;

            return input switch
            {
                InputGroupVM _ => Application.Current.Resources["ParameterGroupInput"] as DataTemplate,
                InputLabelVM _ => Application.Current.Resources["ParameterLabelInput"] as DataTemplate,
                _ => Application.Current.Resources["ParameterInput"] as DataTemplate
            };
        }
    }
}
