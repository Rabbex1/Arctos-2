
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Arctos.Wpf.Dialogs
{
    public class SelectFolderDialogPresenter : IDialogPresenter<SelectFolderDialogVM>
    {
        public void Show(SelectFolderDialogVM vm)
        {
            var dlg = new OpenFolderDialog
            {
                Title = vm.Title,
                FolderName = vm.FolderName,
                InitialDirectory = vm.InitialDirectory,
                DefaultDirectory = vm.DefaultDirectory,
                Multiselect = vm.Multiselect,
            };

            vm.RaiseDialogOpening();
            var result = dlg.ShowDialog();
            vm.Result = result ?? false;

            vm.Title = dlg.Title;
            vm.FolderName = dlg.FolderName;
            vm.FolderNames = dlg.FolderNames;
            vm.InitialDirectory = dlg.InitialDirectory;
            vm.DefaultDirectory = dlg.DefaultDirectory;
            vm.Multiselect = dlg.Multiselect;

            vm.RaiseDialogClosing(new CloseDialogArgs { Result = vm.Result ? DialogResult.Ok : DialogResult.Cancel });
        }
    }
}