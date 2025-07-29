using System.IO;

namespace Arctos.Wpf.Inputs
{
    public class InputFilePathVM : DataInputWrapperVM<string>
    {
        public string DialogTitle { get; set; } = "Select File";

        #region FileName
        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        #endregion

        public string FileGroup { get; set; } = "";

        public string[] FileExtensions { get; set; } = ["*"];

        public bool SaveAs { get; set; }
        public string InitialDirectory { get; set; } = string.Empty;

        public bool AllowSpaces { get; set; } = true;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public InputFilePathVM(string description, string value, Action<string?> onChange)
            :base(description, value, onChange)
        {
            FileName = Path.GetFileName(value);
            OnButtonClicked = OpenDialog;
        }

        #endregion Constructor

        protected void OpenDialog(DataInputWrapperVM<string> wrapper)
        {
            throw new NotImplementedException();
        //    var directory = string.IsNullOrEmpty(InitialDirectory) 
        //        ? string.IsNullOrEmpty(Path.GetDirectoryName(Value)) 
        //            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        //            : Path.GetDirectoryName(Value)
        //        : InitialDirectory;

        //    string filePath;

        //    if (SaveAs)
        //    {
        //        filePath = FileHelper.SaveFileAsDialog
        //        (
        //            DialogTitle,
        //            FileName,
        //            directory,
        //            FileGroup,
        //            FileExtensions,
        //            AllowSpaces
        //        );
        //    }
        //    else
        //    {
        //        filePath = FileHelper.OpenFileDialog
        //        (
        //            DialogTitle,
        //            FileName,
        //            directory,
        //            FileGroup,
        //            FileExtensions
        //        );
        //    }

        //    if (filePath == null) 
        //        return;
        //    Value = filePath;
        //    FileName = Path.GetFileName(filePath);
        //
        }
    }
}
