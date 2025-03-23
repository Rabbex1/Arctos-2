namespace Arctos.Wpf.Dialogs
{
    public class ColorDialogVM : DialogBaseVM, ICommonDialogVM
    {
        public bool AllowFullOpen { get; set; }
        public bool AnyColor { get; set; }
        public System.Drawing.Color Color { get; set; }
        public int[]? CustomColors { get; set; }
        public bool FullOpen { get; set; }
        public virtual bool SolidColorOnly { get; set; }
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