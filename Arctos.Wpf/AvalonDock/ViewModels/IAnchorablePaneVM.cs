namespace Arctos.Wpf.AvalonDock
{
    public interface IAnchorablePaneVM : IDockingPaneVM
    {
        bool IsVisible { get; set; }
        //IMenuNodeVM MenuItem { get; }
    }
}
