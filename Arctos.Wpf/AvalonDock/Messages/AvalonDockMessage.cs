using Arctos.Messenger;

namespace Arctos.Wpf.AvalonDock
{
  public enum AvalonDockAction
  {
    ShowDocument,
    HideDocument,
    DocumentFocused,
    AnchorablePaneLoaded,
    DocumentPaneLoaded
  }

  public class AvalonDockMessage : ActionMessage<AvalonDockAction>
  {
      public AvalonDockMessage(object sender, AvalonDockAction action, object[]? parameters) 
          : base(sender, action, parameters)
      {
      }
  }
}
