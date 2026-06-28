using System.Windows;
using Microsoft.Xaml.Behaviors;
using Ursa.Wpf.DragDrop;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using Point = System.Windows.Point;

#nullable disable

namespace Arctos.Wpf.DragDrop
{
  public enum AcceptDrop
  {
    Yes,
    No,
    Ignore
  }

  public class DropBehavior : Behavior<FrameworkElement>
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Fields
    protected List<Type> AcceptedDataTypes; //the types of the data that can be dropped into this control
    protected Type CurrentDataType; //the current type of data being dragged
    protected object CurrentObject;
    protected IDropable DropTarget; //the target to drop the data onto
    protected DropAdorner Adorner;

    #endregion Fields

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Dependency Properties
    #region IsEnabledProperty
    /// <summary>
    /// This property allows the behavior to be used as a traditional
    /// attached property behavior.
    /// </summary>
    public static DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DropBehavior),
            new FrameworkPropertyMetadata(false));

    /// <summary>
    /// Returns whether DragPositionBehavior is enabled via attached property
    /// </summary>
    /// <param name="uie">Element</param>
    /// <returns>True/False</returns>
    public static bool GetIsEnabled(DependencyObject uie)
    {
      var value = uie.GetValue(IsEnabledProperty);
      return value != null && (bool)value;
    }

    /// <summary>
    /// Adds DragPositionBehavior to an element
    /// </summary>
    /// <param name="uie">Element to apply</param>
    /// <param name="value">True/False</param>
    public static void SetIsEnabled(DependencyObject uie, bool value)
    {
      uie.SetValue(IsEnabledProperty, value);
    }

    #endregion IsEnabledProperty

    #endregion Dependency Properties

    #region Attache / Detach
    protected override void OnAttached()
    {
      base.OnAttached();
      AssociatedObject.AllowDrop = true;

      AssociatedObject.DragEnter += OnDragEnter;
      AssociatedObject.DragOver += OnDragOver;
      AssociatedObject.DragLeave += OnDragLeave;
      AssociatedObject.Drop += OnDrop;
    }

    protected override void OnDetaching()
    {
      AssociatedObject.DragEnter -= OnDragEnter;
      AssociatedObject.DragOver -= OnDragOver;
      AssociatedObject.DragLeave -= OnDragLeave;
      AssociatedObject.Drop -= OnDrop;
      base.OnDetaching();
    }

    #endregion Attache / Detach

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Event Handlers
    #region Drop
    protected virtual void OnDrop(object sender, DragEventArgs e)
    {
      switch (CanDrop(sender, e))
      {
        case AcceptDrop.Yes:
          DoDrop(sender, e);
          e.Handled = true;
          break;
        case AcceptDrop.No:
          e.Handled = false; //leave for Parent
          break;
        case AcceptDrop.Ignore:
          e.Handled = false; //leave for Parent
          break;
      }

      if (Adorner != null)
      {
        Adorner.Remove();
        Adorner = null;
      }
    }

    #endregion Drop

    protected virtual void OnDragEnter(object sender, DragEventArgs e)
    {
      //if the DataContext implements IDropable, record the current dragging datatype if it is in the list of data types that can be dropped
      CurrentDataType = null;
      CurrentObject = null;
      if (AssociatedObject.DataContext != null)
      {
        DropTarget = AssociatedObject.DataContext as IDropable;
        if (DropTarget != null)
        {
          if ((AcceptedDataTypes = DropTarget.DataTypes) != null)
          {
            var availableTypes = e.Data.GetFormats();
            foreach (var t in AcceptedDataTypes)
            {
              foreach (var at in availableTypes)
                if (t.ToString() == at)
                {
                  CurrentDataType = t;
                  CurrentObject = e.Data.GetData(CurrentDataType);
                }
            }
          }
        }
      }
      if (CurrentDataType != null)
      {
        DropTarget.OnDragEnter(CurrentObject, e.GetPosition(AssociatedObject));
        SetDragDropEffects(e, true);
        e.Handled = true;
      }
      else
      {
        e.Effects = DragDropEffects.None;
        e.Handled = false;
      }
    }

    protected virtual void OnDragOver(object sender, DragEventArgs e)
    {
      switch (CanDrop(sender, e))
      {
        case AcceptDrop.Yes:
          DropTarget.OnDragOver(CurrentObject, e.GetPosition(AssociatedObject));
          //give mouse effect after the drop target has updated any location-specific validity
          SetDragDropEffects(e, true);
          //draw the adorner
          UpdateAdorner(e.GetPosition(AssociatedObject));
          e.Handled = true;
          break;
        case AcceptDrop.No:
          //give mouse effect
          SetDragDropEffects(e, false);
          e.Handled = false; //leave for parent
          break;
        case AcceptDrop.Ignore:
          e.Handled = false; //leave for parent 
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }


    protected virtual void OnDragLeave(object sender, DragEventArgs e)
    {
      Adorner?.Remove();
      DropTarget?.OnDragLeave();

      var canDrop = CanDrop(sender, e);
      if (canDrop == AcceptDrop.Ignore || canDrop == AcceptDrop.No)
        e.Handled = false; //leave for parent
      else
        e.Handled = true;

      CurrentDataType = null;
      CurrentObject = null;
    }

    #endregion Event Handlers

    protected virtual void SetDragDropEffects(DragEventArgs e, bool canDrop)
    {
      e.Effects = !canDrop || CurrentDataType == null
        ? DragDropEffects.None
        : DropTarget.GetRequiredEffect(CurrentObject);
    }

    protected virtual AcceptDrop CanDrop(object sender, DragEventArgs e)
    {
      if (CurrentDataType == null)
        return AcceptDrop.No;
      var data = e.Data.GetData(CurrentDataType);
      return data != null && DropTarget.CanDrop(data) ? AcceptDrop.Yes : AcceptDrop.No;
    }

    protected virtual void DoDrop(object sender, DragEventArgs e)
    {
      DropTarget.Drop(e.Data.GetData(CurrentDataType), e.GetPosition(AssociatedObject), 0);
      CurrentDataType = null;
      CurrentObject = null;
      e.Handled = true;
    }

    protected virtual void UpdateAdorner(Point mousePoint)
    {
      //Do nothing 
    }

  }
}
