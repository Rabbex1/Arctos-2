using Arctos.Shell.Commands;

namespace Arctos.Shell.Menu
{
    public class MenuService : IMenuService
    {
        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Fields

        private readonly Dictionary<string, Dictionary<string, IMenuNodeVM>> _menus = new();
        private readonly ICommandService _commandService;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------

        #region Constructor

        public MenuService(ICommandService commandService)
        {
            _commandService = commandService;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void AddNewMenu(string menuName)
        {
            _menus.TryAdd(menuName, new Dictionary<string, IMenuNodeVM>());
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
                if (parent is not MenuVM)
                    throw new MenuServiceException($"Parent must be MenuParentVM or MenuGroupVM");
                node = (T?)Activator.CreateInstance(typeof(T), parent) ??
                       throw new MenuServiceException($"Could not create object of type {typeof(T)}");
                if (node is MenuItemVM item)
                {
                    item.CommandService = _commandService;
                }
            }
            node.Id = id;
            node.Priority = priority;
            AddNode(menu, parent, node);

            return node;

        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private IMenuNodeVM? GetNode(Dictionary<string, IMenuNodeVM> menu, string id)
        {
            return menu.GetValueOrDefault(id);
        }

        private void AddNode(Dictionary<string, IMenuNodeVM> menu, MenuVM? parent, IMenuNodeVM node)
        {
            parent?.AddChild(node);
            menu.Add(node.Id, node);
        }

        #endregion Private Methods

        #endregion Methods

    }
}
