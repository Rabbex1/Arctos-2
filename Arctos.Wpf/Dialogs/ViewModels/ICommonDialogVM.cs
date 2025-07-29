namespace Arctos.Wpf.Dialogs
{
  public interface ICommonDialogVM : IDialogBaseVM
  {
    bool Show(IList<IDialogBaseVM> collection);

  }
}
