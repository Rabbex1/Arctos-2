namespace Arctos.Wpf.Dialogs
{
  public interface IDialogPresenter<in T> where T : IDialogBaseVM
  {
    void Show(T viewModel);
  }
}
