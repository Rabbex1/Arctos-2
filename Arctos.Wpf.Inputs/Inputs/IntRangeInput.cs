using System.Xml.Serialization;

namespace Arctos.Wpf.Inputs
{
    public class IntRangeInput : Input
    {
        #region Min
        private int _min;
        public int Min
        {
            get => _min;
            set
            {
                if (!SetProperty(ref _min, value))
                    return;

                if (_max < _min)
                    _max = _min;

                if (_value < _min)
                    _value = _min;

                OnPropertyChanged(nameof(Max));
                OnPropertyChanged(nameof(Value));
                Wrapper?.OnChange?.Invoke(this);
            }
        }
        #endregion

        #region Max
        private int _max;
        public int Max
        {
            get => _max;
            set
            {
                var max = Math.Max(value, Min);
                if (!SetProperty(ref _max, max))
                    return;

                if (_value > _max)
                {
                    _value = _max;
                    OnPropertyChanged(nameof(Value));
                }

                Wrapper?.OnChange?.Invoke(this);
            }
        }
        #endregion

        #region Value
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                var clamped = Math.Clamp(value, Min, Max);
                if (!SetProperty(ref _value, clamped))
                    return;

                Wrapper?.OnChange?.Invoke(this);
            }
        }
        #endregion

        [XmlIgnore]
        public InputIntRangeVM? Wrapper { get; set; }
    }
}
