# Fonts

`fonts.json` is the central, versioned map of font names/roles to MonoGame content
IDs and logical pixel line heights. For example, `"New Game".Font("TitleMenu")`
selects the `TitleMenu` entry. Case matters. SpriteFont handles font rendering;
the JSON and extension method only select an asset and size. Build and restart
after editing. Previously saved UI scale values multiply the configured line heights.

## Title menu and inner Options text

`TitleMenu` uses **Pixeloid Sans Bold**, matching inner Options text, at its
existing catalog-defined line height of 76. Its measured selector anchor also
resolves the same role. `TitleSaying` uses Pixeloid Sans Bold at 28 for the random
footer saying. The title badge retains Pixelify Sans Bold; Options tab and footer labels
use Jersey 25. All inner option text uses Pixeloid Sans Bold: labels at 26,
values/buttons at 24, help at 20. Labels are two logical pixels larger than values.
The `Silkscreen` entry is available
for explicit selection, for example `"New Game".Font("Silkscreen")`.

## Silkscreen (retained earlier title-menu font)

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

## Atkinson Hyperlegible (retained earlier Options font)

Options headings/body/help previously used **Atkinson Hyperlegible**: Bold (700)
for `OptionsHeading`, Regular (400) for `OptionsBody` and `OptionsHelp`. These
roles now use Geist Pixel; the earlier source files and license are retained.

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

## Jersey 10 (retained earlier tab font)

Jersey 10 Regular (400) was the earlier header-tab selection. Its source and
license are retained; OptionsTab now selects Geist Pixel. Measurement, centering
and drawing use the same catalog role through `.Font("OptionsTab")`; the shared
FontLibrary owns the compiled SpriteFont. Other roles are unchanged.

Downloaded unmodified on 2026-11-08 from the official Google Fonts repository:

- [Jersey10-Regular.ttf](https://raw.githubusercontent.com/google/fonts/main/ofl/jersey10/Jersey10-Regular.ttf)
- [OFL.txt](https://raw.githubusercontent.com/google/fonts/main/ofl/jersey10/OFL.txt)

Copyright 2023 The Soft Type Project Authors
(https://github.com/scfried/soft-type-jersey). Licensed under SIL OFL 1.1; the
original license is bundled as `Jersey10-OFL.txt` and copied to build/publish
output. No system font installation, runtime download or dependency was added.
The spritefont uses the existing size-48 MGCB workflow, kerning, printable ASCII
and `?` fallback.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| Jersey10-Regular.ttf | 77,732 | `DB9CBD091617048A145D249DAA2B815FE7083BE6AB66AC26626E21A4E01C3E82` |
| Jersey10-OFL.txt | 4,395 | `436D5836064CEC796C7DDDE6FB4FAEABDB6F2F17443A24A30D0C38B75D13F922` |

OptionsTab now uses a 24-pixel logical line height and SpriteFont Spacing=2 for slightly more space between letters. These settings follow the existing UI scale and affect only header-tab labels.

## Pixeloid Sans Bold (inner Options text)

All six inner Options roles use `Fonts/PixeloidSans-Bold`: labels at 26,
values/buttons at 24, help at 20 and the display-confirmation heading at 36.
Header/footer roles retain Jersey 25. The existing catalog, measurement, rendering
and point-sampling workflow is unchanged; in-game readability remains a manual check.

Downloaded unmodified on 2026-11-08 from the free download on the
[author's itch.io page](https://ggbot.itch.io/pixeloid-font), package
`Pixeloid_Font_1_0.zip` version 1.0. The original `TrueType (.ttf)/PixeloidSans-Bold.ttf`
is bundled with the original `License.txt` preserved as `Pixeloid-OFL.txt`.
Copyright 2020-2025 GGBotNet; reserved font name Pixeloid. SIL OFL 1.1 permits
commercial embedding with its conditions; the copyright/license accompanies build
and publish output. No system font installation or dependency was added.
The SpriteFont uses size 48, Spacing=0, kerning, printable ASCII and `?` fallback.
Style=Regular selects the supplied bold file without synthetic weight.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| PixeloidSans-Bold.ttf | 109,964 | `D8BC38413101440E2D68B8D886F82DAE298E3D4550728B5C99F717377ED8A195` |
| Pixeloid-OFL.txt | 4,493 | `DE7A71BD2D330C09AAADB38CD09EDA3C62693A55350E288B43DAC73F4A5E67A8` |

## Pixelify Sans SemiBold (retained earlier Options font)

All inner Options roles previously used `Fonts/PixelifySans-SemiBold`: labels at 26,
values/buttons at 24, help at 20, and the display-confirmation heading at 36.
Header tabs and footer actions retain Jersey 25; title typography is unchanged.
The existing `.Font(role)` catalog/measurement/rendering workflow is retained.

Downloaded unmodified on 2026-11-08 from the official Google Fonts
[weight-600 CSS response](https://fonts.googleapis.com/css2?family=Pixelify+Sans:wght@600&display=swap),
which returned this [static TTF](https://fonts.gstatic.com/s/pixelifysans/v3/CHy2V-3HFUT7aC4iv1TxGDR9DHEserHN25py2TQ3131Y.ttf).
The TTF's OS/2 weight class is 600 and it has no variable-font table. The
SpriteFont uses size 48, Spacing=0, kerning, ASCII 32-126 and `?` fallback.
Style stays Regular because the file is already semibold; no synthetic bold is
applied. The family's original `OFL.txt` is already copied to build/publish output.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| PixelifySans-SemiBold.ttf | 49,644 | `24F4A626BEE444156E5C0D8CF14183DA358BFDE9C61FB6B51340C5F9E18A9845` |

## Geist Pixel (retained earlier Options font)

Geist Pixel Regular previously supplied inner Options text: `OptionsLabel` and
`OptionsDropdownLabel` at 26; `OptionsBody` and `OptionsDropdown` at 24;
`OptionsHelp` at 20; display-confirmation `OptionsHeading` at 36. This covers option labels,
values, inner buttons, binding slots, help, messages and display confirmation.
Labels can wrap at 28-pixel spacing; help uses 22-pixel spacing. Header tabs and
footer actions retain Jersey 25. SpriteFont Spacing=2 is retained; measurement and
drawing use the existing FontCatalog / FontLibrary / `.Font(role)` workflow with
point sampling. No renderer or layout changes are needed.

Downloaded unmodified on 2026-11-08 from the official Google Fonts repository:

- [GeistPixel[ELSH].ttf](https://raw.githubusercontent.com/google/fonts/main/ofl/geistpixel/GeistPixel%5BELSH%5D.ttf), stored as `GeistPixel-Regular.ttf`
- [OFL.txt](https://raw.githubusercontent.com/google/fonts/main/ofl/geistpixel/OFL.txt), stored as `GeistPixel-OFL.txt`

The Google Fonts variable font compiles at its default instance through MGCB.
Copyright 2026 The Geist Project Authors (https://github.com/vercel/geist-font).
The original SIL OFL 1.1 license is bundled and copied to build/publish output.
No font installation, runtime download or new dependency is required. SpriteFont
uses size 48, kerning, printable ASCII and `?` fallback, as with the existing fonts.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| GeistPixel-Regular.ttf | 3,657,104 | `DCDE0E2AE59AAD779E71EE3FA45ADE3A7146C2498024FDBB79C8CB619E60A238` |
| GeistPixel-OFL.txt | 4,384 | `021CAA749F89FECD7C2E40A50499DE29E84D2BC19251659B47D499C2FBFFFC17` |

## Press Start 2P (retained earlier Options label font)

`OptionsLabel` previously used **Press Start 2P Regular** at logical line height
15. All current labels now use Pixeloid Sans Bold at 26, with values/buttons at 24 and help at 20.
Long labels can wrap to two lines at 28-pixel spacing; help uses 22-pixel spacing.
Existing button bounds,
header/footer fonts and point sampling are retained.

Downloaded unmodified on 2026-11-08 from the official Google Fonts repository:

- [PressStart2P-Regular.ttf](https://raw.githubusercontent.com/google/fonts/main/ofl/pressstart2p/PressStart2P-Regular.ttf)
- [OFL.txt](https://raw.githubusercontent.com/google/fonts/main/ofl/pressstart2p/OFL.txt), stored as `PressStart2P-OFL.txt`

Copyright 2012 The Press Start 2P Project Authors. The original SIL OFL 1.1 license
is copied to build/publish output. The existing MGCB workflow builds a size-48
SpriteFont with Spacing=0, kerning, printable ASCII and `?` fallback. No system
font installation or new dependency is required.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| PressStart2P-Regular.ttf | 118,204 | `034C77F1F05EC89421E4A63F0E3A4CA1ECF852CC6D2BF611F126F275728E017D` |
| PressStart2P-OFL.txt | 4,413 | `705960C3281A5765ECC0B59BD4ED7CA59EED165748076BC2FC3E8FDBFEB944B0` |

## Not Jam Slab Serif 11 (retained earlier font)

`OptionsDropdown` and `OptionsDropdownLabel` previously selected **Not Jam Slab
Serif 11** at 18/20. Both now use lighter Geist Pixel, along with the other inner
Options roles. The CC0 font and source remain available for future use. The
dropdown's two-pixel downward text offset is retained. Its SpriteFont uses size
33 points (44 source pixels), Spacing=0 and
kerning; the compiled line spacing is 48, scaled by the catalog to the requested
logical height. Other saved UI scales still multiply that size. Existing point
sampling, dropdown art, bounds and input behavior are retained.

Downloaded unmodified on 2026-11-08 through the free download on the
[author's itch.io page](https://not-jam.itch.io/not-jam-slab-serif-11), package
version 2.0.0. `NotJamSlabSerif11.ttf` is stored as
`NotJamSlabSerif11-Regular.ttf`; `Licence.txt` is preserved as
`NotJamSlabSerif11-CC0.txt` and copied to build/publish output. The author releases
the font under CC0 and explicitly permits commercial projects without limitation.
No personal-use-only font is used by this role.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| NotJamSlabSerif11-Regular.ttf | 11,220 | `E640A5ECC81BA875D607B6CDC2978EFC041980A24E4FAECCB3CA1C74D83E14E7` |
| NotJamSlabSerif11-CC0.txt | 6,361 | `9E63E18810473F60C8C5A536F24FD2F2A9EF53B0FD3F41B5708908E5DF77F737` |

## Pixelta (retained earlier local font trial, inactive)

`OptionsDropdown` previously used Pixelta at logical line height 28. It now uses
Not Jam Slab Serif 11. Pixelta's MGCB entry and license-copy entry have been
removed; its original source files are preserved as historical trial evidence.
Option labels retain Press Start 2P; other body/help text retains Geist;
header/footer labels retain Jersey 25.

Downloaded unmodified on 2026-11-08 from [Pixelta on DaFont](https://www.dafont.com/pixelta.font)
via its [download archive](https://dl.dafont.com/dl/?f=pixelta). `Pixelta.ttf` is
stored as `Pixelta-Regular.ttf`; the author's original PDF is retained as
`Pixelta-License.pdf`. Its inactive SpriteFont describes a size-48 atlas with
Spacing=2, kerning, ASCII selection and `?` fallback.
The free font omits some punctuation; the current mode/resolution strings use
supported letters, digits and spaces.

**Local personal-use trial only.** The bundled PDF and DaFont page prohibit
commercial use of the free download. The author's [licensing page](https://blankidsfonts.com/licensing/)
lists app/game embedding separately. No commercial/app-game license has been
provided or purchased here; this font is not approved for shipping/distribution.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| Pixelta-Regular.ttf | 49,444 | `49F419470C0CF1B958000F353B9525B266AE7D21AF21A25F31A7D15D7EC585A1` |
| Pixelta-License.pdf | 146,274 | `9A36515CDCADE5A7AD29F0C4A24A29AC487B68BCBFF3141D52AADD72C5482ACF` |

## Upheaval Pro (retained earlier tab font)

Upheaval Pro Regular, version 1.100, was an earlier tab selection. OptionsTab
now selects Phatone. Line height 24 and SpriteFont Spacing=2 are retained.
The existing SpriteFont / MGCB / fonts.json / FontLibrary workflow is unchanged.

Downloaded on 2026-11-08 from the [requested DaFont page](https://www.dafont.com/upheaval-pro.font)
using its [font ZIP endpoint](https://dl.dafont.com/dl/?f=upheaval_pro). The TTF is
an unmodified archive member. Font metadata credits Aleksandr Savenkov (2012);
the bundled upheaval.txt credits the original Upheaval by Brian Kent.
The original custom freeware license allows personal and commercial use,
requires the text to remain intact with redistributed fonts, and restricts selling
or altering the font without permission. It is preserved byte-for-byte as
`UpheavalPro-License.txt` and copied to build/publish output.

No font installation, runtime download, renderer or dependency was added. The
SpriteFont uses size 48, kerning, printable ASCII and `?` fallback. Earlier font
sources remain available; their roles do not change.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| UpheavalPro.ttf | 301,376 | `F0B89BD7CE9F5413C57633678D3002BA4F51F0BBCA796C2E3F8F772E43F75116` |
| UpheavalPro-License.txt | 2,854 | `0E34935CFE013F5FCEDB7CD0E484436855A5F7E0C25FEBA9E0BED73506B8CED3` |

## Phatone (retained earlier tab-font trial)

Phatone Regular was an earlier tab-font trial. OptionsTab now selects Jersey 25.
SpriteFont Spacing=4 provides increased letter spacing. Only Display, Audio, Comfort and Controls use
this role; button layout, artwork, other fonts and the existing font pipeline
are unchanged. The original OTF compiles directly with the existing size-48
MGCB SpriteFont, kerning, printable ASCII and `?` fallback. No OTF-to-TTF
conversion, font modification, system installation or game dependency was added.

Source: [requested DaFont page](https://www.dafont.com/phatone.font),
[download ZIP](https://dl.dafont.com/dl/?f=phatone), retrieved 2026-11-08.
`Phatone-Regular.otf` and the license PDF are unmodified archive members.
The font is by Stefanie Koerner (PHEIST). The original freeware PDF license is
retained as `Phatone-License.pdf` and copied to local build/publish output.

License status: local font trial, not distribution clearance. The included
license permits personal/commercial use and screen/output display, but restricts
conversion and redistribution of font software. Do not distribute the OTF or
publish it in a source repository without the author's permission. No font-file
redistribution or interpretation of game-embedding rights is approved here.
The source file stays unchanged; MGCB produces the local glyph atlas.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| Phatone-Regular.otf | 24,176 | `005D5F02B53F445DB697D168391AC2C4F0192CDE489086CE99BE03E5E7922E61` |
| Phatone-License.pdf | 217,813 | `70D0B5B4A30EE54C47762C347FF1DB717833DA5F8EECE918F1BD31BD9D07DC94` |

## Jersey 25 (Options header tabs)

`OptionsTab` selects **Jersey 25 Regular (400)** for Display, Audio, Comfort and
Controls only. Logical line height 24 and SpriteFont Spacing=4 are retained.
Measurement and rendering follow the existing FontCatalog/FontLibrary and
`.Font("OptionsTab")` path. Layout, artwork and other font roles are unchanged.

Downloaded unmodified on 2026-11-08 from the official Google Fonts repository:

- [Jersey25-Regular.ttf](https://raw.githubusercontent.com/google/fonts/main/ofl/jersey25/Jersey25-Regular.ttf)
- [OFL.txt](https://raw.githubusercontent.com/google/fonts/main/ofl/jersey25/OFL.txt), bundled as `Jersey25-OFL.txt`

Copyright 2023 The Soft Type Project Authors
(https://github.com/scfried/soft-type-jersey). The original SIL OFL 1.1 license
is copied to build/publish output. SpriteFont uses the existing size-48 MGCB
workflow, kerning, printable ASCII and `?` fallback. No installation, runtime
download or dependency was added.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| Jersey25-Regular.ttf | 142,372 | `4D4ACDD89A748E9548041A3046D95536C500E1A7BE9197C6AEEF7D980F79D370` |
| Jersey25-OFL.txt | 4,395 | `436D5836064CEC796C7DDDE6FB4FAEABDB6F2F17443A24A30D0C38B75D13F922` |

`OptionsAction` reuses Jersey 25 at logical line height 16 for the compact Apply,
Restore Defaults and Back buttons. OptionsTab remains at 24 with Spacing=4; no
additional font asset or texture is loaded for this smaller role.
