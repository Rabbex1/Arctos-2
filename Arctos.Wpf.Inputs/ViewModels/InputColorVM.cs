using System.Windows.Media;

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

        #endregion Constructors

        protected void OpenDialog(DataInputWrapperVM<Color> wrapper)
        {
            throw new NotImplementedException("Color selection dialog is not implemented yet.");
            //var color = Brush.Color;
            //var dialog = new ColorDialogVM
            //{
            //    Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B),
            //    AllowFullOpen = true,
            //    AnyColor = true,
            //    SolidColorOnly = false,
            //    FullOpen = true,
            //    DisplayName = "Select Background Color",
            //};
            //var msg = new AppMessage
            //{
            //    Action = AppAction.ShowCommonDialog,
            //    Params = new object[] { dialog, null },
            //};
            //_messenger?.Send(msg);

            //if (msg.Cancel)
            //    return;
            //var newColor = dialog.Color;
            //Value = Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
            //Brush = new SolidColorBrush(Value);
        }
    }
}
