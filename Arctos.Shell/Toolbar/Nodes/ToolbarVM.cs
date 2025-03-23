
namespace Arctos.Shell.Toolbar
{
    public class ToolbarVM : ParentVM<IToolbarNodeVM, ToolbarVM>, IToolbarNodeVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public ToolbarVM(ToolbarVM? parent)
            : base(parent)
        {
        }

        #endregion Constructors
    }
}
