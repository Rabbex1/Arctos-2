using System.Windows;

namespace Arctos.Wpf.Charts
{
    public class LinearTicks : TickSet
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Children.Clear();
            Ticks.Reset();

            if (Math.Abs(availableSize.Width) < 0.001 && Math.Abs(availableSize.Height) < 0.001)
                return availableSize;
            if (Owner?.VisualRange is not { } visualRange)
                return new Size();
            if (!visualRange.IsValid())
                return new Size();
            if(!IsAuto && visualRange.VisualDistanceAtRank(Rank) < MinimumSpacing)
                return new Size();


            var tick = Ticks.Next();
            tick.Measure(availableSize);
            var markWidth = tick.DesiredSize.Width;
            var markHeight = tick.DesiredSize.Height;
            var first = visualRange.PreviousAtRankAndFrequency(visualRange.Minimum ?? 0.0, Rank, CurrentFrequency);

            var count = 0;
            while (true)
            {
                if (!IsTickVisible(first, tick, availableSize))
                    first = visualRange.AdjustByRank(first, Rank, CurrentFrequency);
                else
                    break;
                //todo: this is a hack
                count++;
                if (count > 200)
                    break;
            }

            tick.Value = first;
            tick.SetCenter(visualRange.ValueToVisual(first), 0, Owner.Orientation);
            Children.Add(tick);

            var last = first;
            count = 0;
            while (true)
            {
                tick = Ticks.Next();
                tick.Measure(availableSize);

                var next = visualRange.AdjustByRank(last, Rank, CurrentFrequency);
                if (!IsTickVisible(next, tick, availableSize))
                {
                    Ticks.MovePrevious();
                    break;
                }

                tick.Value = next;
                tick.SetCenter(visualRange.ValueToVisual(next), 0, Owner.Orientation);
                Children.Add(tick);
                last = next;
                //todo: this is a hack
                count++;
                if (count > 200)
                    break;
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
