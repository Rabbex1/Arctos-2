namespace Arctos.Shell.Options
{
  public class ParentChildPair
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public TreeParentVM? ParentNode { get; set; }
    public TreeParentVM? ChildNode { get; set; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public ParentChildPair()
    { }

    public ParentChildPair(TreeParentVM parent, TreeParentVM child)
    {
      ParentNode = parent;
      ChildNode = child;
    }

    #endregion Constructors
  }
}
