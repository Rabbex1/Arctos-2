using System;

namespace Arctos.Wpf.Charts
{
    public class TimeAxis : Axis
    {
        public TimeAxis()
        {
            VisualRange = new VisualTimeRange();
            DataRange = new Range<DateTime>();
        }

        protected override void AutoScaleHatching(double length, TickSet tickSet)
        {
            if (VisualRange == null)
                return;
            var timeSpan = ((DateTime)(VisualRange.Maximum ?? DateTime.Now)).Subtract((DateTime)(VisualRange.Minimum ?? DateTime.Now));
            if (timeSpan == TimeSpan.Zero)
                return;
            double maxCount = length / tickSet.MinimumSpacing;

            timeSpan = new TimeSpan((long)(timeSpan.Ticks / maxCount));

            double interval = timeSpan.TotalSeconds;
            if (MinuteSecondRoundUp(interval) < 60)
            {
                tickSet.Rank = TimeRank.Seconds;
                tickSet.CurrentFrequency = MinuteSecondRoundUp(interval);
                SettLabelFormat(tickSet, "HH:mm:ss");
                return;
            }

            interval /= 60.0; // as minutes
            if (MinuteSecondRoundUp(interval) < 60)
            {
                tickSet.Rank = TimeRank.Minutes;
                tickSet.CurrentFrequency = MinuteSecondRoundUp(interval);
                SettLabelFormat(tickSet, "HH:mm");
                return;
            }

            interval /= 60.0; // as hours
            if (HourRoundUp(interval) < 24)
            {
                tickSet.Rank = TimeRank.Hours;
                tickSet.CurrentFrequency = HourRoundUp(interval);
                SettLabelFormat(tickSet, "HH:mm");
                return;
            }

            interval /= 24.0; // as days
            if (DayRoundUp(interval) < 7)
            {
                tickSet.Rank = TimeRank.Days;
                tickSet.CurrentFrequency = HourRoundUp(interval);
                SettLabelFormat(tickSet, "dd");
                return;
            }

            interval /= 7.0; // as weeks
            if (DayRoundUp(interval) < 8)
            {
                tickSet.Rank = TimeRank.Weeks;
                tickSet.CurrentFrequency = DayRoundUp(interval);
                SettLabelFormat(tickSet, "");
                return;
            }


        }

        private void SettLabelFormat(TickSet tickSet, string format)
        {
            var baseFormat = tickSet.LabelFormat;
            tickSet.CurrentLabelFormat = format;

            if (baseFormat == null || !baseFormat.Contains('|'))
                return;
            tickSet.CurrentLabelFormat += baseFormat[baseFormat.IndexOf('|')..];
        }

        private int MinuteSecondRoundUp(double timeSpan)
        {
            if (timeSpan <= 1) return 1;
            if (timeSpan <= 2) return 2;
            if (timeSpan <= 5) return 5;
            if (timeSpan <= 15) return 15;
            return timeSpan <= 30 ? 30 : 60;
        }
            //if (timeSpan <= 10) return 10;
            //if (timeSpan <= 20) return 20;

        private int HourRoundUp(double timeSpan)
        {
            if (timeSpan <= 1) return 1;
            if (timeSpan <= 2) return 2;
            if (timeSpan <= 4) return 4;
            if (timeSpan <= 6) return 6;
            if (timeSpan <= 8) return 8;
            return timeSpan <= 12 ? 12 : 24;
        }

        private int DayRoundUp(double timeSpan)
        {
            if (timeSpan <= 1) return 1;
            if (timeSpan <= 2) return 2;
            return timeSpan <= 5 ? 5 : 7;
        }
    }
}
