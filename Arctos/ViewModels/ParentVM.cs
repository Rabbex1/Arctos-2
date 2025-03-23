using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Arctos
{
    public class ParentVM<TChild, TParent> : ChildVM<TParent>
        where TParent : class
        where TChild : class
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties


        #region Children
        private ObservableCollection<TChild> _children = new();
        public ObservableCollection<TChild> Children
        {
            get => _children;
            set
            {
                _children.CollectionChanged -= ChildrenCollectionChanged;
                SetProperty(ref _children, value);
                _children.CollectionChanged += ChildrenCollectionChanged;
            }
        }
        #endregion

        #endregion Properties


        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected ParentVM(TParent? parent)
            : base(parent)
        {
            Children.CollectionChanged += ChildrenCollectionChanged;
        }

        #endregion  Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public virtual void AddChild(TChild child)
        {
            Children.Add(child);
        }

        public virtual void RemoveChild(TChild child)
        {
            Children.Remove(child);
        }

        #endregion  Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void ChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaiseChildrenChanged(e);
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        public event EventHandler<NotifyCollectionChangedEventArgs>? ChildrenChanged;
        protected void RaiseChildrenChanged(NotifyCollectionChangedEventArgs e)
        {
            ChildrenChanged?.Invoke(this, e);
        }

        #endregion Events

    }
}
