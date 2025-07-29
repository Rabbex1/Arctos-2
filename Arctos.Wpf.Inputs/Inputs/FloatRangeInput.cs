using System.Xml.Serialization;

namespace Arctos.Wpf.Inputs
{
    public class FloatRangeInput : Input
    {
        #region Min
        private float _min;
        public float Min
        {
            get => _min;
            set => SetProperty(ref _min, value);
        }
        #endregion

        #region Max
        private float _max;
        public float Max
        {
            get => _max;
            set => SetProperty(ref _max, value);
        }
        #endregion

        #region Value
        private float _value;
        public float Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                Wrapper?.OnChange?.Invoke(this);
            }
        }
        #endregion

        [XmlIgnore]
        public InputFloatRangeVM? Wrapper { get; set; }
    }
}
