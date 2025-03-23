using System.IO;
using Arctos.Messenger;

namespace Arctos.Shell.Documents
{
    public abstract class FileDocumentVM : PersistentDocumentVM
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
                DisplayName = Path.GetFileName(value);
                SetContentComponent("FilePath", _filePath);
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        protected FileDocumentVM(IDocumentService documentService)
            : base(documentService)
        {
        }

        #endregion Constructors

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

        #endregion Methods

    }
}