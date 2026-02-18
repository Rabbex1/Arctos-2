using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class DiscreteAxis : Axis
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string[]? Elements { get; private set; }

        private IDataPresenter? _dataPresenter;
        public override IDataPresenter? DataPresenter
        {
            get => _dataPresenter;
            set
            {
                _dataPresenter = value;
                
                if (_dataPresenter?.DataProvider is IDiscreteDataProvider provider)
                {
                    Elements = (from object? key in provider.Elements.Keys select key.ToString()).Where(e => e is not null).ToArray();
                }
            }
        }


        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constreuctor

        static DiscreteAxis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiscreteAxis), new FrameworkPropertyMetadata(typeof(DiscreteAxis)));
        }

        public DiscreteAxis()
        {
            VisualRange = new VisualDiscreteRange();
            DataRange = null;
        }

        protected override void AutoScaleTickSet(double length, TickSet hatching)
        {
            return;
        }

        #endregion Constreuctor

        protected override void SetMode(AxisMode mode)
        {
            // Discrete axis follows the same primary/auxiliary constraints as the base axis.
            base.SetMode(mode);
        }

        protected override void SetTickLabels(List<TickLabel> labels)
        {
            foreach (var tickSet in TickSets)
            {
                if (tickSet.Ticks.Count == 0)
                    return;
                tickSet.Ticks.ForEach(tick =>
                {
                    if (tickSet.IsLabeled)
                    {
                        if (labels.Any(l => l.Center == tick.Center))
                            return;
                        var index = (int)tick.Value;
                        labels.Add(new TickLabel
                        {
                            Text = Elements != null ? Elements[index] : "Unknown",
                            Center = tick.Center,
                            Foreground = tickSet.LabelForeground,
                            FontSize = tickSet.LabelFontSize,
                            TextAlignment = TextAlignment.Center,
                            Margin = new Thickness(4, 1, 4, 2),
                        });
                    }
                });
            }

        }

    }
}
