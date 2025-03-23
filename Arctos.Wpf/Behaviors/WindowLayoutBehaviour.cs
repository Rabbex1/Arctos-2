using System.Windows;
using Microsoft.Xaml.Behaviors;
using Arctos.Wpf.Controls;
using Arctos.Wpf.Utilities;

namespace Arctos.Wpf.Behaviors
{
    public class WindowLayoutBehaviour : Behavior<MetroWindow>
    {
        private IWindowLayoutSettings? _layoutSettings;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Initialized += OnInitialized;
            AssociatedObject.Closed += OnClosing;

            _layoutSettings = new WindowLayoutSettings();
        }


        private void OnInitialized(object? sender, EventArgs e)
        {
            if (_layoutSettings?.Layout == null)
            {
                AssociatedObject.Width = SystemParameters.PrimaryScreenWidth * 0.8;
                AssociatedObject.Height = SystemParameters.PrimaryScreenHeight * 0.8;
                AssociatedObject.Left = SystemParameters.PrimaryScreenWidth / 2 - AssociatedObject.Width / 2;
                AssociatedObject.Top = SystemParameters.PrimaryScreenHeight / 2 - AssociatedObject.Height / 2;
                AssociatedObject.WindowState = WindowState.Normal;
            }
            else
            {
                var layout = _layoutSettings.Layout;
                AssociatedObject.Width = layout.Width;
                AssociatedObject.Height = layout.Height;
                AssociatedObject.Left = layout.Left;
                AssociatedObject.Top = layout.Top;
                AssociatedObject.WindowState = layout.State;
            }
        }

        private void OnClosing(object? sender, EventArgs args)
        {
            var layout = new WindowLayout
            {
                Width = AssociatedObject.Width,
                Height = AssociatedObject.Height,
                Left = AssociatedObject.Left,
                Top = AssociatedObject.Top,
                State = AssociatedObject.WindowState
            };
            if (_layoutSettings == null) return;
            _layoutSettings.Layout = layout;
            _layoutSettings.Save();
        }
    }
}
