//using Microsoft.Win32;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace Arctos.Wpf.Dialogs
{
    public class OpenFileDialogPresenter : IDialogPresenter<OpenFileDialogVM>
    {
        public void Show(OpenFileDialogVM vm)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = vm.MultiSelect,
                ReadOnlyChecked = vm.ReadOnlyChecked,
                ShowReadOnly = vm.ShowReadOnly,
                FileName = vm.FileName,
                Filter = vm.Filter,
                InitialDirectory = vm.InitialDirectory,
                RestoreDirectory = vm.RestoreDirectory,
                Title = vm.Title,
                ValidateNames = vm.ValidateNames
            };


            vm.RaiseDialogOpening();
            var result = dlg.ShowDialog();
            vm.Result = result == System.Windows.Forms.DialogResult.OK;

            vm.MultiSelect = dlg.Multiselect;
            vm.ReadOnlyChecked = dlg.ReadOnlyChecked;
            vm.ShowReadOnly = dlg.ShowReadOnly;
            vm.FileName = dlg.FileName;
            vm.FileNames = dlg.FileNames;
            vm.Filter = dlg.Filter;
            vm.InitialDirectory = dlg.InitialDirectory;
            vm.RestoreDirectory = dlg.RestoreDirectory;
            vm.SafeFileName = dlg.SafeFileName;
            vm.SafeFileNames = dlg.SafeFileNames;
            vm.Title = dlg.Title;
            vm.ValidateNames = dlg.ValidateNames;

            vm.RaiseDialogClosing(new CloseDialogArgs { Result = vm.Result ? DialogResult.Ok : DialogResult.Cancel });
        }
    }
}