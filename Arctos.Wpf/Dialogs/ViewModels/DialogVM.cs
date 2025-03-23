using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

//https://www.codeproject.com/Articles/820324/Implementing-Dialog-Boxes-in-MVVM

namespace Arctos.Wpf.Dialogs
{
    public partial class DialogVM : DialogBaseVM, IDialogVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public bool IsModal { get; set; }

        #region Caption
        private string _caption = string.Empty;
        public string Caption
        {
            get => _caption;
            set => SetProperty(ref _caption, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void RequestClose()
        {
            OnClose();
        }

        public void CloseOk()
        {
            OnOk();
        }

        protected virtual void OnOk()
        {
            OnCLose?.Invoke(DialogResult.Ok);

            RaiseDialogClosing(new CloseDialogArgs { Result = DialogResult.Ok });
        }

        protected virtual void OnCancel()
        {
            WasCanceled = true;
            OnCLose?.Invoke(DialogResult.Cancel);
            RaiseDialogClosing(new CloseDialogArgs { Result = DialogResult.Cancel });
        }

        protected virtual void OnClose()
        {
            OnCLose?.Invoke(DialogResult.Close);
            RaiseDialogClosing(new CloseDialogArgs { Result = DialogResult.Close });
        }

        protected virtual void OnYes()
        {
            OnCLose?.Invoke(DialogResult.Yes);
            RaiseDialogClosing(new CloseDialogArgs { Result = DialogResult.Yes });
        }

        protected virtual void OnNo()
        {
            OnCLose?.Invoke(DialogResult.No);
            RaiseDialogClosing(new CloseDialogArgs { Result = DialogResult.No });
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events


        //public event EventHandler CancelDialogClosing;
        //public void RaiseCancelDialogClosing()
        //{
        //    CancelDialogClosing?.Invoke(this, EventArgs.Empty);
        //}

        #endregion Events

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Ok()
        {
            OnOk();
        }

        [RelayCommand]
        public void Cancel()
        {
            OnCancel();
        }

        [RelayCommand]
        public void Yes()
        {
            OnYes();
        }

        [RelayCommand]
        public void No()
        {
            OnNo();
        }

        [RelayCommand]
        public void Close()
        {
            OnClose();
        }

        #endregion Commands

    }
}
