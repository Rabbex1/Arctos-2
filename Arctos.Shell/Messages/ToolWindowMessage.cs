using Arctos.Messenger;

namespace Arctos.Shell.Messages
{
    public enum ToolWindowAction
    {
        Open,
        Close,
        ActiveToolWindowChanged,
    }
    public class ToolWindowMessage : ActionMessage<ToolWindowAction>
    {
        public ToolWindowMessage(object sender, ToolWindowAction action, object[]? parameters = null)
            : base(sender, action, parameters)
        {
        }
    }
}
