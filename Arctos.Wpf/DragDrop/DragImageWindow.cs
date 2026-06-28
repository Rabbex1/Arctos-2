using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Arctos.Wpf.Utilities;

#nullable disable

namespace Arctos.Wpf.DragDrop;

/// <summary>
/// Hosts a transparent topmost helper window that shows a drag image and follows the cursor.
/// </summary>
public sealed class DragImageWindow
{
  //------------------------------------------------------------------------------------------------------------------------------------------
  #region Fields
  private Window _window;
  private System.Windows.Controls.Border _contentHost;
  private System.Windows.Controls.Image _image;
  private ImageSource _currentImageSource;
  #endregion Fields

  //------------------------------------------------------------------------------------------------------------------------------------------
  #region Public Methods

  public void Show(ImageSource imageSource)
  {
    if (imageSource == null)
    {
      Hide();
      return;
    }

    _window ??= new Window
    {
      AllowsTransparency = true,
      Background = System.Windows.Media.Brushes.Transparent,
      WindowStyle = WindowStyle.None,
      ResizeMode = ResizeMode.NoResize,
      ShowInTaskbar = false,
      ShowActivated = false,
      Topmost = true,
      IsHitTestVisible = false,
      SizeToContent = SizeToContent.WidthAndHeight
    };

    EnsureContent();

    if (!ReferenceEquals(_currentImageSource, imageSource))
    {
      _currentImageSource = imageSource;
      _image.Source = imageSource;
      _contentHost.Width = Math.Max(1.0, imageSource.Width);
      _contentHost.Height = Math.Max(1.0, imageSource.Height);
    }

    if (!_window.IsVisible)
    {
      _window.Show();
      ApplyWindowStyles();
    }
  }

  public void Hide()
  {
    if (_window == null)
      return;

    _window.Close();
    _window = null;
    _contentHost = null;
    _image = null;
    _currentImageSource = null;
  }

  public void UpdatePositionCenteredOnCursor()
  {
    if (_window == null || !_window.IsVisible)
      return;

    if (!Win32.GetCursorPos(out var point))
      return;

    UpdatePositionCentered(point.x, point.y);
  }

  public void UpdatePositionCentered(double screenX, double screenY)
  {
    if (_window == null || !_window.IsVisible)
      return;

    var width = _window.ActualWidth;
    var height = _window.ActualHeight;

    if (width <= 0.0 || height <= 0.0)
    {
      if (_window.Content is FrameworkElement content)
      {
        content.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
        width = Math.Max(width, content.DesiredSize.Width);
        height = Math.Max(height, content.DesiredSize.Height);
      }
    }

    _window.Left = screenX - (width * 0.5);
    _window.Top = screenY - (height * 0.5);
  }

  #endregion Public Methods

  //------------------------------------------------------------------------------------------------------------------------------------------
  #region Private Methods

  private void EnsureContent()
  {
    if (_contentHost != null)
      return;

    _image = new System.Windows.Controls.Image
    {
      Stretch = Stretch.None,
      SnapsToDevicePixels = true,
      HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
      VerticalAlignment = VerticalAlignment.Top
    };

    _contentHost = new System.Windows.Controls.Border
    {
      Background = System.Windows.Media.Brushes.Transparent,
      Width = 1.0,
      Height = 1.0,
      Child = _image
    };

    _window.Content = _contentHost;
  }

  private void ApplyWindowStyles()
  {
    if (_window == null)
      return;

    var helper = new WindowInteropHelper(_window);
    if (helper.Handle == IntPtr.Zero)
      return;

    var extendedStyle = Win32.GetWindowLongPtr(helper.Handle, Win32.GWL_EXSTYLE).ToInt64();
    extendedStyle |= Win32.WS_EX_TRANSPARENT;
    extendedStyle |= Win32.WS_EX_TOOLWINDOW;
    Win32.SetWindowLongPtr(helper.Handle, Win32.GWL_EXSTYLE, new IntPtr(extendedStyle));
  }

  #endregion Private Methods
}
