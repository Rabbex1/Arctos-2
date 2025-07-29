using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Arctos
{
    public interface IParentVM<TChild, TParent> : IChildVM<TParent>
        where TParent : class
        where TChild : class
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ObservableCollection<TChild> Children { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public void AddChild(TChild child);

        public void RemoveChild(TChild child);

        #endregion  Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        public event EventHandler<NotifyCollectionChangedEventArgs>? ChildrenChanged;

        #endregion Events

    }
}
