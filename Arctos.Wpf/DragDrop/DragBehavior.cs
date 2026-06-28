using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Arctos.Wpf.Utilities;
using Microsoft.Xaml.Behaviors;
using Ursa.Wpf.DragDrop;
using DataObject = System.Windows.DataObject;
using DragAction = System.Windows.DragAction;
using DragDropEffects = System.Windows.DragDropEffects;
using GiveFeedbackEventArgs = System.Windows.GiveFeedbackEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using QueryContinueDragEventArgs = System.Windows.QueryContinueDragEventArgs;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  public class DragBehavior : Behavior<FrameworkElement>
  {

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields
    protected bool IsMouseClicked;
    protected bool IsDragging;
    private static readonly DragService SharedDragService = DragService.Shared;
    private static readonly WpfDragServiceAdapter SharedDragServiceAdapter = new(SharedDragService);
    private IDragFeedbackState _dragFeedbackState;
    #endregion Fields

    public DragDropCursor Cursor { get; set; }

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Dependancy Properties

    #region CursorImage
    public static readonly DependencyProperty CursorImageProperty =
        DependencyProperty.Register("CursorImage",
            typeof(object),
            typeof(DragBehavior),
            new PropertyMetadata(null, OnCursorImageChanged));

    public ImageSource CursorImage
    {
      set { SetValue(CursorImageProperty, value); }
      get { return (ImageSource)GetValue(CursorImageProperty); }
    }

    private static void OnCursorImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      var db = d as DragBehavior;
      if (db == null || args.NewValue == null)
        return;
      if (db.Cursor == null)
        db.Cursor = new DragDropCursor();
      db.Cursor.ImageSource = args.NewValue;
    }
    #endregion CursorImage

    #region DragImage
    public static readonly DependencyProperty DragImageProperty =
        DependencyProperty.Register("DragImage",
            typeof(IDragImageSource),
            typeof(DragBehavior),
            new PropertyMetadata(null));

    public IDragImageSource DragImage
    {
      set { SetValue(DragImageProperty, value); }
      get { return (IDragImageSource)GetValue(DragImageProperty); }
    }
    #endregion DragImage

    #region Cursor Text
    public static readonly DependencyProperty CursorTextProperty =
        DependencyProperty.Register("CursorText",
            typeof(string),
            typeof(DragBehavior),
            new PropertyMetadata(null, OnCursorTextChanged));

    public string CursorText
    {
      set { SetValue(CursorTextProperty, value); }
      get { return (string)GetValue(CursorTextProperty); }
    }

    private static void OnCursorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      var db = d as DragBehavior;
      if (db == null || args.NewValue == null)
        return;
      if (db.Cursor == null)
        db.Cursor = new DragDropCursor();
      db.Cursor.Text = args.NewValue as string;
    }
    #endregion Cursor Text

    #region Allowed Effects
    public static readonly DependencyProperty AllowedEffectsProperty =
        DependencyProperty.Register("AllowedEffects",
            typeof(DragDropEffects),
            typeof(DragBehavior),
            new PropertyMetadata(DragDropEffects.None));

    public DragDropEffects AllowedEffects
    {
      set { SetValue(AllowedEffectsProperty, value); }
      get
      {
        var value = GetValue(AllowedEffectsProperty);
        return (DragDropEffects?)value ?? DragDropEffects.None;
      }
    }

    #endregion Allowed Effects

    #region UseSystemCursor
    public static readonly DependencyProperty UseSystemCursorProperty =
        DependencyProperty.Register("UseSystemCursor",
            typeof(bool),
            typeof(DragBehavior),
            new PropertyMetadata(false));

    public bool UseSystemCursor
    {
      set { SetValue(UseSystemCursorProperty, value); }
      get
      {
        var value = GetValue(UseSystemCursorProperty);
        return (bool?)value ?? false;
      }
    }

    #endregion UseSystemCursor

    #endregion Dependancy Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    #region Protected
    protected override void OnAttached()
    {
      base.OnAttached();
      _ = SharedDragServiceAdapter;
      AssociatedObject.MouseLeftButtonDown += MouseLeftButtonDown;
      AssociatedObject.MouseLeftButtonUp += MouseLeftButtonUp;
      AssociatedObject.MouseLeave += MouseLeave;
      AssociatedObject.MouseMove += MouseMove;
      AssociatedObject.GiveFeedback += GiveFeedback;
      AssociatedObject.QueryContinueDrag += QueryContinueDrag;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.MouseLeftButtonDown -= MouseLeftButtonDown;
      AssociatedObject.MouseLeftButtonUp -= MouseLeftButtonUp;
      AssociatedObject.MouseLeave -= MouseLeave;
      AssociatedObject.MouseMove -= MouseMove;
      SharedDragService.End();
      base.OnDetaching();
    }

    protected virtual object GetDataObjectData()
    {
      return AssociatedObject.DataContext;
    }

    #endregion Protected

    #region Private
    protected virtual void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      IsMouseClicked = true;
    }

    protected virtual void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      IsMouseClicked = false;
      IsDragging = false;
      SharedDragService.End();
      ResetDragFeedbackOverride();
    }

    protected virtual void MouseMove(object sender, MouseEventArgs e)
    {
      if (IsDragging || e.LeftButton != MouseButtonState.Pressed)
        return;

      var dragSource = AssociatedObject.DataContext as IDragable;
      if (dragSource == null)
        return;

      var data = new DataObject();
      var dragType = dragSource.DataType;
      var dragObject = dragSource.DataObject;
      data.SetData(dragType, dragObject);
      _dragFeedbackState = dragObject as IDragFeedbackState;
      if (_dragFeedbackState != null)
        _dragFeedbackState.DragFeedbackEffectOverride = DragDropEffects.None;
      IsDragging = true;

      if (UseSystemCursor)
      {
        BeginSharedDragImage();
      }
      else
      {
        Cursor ??= new DragDropCursor();
        Cursor.ImageSource = GetValue(CursorImageProperty);
        Cursor.Text = CursorText;
        Cursor.Open();
      }

      System.Windows.DragDrop.DoDragDrop(AssociatedObject, data, AllowedEffects);
      IsDragging = false;
      SharedDragService.End();
      ResetDragFeedbackOverride();
    }

    private void MouseLeave(object sender, MouseEventArgs e) { }

    protected virtual void GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
      if (UseSystemCursor)
      {
        UpdateSharedDragPointerPosition();
        SharedDragService.SetEffectOverride(_dragFeedbackState?.DragFeedbackEffectOverride ?? e.Effects);
        var effects = _dragFeedbackState?.DragFeedbackEffectOverride ?? e.Effects;
        Mouse.SetCursor(effects == DragDropEffects.None ? System.Windows.Input.Cursors.No : System.Windows.Input.Cursors.Arrow);
        e.UseDefaultCursors = false;
        e.Handled = true;
        return;
      }

      Cursor.GiveFeedback(e);
      e.Handled = true;
    }

    private void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
      if (e.KeyStates == DragDropKeyStates.None)
      {
        IsDragging = false;
        SharedDragService.End();
        ResetDragFeedbackOverride();
        e.Action = DragAction.Cancel;
      }
      else if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
      {
        UpdateSharedDragPointerPosition();
        e.Action = DragAction.Continue;
      }
    }

    private void BeginSharedDragImage()
    {
      if (DragImage == null || !Win32.GetCursorPos(out var point))
      {
        SharedDragService.End();
        return;
      }

      SharedDragService.Begin(DragImage, DragDropEffects.None, point.x, point.y);
    }

    private static void UpdateSharedDragPointerPosition()
    {
      if (!Win32.GetCursorPos(out var point))
        return;

      SharedDragService.UpdatePointerPosition(point.x, point.y);
    }

    private void ResetDragFeedbackOverride()
    {
      if (_dragFeedbackState != null)
        _dragFeedbackState.DragFeedbackEffectOverride = null;

      _dragFeedbackState = null;
      SharedDragService.SetEffectOverride(null);
    }

    #endregion Private

    #endregion Methods
  }
}
