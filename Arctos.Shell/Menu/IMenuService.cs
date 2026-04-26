namespace Arctos.Shell.Menu
{
    public interface IMenuService
    {
        void AddNewMenu(string menuName);
        T AddNode<T>(string menuName, string parentId, string id, int priority) where T : IMenuNodeVM;
        bool RemoveNode(string menuName, string id);
    }
}
