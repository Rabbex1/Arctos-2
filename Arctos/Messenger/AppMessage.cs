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

    public class AppMessage : ActionMessage<AppAction>
    {
        public AppMessage(object sender, AppAction action, object[]? parameters = null)
            : base(sender, action, parameters)
        {
        }
    }
}