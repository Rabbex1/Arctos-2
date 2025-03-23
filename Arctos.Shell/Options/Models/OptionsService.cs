using System.ComponentModel;
using System.Reflection;

namespace Arctos.Shell.Options
{
    public class OptionsService : IOptionsService
    {
        private readonly UserOptionsDoc _document;
        private readonly Dictionary<OptionsPageKey, OptionsPage> _pages = [];
        private readonly List<OptionsPage> _changedPages = [];

        private static Assembly? _searchFirstAssembly;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public int PageCount => _pages.Count;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionsService()
        {
            _document = new UserOptionsDoc("UserOptions.config");
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void ClearDocument()
        {
            _document.ClearDocument();
            _document.Save();
            _pages.Clear();
            _changedPages.Clear();
        }

        public OptionsPage RegisterPage<TModel>(string categoryId, string pageId, Guid ownerId, TModel? initialData = null)
          where TModel : class, new()
        {
            return RegisterPage(new OptionsPageKey(categoryId, pageId, ownerId), initialData);
        }

        public OptionsPage RegisterPage<TModel>(OptionsPageKey key, TModel? initialData = null)
          where TModel : class, new()
        {
            if (_pages.TryGetValue(key, out var value))
                return value;

            var savedPage = _document.GetPage(key);
            if (savedPage != null)
            {
                if (savedPage.ContentType != typeof(TModel))
                    throw new InvalidOperationException($"A sved version of options page '{key}' with a different model type.");
                _pages.Add(savedPage.Key, savedPage);
                savedPage.PageChanged += OnPageChanged;
                RaisePageAdded(new OptionsDocumentEventArgs(savedPage));
                return savedPage;
            }

            //var pageType = typeof(OptionsPage<>);
            //var makeType = pageType.MakeGenericType(typeof(TModel));
            var newPage = new OptionsPage<TModel>(key);
            if (initialData != null)
                newPage.MapFrom(initialData);
            _document.AddOrReplacePage(newPage);
            _document.Save();
            _pages.Add(newPage.Key, newPage);
            newPage.PageChanged += OnPageChanged;
            RaisePageAdded(new OptionsDocumentEventArgs(newPage));
            return newPage;
        }

        public OptionsPage? GetPage(OptionsPageKey key)
        {
            return !_pages.TryGetValue(key, out var value) ? null : value;
        }

        public OptionsPage? GetPage(string categoryId, string pageId, Guid ownerId)
        {
            return GetPage(new OptionsPageKey(categoryId, pageId, ownerId));
        }

        public object? GetPageContent(string categoryId, string pageId)
        {
            var key = _pages.Keys.FirstOrDefault(k => k.CategoryId == categoryId && k.PageId == pageId);
            return key == null ? null : GetPage(key)?.GetModel();
        }

        public object? GetOptionValue(string categoryId, string pageId, string optionName)
        {
            var key = _pages.Keys.FirstOrDefault(k => k.CategoryId == categoryId && k.PageId == pageId);
            return key == null ? null : GetPage(key)?[optionName].Value;
        }

        public OptionPageProperty? GetOptionProperty(string categoryId, string pageId, string optionName)
        {
            var key = _pages.Keys.FirstOrDefault(k => k.CategoryId == categoryId && k.PageId == pageId);
            return key == null ? null : GetPage(key)?.Properties[optionName];
        }

        public IEnumerable<OptionsPage> GetPages(string categoryId, Guid ownerId)
        {
            var keys = _pages.Keys.Where(k => k.CategoryId == categoryId);//&& k.OwnerId.Equals(ownerId));
            return [.. keys.Select(k => _pages[k])];
        }

        public void RemovePage(string categoryId, string nameId, Guid ownerId)
        {
            RemovePage(new OptionsPageKey(categoryId, nameId, ownerId));
        }

        public void RemovePage(OptionsPageKey key)
        {
            _document.RemovePage(key);
            if (!_pages.TryGetValue(key, out OptionsPage? value))
                return;
            _pages.Remove(key);
            value.PageChanged -= OnPageChanged;
            RaisePageRemoved(new OptionsDocumentEventArgs(value));
        }

        public void UpdatePage(OptionsPage source, bool save = true)
        {
            if (!_pages.TryGetValue(source.Key, out OptionsPage? value))
                throw new InvalidOperationException($"Cannot update options page with key '{source.Key}', it does not exist.");

            var dest = value;
            dest.Update(source);
            _document.AddOrReplacePage(dest);
            if (save)
                _document.Save();
        }

        public void Save()
        {
            foreach (var page in _changedPages)
                if (_pages.ContainsKey(page.Key))
                {
                    _document.AddOrReplacePage(page);
                    RaisePageChanged(new OptionsDocumentEventArgs(page));
                }

            _document.Save();
        }

        public IEnumerable<OptionsPage?>? LoadPages(object filter, Assembly? searchFirst = null)
        {
            //Set which assembly to search first when trying to find the page content type
            _searchFirstAssembly = searchFirst;
            try
            {
                List<OptionsPage?>? loadedPages = null;

                switch (filter)
                {
                    case string s:
                        loadedPages = _document.GetPagesByAttribute("categoryId", s)?.ToList();
                        break;
                    case Guid guid:
                        loadedPages = _document.GetPagesByAttribute("ownerId", "{" + guid + "}")?.ToList();
                        break;
                    case Type type:
                        loadedPages = _document.GetPagesByAttribute("contentType", type.FullName)?.ToList();
                        break;
                }

                if (loadedPages == null)
                    return null;
                foreach (var page in loadedPages)
                {
                    if (page == null)
                        continue;
                    if (_pages.TryGetValue(page.Key, out var value))
                        value.Update(page);
                    else
                    {
                        _pages.Add(page.Key, page);
                        page.PageChanged += OnPageChanged;
                        RaisePageAdded(new OptionsDocumentEventArgs(page));
                    }
                }
                return loadedPages;
            }
            finally
            {
                _searchFirstAssembly = null;
            }
        }

        public static Type? GetPageContentType(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return null;

            Type? type;

            //Try preferred assembly first
            if (_searchFirstAssembly != null)
            {
                type = _searchFirstAssembly.GetType(fullName);
                if (type != null)
                    return type;
            }

            ////Next try executing assembly
            //type = Assembly.GetExecutingAssembly().GetType(fullName);
            //if (type != null)
            //	return type;

            //Last try all assemblies starting with the last loaded.

            type = AppDomain.CurrentDomain.GetAssemblies()
                            .Reverse()
                            .SelectMany(x => x.GetTypes())
                            .FirstOrDefault(t => t.FullName == fullName);
             
            if (type == null)
                throw new InvalidOperationException($"Cannot find type '{fullName}'.");
            return type;
        }

        #endregion Public Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        //---------------------------------------------------------------------
        #region PageAdded
        public event EventHandler<OptionsDocumentEventArgs>? PageAdded;

        private void RaisePageAdded(OptionsDocumentEventArgs args)
        {
            PageAdded?.Invoke(this, args);
        }
        #endregion

        //---------------------------------------------------------------------
        #region PageRemoved
        public event EventHandler<OptionsDocumentEventArgs>? PageRemoved;

        private void RaisePageRemoved(OptionsDocumentEventArgs args)
        {
            PageRemoved?.Invoke(this, args);
        }
        #endregion

        //---------------------------------------------------------------------
        #region PageChanged
        public event EventHandler<OptionsDocumentEventArgs>? PageChanged;

        private void RaisePageChanged(OptionsDocumentEventArgs args)
        {
            PageChanged?.Invoke(this, args);
        }
        #endregion


        #endregion Events

        public void OnPageChanged(object? sender, EventArgs e)
        {
            if (sender is not OptionsPage page)
                return;

            if (!_changedPages.Contains(page))
                _changedPages.Add(page);
        }

    }
}
