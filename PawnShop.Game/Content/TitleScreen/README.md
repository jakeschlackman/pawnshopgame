# Picker's Hollow title artwork

## Active panorama

`title-screen-panorama.png` is an unmodified copy of the user-supplied
`pickers-hollow-panorama-single-pass.png`, imported on 2026-10-31. The source file
is preserved. Native dimensions: **2106x747**. No license beyond the supplied
asset is asserted. SHA-256 of source and imported copy:

```text
55D058617655602B5545B581ECB22949B527F55EEC3559113E04233D29C76DDE
```

The application loads this single background using Texture2D.FromStream. Build
and publish copy it via the existing `Content/TitleScreen/*.png` item; no content
compiler or image transformation is involved. The background uses a uniform
cover scale with PointClamp sampling. The visible source width/height and maximum
horizontal offset are calculated from the current viewport. Main shows the left
edge, Options shows the right edge, with a 0.8-second SmoothStep pan between them.
For a viewport wider than the panorama's aspect ratio, the full source width is
already visible and the image is cropped vertically.

The title logo, menu rows and Options controls are separate UI. They are hidden during
travel and appear at the destination; Back/Escape pans to Main. The title-side
Options panel is translucent over the same panorama. Pause-menu Options keeps its
existing standalone presentation. Users change resolution only through Options;
manual resizing and maximizing are disabled. Title/UI layout and pointer mapping
adapt to display changes without altering the image or gameplay camera.

Active title menu text uses Silkscreen Regular; help/badge and Options text use
Pixelify Sans Bold through `Content/Fonts/fonts.json`. The title logo below remains an image.

## Active title logo

`logo-pickers-hollow.png` is an unmodified copy of the user-supplied
`34dd3659-a6f1-4afe-a7cb-86c3ba40a834.png`, imported on 2026-10-31. The original
is preserved. Native dimensions: **1774x887**, RGBA with transparency. It replaces
the bitmap title heading as a separate overlay, centered in a 920x460 rectangle
at (500, 40) on the existing 1920x1080 title UI canvas. Its original 2:1 aspect
ratio is preserved, and menu text scaling does not change the artwork's size.
Build/publish uses the existing PNG copy rule. Runtime decoding validates its
dimensions and premultiplies alpha for SpriteBatch without modifying the PNG.
TitleScreen owns and disposes the logo texture, including partial-load failure.

## Sprout selection animation

`sprout-title.png` is an unchanged copy of the user-supplied `sprouttitlepage.png`,
imported on 2026-10-31. The Documents original is preserved. Dimensions are
**192x384**, RGBA; 32 frames in a **4-column, 8-row** grid of **48x48** cells,
indexed left-to-right, top-to-bottom. Source and imported SHA-256:

```text
6D79201AA34A122FD279477EA6C44EE3F10EBFD6AA909AFEC5289CEC0115F72B
```

`sprout-title.json` declares Idle (0–15, looping) and Interact (16–31, one-shot),
at **90 ms/frame for Idle** and **50 ms/frame for Interact**. Numeric alpha-bound inspection confirmed nonempty content
in all 32 cells. The shared occupied range is x=3–47, y=7–47. The stable pivot
(26,28) centers that area near the label without recentering individual frames.
At scale 1.5 the full frame is 72x72 on the 1920x1080 title UI canvas; it remains
independent of font scaling, while its vertical anchor follows the font metrics.

Title confirmations play the full 0.8-second reaction before executing an enabled
command. Disabled entries react without executing. Selection/repeated activation
is captured, Escape cancels, and focus loss pauses pending feedback. Ordinary
window-close requests remain immediate on title. Textures are loaded once with
premultiplied alpha and point sampling, disposed with TitleScreen. Existing content
copy rules include the PNG and metadata in build/publish output; no new dependency
or sprite-generation step is involved. Visual acceptance is left to the user.

Sprout is drawn once directly to the final backbuffer after the title UI, using
the same contained viewport transform, point sampling and a snapped output pixel
position. This avoids resampling his small sprite into the 1920x1080 canvas and
then scaling that result again. Source artwork and authored pose offsets remain
unchanged; Idle and Interact use independently configured frame durations.

## Preserved earlier artwork

The following assets remain in the repository but are no longer loaded by the
title screen. This section records their original provenance and reference layout.

Source: the user-supplied **RummageHollow-TitleScreen** starter pack and its
`preview.html`, integrated at the user's request on 2026-10-24. The pack describes
the artwork as generated. No separate license or attribution document was supplied;
no additional license grant is asserted here. The four PNGs are unmodified copies
of the supplied originals. The separate robot-companion PNG is intentionally excluded.

| File | Native dimensions | Transparency | Draw bounds on 1920x1080 canvas |
| --- | --- | --- | --- |
| background-town.png | 1672x941 | Opaque | 0, 0, 1920, 1080 |
| pawnshop.png | 1396x1127 | Alpha | 1060, 480, 720, 581 |
| foreground-salvage.png | 2172x724 | Alpha | 260, 730, 1400, 467 |
| logo-rummage-hollow.png | 1774x887 | Alpha | 500, 40, 920, 460 |

Draw in table order. The foreground intentionally extends below the canvas and
is clipped there. Point sampling and contained scaling preserve the composition;
these detailed illustrations are not gameplay sprites or a world-scale standard.
The bounds above record the original reference layout. Code expresses positions
and sizes as normalized canvas fractions: for example, the shop starts at 55.2083%
of the canvas width and 44.4444% of its height, and occupies 37.5% of its width.
The original 16:9 canvas scaled together to the window resolution, with
letterboxing on other aspect ratios. Options now provides display settings;
the active panorama uses the viewport-cover policy above instead.
Live menu labels use the game's existing bitmap font, not the preview's Georgia
font. Unavailable labels and focused-item explanations preserve the current
Continue behavior. Options is now enabled.

Normal build/publish copies these PNGs; no asset-generation step or download is
required. Runtime decoding validates dimensions and converts straight PNG alpha
to premultiplied alpha for SpriteBatch without modifying the source files.
