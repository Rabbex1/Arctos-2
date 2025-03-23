using System.Configuration;

namespace Arctos.Wpf.Utilities
{
    public interface IWindowLayoutSettings
    {
        WindowLayout? Layout { get; set; }

        void Reload();
        void Save();
    }

    public class WindowLayoutSettings : ApplicationSettingsBase, IWindowLayoutSettings
    {
        [UserScopedSetting]
        public WindowLayout? Layout
        {
            get => (WindowLayout) this["Layout"];
            set => this["Layout"] = value;
        }
    }
}
