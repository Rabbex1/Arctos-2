using System.Reflection;

namespace Arctos.Wpf.Charts
{
    public class DataPoint
    {
        public double Value { get; set; }
        public double Angle { get; set; }
    }


    public class CosDataProvider : DataProvider
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly double _scale = 1.0;
        private readonly int _step = 5;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public override object? GetDataObject(string source, object value)
        {
            var angle = Math.PI / 180 * (double)value;
            return source != "Angle" ? null : new DataPoint { Angle = angle, Value = GetValue(angle) };
        }

        public override void ForAxisRange(Axis axis, Action<object> action)
        {
            var from = 0;
            var to = 360;

            for (double d = from; d <= to; d += _step)
            {
                var obj = new DataPoint { Angle = d, Value = GetValue(d) };
                action(obj);
            }
        }

        public override bool GetMinMax(string source, out object? min, out object? max)
        {
            if (source == "Time")
            {
                min = DateTime.MinValue;
                max = DateTime.MaxValue;
                return true;
            }
            if (source == "Value")
            {
                min = _scale * -1;
                max = _scale;
                return true;
            }
            min = null;
            max = null;
            return false;
        }

        public override bool GetRangeMinMax(string rangeSource, IRange range, string source, out object min, out object max)
        {
            //var timeRange = range as VisualTimeRange;
            //if (timeRange == null)
            //{
            //  min = null;
            //  max = null;
            //  return false;
            //}

            //var vMin = double.MaxValue;
            //var vMax = double.MinValue;
            //for (var d = timeRange.Minimum.RoundDown(TimeSpan.FromMinutes(_step)); d <= timeRange.Maximum.RoundUp(TimeSpan.FromMinutes(_step)); d = d.AddMinutes(_step))
            //{
            //  var value = GetValue(d);
            //  if (value < vMin) vMin = value;
            //  if (value > vMax) vMax = value;
            //}
            min = -_scale;
            max = _scale;
            return true;
        }

        public override object? LastAtOrBefore(object? value)
        {
            //if (!(value is double))
            //  throw new ArgumentException("The value must be of type DateTime");
            //return ((DateTime)value).RoundDown(TimeSpan.FromMinutes(_step));
            if (value == null)
                return null;
            return (double)value * -_step;
        }

        public override object? FirstAtOrAfter(object? value)
        {
            //if (!(value is DateTime))
            //  throw new ArgumentException("The value must be of type DateTime");
            //return ((DateTime)value).RoundUp(TimeSpan.FromMinutes(_step));
            if (value == null)
                return null;
            return (double)value * -_step;
        }

        public override PropertyInfo? PropertyInfo(string name)
        {
            var type = typeof(DataPoint);
            return type.GetProperty(name);
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private double GetValue(double a)
        {
            return Math.Cos(Math.PI / 180 * a) * _scale;
        }

        #endregion Private Methods

        #endregion Methods
    }
}