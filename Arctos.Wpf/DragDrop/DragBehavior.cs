using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

    #endregion Dependancy Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    #region Protected
    protected override void OnAttached()
    {
      base.OnAttached();
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
    }

    protected virtual void MouseMove(object sender, MouseEventArgs e)
    {
      if (IsDragging || e.LeftButton != MouseButtonState.Pressed)
        return;
      //set the item's DataContext as the data to be transferred
      var dragSource = AssociatedObject.DataContext as IDragable;
      if (dragSource == null)
        return;
      var data = new DataObject();
      var dragType = dragSource.DataType;
      var dragObject = dragSource.DataObject;
      data.SetData(dragType, dragObject); // temp
      IsDragging = true;
      Cursor.Open();
      System.Windows.DragDrop.DoDragDrop(AssociatedObject, data, AllowedEffects);
    }


    private void MouseLeave(object sender, MouseEventArgs e) { }

    protected virtual void GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
      Cursor.GiveFeedback(e);
      e.Handled = true;
    }

    private void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
      if (e.KeyStates == DragDropKeyStates.None)
      {
        IsDragging = false;
        e.Action = DragAction.Cancel;
      }
      else if (e.KeyStates == DragDropKeyStates.LeftMouseButton)
      {
        //Cursor.UpdatePosition();
        e.Action = DragAction.Continue;
      }
    }

    #endregion Private

    #endregion Methods
  }
}
