using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Arctos.Wpf.Controls
{
    public class ControlHelper
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached
        (
            "CornerRadius",
            typeof(double),
            typeof(ControlHelper),
            new PropertyMetadata(default(double)
            ));

        public static void SetCornerRadius(UIElement element, double value)
        {
            element.SetValue(CornerRadiusProperty, value);
        }

        public static double GetCornerRadius(UIElement element)
        {
            return (double)element.GetValue(CornerRadiusProperty);
        }

        #endregion CornerRadius

    }
}
