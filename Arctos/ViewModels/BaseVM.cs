using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;

namespace Arctos
{
    public class BaseVM : ObservableObject, IBaseVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region DisplayName
        private string? _displayName;
        public string? DisplayName
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

        // Compatibility shim for legacy Ursa-based VMs during migration.
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
