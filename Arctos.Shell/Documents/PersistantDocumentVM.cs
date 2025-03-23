using System.Windows.Input;
using Arctos.Wpf.Utilities;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace Arctos.Shell.Documents
{
    public abstract partial class PersistentDocumentVM : DocumentVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Action<DocumentVM>? SaveCommandOverride { get; set; } = null;

        public bool CloseOnExitIfUnsaved { get; protected set; } = false;

        #region IsChanged
        private bool _isChanged;
        public virtual bool IsChanged
        {
            get => _isChanged;
            set => SetProperty(ref _isChanged, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        protected PersistentDocumentVM(IDocumentService documentService)
            : base(documentService)
        {
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public bool Save()
        {
            return DoSave();
        }

        public async Task SaveAsync()
        {
            await DoSaveAsync();
        }

        public bool Load()
        {
            var loaded = DoLoad();
            IsChanged = false;
            return loaded;
        }

        public async Task<bool> LoadAsync()
        {
            var loaded = await DoLoadAsync();
            IsChanged = false;
            return loaded;
        }


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected virtual bool DoSave()
        {
            //Do nothing if not overloaded
            return false;
        }

        protected virtual Task DoSaveAsync()
        {
            //Do nothing if not overloaded
            return Task.FromResult<object?>(null);
        }

        protected virtual bool DoLoad()
        {
            return false;
        }

        protected virtual Task<bool> DoLoadAsync()
        {
            //Do nothing if not overloaded
            return Task.FromResult<bool>(false);
        }

        #endregion Protected Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        [RelayCommand]
        public void OnSave()
        {
            if (SaveCommandOverride != null)
                SaveCommandOverride.Invoke(this);
            else
            {
                Save();
                var _ = SaveAsync();
            }
        }

        #endregion


    }
}
