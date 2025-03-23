using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Arctos.Shell.Dialogs
{
    public class DialogService : IDialogService
    {
        public void ShowMessageBox(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            MessageBox.Show(text, caption, buttons, icon);
        }
    }
}
