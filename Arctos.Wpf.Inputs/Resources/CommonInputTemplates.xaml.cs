using Arctos.Plugins;

namespace Arctos.Wpf.Inputs.Resources
{
    /// <summary>
    /// Interaction logic for CommonInputs.xaml
    /// </summary>
    public partial class CommonInputTemplates : IWpfResourceProvider
    {
        public CommonInputTemplates()
        {
            InitializeComponent();
        }

        public Uri ResourceUri =>
            new Uri("/Arctos.Wpf.Inputs;component/Resources/CommonInputTemplates.xaml", UriKind.RelativeOrAbsolute);
    }
}
