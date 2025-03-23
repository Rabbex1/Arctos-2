using Arctos.Messenger;

namespace Arctos.Shell.Messages
{
    public enum DocumentAction
    {
        Add,
        Open,
        Close,
        CloseAll,
        CloseAllButActive,
        RequestCreation,
        DocumentAdded,
        DocumentRemoved,
        ActiveDocumentChanged,
        ActiveDocumentNameChanged
    }

    public class DocumentMessage : ActionMessage<DocumentAction>
    {
        public DocumentMessage(object sender, DocumentAction action, object?[]? parameters = null)
            : base(sender, action, parameters)
        {
        }
    }
}
