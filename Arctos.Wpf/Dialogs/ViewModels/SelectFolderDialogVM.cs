
namespace Arctos.Wpf.Dialogs
{
  public class SelectFolderDialogVM : DialogBaseVM, ICommonDialogVM
  {
      private bool AddToRecent { get; set; }
      public string Title { get; set; } = "Select Folder";
      public string FolderName { get; set; } = string.Empty;
      public IEnumerable<string>? FolderNames { get; set; }
      public string InitialDirectory { get; set; } = string.Empty;
      public string DefaultDirectory { get; set; } = @"C:\";
      public bool Multiselect { get; set; } = false;
      public bool ShowPlacesList { get; set; } = true;
      public bool Result { get; set; }


        public bool Show(IList<IDialogBaseVM> collection)
    {
      collection.Add(this);
      if (!Result)
        WasCanceled = true;
      return Result;
    }
  }
}
