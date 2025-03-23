namespace Arctos.Wpf.Dialogs
{
  public class OpenFileDialogVM : DialogBaseVM, ICommonDialogVM
  {
    public bool MultiSelect { get; set; }
    public bool ReadOnlyChecked { get; set; }
    public bool ShowReadOnly { get; set; }
    public string? FileName { get; set; }
    public string[]? FileNames { get; set; }
    public string? Filter { get; set; }
    public string? InitialDirectory { get; set; }
    public bool RestoreDirectory { get; set; }
    public string? SafeFileName { get; set; }
    public string[]? SafeFileNames { get; set; }
    public string? Title { get; set; }
    public bool ValidateNames { get; set; }
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
