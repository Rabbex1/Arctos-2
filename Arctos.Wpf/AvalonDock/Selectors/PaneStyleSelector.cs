using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.AvalonDock
{
    public class PaneStyleSelector : StyleSelector
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Style? ToolWindowStyle { get; set; }

        public Style? DocumentWindowStyle { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            return item switch
            {
                AnchorablePaneVM _ => ToolWindowStyle,
                DocumentPaneVM _ => DocumentWindowStyle,
                _ => base.SelectStyle(item, container)
            };
        }
        #endregion Methods
    }
}
