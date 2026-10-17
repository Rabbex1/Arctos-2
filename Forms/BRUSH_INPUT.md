# Arctos Forms brush input

`InputBrushVM` is a string-valued, transactional brush editor shared by the WPF and MudBlazor adapters. It is independent of any consuming application's theme model. `InputColorVM` remains unchanged for existing color-only consumers.

## Canonical string grammar

Whitespace is not significant around the complete value, but is not permitted inside grammar tokens. Parsing and formatting always use invariant culture.

```text
brush       = named | solid | linear | radial | angular
named       = "@" encoded-name
solid       = "#" RRGGBBAA
linear      = "linear(" x1 "," y1 "," x2 "," y2 ";" stops ")"
radial      = "radial(" cx "," cy "," rx "," ry ";" stops ")"
angular     = "angular(" cx "," cy "," start-deg "," end-deg ";" stops ")"
stops       = stop ";" stop *(";" stop)
stop        = offset ":" solid
```

- Colors are exactly eight hexadecimal digits in red, green, blue, alpha order. Canonical output uses uppercase digits.
- Numbers are finite invariant-culture decimals. Canonical output rounds to at most six fractional digits, removes trailing zeroes, and never emits an exponent.
- Stop offsets, linear endpoints and angular centres use `0..1`. Linear endpoints must differ. Radial centres may be any finite coordinates, including outside the unit brush area; radial radii must be finite and greater than zero, with no upper bound of one.
- Stops are non-decreasing by offset and a gradient has at least two stops. Colors outside the first and last stop use the nearest end color.
- Angular gradients run clockwise from 12 o'clock. `end-deg` must exceed `start-deg`; the sweep may not exceed 360 degrees. The default full sweep is `0,360`.
- Names are case-sensitive using ordinal comparison. Only URI-unreserved ASCII characters may appear directly; all other UTF-8 name characters are percent-encoded. Canonical output uses uppercase percent escapes. Palette names are stored decoded, for example the name `Surface/Primary Dark` is referenced as `@Surface%2FPrimary%20Dark`.

Examples:

```text
@Surface
#336699CC
linear(0,0.5,1,0.5;0:#FF0000FF;1:#0000FF80)
radial(0.5,0.5,0.5,0.5;0:#FFFFFFFF;1:#00000000)
angular(0.5,0.5,0,360;0:#FF0000FF;0.333333:#00FF00FF;0.666667:#0000FFFF;1:#FF0000FF)
```

Use `InputBrushFormat.TryParse`, `Parse`, `Format`, and `Canonicalize` when a UI is not involved.

## Palette and restrictions

A consumer supplies `InputBrushPaletteEntry` values containing a decoded, case-sensitive name and a current literal solid or gradient description. Palette entries cannot reference another named entry. This keeps resolution deterministic and produces clear errors for malformed values.

```csharp
var palette = new[]
{
    new InputBrushPaletteEntry("Surface", "#F7F8FAFF"),
    new InputBrushPaletteEntry("Hero", "linear(0,0,1,1;0:#DFF3FFFF;1:#FFE8CFFF)")
};

var input = new InputBrushVM(
    "Background",
    "@Surface",
    palette,
    InputBrushKinds.All,
    value => settings.Background = value);
```

Selecting a palette item commits its `@Name` reference rather than copying the resolved literal. Call `ReplacePalette` when a theme or application palette changes. Previews and validation refresh, while `Value` and its change callback remain untouched.

Restrictions combine flags. A color-only property that accepts named solid palette entries uses:

```csharp
InputBrushKinds.Named | InputBrushKinds.Solid
```

The named flag permits a reference; its resolved literal kind must also be permitted. Unknown names, malformed entries, nested references, and disallowed resolved kinds fail validation.

Inheritance is not a brush kind. Pass `allowClear: true` only when the consuming property has explicit clear/inherit semantics. The adapter then exposes a clear action which commits an empty string. Consumers should translate that explicit empty value into their own inheritance behavior.

## Editing and adapters

`BeginEdit` creates a local draft. `CommitEdit` canonicalizes and raises at most one value change; `CancelEdit` restores the original value. Invalid drafts cannot commit, and accepting an unchanged value raises no change. Both adapters use this state machine.

Click the compact field/swatches to open the fixed 580 × 520 modal editor. Its five tabs are Solid, Linear, Radial, Sweep, and Palette; disallowed modes are disabled without changing the layout. Solid colours open directly on a hue ring surrounding a hue/white/black triangle. Drag the ring to change hue, drag the triangle to mix its corner colours, or use the Alpha, Format (RGB/HSV/HSL), value, and Hex rows. Solid centres these rows beneath the selector and preview; gradients put them beneath the selector. Swatches and the right-hand brush preview use a rounded square for Solid, a square for Linear, and a circle for Radial and Sweep. Preview handles have a small outer margin to remain visible at the edges.

Only Accept and Cancel dismiss the modal. WPF has no window chrome and blocks Alt+F4; its slim grip strip above the tabs lets you drag the dialog without changing its size or interacting with the editing controls. Neither editor dismisses on Escape or outside clicks. Clear/inherit, where permitted, now changes the draft and requires Accept. Palette behaviour is retained pending its separate redesign.

Linear has Points/Angle controls, point selection and X/Y entry, or angle entry with draggable rotation endpoints. Endpoint points can reach the entire 0–1 square, including its corners; intermediate points project onto the gradient line. Rotation fits the segment to the square rather than an inscribed circle. Radial has Points/Centre controls with stop handles on a guide directed towards the farthest unit-circle edge; moving the centre adjusts the circular radius to that edge. Its centre can be outside the brush area. Sweep uses point selection and an absolute angle (clockwise from 12 o'clock) with handles around the circumference. Its overlapping 0°/360° handles are staggered radially so both can be selected. Point dropdowns include a colour dot, +, and delete; at least two stops are retained. Tab drafts are kept separately during the edit. All geometry and stop edits use the shared `BrushEditingModel`.

Radial Points and Centre both show a View range slider, sharing the same scale for the current editing session. At 1× the brush's unit circle fills the preview; at 4× its displayed radius is one-quarter of the view radius. A contrasting circle marks the actual brush boundary while the surrounding area shows the extended gradient. Zoom changes only the view, never the draft, geometry, or committed value. Dragging is constrained to the visible editing circle; X/Y accepts positions beyond it. On Centre, an off-screen centre is represented by an outward-pointing edge arrow (an indicator, not a draggable proxy). Increase the range or edit X/Y to reveal its normal handle. Off-screen point handles are hidden until brought into view; the point dropdown and numeric editing remain available.

The radial guide follows the gradient's actual radii, rather than stopping at the unit-circle boundary. Dragging any stop (including the first) beyond its existing end extends both radii proportionally and rescales the other stop offsets to preserve their physical positions. The centre stays fixed, elliptical proportions are preserved, and stops remain ordered along the guide. This works with the centre inside or outside the unit circle.

The default maximum view range is 4. Applications can share an ordinary options instance by passing it to each input; there is no static or dependency-injection requirement:

```csharp
var options = new BrushInputOptions { MaximumRadialViewRange = 8 };
var input = new InputBrushVM("Brush", "#FFFFFFFF", palette: null,
    allowedKinds: InputBrushKinds.All, onChange: value => { }, options: options);
```

This options class currently configures the radial view range; colour/font and palette settings have not been migrated into it. Both test harnesses demonstrate sharing the options instance between their inputs.

The application supplies editor colours independently of any theme:

```csharp
input.EditorBackground = new InputColor(38, 38, 42);
input.EditorForeground = new InputColor(240, 240, 240);
```

Use an opaque background and a contrasting foreground. Both harnesses now have a separate Dark mode switch that sets these colours; it does not replace or alter the palette.

The triangle picker adds no third-party dependencies. WPF uses `ColourTriangleSurface` and a DPI-sized `WriteableBitmap`; the web component uses a Canvas triangle, CSS conic-gradient ring, and HTML selection markers. Both use the same sRGB barycentric mixing equations (`ColourTriangle.cs`, mirrored in `wwwroot/colourTriangle.js`). Ring images are cached in WPF, and triangle pixels are regenerated only for hue or size/DPI changes, not selection-marker or alpha changes. Pointer capture keeps dragging active outside the triangle, clamping to its closest edge. Numeric channels use 0–255; hex accepts `#RRGGBB` (preserves alpha) or `#RRGGBBAA`.

Keyboard: in WPF, focus the wheel and use arrows to move the colour, Ctrl+arrows for hue, and Shift for larger colour steps. On the web, the ring and triangle are separate tab stops; arrows adjust the focused control and Shift increases the step. The web host must include its generated `.styles.css` bundle and serve the library's `_content/Arctos.Forms.MudBlazor/colourTriangle.js` static asset, as the harness does. Existing legacy `InputColorVM` controls are unchanged.

WPF supplies:

- the `InputBrushVM` data template in `Resources/CommonInputTemplates.xaml`;
- `BrushEditorWindow` for direct hosting;
- `BrushPreview`, which uses native WPF brushes for solid, linear, and radial values;
- a 256 × 256 maximum `WriteableBitmap` sweep renderer for angular values. Its bitmap, pixel buffer, and per-pixel angles are reused while size and center are unchanged. WPF's dispatcher coalesces repeated invalidations.

MudBlazor supplies `BrushInput`, also registered by `ArctosInputRenderer`. Its visual interpretation uses CSS `linear-gradient`, `radial-gradient`, and `conic-gradient`, while parsing, validation, stops, restrictions, and commit behavior remain in `Arctos.Forms`.

## Test harnesses

Open `Forms/Arctos.Forms.slnx` in Visual Studio 2026. The solution contains the Forms libraries, both harnesses, tests, and their required Arctos project dependencies.

Run the WPF desktop harness:

```powershell
dotnet run --project Forms/Arctos.Forms.Wpf.TestHarness/Arctos.Forms.Wpf.TestHarness.csproj
```

Run the MudBlazor web harness:

```powershell
dotnet run --project Forms/Arctos.Forms.MudBlazor.TestHarness/Arctos.Forms.MudBlazor.TestHarness.csproj
```

Both harnesses contain named, restricted solid-only, linear, radial, angular, and clearable examples. Their palette replacement button switches resolved values without recording a commit.
The WPF harness also has an angular-preview benchmark which renders 120 complete 256 × 256 preview frames through the real control and reports measured frames per second on the current machine.

Run automated contract tests with:

```powershell
dotnet test Forms/Arctos.Forms.Tests/Arctos.Forms.Tests.csproj
```
