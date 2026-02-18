using System.Windows.Controls;
using Arctos.Shell.Options.Selectors;

namespace Arctos.Shell.Options
{
    public class OptionsFormsManager
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly List<OptionsFormVM> _forms = new List<OptionsFormVM>();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public IOptionsService OptionsDocumentService { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public OptionsFormsManager(IOptionsService optionsDocumentService)
        {
            OptionsDocumentService = optionsDocumentService;
        }

        #endregion Constructor

        /// <summary>
        /// Adds a new options form.
        /// </summary>
        /// <param name="displayName">The display name of the new page</param>
        /// <param name="optionsPage"><see cref="OptionsPage"/> containg data for the new page</param>
        /// <param name="vmType">The view model for type the new page, defaults to <see cref="OptionsFormVM"/> if null</param>
        /// <param name="viewType">The view type for the new page</param>
        /// <param name="parentId">The Id of the paren page</param>
        /// <param name="formId">The Id of the new page</param>
        /// <returns>The <see cref="OptionsFormVM"/> for the new form</returns>
        public OptionsFormVM AddForm(string displayName, OptionsPage optionsPage, Type? vmType, Type? viewType, string parentId, string formId)
        {
            Argument.MustBeSubClassOfOrNull("vmType", vmType, typeof(OptionsFormVM));
            Argument.MustBeSubClassOf("vmType", viewType, typeof(UserControl));

            //if the view model type is not specified use the default one
            vmType ??= typeof(OptionsFormVM);

            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));

            //If the form has already been added simply return it's view model
            var existingFormVM = _forms.FirstOrDefault(f => f.ParentId == parentId && f.FormId == formId);
            if (existingFormVM != null)
                return existingFormVM;

            //Create a view model for the form
            var newFormVM = (OptionsFormVM?)Activator.CreateInstance(vmType) ?? throw new ArctosOptionsException($"Cannot create FormVM {vmType}");
            newFormVM.ParentId = parentId;
            newFormVM.FormId = formId;
            newFormVM.DisplayName = displayName;
            newFormVM.Data = optionsPage;
            _forms.Add(newFormVM);

            //Create a view for the form and add it to the template selector
            var view = (UserControl?)Activator.CreateInstance(viewType) ?? throw new ArctosOptionsException($"Cannot create View {viewType}");
            OptionsFormDataTemplateSelector.AddOptionsFormView(formId, view);

            return newFormVM;
        }

        /// <summary>
        /// Removes a page from the options dialog.
        /// </summary>
        /// <param name="parentId">The Id of the parent page.</param>
        /// <param name="formId">The Id of the form.</param>
        public void RemoveDialogPage(string parentId, string formId)
        {
            var oldForm = _forms.FirstOrDefault(f => f.ParentId == parentId && f.FormId == formId);
            if (oldForm == null)
                return;

            //Remove the options page from the document if this is the only dialog page using it
            if (oldForm.Data != null)
            {
                if (_forms.Count(f => f.Data == oldForm.Data) == 1)
                    OptionsDocumentService.RemovePage(oldForm.Data.Key);
            }
            OptionsFormDataTemplateSelector.RemoveOptionsFormView(formId);
            _forms.Remove(oldForm);
        }



        //---------------------------------------------------------------------
        #region Private Methods

        /// <summary>
        /// Creates a new dialog page view model 
        /// </summary>
        /// <param name="displayName">The display name</param>
        /// <param name="vmType"><see cref="OptionsFormVM"/> or sub class.</param>
        /// <param name="parentId">The Id of the parent page.</param>
        /// <param name="pageId">The Id of the page.</param>
        /// <returns>The newly created <see cref="OptionsFormVM"/></returns>
        private static OptionsFormVM CreateFormVM(string displayName, Type vmType, string parentId, string pageId)
        {
            var newFormVM = (OptionsFormVM?)Activator.CreateInstance(vmType) ?? throw new ArctosOptionsException($"Cannot create OptionsFormVM {vmType}");
            newFormVM.FormId = pageId;
            newFormVM.DisplayName = displayName;
            return newFormVM;
        }

        #endregion Private Methods

    }
}
