using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
    public class NotifiableUiElementCollection : UIElementCollection
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly INotifiableParent _parent;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public NotifiableUiElementCollection(UIElement visualParent, FrameworkElement logicalParent)
            : base(visualParent, logicalParent)
        {
            _parent = (INotifiableParent)logicalParent;
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public override int Add(UIElement element)
        {
            return _parent?.AddChild(element) ?? -1;
        }

        public override void Remove(UIElement element)
        {
            _parent?.RemoveChild(element);
        }

        #endregion Methods
    }
}