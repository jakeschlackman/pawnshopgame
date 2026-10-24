# Rummage Hollow title artwork

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
The entire 16:9 canvas scales together to the current window resolution, with
letterboxing on other aspect ratios. Window resizing is enabled; no resolution
settings menu or fullscreen toggle has been added.
Live menu labels use the game's existing bitmap font, not the preview's Georgia
font. Unavailable labels and focused-item explanations preserve the current
Continue/Settings behavior.

Normal build/publish copies these PNGs; no asset-generation step or download is
required. Runtime decoding validates dimensions and converts straight PNG alpha
to premultiplied alpha for SpriteBatch without modifying the source files.
