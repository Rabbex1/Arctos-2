using System.ComponentModel;
using System.Reflection;

namespace Arctos.Shell.Options
{
    public interface IOptionsService
    {
        #region Properties
        public int PageCount { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void ClearDocument();
        public OptionsPage RegisterPage<TModel>(string categoryId, string pageId, Guid ownerId, TModel? initialData = null) where TModel : class, new();
        public OptionsPage RegisterPage<TModel>(OptionsPageKey key, TModel? initialData) where TModel : class, new();
        public OptionsPage? GetPage(OptionsPageKey key);
        public OptionsPage? GetPage(string categoryId, string pageId, Guid ownerId);
        public object? GetPageContent(string categoryId, string pageId);
        public object? GetOptionValue(string categoryId, string pageId, string optionName);
        public OptionPageProperty? GetOptionProperty(string categoryId, string pageId, string optionName);
        public IEnumerable<OptionsPage> GetPages(string categoryId, Guid ownerId);
        public void RemovePage(string categoryId, string nameId, Guid ownerId);
        public void RemovePage(OptionsPageKey key);
        public void UpdatePage(OptionsPage source, bool save);
        public void Save();
        public IEnumerable<OptionsPage?>? LoadPages(object filter, Assembly? searchFirst);

        #endregion Methods

    }
}
