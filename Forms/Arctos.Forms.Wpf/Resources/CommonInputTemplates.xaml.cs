using Arctos.Plugins;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Forms.Wpf.Resources;

public partial class CommonInputTemplates : IWpfResourceProvider
{
    public CommonInputTemplates()
    {
        FormsWpfModule.EnsureRegistered();
        InitializeComponent();
    }

    public Uri ResourceUri => new("/Arctos.Forms.Wpf;component/Resources/CommonInputTemplates.xaml", UriKind.RelativeOrAbsolute);

    private void OpenBrushEditor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: InputBrushVM input })
            return;

        var window = new BrushEditorWindow(input)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(candidate => candidate.IsActive)
        };
        window.ShowDialog();
    }
}
