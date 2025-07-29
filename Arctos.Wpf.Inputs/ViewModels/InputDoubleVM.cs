namespace Arctos.Wpf.Inputs
{
    public class InputDoubleVM(string description, double value, Action<double> onChange)
        : DataInputWrapperVM<double>(description, value, onChange)
    {
    }
}

