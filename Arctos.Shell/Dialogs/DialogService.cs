using Arctos.Wpf.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Arctos.Shell.Dialogs
{
    public class DialogService : IDialogService
    {
        public ObservableCollection<IDialogBaseVM> Dialogs { get; } = [];

        public bool Show(IDialogBaseVM dialog)
        {
            if (dialog is ICommonDialogVM common)
                return common.Show(Dialogs);
            Dialogs.Add(dialog);
            return true;
        }

        public void ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            MessageBox.Show(text, caption, buttons, icon);
        }
    }
}
