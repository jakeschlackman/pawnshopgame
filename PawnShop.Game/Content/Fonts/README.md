# Fonts

`fonts.json` is the central, versioned map of font names/roles to MonoGame content
IDs and logical pixel line heights. For example, `"New Game".Font("TitleMenu")`
selects the `TitleMenu` entry. Case matters. SpriteFont handles font rendering;
the JSON and extension method only select an asset and size. Build and restart
after editing. The UI scale option multiplies the configured line heights.

## Silkscreen

`TitleMenu` uses **Silkscreen Regular (400)** at its catalog-defined line
height. Title badge/help retain Pixelify Sans Bold; Options uses Atkinson Hyperlegible.
The `Silkscreen` entry is available
for explicit selection, for example `"New Game".Font("Silkscreen")`.

`Silkscreen-Regular.ttf` was downloaded unchanged on 2026-10-31 from the
[Google Fonts repository](https://github.com/google/fonts/tree/main/ofl/silkscreen).
It is 32,220 bytes. SHA-256:

```text
C845473330B94C2079CE9AF01C51AC8BA2D99C24F4D14C039843BBB8E642EBD8
```

Copyright 2001 The Silkscreen Project Authors
(https://github.com/googlefonts/silkscreen). The original SIL OFL 1.1 license is
bundled as `Silkscreen-OFL.txt` and copied to build/publish output. Its spritefont
uses the existing MonoGame pipeline, size 48, printable ASCII, and `?` fallback.
No system font installation or new dependencies are required.

## Atkinson Hyperlegible (Options)

All Options text uses **Atkinson Hyperlegible**: Bold (700) for `OptionsHeading`,
Regular (400) for `OptionsBody` and `OptionsHelp`. This covers every category,
label, value, binding slot, button, message, help line and display confirmation.
Logical line heights remain 36/24/22 pixels respectively and follow UI scale.
The Options renderer uses linear sampling for glyph scaling and presentation;
other renderers and pixel-sprite overlays retain point sampling.

The unmodified static TTFs and original license were downloaded on 2026-11-05
from the [Google Fonts repository](https://github.com/google/fonts/tree/main/ofl/atkinsonhyperlegible).
Copyright 2020 Braille Institute of America, Inc. The SIL OFL 1.1 license is bundled
as `AtkinsonHyperlegible-OFL.txt` and copied to build/publish output.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| AtkinsonHyperlegible-Regular.ttf | 54,348 | `7FB917C89019896D0B52EE84B7CBB3304C18CB90B19A62F5E32712BD23E97669` |
| AtkinsonHyperlegible-Bold.ttf | 55,256 | `5A3B0C8CC8CA545155150B4512A1FA248298DF121C50D6557E651E61FBDAB92F` |
| AtkinsonHyperlegible-OFL.txt | 4,352 | `F32D22B3908FCAD2C86A74000614EC22E6A7F66EA7E867E616026A27AEBDC143` |

Both spritefonts compile from bundled files at size 48 through the existing MGCB
workflow, with kerning, printable ASCII and `?` fallback. Bold references the
actual bold TTF with `Style=Regular`, avoiding synthetic weight. No font installation,
runtime download or new dependency is required. Visual readability and layout at
each UI scale remain manual acceptance checks.

## Pixelify Sans Bold

Source: [Google Fonts, Pixelify Sans](https://fonts.google.com/specimen/Pixelify+Sans).
Requested weight: **700**, normal style. Downloaded 2026-10-31 using the
[Google Fonts CSS response](https://fonts.googleapis.com/css2?family=Pixelify+Sans:wght@700&display=swap),
which returned this [static TTF](https://fonts.gstatic.com/s/pixelifysans/v3/CHy2V-3HFUT7aC4iv1TxGDR9DHEserHN25py2TQO131Y.ttf).

`PixelifySans-Bold.ttf` is the unmodified downloaded font (49,396 bytes). Its
OS/2 weight class is 700, with no variable-font table. SHA-256:

```text
C67401DF48CE6BE02FFDF8129CDF274C5D7B27A673E15ED0F018E465028A8363
```

Copyright 2021 The Pixelify Sans Project Authors
(https://github.com/eifetx/Pixelify-Sans). The font is licensed under the SIL Open
Font License 1.1. `OFL.txt` is copied from the
[Google Fonts repository](https://github.com/google/fonts/tree/main/ofl/pixelifysans)
and accompanies the compiled font in build and publish output.

`PixelifySans-Bold.spritefont` references the bundled TTF, requiring no installed
font. Its style is Regular because the supplied file itself is already Bold;
no synthetic bold or weight conversion is applied. MGCB builds one 48-point
SpriteFont atlas for printable ASCII (32–126), with `?` as the explicit missing
glyph. Expand character regions when non-ASCII game text is introduced.
`fonts.json` defines purpose-based sizes referencing the same compiled atlas;
changing sizes does not create more textures. MGCB and its build task are both
pinned to 3.8.4.1, matching the existing MonoGame runtime. Generated XNB/bin/obj
files remain ignored. Existing title PNGs still use their original loading path.
