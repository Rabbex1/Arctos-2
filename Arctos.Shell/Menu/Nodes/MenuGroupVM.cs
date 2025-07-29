namespace Arctos.Shell.Menu
{
  public class MenuGroupVM(MenuVM parent) : MenuVM(parent)
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public override void AddChild(IMenuNodeVM child)
    {
      Parent?.AddChild(child);
    }

    public override void RemoveChild(IMenuNodeVM child)
    {
      Parent?.RemoveChild(child);
    }

    #endregion  Methods

  }
}
