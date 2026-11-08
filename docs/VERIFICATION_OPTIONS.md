# Options menu verification

Date: 2026-10-31.

Follow-up: [2026-11-05 dual bindings](VERIFICATION_DUAL_BINDINGS.md) adds movement
rebinding, primary/alternate columns and the Pause / Back label/behavior. The
original evidence below describes the earlier two-action menu.

Implemented application preferences, a four-category options menu on title/pause,
display modes and resolution presets through 3840x2160, display confirmation and
rollback, UI/text scaling, audio preview sliders, focus pause/mute, dialogue speed
preview, and rebinding for the existing Interact/Pause actions. Manual resizing is
disabled. Settings are independent of gameplay saves; no gameplay save format or
dependencies changed. Existing uncommitted agents.md work was retained.

## Automated evidence

- Baseline `dotnet build PawnShopSimulator.sln --no-restore`: passed, zero warnings/errors.
- Baseline `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: 10/10 passed.
- Implementation build: passed, zero warnings/errors.
- Implementation regression runner: 16/16 passed. Added checks cover title/pause
  input capture and session preservation; display confirmation, cancellation,
  timeout and focus rollback; isolated temporary settings round trips, backups,
  malformed/unsupported data preservation; key release and duplicate bindings;
  category audio gain and focus mute; dialogue reveal rates; configurable focus
  pause; and failed-write rollback.
- Final build after retaining the original desktop dimensions across fullscreen
  changes: `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/OptionsVerification/`
  passed with zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/OptionsVerification/`
  passed 16/16. Separate ignored output avoided disturbing the launched executable.
- `git diff --check` passed; Git reported only LF/CRLF conversion notices.
- Normal C# compilation builds the preview tone generator and UI; existing PNG
  content is copied by the existing workflow. No new asset compiler is required.

## Desktop evidence and limits

Launched the rebuilt executable with the computer-use skill. Screenshot inspection
confirmed the title's enabled Options entry. Native accessibility inspection
confirmed the Maximize button is disabled. Automated confirm-key attempts did not
produce an observable transition. The user then stopped Computer Use with physical
Escape, and all desktop automation stopped.

The options screen layout, 100/125/150 percent readability, resolution/mode switching,
physical edge-drag behavior, audible previews, runtime rebinding, and settings reload
remain manual acceptance checks. Actual 4K output requires a suitable display and
has not been verified. Headless preview tests do not establish graphics-driver
rollback behavior. No complete M01 desktop/resource acceptance is claimed.

## Pixel-art frame and scrolling foundation ? 2026-11-07

The outer frame now uses PixelLab's dedicated `create_ui_asset` output, generated
with the user's decorative reference as a style image: amber wood, brass curls,
ivy, and a quiet dark inset. Source PNGs and generation IDs/hashes/slice bounds
are recorded in `PawnShop.Game/Content/UI/provenance.json`. The aesthetic applies
only to the outside box. Earlier button/tab/track/surface artwork is provisional;
the user will design controls separately. No asset is visually approved.

Fixed tabs, clipped independently scrolling fields/help, and fixed Apply / Restore
Defaults / Back use shared layout geometry. Wheel input and draggable scrollbar
change a bounded offset. Keyboard navigation reveals rows; tabs reset scroll.
The renderer flushes batches before/after a GPU scissor on the 1280x720 target.
Corners use fixed pixel scale and edges repeat; all Options sampling is point.
Settings values, previews, confirmation, binding capture and persistence retain
the existing command paths. No dependencies or save formats changed.

Baseline build: zero warnings/errors; 57/57 tests passed. The expanded headless
suite passed 58/58, including all tabs at 100/125/150%, scroll clamps and tab reset,
keyboard reveal, clipped hit boundaries, fixed footer, wheel focus suppression,
scrollbar dragging and scrolled pointer mapping at 960x540, 1280x720, 2560x1440
and 3440x1440. Existing settings and lifecycle checks also pass.

Commands (separate ignored output avoids touching a running executable):

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/UiVerification/
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/UiVerification/
git diff --check
```

An earlier draft was launched and one Options screenshot was inspected before
the user instructed the agent to stop. All desktop interaction stopped. The final
reference-inspired frame has not been inspected in-game. Final composition,
readability, input feel, display changes and resolution/scale acceptance remain
for the user. Headless checks establish geometry and behavior, not appearance.
Fractional presentation scales use nearest-neighbor sampling but cannot guarantee
a uniform physical pixel grid. No further game launches or screenshots are needed
for this handoff.

### Panel opacity follow-up

The frame's three authored dark inset colors now render at 75 percent opacity.
Wood, brass, foliage, controls and text remain opaque. The source PNG is preserved;
opacity is applied to the loaded texture once. Removed the opaque parchment and
socket backgrounds from the scrolling viewport so stacked surfaces do not hide
the scenery. Content text now uses cream with a one-pixel dark shadow for contrast.
The display-confirmation prompt retains its opaque paper surface for readability.
Build and headless checks were rerun; in-game appearance remains for the user.

### Background composition correction

Pause-side Options previously had no room drawing callback and used an opaque
canvas clear. It now composites over a freshly drawn, frozen room; title-side
Options continues to composite over the panorama. Options always clears its own
canvas transparently. Frame opacity and premultiplication now run on decoded
RGBA bytes before GPU upload rather than through texture GetData/SetData.
All PNG controls are also premultiplied before AlphaBlend rendering.

A headless check decodes the actual frame and verifies every pixel in its central
160x90 area uploads with alpha 191/255, with premultiplied RGB; opaque decoration
is unchanged and transparent pixels contain zero RGB. It does not assert GPU or
in-game appearance. Standard Debug build (without an alternate OutputPath) passed
with zero warnings/errors, and the rebuilt regression runner passed 59/59.
No game launch, control or screenshot inspection was performed for this fix.

### User screenshot opacity correction

The user-supplied screenshot showed that the 10% fill was too faint against the
sunset. Set the inset to 75% opacity (25% see-through) to retain the dark panel
surface. The pixel regression now checks alpha 191/255 and corresponding
premultiplied RGB. Borders and controls retain their opacity. The screenshot was
supplied by the user; no game interaction or screenshot capture was performed.

### Separate tab header

Added the reference's distinct upper wooden section, using grain and a straight
amber/brass rail sampled from the existing generated PixelLab artwork. Display,
Audio, Comfort and Controls now sit centered in that fixed section. Shared tab
hit bounds moved with the artwork. The Options heading is omitted while the user
decides its placement. Existing button designs remain provisional; content and
footer geometry, 75% inset opacity and settings behavior are retained. Geometry
checks require each tab inside the header and the divider above the viewport.
No game launch or desktop inspection was performed.

Header verification: normal Debug output was locked by the running game. Separate bin/UiVerification build passed with zero warnings/errors; its rebuilt regression runner passed 59/59. Close the game and rebuild normal output before reviewing there. No running process was controlled or stopped.

### Complete source panel replacement

At the user's request, the active panel is now a byte-identical copy of their
1586x992 source image: `Content/UI/options-panel-source.png`. SHA-256:
`77d11e44f365b1998822110ccb5c2ac17ffe3e0ed39a7cbfcb33a73f4c0ad94c`.
It draws as one complete image with point sampling and aspect ratio preserved to
integer rounding. Removed assembled wood/header/divider rendering and the custom
opacity processor. The source's original fill and alpha now apply. Prior source
artwork is preserved and marked superseded in provenance. Controls remain
provisional, not regenerated or redesigned.

Tabs are fitted to the source header; scrolling content, scrollbar, messages and
fixed actions are inset into the narrower source body. Shared mouse geometry
matches. The Options heading remains omitted. The display-confirmation overlay
uses the same complete image and preserves existing confirmation semantics.
The obsolete palette-specific opacity test is replaced by source dimensions,
aspect ratio and contained layout checks; provenance tests verify packaged bytes.
No game launch, control or screenshots were used. Visual acceptance is pending.

Source-panel verification: standard Debug solution build passed with zero warnings/errors; rebuilt runner passed 59/59; git diff --check passed. Source copy hash matches the supplied image. In-game appearance remains unverified.

### Walnut category-tab artwork only

Replaced only Display / Audio / Comfort / Controls artwork with four states
generated by PixelLab's UI-specific `create_ui_asset` tool, ID
`e9814ebf-f1b1-42c7-a0e9-07d1bd7ac23b`, seed 71601. The existing panel image
was provided as the material/style reference. A four-button shape template
produced inactive dark walnut/muted brass, selected honey/brighter brass, warm
hover and recessed pressed states without text or extra ornaments.

The unmodified 256x256 generator output is retained in
`Content/UI/Sources/walnut-tab-states.png`. PixelLab varied the four generated
bounds slightly. Native-pixel nine-slice preparation preserved six-pixel corners
and repeated/cropped edge and center pixels into four aligned 110x28 RGBA PNGs:
`tab-inactive.png`, `tab-selected.png`, `tab-hovered.png`, `tab-pressed.png`.
All four have alpha bounds (0,0)-(109,27), transparent corners and the same
canvas/origin. No interpolation or painted replacement art was used. Original
bounds, source ID and file hashes are recorded in `Content/UI/provenance.json`.

The existing Button drawing method selects these assets only for category tabs,
with pressed > hovered > selected > inactive priority. Each draws at exactly 2x
inside the unchanged 220x56 tab rectangles with point sampling. Existing font,
labels, centering, pressed text offset and semantic input paths are retained.
Footer buttons, values, bindings, sliders, scrollbar, panel and layout are untouched.
No new UI controls, architecture, generation scripts or dependencies were added.
Static generated sprites were inspected; in-game acceptance remains with the user.
No game launch, control or screenshot capture was performed.

Tab-art verification: unchanged hashes for panel source, OptionsMenuLayout, MenuPointerInput, OptionsModel and font catalog. Standard Debug build was locked by the running game; separate bin/UiVerification build passed with zero warnings/errors. Rebuilt regression runner passed 59/59, including packaged asset hashes/dimensions. git diff --check passed. Close the game and rebuild standard output before reviewing there.

Tab artwork size follow-up: reused the existing four sprites with no PixelLab
call. Artwork is now 90% of its prior display size (198x50 after integer rounding),
centered inside unchanged 220x56 hit bounds. Labels, font rendering, panel and
interaction are retained. Point sampling remains enabled. Visual review is for
the user.

Reduced tab artwork by a further 8% of its current size: cumulative scale 82.8%,
182x46 after rounding. Centering, text, hit targets and source sprites are retained.
No PixelLab calls or game interaction.

Tab spacing follow-up: reduced horizontal tab step from 236 to 224 logical pixels, keeping the first tab anchored. This moves Audio 12, Comfort 24 and Controls 36 pixels left. Artwork dimensions, labels and shared drawing/hit bounds are retained.
