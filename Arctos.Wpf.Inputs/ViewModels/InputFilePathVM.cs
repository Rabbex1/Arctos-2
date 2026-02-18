using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arctos.Wpf.Dialogs;

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
            : base(description, value, onChange)
        {
            FileName = Path.GetFileName(value);
            OnButtonClicked = OpenDialog;
        }

        #endregion Constructor

        protected void OpenDialog(DataInputWrapperVM<string> wrapper)
        {
            var directory = string.IsNullOrEmpty(InitialDirectory)
                ? string.IsNullOrEmpty(Path.GetDirectoryName(Value))
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : Path.GetDirectoryName(Value)
                : InitialDirectory;

            var filter = BuildFilter(FileGroup, FileExtensions);
            string? filePath = null;

            if (SaveAs)
            {
                var dialog = new SaveFileDialogVM
                {
                    FileName = AllowSpaces ? FileName : FileName?.Replace(' ', '_'),
                    InitialDirectory = directory,
                    Filter = filter,
                    Title = DialogTitle,
                };

                new SaveFileDialogPresenter().Show(dialog);
                if (dialog.Result)
                    filePath = dialog.FileName;

                if (!AllowSpaces && !string.IsNullOrWhiteSpace(filePath))
                    filePath = filePath.Replace(' ', '_');
            }
            else
            {
                var dialog = new OpenFileDialogVM
                {
                    FileName = FileName,
                    InitialDirectory = directory,
                    Filter = filter,
                    Title = DialogTitle,
                };

                new OpenFileDialogPresenter().Show(dialog);
                if (dialog.Result)
                    filePath = dialog.FileName;
            }

            if (string.IsNullOrWhiteSpace(filePath))
                return;

            Value = filePath;
            FileName = Path.GetFileName(filePath);
        }

        private static string BuildFilter(string fileGroup, IEnumerable<string> extensions)
        {
            var patterns = extensions
                .Select(NormalizePattern)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (patterns.Length == 0)
                return "All Files (*.*)|*.*";

            var groupName = string.IsNullOrWhiteSpace(fileGroup) ? "Files" : fileGroup;
            return string.Join("|", patterns.Select(p => $"{groupName} ({p})|{p}"));
        }

        private static string NormalizePattern(string? extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return string.Empty;

            var value = extension.Trim();

            if (value == "*")
                return "*.*";

            if (value.StartsWith('.'))
                return $"*{value}";

            if (value.Contains('*') || value.Contains('?'))
                return value;

            return $"*.{value}";
        }
    }
}
