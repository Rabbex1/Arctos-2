using Arctos.Messenger;

namespace Ursa.Messenger
{
  public enum CommonDialogResult
  {
    Ok,
    Cancel
  }
  public class CommonDialogResultMessage : ActionMessage<CommonDialogResult>
  {
    public CommonDialogResultMessage(object sender, CommonDialogResult action, object[]? parameters = null) 
        : base(sender, action, parameters)
    {
    }
  }
}
