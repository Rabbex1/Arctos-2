using System.Windows;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace Arctos.Wpf.Splash
{
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LogoSourceProperty =
            DependencyProperty.Register(nameof(LogoSource), typeof(ImageSource), typeof(SplashScreenWindow));

        public ImageSource LogoSource
        {
            get => (ImageSource)GetValue(LogoSourceProperty);
            set => SetValue(LogoSourceProperty, value);
        }

        public void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

    }
}
