using Arctos.Shell.Commands;

namespace Arctos.Shell.Menu
{
    public class MenuService(ICommandService commandService) : IMenuService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Dictionary<string, Dictionary<string, IMenuNodeVM>> _menus = [];

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void AddNewMenu(string menuName)
        {
            _menus.TryAdd(menuName, []);
        }

        public T AddNode<T>(string menuName, string parentId, string id, int priority)
            where T : IMenuNodeVM
        {
            if(!_menus.ContainsKey(menuName))
                AddNewMenu(menuName);
            var menu = _menus[menuName];
            var parent = GetNode(menu, parentId) as MenuVM;

            T node;

            if (parentId == id)
            {
                node = (T?)Activator.CreateInstance(typeof(T), [null]) ??
                       throw new MenuServiceException($"Could not create object of type {typeof(T)}");
            }
            else
            {
                if (parent == null)
                    throw new MenuServiceException($"Parent node not found: {parentId}");
                if (parent is null)
                    throw new MenuServiceException($"Parent must be MenuParentVM or MenuGroupVM");
                node = (T?)Activator.CreateInstance(typeof(T), parent) ??
                       throw new MenuServiceException($"Could not create object of type {typeof(T)}");
                if (node is MenuItemVM item)
                {
                    item.CommandService = commandService;
                }
            }
            node.Id = id;
            node.Priority = priority;
            AddNode(menu, parent, node);

            return node;

        }

        public bool RemoveNode(string menuName, string id)
        {
            if (!_menus.TryGetValue(menuName, out var menu))
                return false;

            if (!menu.TryGetValue(id, out var node))
                return false;

            RemoveNodeRecursive(menu, node);
            return true;
        }


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private static IMenuNodeVM? GetNode(Dictionary<string, IMenuNodeVM> menu, string id)
        {
            return menu.GetValueOrDefault(id);
        }

        private static void AddNode(Dictionary<string, IMenuNodeVM> menu, MenuVM? parent, IMenuNodeVM node)
        {
            parent?.AddChild(node);
            menu.Add(node.Id, node);
        }

        private static void RemoveNodeRecursive(Dictionary<string, IMenuNodeVM> menu, IMenuNodeVM node)
        {
            if (node is MenuVM parentNode)
            {
                var children = parentNode.Children.OfType<IMenuNodeVM>().ToList();
                foreach (var child in children)
                    RemoveNodeRecursive(menu, child);
            }

            if (TryGetParent(node) is MenuVM parent)
                parent.RemoveChild(node);

            menu.Remove(node.Id);
        }

        private static MenuVM? TryGetParent(IMenuNodeVM node)
        {
            var parentProperty = node.GetType().GetProperty("Parent");
            return parentProperty?.GetValue(node) as MenuVM;
        }


        #endregion Private Methods

        #endregion Methods

    }
}
