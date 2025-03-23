namespace Arctos.Shell.Menu
{
  public class MenuGroupVM : MenuVM
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructor

    public MenuGroupVM(MenuVM parent)
      : base(parent)
    {
    }

    #endregion Constructor

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
