using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class NumericAxis : Axis
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region ShowZero
        public static readonly DependencyProperty ShowZeroProperty = DependencyProperty.Register
        (
          nameof(ShowZero),
          typeof(bool),
          typeof(NumericAxis),
          new PropertyMetadata(default(bool), OnShowZeroChanged)
        );

        public bool ShowZero
        {
            get => (bool)GetValue(ShowZeroProperty);
            set => SetValue(ShowZeroProperty, value);
        }

        private static void OnShowZeroChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constreuctor

        static NumericAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericAxis), new FrameworkPropertyMetadata(typeof(NumericAxis)));
        }

        public NumericAxis()
        {
            VisualRange = new VisualNumericRange();
            DataRange = new Range<double>();
        }

        #endregion Constreuctor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        protected override void AutoScaleTickSet(double length, TickSet hatching)
        {
            if (VisualRange == null)
                return;

            var range = Math.Abs((double)(VisualRange.Maximum ?? 0.0) - (double)(VisualRange.Minimum ?? 0.0));
            if (double.IsInfinity(range))
                return;
            if (range == 0)
                return;

            double maxCount = length / hatching.MinimumSpacing;

            double Exponent(double x) => Math.Ceiling(Math.Log(x, 10));
            double Mantissa(double x) => x / Math.Pow(10, Exponent(x) - 1);

            double interval = Math.Pow(10, Exponent(range));
            double tempInterval = interval;

            // decrease interval until interval count becomes less than maxIntervalCount
            while (true)
            {
                int mantissa = (int)Mantissa(tempInterval);
                if (mantissa == 5)
                {
                    // reduce 5 to 2
                    tempInterval /= 2.5;
                }
                else if (mantissa is 2 or 1 or 10)
                {
                    // reduce 2 to 1,10 to 5,1 to 0.5
                    tempInterval /= 2.0;
                }
                if (range / tempInterval > maxCount)
                {
                    break;
                }
                interval = tempInterval;
            }

            var pow = Math.Floor(Math.Log10(Math.Abs(interval)));

            hatching.Rank = (int)pow;
            hatching.CurrentFrequency = hatching.Frequency * (int)(interval / Math.Pow(10, pow));
            if (pow < 0)
                hatching.CurrentLabelFormat = "F" + (int)pow * -1;
        }

        #endregion Methods
    }
}
