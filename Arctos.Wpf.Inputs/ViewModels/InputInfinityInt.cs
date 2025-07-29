namespace Arctos.Wpf.Inputs
{
    public class InputInfinityIntVM : DataInputWrapperVM<int>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        #region StringValue
        private string _stringValue = string.Empty;
        public string StringValue
        {
            get => _stringValue;
            set
            {
                if (_stringValue == value) return;
                switch (value)
                {
                    case "+":
                    case "-":
                        _stringValue = value;
                        break;
                    case "i":
                    case "+i":
                        _stringValue = "+\u221E";
                        Value = int.MaxValue;
                        break;
                    case "-i":
                        _stringValue = "-\u221E";
                        Value = int.MinValue;
                        break;
                    default:
                        var isValid = int.TryParse(value, out var val);
                        if (isValid)
                            Value = val;
                        _stringValue = value;
                        break;
                }
                SetProperty(ref _stringValue, value);
            }
        }
        #endregion

       #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors


        public InputInfinityIntVM(string description, int value, Action<int> onChange)
            : base(description, value, onChange)
        {
            StringValue = value switch
            {
                int.MaxValue => "+i",
                int.MinValue => "-i",
                _ => value.ToString()
            };
        }

        #endregion Constructors
    }
}