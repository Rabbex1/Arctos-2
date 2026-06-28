using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  /// <summary>
  /// WPF presentation adapter for a shared <see cref="DragService"/>.
  /// </summary>
  public sealed class WpfDragServiceAdapter
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields
    private readonly DragService _dragService;
    private readonly DragImageWindow _dragImageWindow = new();
    private IDragImageSource _currentImage;
    private ImageSource _currentImageSource;
    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors
    public WpfDragServiceAdapter(DragService dragService)
    {
      _dragService = dragService ?? throw new ArgumentNullException(nameof(dragService));
      _dragService.StateChanged += OnDragServiceStateChanged;
    }
    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods

    private void OnDragServiceStateChanged()
    {
      if (!_dragService.IsVisible || _dragService.Image == null)
      {
        _dragImageWindow.Hide();
        return;
      }

      var imageSource = GetOrCreateImageSource(_dragService.Image);
      if (imageSource == null)
      {
        _dragImageWindow.Hide();
        return;
      }

      _dragImageWindow.Show(imageSource);
      _dragImageWindow.UpdatePositionCentered(_dragService.ScreenX, _dragService.ScreenY);
    }

    private ImageSource GetOrCreateImageSource(IDragImageSource image)
    {
      if (ReferenceEquals(_currentImage, image) && _currentImageSource != null)
        return _currentImageSource;

      if (image.PngData == null || image.PngData.Length == 0)
        return null;

      using var stream = new MemoryStream(image.PngData, writable: false);
      var bitmap = new BitmapImage();
      bitmap.BeginInit();
      bitmap.CacheOption = BitmapCacheOption.OnLoad;
      bitmap.StreamSource = stream;
      bitmap.EndInit();
      bitmap.Freeze();

      _currentImage = image;
      _currentImageSource = bitmap;
      return _currentImageSource;
    }

    #endregion Private Methods
  }
}
