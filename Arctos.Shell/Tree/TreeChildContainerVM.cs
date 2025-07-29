namespace Arctos.Shell
{
    public class TreeChildContainerVM<T> : TreeChildVM, ITreeItemContainerVM<T>
        where T : BaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Properties

        public new string? DisplayName => Content?.DisplayName;
        public T? Content { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public TreeChildContainerVM(TreeParentVM parent)
            : base(parent)
        {
        }

        #endregion Constructor
    }
}
