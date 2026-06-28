using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arctos.Wpf.Utilities;
using Ursa.Wpf.DragDrop;
using Cursor = System.Windows.Input.Cursor;
using DragDropEffects = System.Windows.DragDropEffects;
using GiveFeedbackEventArgs = System.Windows.GiveFeedbackEventArgs;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  public class DragDropCursor
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields

    private readonly int _dpix;
    private readonly int _dpiy;
    private CustomCursor _customCursor;
    private readonly Dictionary<DragDropEffects, Cursor> _cursors = [];
    private Image _noneIcon;
    private Image _linkIcon;
    private Image _copyIcon;
    private Image _moveIcon;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    //#region ImageSource
    //private ImageSource _imageSource;
    //public ImageSource ImageSource 
    //{
    // get { return _imageSource; }
    // set
    // {
    //  _imageSource = value;
    //  if (_customCursor?.FindName("CursorImage") is Image image)
    //	  image.Source = ImageSource;
    // }
    //}
    //#endregion

    #region ImageSource
    private object _imageSource;
    public object ImageSource
    {
      get => _imageSource;
      set
      {
        _imageSource = value;
        if (_imageSource is FrameworkElement fe)
        {
          fe.Width = 64;
          fe.Height = 64;
        }
        if (_customCursor?.FindName("CursorImage") is ContentPresenter image)
          image.Content = CreateCursorContent(ImageSource);
      }
    }
    #endregion

    #region Text
    private string _text;
    public string Text
    {
      get { return _text; }
      set
      {
        _text = value;
        if (_customCursor?.FindName("CursorText") is TextBlock textBlock)
          textBlock.Text = Text;
      }
    }
    #endregion

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    public DragDropCursor()
    {
      var deskTop = Win32.GetDesktopWindow();
      var hdc = Win32.GetDC(deskTop);
      _dpix = Win32.GetDeviceCaps(hdc, Win32.LOGPIXELSX);
      _dpiy = Win32.GetDeviceCaps(hdc, Win32.LOGPIXELSY);
    }

    #endregion Constructors

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    //---------------------------------------------------------------------
    #region Public Methods

    public CustomPopupPlacement[] PlacePopup(Size __, Size _, Point offset)
    {
      var placement1 = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Vertical);
      var placement2 = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Horizontal);
      CustomPopupPlacement[] ttPlaces = [placement1, placement2];
      return ttPlaces;
    }

    public void Open()
    {
      if (_customCursor == null)
      {
        _customCursor = new CustomCursor();
        _noneIcon = _customCursor.FindName("NoneIcon") as Image;
        _linkIcon = _customCursor.FindName("LinkIcon") as Image;
        _copyIcon = _customCursor.FindName("CopyIcon") as Image;
        _moveIcon = _customCursor.FindName("MoveIcon") as Image;
      }
      var cursorImage = (ContentPresenter)_customCursor.FindName("CursorImage");
      if (cursorImage != null)
        cursorImage.Content = CreateCursorContent(ImageSource);
      var cursorText = (TextBlock)_customCursor.FindName("CursorText");
      if (cursorText != null)
        cursorText.Text = Text;
      CreateCursors();
    }

    public void GiveFeedback(GiveFeedbackEventArgs e)
    {
      if (_customCursor == null)
        return;
      if (_cursors.TryGetValue(e.Effects, out var cursor))
        Mouse.SetCursor(cursor);
    }

    #endregion Public Methods

    //---------------------------------------------------------------------
    #region Private Methods

    private void CreateCursors()
    {
      _cursors.Clear();
      _linkIcon.Visibility = Visibility.Collapsed;
      _noneIcon.Visibility = Visibility.Visible;
      _copyIcon.Visibility = Visibility.Collapsed;
      _moveIcon.Visibility = Visibility.Collapsed;
      _cursors.Add(DragDropEffects.None, ConvertToCursor(_customCursor, _noneIcon, new Point(0.5, 0.5)));

      _linkIcon.Visibility = Visibility.Visible;
      _noneIcon.Visibility = Visibility.Collapsed;
      _copyIcon.Visibility = Visibility.Collapsed;
      _moveIcon.Visibility = Visibility.Collapsed;
      _cursors.Add(DragDropEffects.Link, ConvertToCursor(_customCursor, _linkIcon, new Point(0.5, 0.5)));

      _linkIcon.Visibility = Visibility.Collapsed;
      _noneIcon.Visibility = Visibility.Collapsed;
      _copyIcon.Visibility = Visibility.Visible;
      _moveIcon.Visibility = Visibility.Collapsed;
      _cursors.Add(DragDropEffects.Copy, ConvertToCursor(_customCursor, _copyIcon, new Point(0.5, 0.5)));

      _linkIcon.Visibility = Visibility.Collapsed;
      _noneIcon.Visibility = Visibility.Collapsed;
      _copyIcon.Visibility = Visibility.Collapsed;
      _moveIcon.Visibility = Visibility.Visible;
      _cursors.Add(DragDropEffects.Move, ConvertToCursor(_customCursor, _moveIcon, new Point(0.5, 0.5)));
    }

    private Cursor ConvertToCursor(UIElement control, Image icon, Point realativehotSpot)
    {
      // convert FrameworkElement to PNG stream
      var pngStream = new MemoryStream();
      control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      var rect = new Rect(0, 0, control.DesiredSize.Width, control.DesiredSize.Height);
      var pixelWidth = (int)(control.DesiredSize.Width * _dpix / 96.0);
      var pixelHeight = (int)(control.DesiredSize.Height * _dpiy / 96.0);
      var rtb = new RenderTargetBitmap(pixelWidth, pixelHeight, _dpix, _dpiy, PixelFormats.Pbgra32);

      control.Arrange(rect);
      rtb.Render(control);

      // calculate the hotspot location
      Point hotSpot = icon.TranslatePoint(new Point(0, 0), control);
      hotSpot.X += icon.ActualWidth * realativehotSpot.X;
      hotSpot.Y += icon.ActualWidth * realativehotSpot.Y;
      hotSpot.X *= _dpix / 96.0;
      hotSpot.Y *= _dpiy / 96.0;

      var png = new PngBitmapEncoder();
      png.Frames.Add(BitmapFrame.Create(rtb));
      png.Save(pngStream);

      // write cursor header info
      var cursorStream = new MemoryStream();
      cursorStream.Write([0x00, 0x00], 0, 2);                               // ICONDIR: Reserved. Must always be 0.
      cursorStream.Write([0x02, 0x00], 0, 2);                               // ICONDIR: Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid
      cursorStream.Write([0x01, 0x00], 0, 2);                               // ICONDIR: Specifies number of images in the file.
      cursorStream.Write([(byte)control.DesiredSize.Width], 0, 1);               // ICONDIRENTRY: Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
      cursorStream.Write([(byte)control.DesiredSize.Height], 0, 1);              // ICONDIRENTRY: Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.
      cursorStream.Write([0x00], 0, 1);                                     // ICONDIRENTRY: Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette.
      cursorStream.Write([0x00], 0, 1);                                     // ICONDIRENTRY: Reserved. Should be 0.
      cursorStream.Write([(byte)hotSpot.X, 0x00], 0, 2);                    // ICONDIRENTRY: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
      cursorStream.Write([(byte)hotSpot.Y, 0x00], 0, 2);                    // ICONDIRENTRY: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
      cursorStream.Write([
          (byte)(pngStream.Length & 0x000000FF),
        (byte)((pngStream.Length & 0x0000FF00) >> 0x08),
        (byte)((pngStream.Length & 0x00FF0000) >> 0x10),
        (byte)((pngStream.Length & 0xFF000000) >> 0x18)
      ], 0, 4);
      cursorStream.Write([0x16, 0x00, 0x00, 0x00], 0, 4);

      // copy PNG stream to cursor stream
      pngStream.Seek(0, SeekOrigin.Begin);
      pngStream.CopyTo(cursorStream);

      // return cursor stream
      cursorStream.Seek(0, SeekOrigin.Begin);
      return new Cursor(cursorStream);
    }

    private static object CreateCursorContent(object imageSource)
    {
      return imageSource switch
      {
        null => null,
        ImageSource source => new Grid
        {
          Width = 128,
          Height = 128,
          Background = System.Windows.Media.Brushes.Transparent,
          Children =
          {
            new Image
            {
              Source = source,
              Stretch = Stretch.None,
              HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
              VerticalAlignment = VerticalAlignment.Top,
              SnapsToDevicePixels = true,
            },
          },
        },
        _ => imageSource
      };
    }

    #endregion Private Methods

    #endregion Methods
  }
}
