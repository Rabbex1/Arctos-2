using System.IO;

namespace Arctos.Shell.Documents
{
    public abstract class FileDocumentVM : PersistentDocumentVM, IFileDocumentVM
    {

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region FilePath
        private string _filePath = string.Empty;
        public string FilePath
        {
            get => _filePath;
            set
            {
                SetProperty(ref _filePath, value); 
                FilePathChanged(value);
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public bool Load(string filePath)
        {
            FilePath = filePath;
            var loaded = DoLoad();
            IsChanged = false;
            return loaded;
        }

        public async Task<bool> LoadAsync(string filePath)
        {
            FilePath = filePath;
            var loaded = await DoLoadAsync();
            IsChanged = false;
            return loaded;
        }


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected internal override DocumentMetadata CreateContentMetadata()
        {
            var metadata = base.CreateContentMetadata();
            metadata.DocumentType = "File";
            metadata.Source = FilePath;
            metadata.Title = Path.GetFileName(FilePath);

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                var extension = Path.GetExtension(FilePath).TrimStart('.');
                if (!string.IsNullOrWhiteSpace(extension))
                    metadata.Properties["extension"] = extension;
            }

            return metadata;
        }

        protected virtual void FilePathChanged(string value)
        {
            DisplayName = Path.GetFileName(value);
            Metadata = CreateContentMetadata();
            SetContentComponent("FilePath", _filePath);
        }

        #endregion Protected Methods


        #endregion Methods

    }
}
