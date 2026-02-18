using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Messenger;

public static class MessengerCompatExtensions
{
    public static void RegisterRecipient<TMessage>(this IMessenger messenger, IRecipient<TMessage> recipient)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient);
    }

    public static void RegisterRecipient<TMessage>(this IMessenger messenger, object recipient, Action<TMessage> action)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, (_, message) => action(message));
    }

    public static TMessage SendMessage<TMessage>(this IMessenger messenger, TMessage message)
        where TMessage : class
    {
        return messenger.Send(message);
    }

    public static void UnregisterRecipient<TMessage>(this IMessenger messenger, object recipient)
        where TMessage : class
    {
        messenger.Unregister<TMessage>(recipient);
    }
}
