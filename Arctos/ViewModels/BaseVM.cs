using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos
{
    public class BaseVM : ObservableObject 
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region DisplayName
        private string? _displayName;
        public virtual string? DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }
        #endregion

        #region ToolTip
        private string? _toolTip;
        public string? ToolTip
        {
            get => _toolTip;
            set => SetProperty(ref _toolTip, value);
        }
        #endregion

        #region Icon
        private object? _icon;
        public object? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }
        #endregion

        #endregion Properties

    }

}
