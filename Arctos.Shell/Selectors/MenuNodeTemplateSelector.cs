using System.Windows;
using System.Windows.Controls;

namespace Arctos.Shell.Selectors
{
    public class MenuNodeTemplateSelector : ItemContainerTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
        {
            var key = new DataTemplateKey(item.GetType());
            var template = (DataTemplate)parentItemsControl.FindResource(key);
            return template;
        }
    }

}
