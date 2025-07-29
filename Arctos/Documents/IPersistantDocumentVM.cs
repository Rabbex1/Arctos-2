// ReSharper disable once CheckNamespace
namespace Arctos.Shell.Documents
{
    public interface IPersistentDocumentVM : IDocumentVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        Action<IDocumentVM>? SaveCommandOverride { get; set; }

        bool CloseOnExitIfUnsaved { get; }

        public bool IsChanged { get; set; }

        #endregion Properties


        bool Save();
        Task SaveAsync();
    }
}
