using System.Windows;

namespace Arctos.Wpf.Charts
{
    interface INotifiableParent
    {
        int AddChild(UIElement child);
        void RemoveChild(UIElement child);
    }
}