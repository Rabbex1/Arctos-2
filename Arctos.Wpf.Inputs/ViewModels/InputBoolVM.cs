namespace Arctos.Wpf.Inputs
{
    public class InputBoolVM : DataInputWrapperVM<bool>
    {
        public InputBoolVM(string description, bool value, Action<bool> onChange)
            : base(description, value, onChange)
        {
        }

        public InputBoolVM(string description, bool value, Action<DataInputWrapperVM<bool>, bool> onChange)
        {
            Description = description;
            OnChangeWithSender = onChange;
            _value = value;
        }
    }
}
