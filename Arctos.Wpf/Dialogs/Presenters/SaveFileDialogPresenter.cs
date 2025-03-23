using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace Arctos.Wpf.Dialogs
{
    public class SaveFileDialogPresenter : IDialogPresenter<SaveFileDialogVM>
    {
        public void Show(SaveFileDialogVM vm)
        {
            var dlg = new SaveFileDialog
            {
                AddExtension = vm.AddExtension,
                CheckFileExists = vm.CheckFileExists,
                CheckPathExists = vm.CheckPathExists,
                FileName = vm.FileName,
                Filter = vm.Filter,
                FilterIndex = vm.FilterIndex,
                InitialDirectory = vm.InitialDirectory,
                OverwritePrompt = vm.OverwritePrompt,
                RestoreDirectory = vm.RestoreDirectory,
                Title = vm.Title,
                ValidateNames = vm.ValidateNames
            };

            vm.RaiseDialogOpening();
            var result = dlg.ShowDialog();
            vm.Result = result == System.Windows.Forms.DialogResult.OK;

            vm.AddExtension = dlg.AddExtension;
            vm.CheckFileExists = dlg.CheckFileExists;
            vm.CheckPathExists = dlg.CheckPathExists;
            vm.FileName = dlg.FileName;
            vm.Filter = dlg.Filter;
            vm.FilterIndex = dlg.FilterIndex;
            vm.InitialDirectory = dlg.InitialDirectory;
            vm.OverwritePrompt = dlg.OverwritePrompt;
            vm.RestoreDirectory = dlg.RestoreDirectory;
            vm.Title = dlg.Title;
            vm.ValidateNames = dlg.ValidateNames;

            vm.RaiseDialogClosing(new CloseDialogArgs { Result = vm.Result ? DialogResult.Ok : DialogResult.Cancel });
        }
    }
}