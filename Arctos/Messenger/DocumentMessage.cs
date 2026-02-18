namespace Arctos.Messenger
{
    public enum DocumentAction
    {
        Add,
        Open,
        Close,
        CloseAll,
        CloseAllButThis,
        CloseAllButActive,
        RequestCreation,
        DocumentAdded,
        DocumentRemoved,
        ActiveDocumentChanged,
        ActiveDocumentNameChanged
    }

    public class DocumentMessage(object sender, DocumentAction action, object?[]? parameters = null)
        : ActionMessage<DocumentAction>(sender, action, parameters);
}
