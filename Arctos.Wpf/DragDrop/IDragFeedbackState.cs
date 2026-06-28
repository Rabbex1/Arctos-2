using DragDropEffects = System.Windows.DragDropEffects;

namespace Arctos.Wpf.DragDrop
{
  /// <summary>
  /// Optional drag-state contract that allows drag sources and drop targets to override the displayed drag feedback effect.
  /// </summary>
  public interface IDragFeedbackState
  {
    DragDropEffects? DragFeedbackEffectOverride { get; set; }
  }
}
