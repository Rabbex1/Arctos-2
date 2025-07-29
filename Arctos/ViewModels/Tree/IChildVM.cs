using CommunityToolkit.Mvvm.ComponentModel;

namespace Arctos
{
    public interface IChildVM<TParent> : IBaseVM
    {
        TParent? Parent { get; set; }
        bool IsVisible { get; }
        bool IsEnabled { get; }
    }
}
