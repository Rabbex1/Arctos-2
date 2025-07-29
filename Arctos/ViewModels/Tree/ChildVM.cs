using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos
{
    public partial class ChildVM<TParent> : BaseVM, IChildVM<TParent>
        where TParent : class
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        [ObservableProperty]
        private TParent? _parent;

        [ObservableProperty] 
        private bool _isVisible = true;

        [ObservableProperty] 
        private bool _isEnabled = true;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected ChildVM(TParent? parent)
        {
            _parent = parent;
        }

        #endregion Constructor

    }


}
