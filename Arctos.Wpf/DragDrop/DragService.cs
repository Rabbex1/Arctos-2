using DragDropEffects = System.Windows.DragDropEffects;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  /// <summary>
  /// Holds the shared drag-image state independently of any specific UI framework presentation.
  /// </summary>
  public sealed class DragService
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Static
    public static DragService Shared { get; } = new DragService();
    #endregion Static

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties
    public IDragImageSource Image { get; private set; }
    public DragDropEffects? EffectOverride { get; private set; }
    public bool IsVisible { get; private set; }
    public double ScreenX { get; private set; }
    public double ScreenY { get; private set; }
    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Events
    public event Action StateChanged;
    #endregion Events

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Public Methods

    public void Begin(IDragImageSource image, DragDropEffects? effectOverride, double screenX, double screenY)
    {
      Image = image;
      EffectOverride = effectOverride;
      IsVisible = image != null;
      ScreenX = screenX;
      ScreenY = screenY;
      RaiseStateChanged();
    }

    public void SetEffectOverride(DragDropEffects? effectOverride)
    {
      if (EffectOverride == effectOverride)
        return;

      EffectOverride = effectOverride;
      RaiseStateChanged();
    }

    public void UpdatePointerPosition(double screenX, double screenY)
    {
      if (Math.Abs(ScreenX - screenX) < 0.01 && Math.Abs(ScreenY - screenY) < 0.01)
        return;

      ScreenX = screenX;
      ScreenY = screenY;
      RaiseStateChanged();
    }

    public void End()
    {
      if (!IsVisible && Image == null && EffectOverride == null)
        return;

      Image = null;
      EffectOverride = null;
      IsVisible = false;
      ScreenX = 0.0;
      ScreenY = 0.0;
      RaiseStateChanged();
    }

    #endregion Public Methods

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Private Methods
    private void RaiseStateChanged()
    {
      StateChanged?.Invoke();
    }
    #endregion Private Methods
  }
}
