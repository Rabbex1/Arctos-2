using System.IO;
using Arctos.Wpf.Dialogs;

namespace Arctos.Wpf.Inputs
{
    public class InputFolderVM : DataInputWrapperVM<DirectoryInfo>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        #region FolderName
        private string _folderName = string.Empty;
        public string FolderName
        {
            get => _folderName;
            set => SetProperty(ref _folderName, value);
        }
        #endregion


        public string Title { get; set; } = "Select Folder";

        private Environment.SpecialFolder RootFolder { get; set; } = Environment.SpecialFolder.Personal;

        #endregion Properties


        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public InputFolderVM(string description, DirectoryInfo value, Action<DirectoryInfo?> onChange)
            : base(description, value, onChange)
        {
            OnButtonClicked = OpenDialog;
            Icon = "ArctosWpf:MeatballMenu";
            FolderName = value.Name;
        }

        #endregion Constructor


        protected void OpenDialog(DataInputWrapperVM<DirectoryInfo> wrapper)
        {
            var initialDirectory = string.IsNullOrEmpty(Value?.FullName)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : Value.FullName;

            var dialog = new SelectFolderDialogVM
            {
                Title = Title,
                InitialDirectory = initialDirectory,
                FolderName = initialDirectory
            };

            new SelectFolderDialogPresenter().Show(dialog);

            if (!dialog.Result || string.IsNullOrWhiteSpace(dialog.FolderName))
                return;

            Value = new DirectoryInfo(dialog.FolderName);
            FolderName = Value.Name;
        }
    }
}
