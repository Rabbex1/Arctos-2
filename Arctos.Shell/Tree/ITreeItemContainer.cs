namespace Arctos.Shell
{
    public interface ITreeItemContainerVM<T> : ITreeItemVM
        where T : BaseVM
    {
        public T? Content { get; set; }
    }
}
