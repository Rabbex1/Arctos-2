using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Arctos.Wpf.Charts
{
    public class DiscreteTicks : TickSet
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Math.Abs(availableSize.Width) < 0.001 && Math.Abs(availableSize.Height) < 0.001)
                return availableSize;
            if (Owner is not DiscreteAxis owner)
                return availableSize;
            if (owner.VisualRange is not VisualDiscreteRange visualRange)
                return availableSize;
            if (!visualRange.IsValid())
                return new Size();
            Ticks.Reset();
            Children.Clear();

            var markWidth = 0.0;
            var markHeight = 0.0;

            if (owner.Elements != null)
            {
                visualRange.ElementCount = owner.Elements.Length;

                for (var i = 0; i < owner.Elements.Length; i++)
                {
                     var tick = Ticks.Next();
                    tick.Measure(availableSize);
                    if(i == 0)
                    {
                        markWidth = tick.DesiredSize.Width;
                        markHeight = tick.DesiredSize.Height;
                    }

                    tick.Value = i;
                    tick.SetCenter(visualRange.ValueToVisual(i), 0, Owner.Orientation);
                    Children.Add(tick);
                }
            }

            Ticks.Done();

            return Owner?.Orientation == PartOrientation.Horizontal
                ? new Size(availableSize.Width, markHeight)
                : new Size(markWidth, availableSize.Height);

        }

        protected override Size ArrangeOverride(Size totalSize)
        {
            if (HideTicks)
                return totalSize;
            foreach (Tick child in Children)
                child.Arrange(new Rect(child.Position, child.DesiredSize));
            return totalSize;
        }

    }
}

