using System.Reflection;
using Arctos.Shell.Documents;
using Arctos.Wpf.Dialogs;

namespace Arctos.Shell.Dialogs
{
  public enum CloseDocument
  {
    Yes,
    No,
    Query
  }

  public sealed class SaveChangesDialogVM : DialogVM
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public List<PersistentDocumentVM?>? Documents { get; set; }
    public CloseDocument CloseIfSaved { get; set; }
    public CloseDocument CloseIfNotSaved { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public SaveChangesDialogVM()
    {
            DisplayName = (Assembly.GetEntryAssembly() ?? throw new InvalidOperationException())
                                   .GetCustomAttributes(typeof(AssemblyProductAttribute))
                                   .OfType<AssemblyProductAttribute>()
                                   .FirstOrDefault()?.Product + " - Save Changes";
            IsModal = true;
    }

    #endregion Constructors

  }
}
