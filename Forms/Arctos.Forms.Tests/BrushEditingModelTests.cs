using Xunit;

namespace Arctos.Forms.Tests;

public class BrushEditingModelTests
{
    private static InputBrushVM Input(string value) => new("Brush", value, [], InputBrushKinds.All, null);

    [Fact]
    public void SwitchingTabsPreservesEachDraftAndDoesNotCommit()
    {
        var input = Input("linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FFFF)");
        var original = input.Value;
        var model = new BrushEditingModel(input);
        model.SetColor(new(20, 30, 40));
        var linearDraft = input.DraftValue;
        model.SwitchMode("Radial"); model.SetTool("Centre"); model.MoveHandle("centre", .5, .2);
        model.SwitchMode("Linear");
        Assert.Equal(linearDraft, input.DraftValue);
        Assert.Equal(original, input.Value);
    }

    [Fact]
    public void RadialGuidePointsAwayFromMovedCentre()
    {
        var model = new BrushEditingModel(Input("radial(0.5,0.5,0.5,0.5;0:#FFFFFFFF;1:#000000FF)"));
        model.SetTool("Centre"); model.MoveHandle("centre", .5, .2);
        Assert.Equal(.5, model.GuideEnd.X, 6);
        Assert.Equal(1, model.GuideEnd.Y, 6);
    }

    [Fact]
    public void SweepPointAngleAndOffsetsStayInSync()
    {
        var model = new BrushEditingModel(Input("angular(0.5,0.5,0,360;0:#FF0000FF;1:#0000FFFF)"));
        model.Add(); model.Angle = 90;
        Assert.Equal(.25, model.Stops[model.Selected].Offset, 6);
        Assert.Equal(90, model.Angle, 6);
        Assert.Equal(3, model.Handles().Count);
    }

    [Fact]
    public void LinearAngleRotatesEndpointsAndKeepsThemInThePreview()
    {
        var model = new BrushEditingModel(Input("linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FFFF)"));
        model.SetTool("Angle"); model.Angle = 0;
        Assert.Equal(.5, model.X1, 6); Assert.Equal(.5, model.X2, 6);
        Assert.Equal(1, model.Y1, 6); Assert.Equal(0, model.Y2, 6);
    }

    [Fact]
    public void LinearEndpointsCanReachOppositeSquareCorners()
    {
        var model = new BrushEditingModel(Input("linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FFFF)"));
        model.MoveHandle("stop:0", 0, 0);
        model.MoveHandle("stop:1", 1, 1);
        Assert.Equal(0, model.X1); Assert.Equal(0, model.Y1);
        Assert.Equal(1, model.X2); Assert.Equal(1, model.Y2);
        Assert.True(model.Input.CanCommit);
    }

    [Fact]
    public void RotatingADiagonalDoesNotClampItToTheInscribedCircle()
    {
        var model = new BrushEditingModel(Input("linear(0,0,1,1;0:#FF0000FF;1:#0000FFFF)"));
        model.Angle = 45;
        Assert.Equal(0, model.X1, 6); Assert.Equal(1, model.Y1, 6);
        Assert.Equal(1, model.X2, 6); Assert.Equal(0, model.Y2, 6);
    }

    [Fact]
    public void LinearEndpointDraggingClampsToSquareEdges()
    {
        var model = new BrushEditingModel(Input("linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FFFF)"));
        model.MoveHandle("stop:0", -2, 2);
        Assert.Equal(0, model.X1); Assert.Equal(1, model.Y1);
    }

    [Fact]
    public void RadialViewRangeIsSharedByToolsAndDoesNotChangeTheDraft()
    {
        var input = Input("radial(0.5,-1,2,2;0:#FFFFFFFF;1:#000000FF)");
        var model = new BrushEditingModel(input);
        var draft = input.DraftValue;
        model.SetTool("Centre"); model.ViewRange = 4;
        model.SetTool("Points");
        Assert.Equal(4, model.ViewRange);
        Assert.Equal(draft, input.DraftValue);
        Assert.Equal(draft, input.Value);
        var preview = Assert.IsType<InputRadialGradientBrushDescription>(model.PreviewDescription());
        Assert.Equal(.125, preview.CenterY, 6);
        Assert.Equal(.5, preview.RadiusX, 6);
    }

    [Fact]
    public void RadialCentreArrowDisappearsWhenViewRevealsTheCentre()
    {
        var model = new BrushEditingModel(Input("radial(0.5,-1,2,2;0:#FFFFFFFF;1:#000000FF)"));
        model.SetTool("Centre");
        var arrow = Assert.Single(model.PreviewHandles());
        Assert.True(arrow.Offscreen);
        Assert.Equal(.5, arrow.X, 6); Assert.Equal(0, arrow.Y, 6);
        model.ViewRange = 4;
        var handle = Assert.Single(model.PreviewHandles());
        Assert.False(handle.Offscreen);
        Assert.Equal(.125, handle.Y, 6);
        model.ViewRange = 1;
        Assert.Equal(-1, model.Y1);
        Assert.True(Assert.Single(model.PreviewHandles()).Offscreen);
    }

    [Fact]
    public void ScaledCentreDraggingSavesOutsideCoordinatesAndCanBeCancelled()
    {
        var input = Input("radial(0.5,0.5,0.5,0.5;0:#FFFFFFFF;1:#000000FF)");
        var original = input.Value;
        var model = new BrushEditingModel(input);
        model.SetTool("Centre"); model.ViewRange = 4;
        model.MovePreviewHandle("centre", .5, .125);
        Assert.Equal(-1, model.Y1, 6);
        Assert.Equal(2, model.X2, 6);
        Assert.True(input.CanCommit);
        Assert.Equal(original, input.Value);
        input.CancelEdit(); Assert.Equal(original, input.DraftValue);
    }

    [Fact]
    public void RadialPointsOutsideUnitCircleAreVisibleAndDraggableWhenZoomedOut()
    {
        var input = Input("radial(0.5,-1,2,2;0:#FFFFFFFF;0.25:#FF0000FF;1:#000000FF)");
        var model = new BrushEditingModel(input) { ViewRange = 4 };
        Assert.Equal(3, model.PreviewHandles().Count);
        var point = model.PreviewHandles()[1];
        Assert.Equal(.25, point.Y, 6); // Brush Y=-0.5 lies outside the unit circle.
        model.MovePreviewHandle("stop:1", .5, .3125); // Brush Y=-0.25.
        Assert.Equal(.375, model.Stops[model.Selected].Offset, 6);
        Assert.True(model.Accept());
        var reopened = new BrushEditingModel(input);
        Assert.Equal(-1, reopened.Y1);
        Assert.Equal(.375, reopened.Stops[1].Offset, 6);
    }

    [Fact]
    public void PreviewGuideRemainsFiniteForEveryMode()
    {
        var model = new BrushEditingModel(Input("#FFFFFFFF"));
        foreach (var mode in BrushEditingModel.Tabs)
        {
            model.SwitchMode(mode);
            Assert.True(double.IsFinite(model.GuideEnd.X));
            Assert.True(double.IsFinite(model.GuideEnd.Y));
        }
    }

    [Fact]
    public void RadialGuideUsesActualEllipseRadii()
    {
        var model = new BrushEditingModel(Input("radial(0.5,0.2,2,1.5;0:#FFFFFFFF;1:#000000FF)"));
        Assert.Equal(.5, model.GuideEnd.X, 6);
        Assert.Equal(1.7, model.GuideEnd.Y, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AnyRadialStopCanExtendBeyondUnitCircleWithCentreInside(int index)
    {
        var input = Input("radial(0.5,0.5,0.5,0.5;0:#FFFFFFFF;0.5:#FF0000FF;1:#000000FF)");
        var model = new BrushEditingModel(input) { ViewRange = 4 };
        var original = input.Value;
        var others = model.Stops.Where((_, i) => i != index).Select(s => (Stop: s, Radius: s.Offset * model.X2)).ToArray();
        var selected = model.Stops[index];
        model.MovePreviewHandle($"stop:{index}", .875, .5); // Brush X=2, beyond unit-circle X=1.
        Assert.Same(selected, model.Stops[model.Selected]);
        Assert.Equal(2, model.PointPosition.X, 6);
        Assert.Equal(1.5, model.X2, 6);
        Assert.Equal(.5, model.X1, 6);
        foreach (var other in others) Assert.Equal(other.Radius, other.Stop.Offset * model.X2, 6);
        Assert.Equal(original, input.Value);
        Assert.True(model.Accept());
        var reopened = new BrushEditingModel(input);
        Assert.Equal(2, reopened.Handles()[^1].X, 6);
    }

    [Fact]
    public void FirstRadialStopCanPassFarEdgeWithCentreOutside()
    {
        var model = new BrushEditingModel(Input("radial(0.5,-1,2,2;0:#FFFFFFFF;1:#000000FF)")) { ViewRange = 4 };
        var first = model.Stops[0]; var other = model.Stops[1];
        model.MovePreviewHandle("stop:0", .5, .875); // Brush Y=2, past the previous guide end Y=1.
        Assert.Same(first, model.Stops[model.Selected]);
        Assert.Equal(2, model.PointPosition.Y, 6);
        Assert.Equal(-1, model.Y1, 6);
        Assert.Equal(3, model.Y2, 6);
        Assert.Equal(1, model.Y1 + other.Offset * model.Y2, 6);
        Assert.True(model.Input.CanCommit);
    }

    [Fact]
    public void ExpandingEllipticalRadiusPreservesAspectRatio()
    {
        var model = new BrushEditingModel(Input("radial(0.5,0.5,0.5,0.25;0:#FFFFFFFF;1:#000000FF)")) { ViewRange = 4 };
        model.MovePreviewHandle("stop:1", .875, .5);
        Assert.Equal(1.5, model.X2, 6);
        Assert.Equal(.75, model.Y2, 6);
    }

    [Fact]
    public void ApplicationOptionsControlRangeWithoutStaticState()
    {
        var options = new BrushInputOptions { MaximumRadialViewRange = 8 };
        var input = new InputBrushVM("Brush", "#FFFFFFFF", [], InputBrushKinds.All, null, options: options);
        var model = new BrushEditingModel(input) { ViewRange = 10 };
        Assert.Same(options, input.Options);
        Assert.Equal(8, model.ViewRange);
        Assert.Equal(4, new BrushInputOptions().MaximumRadialViewRange);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaximumRadialViewRange = .5);
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaximumRadialViewRange = double.NaN);
    }

    [Fact]
    public void DeletePreservesMinimumTwoPoints()
    {
        var model = new BrushEditingModel(Input("linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FFFF)"));
        model.Remove(); Assert.Equal(2, model.Stops.Count);
        model.Add(); Assert.Equal(3, model.Stops.Count);
        model.Remove(); Assert.Equal(2, model.Stops.Count);
    }
}
