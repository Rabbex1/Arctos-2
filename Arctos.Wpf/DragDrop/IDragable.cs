namespace Arctos.Wpf.DragDrop
{
  public interface IDragable
  {
    /// <summary>
    /// Type of the data item
    /// </summary>
    Type DataType { get; }

    object DataObject { get; }

    /// <summary>
    /// Remove the object from the collection
    /// </summary>
    void Remove(object i);
  }
}
