namespace Arctos.Wpf.Inputs
{
    public class InputIntegerVM(string description, int value, Action<int>? onChange)
        : DataInputWrapperVM<int>(description, value, onChange)
    {
        public InputIntegerVM(string description, int value, Action<DataInputWrapperVM<int>, int> onChange)
            : this(description, value, (Action<int>?)null)
        {
            OnChangeWithSender = onChange;
        }
    }
}
