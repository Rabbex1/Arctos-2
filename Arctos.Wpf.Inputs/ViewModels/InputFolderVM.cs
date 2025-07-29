using System.IO;

namespace Arctos.Wpf.Inputs
{
    internal class InputFolderVM : DataInputWrapperVM<DirectoryInfo>
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

        public InputFolderVM(string description, DirectoryInfo value, Action<DirectoryInfo> onChange)
            : base(description, value, onChange)
        {
            OnButtonClicked = OpenDialog;
            Icon = "UrsaWpf:MeatballMenu";
            FolderName = value.Name;
        }

        #endregion Constructor


        protected void OpenDialog(DataInputWrapperVM<DirectoryInfo> wrapper)
        {
            throw new NotImplementedException();
            //var initialDirectory = string.IsNullOrEmpty(Value.FullName)
            //    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            //    : Value.FullName;

            //var folder = FileHelper.SelectFolderDialog(Title, initialDirectory, RootFolder);

            //if (folder == null)
            //    return;
            //Value = new DirectoryInfo(folder);
            //FolderName = Value.Name;
        }
    }
}
