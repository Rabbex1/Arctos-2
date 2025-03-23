using System.Windows;

namespace Arctos.Wpf.AttachedProperties
{
    public enum Highlight
    {
        None,
        Solid,
        Outline,
        Both
    }

    public class SelectedItem : UIElement
    {
        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.RegisterAttached(
                "Highlight",
                typeof(Highlight),
                typeof(SelectedItem),
                new FrameworkPropertyMetadata(defaultValue: Highlight.Solid,
                    flags: FrameworkPropertyMetadataOptions.AffectsRender)
            );

        public static Highlight GetHighlight(UIElement target) =>
            (Highlight)target.GetValue(HighlightProperty);

        public static void SetHighlight(UIElement target, Highlight value) =>
            target.SetValue(HighlightProperty, value);
    }
}
