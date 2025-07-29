using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Arctos.Wpf.Dialogs;

namespace Arctos.Shell.Dialogs
{
    public interface IDialogService
    {
        public ObservableCollection<IDialogBaseVM> Dialogs { get; }
        public bool Show(IDialogBaseVM dialog);
        public void ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }
}
