namespace Arctos.Wpf.Inputs
{
    public class InputTimeSpanVM(string description, TimeSpan value, Action<TimeSpan> onChange)
        : DataInputWrapperVM<TimeSpan>(description, value, onChange)
    {
    }
}
