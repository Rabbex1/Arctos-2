using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TreeView = System.Windows.Controls.TreeView;

//https://stackoverflow.com/questions/664632/highlight-whole-treeviewitem-line-in-wpf

namespace Arctos.Wpf.ValueConverters
{
    public class TreeViewDepthToMarginConverter : IValueConverter
    {
        public double Offset { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Offset * GetDepth(item), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        private int GetDepth(TreeViewItem item)
        {
            TreeViewItem? parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem? GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (parent is not (TreeViewItem or TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if(parent is TreeView)
                return null;
            return parent as TreeViewItem ?? throw new DataException("Parent must be 'TreeViewItem'");
        }
    }

}