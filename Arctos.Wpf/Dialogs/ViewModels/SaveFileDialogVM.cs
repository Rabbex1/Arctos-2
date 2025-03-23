namespace Arctos.Wpf.Dialogs
{
  public class SaveFileDialogVM : DialogBaseVM, ICommonDialogVM
  {
    public bool AddExtension { get; set; } = true;
    public bool CheckFileExists { get; set; } = true;
    public bool CheckPathExists { get; set; } = true;
    public string? FileName { get; set; }
    public string? Filter { get; set; }
    public int FilterIndex { get; set; } = 1;
    public string? InitialDirectory { get; set; }
    public bool OverwritePrompt { get; set; } = true;
    public bool RestoreDirectory { get; set; } = false;
    public string? Title { get; set; }
    public bool ValidateNames { get; set; } = false;
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
