using System.Globalization;
using Xunit;

namespace Arctos.Forms.Tests;

public sealed class InputBrushFormatTests
{
    public static TheoryData<string, string> ValidDescriptions => new()
    {
        { "@Surface", "@Surface" },
        { "@Surface%2FPrimary%20Dark", "@Surface%2FPrimary%20Dark" },
        { "#0a10ff80", "#0A10FF80" },
        { "linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FF80)", "linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FF80)" },
        { "radial(0.5,0.5,0.75,0.25;0:#FFFFFFFF;1:#00000000)", "radial(0.5,0.5,0.75,0.25;0:#FFFFFFFF;1:#00000000)" },
        { "radial(-1,2,3,4;0:#FFFFFFFF;1:#00000000)", "radial(-1,2,3,4;0:#FFFFFFFF;1:#00000000)" },
        { "angular(0.5,0.5,-90,270;0:#FF0000FF;0.5:#00FF0080;1:#0000FFFF)", "angular(0.5,0.5,-90,270;0:#FF0000FF;0.5:#00FF0080;1:#0000FFFF)" }
    };

    [Theory]
    [MemberData(nameof(ValidDescriptions))]
    public void ParseAndFormatRoundTrips(string input, string canonical)
    {
        var parsed = InputBrushFormat.Parse(input);
        Assert.Equal(canonical, InputBrushFormat.Format(parsed));
        Assert.Equal(canonical, InputBrushFormat.Format(InputBrushFormat.Parse(canonical)));
    }

    [Fact]
    public void NamesAreCaseSensitiveAndEscaped()
    {
        var upper = Assert.IsType<InputNamedBrushDescription>(InputBrushFormat.Parse("@Surface"));
        var lower = Assert.IsType<InputNamedBrushDescription>(InputBrushFormat.Parse("@surface"));
        Assert.NotEqual(upper.Name, lower.Name);
        Assert.Equal("Surface/Primary Dark", Assert.IsType<InputNamedBrushDescription>(InputBrushFormat.Parse("@Surface%2FPrimary%20Dark")).Name);
        Assert.False(InputBrushFormat.TryParse("@Bad%2", out _, out _));
        Assert.False(InputBrushFormat.TryParse("@Bad/Raw", out _, out _));
    }

    [Fact]
    public void ColorUsesRgbaOrdering()
    {
        var solid = Assert.IsType<InputSolidBrushDescription>(InputBrushFormat.Parse("#11223344"));
        Assert.Equal(new InputColor(0x11, 0x22, 0x33, 0x44), solid.Color);
    }

    [Fact]
    public void ParsingIsInvariantCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.True(InputBrushFormat.TryParse("linear(0,0.5,1,0.5;0:#000000FF;1:#FFFFFFFF)", out _, out _));
            Assert.False(InputBrushFormat.TryParse("linear(0,0,5,1,0,5;0:#000000FF;1:#FFFFFFFF)", out _, out _));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("#123456")]
    [InlineData("linear(0,0,1,1;0:#000000FF)")]
    [InlineData("linear(0,0,1,1;0.8:#000000FF;0.2:#FFFFFFFF)")]
    [InlineData("radial(0.5,0.5,0,0.5;0:#000000FF;1:#FFFFFFFF)")]
    [InlineData("radial(NaN,0.5,1,1;0:#000000FF;1:#FFFFFFFF)")]
    [InlineData("radial(0.5,-1,Infinity,1;0:#000000FF;1:#FFFFFFFF)")]
    [InlineData("radial(0.5,-1,-2,1;0:#000000FF;1:#FFFFFFFF)")]
    [InlineData("angular(0.5,0.5,0,361;0:#000000FF;1:#FFFFFFFF)")]
    [InlineData("linear(NaN,0,1,1;0:#000000FF;1:#FFFFFFFF)")]
    public void InvalidSyntaxIsRejected(string value)
    {
        Assert.False(InputBrushFormat.TryParse(value, out _, out var error));
        Assert.False(string.IsNullOrWhiteSpace(error));
    }
}

public sealed class InputBrushViewModelTests
{
    [Fact]
    public void AppearanceChangesNotifyBindingsWithoutChangingTheBrush()
    {
        var commits = new List<string?>();
        var input = new InputBrushVM("Brush", "#FFFFFFFF", [], InputBrushKinds.All, commits.Add);
        var changes = new List<string?>();
        input.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
        input.EditorBackground = new(38, 38, 42);
        input.EditorForeground = new(240, 240, 240);
        input.EditorBackground = new(38, 38, 42);
        Assert.Equal(new[] { nameof(InputBrushVM.EditorBackground), nameof(InputBrushVM.EditorForeground) }, changes);
        Assert.Equal("#FFFFFFFF", input.Value);
        Assert.Empty(commits);
    }

    private static readonly InputBrushPaletteEntry[] Palette =
    [
        new("Surface", "#FFFFFFFF"),
        new("Hero", "linear(0,0,1,1;0:#000000FF;1:#FFFFFFFF)")
    ];

    [Fact]
    public void NamedReferenceIsPreservedAndPaletteCanRefreshWithoutCommit()
    {
        var commits = new List<string?>();
        var input = new InputBrushVM("Brush", "@Surface", Palette, InputBrushKinds.All, commits.Add);
        input.ReplacePalette([new("Surface", "#000000FF"), new("Hero", Palette[1].Description)]);

        Assert.Equal("@Surface", input.Value);
        Assert.Empty(commits);
        Assert.True(input.TryResolve(input.Value, out var resolved, out _));
        Assert.Equal(new InputColor(0, 0, 0), Assert.IsType<InputSolidBrushDescription>(resolved).Color);
    }

    [Fact]
    public void AcceptCommitsOnceAndUnchangedAcceptDoesNotCommit()
    {
        var commits = new List<string?>();
        var input = new InputBrushVM("Brush", "#000000FF", Palette, InputBrushKinds.All, commits.Add);
        input.BeginEdit();
        Assert.True(input.CommitEdit());
        Assert.Empty(commits);

        input.BeginEdit();
        input.DraftValue = "#ffffffff";
        Assert.True(input.CommitEdit());
        Assert.Equal(["#FFFFFFFF"], commits);
    }

    [Fact]
    public void CancelLeavesOriginalUnchanged()
    {
        var commits = new List<string?>();
        var input = new InputBrushVM("Brush", "#000000FF", Palette, InputBrushKinds.All, commits.Add);
        input.BeginEdit();
        input.DraftValue = "#FFFFFFFF";
        input.CancelEdit();
        Assert.Equal("#000000FF", input.Value);
        Assert.Empty(commits);
    }

    [Fact]
    public void UnknownMalformedAndDisallowedPaletteValuesFailClearly()
    {
        var solidOnly = new InputBrushVM("Brush", "#000000FF", Palette, InputBrushKinds.Named | InputBrushKinds.Solid, null);
        Assert.Contains("does not contain", solidOnly.Validate("@Missing").FirstError);
        Assert.Contains("disallowed", solidOnly.Validate("@Hero").FirstError);

        solidOnly.ReplacePalette([new("Broken", "linear(nope)")]);
        Assert.Contains("malformed", solidOnly.Validate("@Broken").FirstError);
        Assert.False(solidOnly.Validate("linear(0,0,1,1;0:#000000FF;1:#FFFFFFFF)").IsValid);
    }

    [Fact]
    public void InvalidDraftCannotCommit()
    {
        var commits = new List<string?>();
        var input = new InputBrushVM("Brush", "#000000FF", Palette, InputBrushKinds.All, commits.Add);
        input.BeginEdit();
        input.DraftValue = "@Unknown";
        Assert.False(input.CommitEdit());
        Assert.True(input.IsEditing);
        Assert.Empty(commits);
    }

    [Fact]
    public void ExplicitClearIsNotResolvedAsABrush()
    {
        var input = new InputBrushVM("Brush", "#000000FF", Palette, InputBrushKinds.All, null, allowClear: true);
        Assert.True(input.CommitClear());
        Assert.Equal(string.Empty, input.Value);
        Assert.False(input.TryResolve(input.Value, out var resolved, out _));
        Assert.Null(resolved);
    }
}
