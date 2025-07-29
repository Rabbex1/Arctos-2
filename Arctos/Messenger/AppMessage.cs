namespace Arctos.Messenger
{
    public enum AppAction
    {
        AppLoaded,
        AppExit,
        RequestCloseApp,
        AppClosing,
        ShowDialog,
        ShowCommonDialog,
        Waiting,
        SelectFile,
        SelectFolder,
    }

    public class AppMessage(object sender, AppAction action, object[]? parameters = null)
        : ActionMessage<AppAction>(sender, action, parameters);
}