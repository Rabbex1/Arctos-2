namespace Arctos.Wpf.AvalonDock
{
  public interface IDockingPaneVM
  {
    bool IsActive { get; set; }
    bool IsSelected { get; set; }
    string ContentId { get; }
  }
}
