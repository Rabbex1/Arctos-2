namespace Arctos.Wpf.Inputs
{
    public class InputTimeSpanVM(string description, TimeSpan value, Action<TimeSpan> onChange)
        : DataInputWrapperVM<TimeSpan>(description, value, onChange)
    {
        public InputTimeSpanVM(string description, TimeSpan value, Action<DataInputWrapperVM<TimeSpan>, TimeSpan> onChange)
            : this(description, value, _ => { })
        {
            OnChangeWithSender = onChange;
            OnChange = null;
        }
    }
}
