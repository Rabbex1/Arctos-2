using System.Collections.ObjectModel;
using System.Windows.Controls;
using Arctos.Shell.Options;
using Arctos;
using Arctos.Shell.Options.Selectors;
using Arctos.Wpf.Dialogs;
using CommunityToolkit.Mvvm.Input;

namespace Arctos.Shell.Options
{
    public partial class OptionsDialogVM : DialogVM, IOptionsDialogVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly IOptionsService _optionsService;

        private readonly Dictionary<Guid, List<TreeParentContainerVM<OptionsFormVM>>> _dialogPages = new();

        private readonly List<OptionsFormVM> _pendingRemove = new();
        private readonly List<OptionsFormVM> _pendingAdd = new();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region FontSize
        private double _fontSize = 12;
        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }
        #endregion

        public ObservableCollection<ITreeItemVM> Tree { get; } = new ObservableCollection<ITreeItemVM>();

        #region List
        public List<TreeParentVM> List
        {
            get
            {
                var list = new List<TreeParentVM>();
                foreach (var subList in _dialogPages.Values)
                    list.AddRange(subList);
                return list;
            }
        }
        #endregion

        #region SelectedPage
        private object? _selectedPage;
        public object? SelectedPage
        {
            get => _selectedPage;
            set => SetProperty(ref _selectedPage, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors		

        public OptionsDialogVM(IOptionsService optionsService)
        {
            DisplayName = "XPlunder 5 -  User Options";
            _optionsService = optionsService;
            IsModal = true;
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        /// <summary>
        /// Adds a new page to the options dialog.
        /// </summary>
        /// <param name="displayName">The display name of the new page</param>
        /// <param name="optionsPage"><see cref="OptionsPage"/> containg data for the new page</param>
        /// <param name="vmType">The view model for type the new page, defaults to <see cref="OptionsFormVM"/> if null</param>
        /// <param name="viewType">The view type for the new page</param>
        /// <param name="parentId">The Id of the paren page</param>
        /// <param name="pageId">The Id of the new page</param>
        /// <param name="ownerId">The <see cref="Guid"/> of the new page owner.</param>
        /// <param name="expandParent">Indicates whether the parent should be expanded to show the new page in the options page tree</param>
        /// <returns></returns>
        public OptionsFormVM? AddDialogPage(string displayName, OptionsPage? optionsPage, Type? vmType, Type viewType,
          string? parentId, string pageId, Guid ownerId, bool expandParent = false)
        {
            Argument.MustBeSubClassOfOrNull("vmType", vmType, typeof(OptionsFormVM));
            Argument.MustBeSubClassOf("vmType", viewType, typeof(UserControl));

            vmType ??= typeof(OptionsFormVM);

            var view = (UserControl?) Activator.CreateInstance(viewType) 
                       ?? throw new ArctosOptionsException($"Cannot create view {viewType}");
            OptionsFormDataTemplateSelector.AddOptionsFormView(pageId, view);

            var pageNode = GetTreeNode(pageId, ownerId);
            var parentNode = GetTreeNode(parentId, ownerId);
            if (pageNode != null)
            {
                if (parentNode == null)
                    throw new ArgumentException($"A page with the Id '{pageId}' exists in another category.");
                if (pageNode.Content?.DisplayName != displayName)
                    throw new ArgumentException($"A page with the Id '{pageId}' exists already exists but has a different display name.");
                return pageNode.Content as OptionsFormVM;
            }

            var dialogPageVM = CreateDialogPageVM(displayName, vmType, parentId, pageId, ownerId);
            pageNode = new TreeParentContainerVM<OptionsFormVM>(parentId == null ? null : parentNode){Content = dialogPageVM};
            if (!_dialogPages.ContainsKey(ownerId))
                _dialogPages.Add(ownerId, new List<TreeParentContainerVM<OptionsFormVM>>());
            _dialogPages[ownerId].Add(pageNode);
            dialogPageVM.Data = optionsPage;
            DialogOpening += dialogPageVM.OnDialogOpening;
            DialogClosing += dialogPageVM.OnDialogClosing;

            if (parentId == null)
                Tree.Add(pageNode);
            else
            {
                if (parentNode == null)
                    throw new ArgumentException($"Cannot add page with Id '{pageId}', the catagory  '{parentId}' does not exist.");
                parentNode.AddChild(pageNode);
                if (expandParent)
                    parentNode.IsExpanded = true;
            }
            return dialogPageVM;
        }

        /// <summary>
        /// Adds a new page to the options dialog.
        /// The new page will be made permanent if 'Ok' is pressed or removed if canceled.
        /// </summary>
        /// <param name="displayName">The display name of the new page</param>
        /// <param name="optionsPage"><see cref="OptionsPage"/> containg data for the new page</param>
        /// <param name="vmType">The view model for type the new page, defaults to <see cref="OptionsFormVM"/> if null</param>
        /// <param name="viewType">The view type for the new page</param>
        /// <param name="parentId">The Id of the paren page</param>
        /// <param name="pageId">The Id of the new page</param>
        /// <param name="ownerId">The <see cref="Guid"/> of the new page owner.</param>
        /// <param name="expandParent">Idicates whether the parent should be expanded to show the new page in the options page tree</param>
        /// <returns></returns>
        public OptionsFormVM? AddDialogPagePending(string displayName, OptionsPage optionsPage, Type vmType, Type viewType,
          string parentId, string pageId, Guid ownerId, bool expandParent = false)
        {
            var newDialogPage = AddDialogPage(displayName, optionsPage, vmType, viewType, parentId, pageId, ownerId, expandParent);

            if(newDialogPage != null)
            {
                var pageNode = GetTreeNode(pageId, ownerId);
                _pendingAdd.Add(newDialogPage);
                if(pageNode  != null)
                    pageNode.Icon = "ArctosShell:PendingAdd";
            }
            return newDialogPage;
        }

        /// <summary>
        /// Removes a page from the options dialog.
        /// </summary>
        /// <param name="parentId">The Id of the parent page.</param>
        /// <param name="pageId">The Id of the page.</param>
        /// <param name="ownerId">The <see cref="Guid"/> of the page owner.</param>
        public void RemoveDialogPage(string? parentId, string? pageId, Guid ownerId)
        {
            var pageNode = GetTreeNode(pageId, ownerId);
            var parentNode = GetTreeNode(parentId, ownerId);
            if (parentNode == null || pageNode == null)
                return;
            var dialogPageVM = (OptionsFormVM?)pageNode.Content;
            if (dialogPageVM != null)
            {
                DialogOpening += dialogPageVM.OnDialogOpening;
                DialogClosing += dialogPageVM.OnDialogClosing;
                //Remove the options page from the document if this is the only dialog page using it
                if (dialogPageVM.Data != null)
                {
                    var pagesUsingData = new List<OptionsFormVM?>();
                    foreach (var list in _dialogPages.Values)
                    {
                        pagesUsingData.AddRange(list.Where(ph => ((OptionsFormVM?) ph.Content)?.Data == dialogPageVM.Data)
                            .Select(dp => dp.Content as OptionsFormVM).ToList());
                    }

                    if (pagesUsingData.Count == 1 && pagesUsingData[0] == dialogPageVM)
                        _optionsService.RemovePage(dialogPageVM.Data.Key);
                }
            }

            OptionsFormDataTemplateSelector.RemoveOptionsFormView(pageId);
            parentNode.RemoveChild(pageNode);
            _dialogPages[ownerId].Remove(pageNode);
        }

        /// <summary>
        /// Sets a page to be removed from the options dialog when 'Ok' is pressed
        /// </summary>
        /// <param name="parentId">The Id of the parent page.</param>
        /// <param name="pageId">The Id of the page.</param>
        /// <param name="ownerId">The <see cref="Guid"/> of the page owner.</param>
        public void RemoveDialogPagePending(string parentId, string pageId, Guid ownerId)
        {
            var pageNode = GetTreeNode(pageId, ownerId);
            if (pageNode == null)
                return;
            pageNode.IsExpanded = false;
            pageNode.IsEnabled = false;
            pageNode.Icon = "ArctosShell:PendingRemove";
            if(pageNode.Content != null)
                _pendingRemove.Add((OptionsFormVM)pageNode.Content);
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region DialogVM Override

        /// <summary>
        /// Called when the 'Ok' button is pressed.
        /// </summary>
        protected override void OnOk()
        {
            OkPending();
            base.OnOk();
            _optionsService.Save();
        }

        /// <summary>
        /// Called when the 'Cancel' button is pressed.
        /// </summary>
        protected override void OnCancel()
        {
            CancelPending();
            base.OnCancel();
            //_optionsService.Save();
        }

        /// <summary>
        /// Called when the title bar 'X' button is pressed.
        /// </summary>
        protected override void OnClose()
        {
            CancelPending();
            base.OnClose();
            //_optionsService.Save();
        }

        #endregion DialogVM Override

        //---------------------------------------------------------------------
        #region Private Methods

        /// <summary>
        /// Gets the tree node that has the specified <see cref="OptionsFormVM"/> as its content/>
        /// </summary>
        /// <param name="pageId">the page Id of the <see cref="OptionsFormVM"/></param>
        /// <param name="ownerId">The <see cref="Guid"/> of the page owner.</param>
        /// <returns>The <see cref="TreeParentVM"/> that contains the <see cref="OptionsFormVM"/> or null if not found</returns>
        public TreeParentContainerVM<OptionsFormVM>? GetTreeNode(string? pageId, Guid ownerId)
        {
            return !_dialogPages.TryGetValue(ownerId, out var value) 
                ? null
                : value.FirstOrDefault(c => ((OptionsFormVM?)c.Content)?.FormId == pageId);
        }

        /// <summary>
        /// Creates a new dialog page view model 
        /// </summary>
        /// <param name="displayName">The display name</param>
        /// <param name="vmType"><see cref="OptionsFormVM"/> or sub class.</param>
        /// <param name="parentId">The Id of the parent page.</param>
        /// <param name="pageId">The Id of the page.</param>
        /// <param name="ownerId">The <see cref="Guid"/> of the page owner.</param>
        /// <returns></returns>
        private static OptionsFormVM CreateDialogPageVM(string displayName, Type vmType, string? parentId, string pageId, Guid ownerId)
        {
            var newPageVM = (OptionsFormVM?)Activator.CreateInstance(vmType) 
                            ?? throw new ArctosOptionsException($"Cannot OptionsFormVM {vmType}");
            newPageVM.ParentId = parentId;
            newPageVM.FormId = pageId;
            newPageVM.OwnerId = ownerId;
            newPageVM.DisplayName = displayName;
            return newPageVM;
        }

        /// <summary>
        /// Act on any pending changes to the Options Dialog.
        /// </summary>
        private void OkPending()
        {
            //if a page was added then removed make sure the removal takes priority
            _pendingAdd.RemoveAll(p => _pendingRemove.Contains(p));

            //Execute any pending removals
            foreach (var pageVM in _pendingRemove)
                RemoveDialogPage(pageVM.ParentId, pageVM.FormId, pageVM.OwnerId);
            _pendingRemove.Clear();

            //Since the pending adds have already been created the only thing to do is get rid of the pending icon.
            foreach (var pageVM in _pendingAdd)
            {
                var node = GetTreeNode(pageVM.FormId, pageVM.OwnerId);
                if(node != null) 
                    node.Icon = null;
            }
            _pendingAdd.Clear();
        }

        /// <summary>
        /// Cancel any pending changes to the Options Dialog.
        /// </summary>
        private void CancelPending()
        {
            //Re-enable any pending removals and get rid of the icon
            foreach (var pageVM in _pendingRemove)
            {
                var pageNode = GetTreeNode(pageVM.FormId, pageVM.OwnerId);
                if (pageNode == null)
                    continue;
                pageNode.IsEnabled = true;
                pageNode.Icon = null;
            }
            _pendingRemove.Clear();

            //Remove any pending adds since they've alredy been created.
            //First reverse the list because the parent pages would have been added first but must be removed last.
            _pendingAdd.Reverse();
            foreach (var pageVM in _pendingAdd)
                RemoveDialogPage(pageVM.ParentId, pageVM.FormId, pageVM.OwnerId);
            _pendingAdd.Clear();
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Commands

        #region SelectedItemChangedCommand

        /// <summary>
        /// The command to execute when the options page tree selection is change.
        /// </summary>
        [RelayCommand]
        public void SelectedItemChanged(ITreeItemContainerVM<OptionsFormVM>? page)
        {
            if (page != null)
            {
                SelectedPage = page.Content;
            }
        }
        #endregion

        #endregion Commands
    }
}
