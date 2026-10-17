namespace Arctos.Forms;

/// <summary>Application-owned picker options. Instances may be shared without static state.</summary>
public sealed class BrushInputOptions
{
    private double _maximumRadialViewRange = 4;

    /// <summary>Maximum zoom-out factor for the radial editing view; must be finite and at least one.</summary>
    public double MaximumRadialViewRange
    {
        get => _maximumRadialViewRange;
        set
        {
            if (!double.IsFinite(value) || value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "View range must be finite and at least one.");
            _maximumRadialViewRange = value;
        }
    }
}
