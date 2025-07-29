using System.ComponentModel;

namespace Arctos
{
    public interface IBaseVM
    {
        public string? DisplayName { get; set; }
        public string? ToolTip { get; set; }
        public object? Icon { get; set; }

        event PropertyChangedEventHandler? PropertyChanged;
    }
}
