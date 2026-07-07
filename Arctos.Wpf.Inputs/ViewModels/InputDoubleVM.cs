namespace Arctos.Wpf.Inputs
{
    public class InputDoubleVM(string description, double value, Action<double> onChange)
        : DataInputWrapperVM<double>(description, value, onChange)
    {
        public InputDoubleVM(string description, double value, Action<DataInputWrapperVM<double>, double> onChange)
            : this(description, value, _ => { })
        {
            OnChangeWithSender = onChange;
            OnChange = null;
        }
    }
}
