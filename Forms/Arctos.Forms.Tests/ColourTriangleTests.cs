using Xunit;

namespace Arctos.Forms.Tests;

public class ColourTriangleTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(38)]
    [InlineData(120)]
    [InlineData(240)]
    [InlineData(359)]
    public void CornersProduceHueWhiteAndBlack(double hue)
    {
        var pure = ColourTriangle.PureHue(hue);
        Assert.Equal(pure, ColourTriangle.Mix(pure, ColourTriangle.GetWeights(ColourTriangle.Vertex(hue), hue)));
        Assert.Equal(new InputColor(255, 255, 255), ColourTriangle.Mix(pure, ColourTriangle.GetWeights(ColourTriangle.Vertex(hue, 120), hue)));
        Assert.Equal(new InputColor(0, 0, 0), ColourTriangle.Mix(pure, ColourTriangle.GetWeights(ColourTriangle.Vertex(hue, 240), hue)));
    }

    [Theory]
    [InlineData(36, 87, 214, 128)]
    [InlineData(255, 0, 0, 0)]
    [InlineData(84, 100, 142, 255)]
    [InlineData(128, 128, 128, 77)]
    [InlineData(0, 0, 0, 255)]
    [InlineData(255, 255, 255, 255)]
    public void ColourAndPositionRoundTrip(int r, int g, int b, int a)
    {
        var color = new InputColor((byte)r, (byte)g, (byte)b, (byte)a);
        var (hue, weights) = ColourTriangle.FromColor(color, 38);
        var p = ColourTriangle.Position(hue, weights);
        var result = ColourTriangle.Mix(ColourTriangle.PureHue(hue), ColourTriangle.GetWeights(p, hue), color.Alpha);
        Assert.InRange(Math.Abs(result.Red - r), 0, 1);
        Assert.InRange(Math.Abs(result.Green - g), 0, 1);
        Assert.InRange(Math.Abs(result.Blue - b), 0, 1);
        Assert.Equal(color.Alpha, result.Alpha);
        if (r == g && g == b) Assert.Equal(38, hue);
    }

    [Fact]
    public void OutOfBoundsDragClampsToAnEdge()
    {
        var weights = ColourTriangle.ClampToTriangle(new(-1, 2), 38);
        Assert.True(ColourTriangle.Contains(weights));
        Assert.Equal(1, weights.Hue + weights.White + weights.Black, 10);
        Assert.InRange(Math.Min(weights.Hue, Math.Min(weights.White, weights.Black)), 0, 1e-10);
    }
}
