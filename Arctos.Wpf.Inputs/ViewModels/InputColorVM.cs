using System.Windows.Media;
using Arctos.Wpf.Dialogs;

namespace Arctos.Wpf.Inputs
{
    public class InputColorVM : DataInputWrapperVM<Color>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Brush
        private SolidColorBrush? _brush;
        public SolidColorBrush? Brush
        {
            get => _brush;
            set => SetProperty(ref _brush, value);
        }
        #endregion

        #endregion Properties
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public InputColorVM(string description, Color color, Action<Color> onChange)
            : base(description, color, onChange)
        {
            Brush = new SolidColorBrush(color);
            OnButtonClicked = OpenDialog;
        }

        public InputColorVM(string description, Color color, Action<DataInputWrapperVM<Color>, Color> onChange)
        {
            Description = description;
            OnChangeWithSender = onChange;
            _value = color;
            Brush = new SolidColorBrush(color);
            OnButtonClicked = OpenDialog;
        }

        #endregion Constructors

        public void UpdateColor(Color color)
        {
            UpdateValue(color);
            Brush = new SolidColorBrush(color);
        }

        protected void OpenDialog(DataInputWrapperVM<Color> wrapper)
        {
            var currentColor = Brush?.Color ?? Value;
            var dialog = new ColorDialogVM
            {
                Color = System.Drawing.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B),
                AllowFullOpen = true,
                AnyColor = true,
                SolidColorOnly = false,
                FullOpen = true,
                DisplayName = "Select Color",
            };

            new ColorDialogPresenter().Show(dialog);

            if (!dialog.Result)
                return;

            var newColor = dialog.Color;
            Value = Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
            Brush = new SolidColorBrush(Value);
        }
    }
}
