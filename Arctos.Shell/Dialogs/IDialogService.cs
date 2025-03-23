using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Arctos.Shell.Dialogs
{
    public interface IDialogService
    {
        public void ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }
}
