using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Inputs.Selectors
{
    public class InputTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            if (item is not DataInputWrapperVM input)
                return null;

            if (input is IInputMatrixVM)
                return Application.Current.Resources["InputMatrixTemplate"] as DataTemplate;

            var key = new DataTemplateKey(input.GetType());
            var dt = (DataTemplate)Application.Current.TryFindResource(key);
            if (dt != null) return dt;
            Debug.WriteLine($"InputTemplateSelector cannot find DataTemplate with Key: {input}");
            return null;
        }
    }
}
