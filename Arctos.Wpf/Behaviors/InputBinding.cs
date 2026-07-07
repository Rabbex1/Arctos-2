using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Arctos.Wpf.Behaviors
{
    public class InputBinding
    {
        static InputBinding()
        {

        }

        public static readonly DependencyProperty UpdateOnEnterProperty =
            DependencyProperty.RegisterAttached(
                "UpdateOnEnter", typeof(DependencyProperty), typeof(InputBinding),
                new PropertyMetadata(null, UpdateOnEnterPropertyChanged));

        public static void SetUpdateOnEnter(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdateOnEnterProperty, value);
        }

        public static DependencyProperty GetUpdateOnEnter(DependencyObject dp)
        {
            return (DependencyProperty) dp.GetValue(UpdateOnEnterProperty);
        }

        private static void UpdateOnEnterPropertyChanged(DependencyObject dp,
            DependencyPropertyChangedEventArgs e)
        {
            if (dp is not UIElement element)
            {
                return;
            }

            if (e.OldValue != null)
            {
                element.PreviewKeyDown -= HandlePreviewKeyDown;
            }

            if (e.NewValue != null)
            {
                element.PreviewKeyDown += HandlePreviewKeyDown;
            }
        }

        private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            if (sender is WpfTextBox { AcceptsReturn: true } textBox)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                    return;

                DoUpdateSource(textBox);
                e.Handled = true;
                return;
            }

            DoUpdateSource(e.Source);
        }

        private static void DoUpdateSource(object source)
        {
            if (source is not DependencyObject dependencyObject)
                return;
            var property = GetUpdateOnEnter(dependencyObject);

            if (dependencyObject is not UIElement element)
            {
                return;
            }

            var binding = BindingOperations.GetBindingExpression(element, property);

            binding?.UpdateSource();
        }
    }
}
