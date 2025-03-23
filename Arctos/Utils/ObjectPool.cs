namespace Arctos.Utils
{
  public class ObjectPool<T>
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private const int DefaultMinimumSize = 10;
    private readonly int _minimumSize;
    private readonly Func<T> _createObject;
    private readonly List<T> _objects;
    private int _currentIndex;

    #endregion Fields

    public int Count => _currentIndex;

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public ObjectPool(int minimumSize, Func<T> createObject)
    {
      _objects = new List<T>(minimumSize);
      _minimumSize = minimumSize;
      _createObject = createObject;
      Reset();
    }

    public ObjectPool(Func<T> createObject)
      : this(DefaultMinimumSize, createObject)
    {
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public void ForEachRemaining(Action<T> action)
    {
      for (var index = _currentIndex; index < _objects.Count; index++)
      {
        action(_objects[index]);
      }
    }

    public void ForEach(Action<T> action)
    {
      for (var index = 0; index < _currentIndex; index++)
      {
        action(_objects[index]);
      }
    }

    public T Next()
    {
      if (_currentIndex == _objects.Count)
      {
        _objects.Add(_createObject());
      }
      var item = _objects[_currentIndex];
      _currentIndex++;
      return item;
    }

    public void MovePrevious()
    {
      _currentIndex--;
    }

    public void Reset()
    {
      _currentIndex = 0;
    }

    public void Done()
    {
      // Remove the rest of the objects if we are using less than half of the pool.
      if (_currentIndex != 0 && _objects.Count > 0 && _currentIndex >= _minimumSize && _currentIndex < _objects.Count / 2)
      {
        _objects.RemoveRange(_currentIndex, _objects.Count - _currentIndex);
      }
    }

    public void Clear()
    {
      _objects.Clear();
    }

    #endregion Methods

  }
}
