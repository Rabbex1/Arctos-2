namespace Arctos.Wpf.Dialogs
{
    public class ColorDialogPresenter : IDialogPresenter<ColorDialogVM>
    {
        public void Show(ColorDialogVM vm)
        {
            var dlg = new ColorDialog
            {
                AllowFullOpen = vm.AllowFullOpen,
                Color = vm.Color,
                CustomColors = vm.CustomColors,
                AnyColor = vm.AnyColor,
                FullOpen = vm.FullOpen,
                SolidColorOnly = vm.SolidColorOnly,
            };


            vm.RaiseDialogOpening();
            var result = dlg.ShowDialog();
            vm.Result = result == System.Windows.Forms.DialogResult.OK;

            vm.AllowFullOpen = dlg.AllowFullOpen;
            vm.Color = dlg.Color;
            vm.CustomColors = dlg.CustomColors;
            vm.AnyColor = dlg.AnyColor;
            vm.FullOpen = dlg.FullOpen;
            vm.SolidColorOnly = dlg.SolidColorOnly;

            vm.OnCLose?.Invoke(vm.Result ? DialogResult.Ok : DialogResult.Cancel);
            vm.RaiseDialogClosing(new CloseDialogArgs { Result = vm.Result ? DialogResult.Ok : DialogResult.Cancel });
        }
    }
}
