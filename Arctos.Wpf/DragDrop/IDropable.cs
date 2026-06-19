using DragDropEffects = System.Windows.DragDropEffects;
using Point = System.Windows.Point;

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

    /// <summary>
    /// Called when a compatible drag payload first enters the drop target.
    /// </summary>
    /// <param name="data">The dragged data object.</param>
    /// <param name="location">Pointer location relative to the drop target.</param>
    void OnDragEnter(object data, Point location);

    /// <summary>
    /// Called while a compatible drag payload moves over the drop target.
    /// </summary>
    /// <param name="data">The dragged data object.</param>
    /// <param name="location">Pointer location relative to the drop target.</param>
    void OnDragOver(object data, Point location);

    /// <summary>
    /// Called when the current drag payload leaves the drop target without being dropped.
    /// </summary>
    void OnDragLeave();

    /// <summary>
    /// Drop the supplied data onto the target at the specified pointer location.
    /// </summary>
    /// <param name="data">The dropped data object.</param>
    /// <param name="location">Pointer location relative to the drop target.</param>
    /// <param name="index">Optional index for list-style targets.</param>
    void Drop(object data, Point location, int index = -1);

    DragDropEffects GetRequiredEffect(object dropObject);

    bool CanDrop(object dropObject);

    #endregion Methods
  }
}
