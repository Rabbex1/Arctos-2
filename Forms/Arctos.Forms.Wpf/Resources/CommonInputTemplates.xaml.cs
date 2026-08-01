using Arctos.Plugins;

namespace Arctos.Forms.Wpf.Resources;

public partial class CommonInputTemplates : IWpfResourceProvider
{
    public CommonInputTemplates()
    {
        FormsWpfModule.EnsureRegistered();
        InitializeComponent();
    }

    public Uri ResourceUri => new("/Arctos.Forms.Wpf;component/Resources/CommonInputTemplates.xaml", UriKind.RelativeOrAbsolute);
}
