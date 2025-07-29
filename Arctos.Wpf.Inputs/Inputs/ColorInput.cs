using System.Windows.Media;

namespace Arctos.Wpf.Inputs
{
    public class ColorInput : Input
    {
        #region Color
        private Color _color;
        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }
        #endregion

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ColorInput(Color color)
        {
            Color = color;
        }

        #endregion Constructor
    }
}
