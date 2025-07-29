using DragDropEffects = System.Windows.DragDropEffects;

namespace Arctos.Wpf.DragDrop
{
  public interface IDropable
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    List<Type> DataTypes { get; }

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    void Drop(object data, int index = -1);
    DragDropEffects GetRequiredEffect(object dropObject);
    bool CanDrop(object dropObject);

    #endregion Methods
  }
}
