namespace Arctos.Shell
{
    public class TreeParentContainerVM<T> : TreeParentVM, ITreeItemContainerVM<T>
        where T : BaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public new string? DisplayName => Content?.DisplayName;
        public T? Content { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public TreeParentContainerVM(TreeParentVM? parent, bool isExpanded = false)
        : base(parent, isExpanded)
        { }

        #endregion Constructor
    }
}
