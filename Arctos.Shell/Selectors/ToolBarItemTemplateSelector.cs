using System.Windows;
using System.Windows.Controls;
using Arctos.Shell.Toolbar;

namespace Arctos.Shell.Selectors
{
    public class ToolBarItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ToolBarButtonTemplate { get; set; }
        public DataTemplate? ToolBarToggleButtonTemplate { get; set; }
        //public DataTemplate ToolBarRadioButtonTemplate { get; set; }
        //public DataTemplate ToolBarTextBoxTemplate { get; set; }
        //public DataTemplate ToolBarComboTemplate { get; set; }
        //public DataTemplate ToolBarSeparatorTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {

            switch (item)
            {
                //case CommandRadioButtonVM _:
                //    return ToolBarRadioButtonTemplate;
                case ToolbarButtonVM buttonVM:
                    return buttonVM.IsCheckable ? ToolBarToggleButtonTemplate : ToolBarButtonTemplate;
                //case CommandTextBoxVM _:
                //    return ToolBarTextBoxTemplate;
                //case CommandComboVM _:
                //    return ToolBarComboTemplate;
                //case CommandGroupVM _:
                //    return ToolBarSeparatorTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}