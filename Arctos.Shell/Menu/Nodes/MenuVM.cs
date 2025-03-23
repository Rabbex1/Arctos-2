using System.Collections.Specialized;

namespace Arctos.Shell.Menu
{
    public class MenuVM : ParentVM<IMenuNodeVM, MenuVM>, IMenuNodeVM
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region Id
        private string _id = string.Empty;
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        #region Priority
        private int _priority;
        public int Priority
        {
            get => _priority;
            set => SetProperty(ref _priority, value);
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public MenuVM(MenuVM parent)
          : base(parent)
        {
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override void AddChild(IMenuNodeVM child)
        {
            var index = Children.TakeWhile(c => !(c?.Priority > child?.Priority)).Count();
            Children.Insert(index, child);
            SetSeparatorVisibility();
            RaiseChildrenChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, child));
        }

        public override void RemoveChild(IMenuNodeVM child)
        {
            Children.Remove(child);
            SetSeparatorVisibility();
            RaiseChildrenChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, child));
        }

        public virtual void SetSeparatorVisibility()
        {
            if (Children.Count == 0)
                return;

            foreach (var child in Children)
                if (child is MenuGroupVM)
                    child.IsVisible = true;

            var first = Children.First();
            if (first is MenuGroupVM)
                first.IsVisible = false;

            if (Children.Count == 0)
                return;
            var last = Children.Last();
            if (last is MenuGroupVM)
                last.IsVisible = false;
            foreach (var child in Children)
            {
                var item = child as MenuVM;
                item?.SetSeparatorVisibility();
            }
        }

        #endregion Methods

    }
}
