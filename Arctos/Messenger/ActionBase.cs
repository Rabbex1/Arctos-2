namespace Arctos.Messenger
{
  public class ActionMessage<T>
  {
    public object Sender { get; }
    public T Action { get; }
    public object?[]? Params { get; }
    public bool Cancel { get; set; }

    public ActionMessage(object sender, T action, object?[]? parameters)
    {
        Sender = sender;
        Action = action;
        Params = parameters;
    }
  }
}
