using Arctos.Shell.Messages;
using Arctos.Wpf.AvalonDock;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Shell.Documents
{
    public abstract partial class DocumentVM()
        : DocumentPaneVM("New Document"), IDocumentVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void Closing()
        {
        }

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void Close()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.Close, [this]));
        }

        [RelayCommand]
        public void CloseAll()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.CloseAll));
        }

        [RelayCommand]
        public void CloseAllButThis()
        {
            WeakReferenceMessenger.Default.Send(new DocumentMessage(this, DocumentAction.CloseAllButThis, [this]));
        }

        #endregion Commands

    }
}
