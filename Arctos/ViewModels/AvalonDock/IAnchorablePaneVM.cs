namespace Arctos
{
    public interface IAnchorablePaneVM : IDockingPaneVM
    {
        bool IsVisible { get; set; }

        public void CloseCommand();

    }
}
