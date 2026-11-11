# Picker's Hollow title artwork

## Original panorama

`title-screen-panorama.png` is an unmodified copy of the user-supplied
`pickers-hollow-panorama-single-pass.png`, imported on 2026-10-31. The source file
is preserved. Native dimensions: **2106x747**. No license beyond the supplied
asset is asserted. SHA-256 of source and imported copy:

```text
55D058617655602B5545B581ECB22949B527F55EEC3559113E04233D29C76DDE
```

## Active panorama

`title-screen-panorama-day-to-dusk.png` is **2106×1494**. The upper 747 rows
extend the original with ImageGen artwork. A targeted ImageGen retouch connects
the diagonal cloud banks across the join. The dusk framing still starts at
`(0, 747)`. Rows 537–863 contain the retouch and its feathered edges. The
original dusk pixels from row 864 down were verified in place.

The application loads the extended PNG using Texture2D.FromStream. Build and
publish copy it via the existing `Content/TitleScreen/*.png` item. The background
uses point sampling and a uniform cover scale based on the original band and the
current viewport. On launch, the camera descends for 3.2 seconds from the top of
the sky to the original crop. The 0.8-second fade starts 0.25 seconds before
arrival and reveals the logo, menu and selector. Main shows the left edge and Options
shows the right edge, with a 0.8-second SmoothStep pan between them. For a viewport
wider than the original band, the full width is already visible and the band is
cropped vertically.

The title logo, menu rows and Options controls are separate UI. The Options panel is visible during outgoing flight, with input captured until arrival;
UI is hidden on the return pan. Back/Escape pans to Main. The title-side
Options panel is translucent over the same panorama. Pause-menu Options keeps its
existing standalone presentation. Users change resolution only through Options;
manual resizing and maximizing are disabled. Title/UI layout and pointer mapping
adapt to display changes without altering the image or gameplay camera.

Active title menu text uses Silkscreen Regular; help/badge and Options text use
Pixelify Sans Bold through `Content/Fonts/fonts.json`. The title logo below remains an image.

## Studio splash

`logo-torcalev-studios.png` is a byte-identical copy of the studio-owned
`Torcalev Tiger Bomber Pixel Emblem.png` supplied by the user. Dimensions are
**1254×1254**, RGBA with transparency. SHA-256 is
`FDA003D1324FC428EA05068FF4C077C69B4A35598FF6ACE59612B78D9CE9845D`.

The splash is centered in a 648×648 rectangle at (636, 216) on the title UI
canvas. It fades in for one second, holds for two seconds and fades out for
one second while the camera stays at the top. The normal descent follows.
Escape during the intro fades the splash out over 0.8 seconds and finishes the
descent and menu reveal within two seconds. Focus loss freezes the sequence.
TitleScreen loads and disposes the texture with the other title artwork.

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

The active runtime sheet is now `sprout-title-menu.png` (272x1520), assembled
without resampling into a 4-column, 19-row grid of 68x80 cells. Idle uses frames
0-12 from PixelLab's **Floating Point Right Loop**, east direction, at the existing
90 ms/frame (1.17 seconds per loop). Cells 13-15 are unused. Interact retains the
original frames 16-31 at 50 ms/frame, padded by (10,16). The new fixed pivot
(36,44) preserves the old interaction's (26,28) anchor and scale 1.5 exactly.
No frame is individually recentered. The Idle export canvas is 68x76 even though the
base PixelLab character is 48x48; downloaded alpha bounds span x=18-50, y=12-63.

Imported 2026-11-05 through the PixelLab MCP download URLs:
- Character: `106b34ee-a26a-4424-baec-61331b5d1a8a` (SPROUT — Original Brief Revised).
- Animation group: `d921f70e-9c0a-4f5a-b2c7-1f14db71c859`.
- East animation: `6f6f3e65-f403-4ccb-8e96-d35784478a8c`.
- Original downloaded PNGs: `Sources/sprout-floating-point-right/frame_0.png`
  through `frame_12.png`, preserved byte-for-byte. All 13 are 68x76 RGBA.
- User-owned PixelLab asset; no additional license grant is asserted.

The original sheet above remains unchanged. The assembled runtime sheet copies
each downloaded pixel into its numbered cell and each original interaction pixel
into the same numbered cell plus (10,16), leaving all other pixels transparent.
Only root title PNGs/JSON are copied to output; source frames are archival inputs.
The game loads bundled files only; no runtime PixelLab/network connection exists.

Non-Options title confirmations play the full 0.8-second reaction before executing an enabled
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

## Options flight animation

Additional east-facing PixelLab exports from the same character, imported 2026-11-05:

| Clip | Animation ID | Export canvas | Frames / sheet range | Timing |
| --- | --- | --- | --- | --- |
| Point to Fast Flight | `68570332-21d2-4c54-8f3d-a409f951c3b1` | 68x76 | 13 / 32-44 | 50 ms, once |
| Menu Fast Jet Flight | `88aa7830-b80d-4d48-a4d5-8098802c144e` | 60x68 | 17 / 45-61 | 50 ms, loop while traveling |
| Fast Flight to Point | `91fd5324-7c47-4baf-aa0d-eb5b934f512e` | 68x80 | 13 / 62-74 | 50 ms, once |

Original downloads are preserved in the matching `Sources/sprout-*/frame_N.png`
folders. The asset is user-owned PixelLab content; no additional license is asserted.
`tools/Build-SproutTitleSheet.ps1` rebuilds the runtime PNG from those local files
and the original selection sheet. It performs exact pixel copies with centered
transparent padding, never resizing: Idle/takeoff +(0,2), jet +(4,6), landing
+(0,0), original Interact +(10,16). Cells 13-15 and 75 remain transparent.
Pivot (36,44), scale 1.5, and the 9-title-pixel downward adjustment preserve the
previous pointing/selection position. The varying canvases are export dimensions,
not a new character body-size standard.

The flight exports above are preserved but are no longer used by Options.
Options finishes the current Idle cycle, then floats Sprout upright to the first
Display field along a SmoothStep path over the 0.8-second panorama pan. The same
pointing loop continues throughout travel and arrival; there is no rotation,
takeoff, jet animation or landing transition. Input unlocks after arrival.
Focus loss freezes travel; Escape cancels and returns to title. Other title
confirmations keep Interact. No runtime download or MCP connection is involved.

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
