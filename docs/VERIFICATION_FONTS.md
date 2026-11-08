# Central font catalog verification

### JSON title sayings and matching font - 2026-11-08

Moved the main-menu footer saying into Content/TitleScreen/title-sayings.json,
with the original line and nine new cozy pawn-shop sayings. TitleSayings validates
version, nonempty unique entries, printable ASCII/one-line text and an 80-character
limit. ApplicationFlow chooses once at startup and shares that immutable line
with OptionsModel's Comfort dialogue-speed preview. A single-entry pool works.
Nothing rerolls on returns to the main menu, reopening Options, Draw,
menu navigation, disabled-entry feedback, window changes or ordinary updates.
Unavailable entries retain their badge and disabled behavior without replacing
the saying. The footer navigation hint has been removed.

TitleSaying uses the existing Pixeloid Sans Bold asset at 28, matching inner
Options typography, through the normal font catalog path. The badge font is
retained. Long authored lines fit the available footer width; the
original scale is restored afterward. Existing JSON
copy rules package the file without an MGCB change or new dependency. Headless
checks cover content validation, copied JSON, seeded/no-repeat selection, single
entry and stable startup sayings across navigation, Options return, character
selection and gameplay return. Comfort checks cover matching text, speed reveal,
resetting the preview timer and reopening Options without another random selection.
No game was launched; visual font/line fit remains
for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 68/68.
`git diff --check` passed.

### Main menu matches inner Options - 2026-11-08

TitleMenu now selects the existing Fonts/PixeloidSans-Bold asset for New Game,
Continue, Options and Exit, keeping logical line height 76. Labels and Sprout's
glyph-center anchor measure/draw through the same catalog role, including both
directions of the Options flight. Existing bundled TTF/SpriteFont/MGCB registration
and OFL copy are reused; no font download or installation was needed. Layout,
title badge/help roles and Options font sizes are retained. Font-content checks
now expect Pixeloid for TitleMenu. No game was launched; in-game typography and
selector alignment remain for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 66/66.
`git diff --check` passed.

### Pixeloid Sans Bold trial - 2026-11-08

All six inner Options roles now select the unmodified Pixeloid Sans Bold TTF from
the author's version-1.0 package through the existing SpriteFont/MGCB/catalog
workflow. Labels retain 26, values/buttons 24, help 20 and display-confirmation
heading 36. Header/footer fonts, layout and behavior are unchanged. The actual
bold file uses Style=Regular to avoid synthetic weight. The original SIL OFL 1.1
license is copied to build/publish output; provenance/hashes are in the font README.
Font-content checks require the new compiled asset and copied license. In-game
readability, size and wrapping remain for the user; no game was launched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed,
including the new font content build, with zero warnings/errors. The existing
regression runner (`dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/`) passed 62/62.
`git diff --check` passed.

### Pixelify Sans SemiBold trial — 2026-11-08

All inner Options roles now select the official static weight-600 Pixelify Sans
TTF, compiled through the existing SpriteFont/MGCB workflow. Font metadata was
checked: OS/2 weight class 600, no variable-font table. Labels retain size 26,
values/buttons 24, help 20 and display-confirmation heading 36; header/footer
fonts are unchanged. The SpriteFont references the actual semibold file with
Style=Regular, so no synthetic bold is applied. The family's existing original
OFL.txt accompanies output. Source URL, size and hash are in Content/Fonts/README.md.
Font-content checks require the newly compiled semibold XNB for the inner roles.
In-game weight, readability and wrapping remain for the user; no game was launched.

### Lighter inner typography — 2026-11-08

All four Options tabs now use the existing Geist Pixel Regular asset for inner
text, replacing Not Jam in dropdown/Audio roles. Labels use logical height 26;
values/buttons use 24 and help uses 20. Label/help line spacing is 28/22. Header tabs
and footer actions retain Jersey 25, and the existing display-confirmation title
keeps its 36-pixel Geist role. The dropdown's two-pixel downward offset is retained.
Comfort's shorter preview now reserves one line and fits at all saved scales;
scroll checks expect overflow only on Audio and Controls. Existing assets,
SpriteFont/MGCB registration and OFL license are reused. In-game readability
remains for the user to inspect.

Audio typography now matches Display: OptionsDropdownLabel (Not Jam at 20) for
all five option labels; OptionsDropdown (Not Jam at 18) for the four volume
values, mute button, help and status messages. Measurement and drawing use the
same roles. Audio help uses 20-pixel line spacing with two lines reserved, so
scroll bounds reflect the smaller text. Other tabs, header/footer fonts, slider
artwork, control geometry and settings behavior are retained. In-game typography
and wrapping remain for the user's check.

### Labels match their value controls — 2026-11-08

Display labels now use OptionsDropdownLabel (Not Jam at 20), matching the
OptionsDropdown values at 18. Audio, Comfort and Controls labels use OptionsLabel
(Geist Pixel at 30), matching their value buttons at 28. Labels remain in their
existing column, with two-line wrapping at 22/32-pixel line spacing. The existing
dropdown two-pixel downward offset, button geometry, header/footer fonts and help
roles are retained. Font-content checks require both label roles and their assets.
Visual balance and wrapping remain for the user's in-game check.

### Not Jam dropdown replacement — 2026-11-08

OptionsDropdown now selects Not Jam Slab Serif 11 for both button values and
popup choices. The official package's original TTF and CC0 license are bundled
through the existing MGCB/catalog workflow. A size-33-point atlas compiles with
line spacing 48; the role now uses logical height 18 with a two-pixel downward
text offset in dropdown buttons and choices. Artwork, positions, other font roles,
and dropdown/settings behavior are unchanged. Source URLs and hashes are in
Content/Fonts/README.md.

Removed Pixelta from MGCB and runtime license copying while preserving its trial
sources. MGCB removed the superseded compiled content, and a fresh ignored
`bin/NotJamVerification/` output contains no Pixelta files. The initial sandbox
font-discovery failure was resolved by an approved build outside the sandbox.
That solution/content build passed with zero warnings/errors. The regression
runner passed 61/61, including the new font asset and copied CC0 license. Compiled
font metrics were inspected without graphics; no game launch or desktop check
occurred. Final visual size and readability remain for the user.

### Pixelta dropdown font trial — 2026-11-08

Added the unmodified DaFont Pixelta TTF and original author license PDF, compiled
through the existing SpriteFont/MGCB workflow. OptionsDropdown selects Pixelta
at logical line height 28 for the two display dropdown buttons and all popup
choices. Measurement/drawing use that role together. All other font roles and
button/popup bounds are retained. The free font is personal-use only, so this is
a local trial; commercial/app-game distribution clearance is not established.
Source URLs, hashes and restrictions are recorded in Content/Fonts/README.md.
The font-content check requires the compiled Pixelta XNB and copied license PDF.
In-game appearance remains for the user; no game was launched.

### Larger inner text and separate labels — 2026-11-08

Added the unmodified official Press Start 2P font, OFL license, SpriteFont and
MGCB entry, using the existing catalog workflow. OptionsLabel selects it at 22;
its full-height glyphs replace the former Geist option labels. Long labels wrap
within the label column, with up to two lines at 24-pixel spacing. Inner button
values now use Geist OptionsBody at 28 (previously OptionsHelp at 22); help text
increases to 26 with 28-pixel spacing. Header and footer roles retain Jersey 25.
Comfort help reserves two lines; at the largest saved scale that tab now scrolls.

The initial sandbox content build failed in MonoGame's Windows font discovery;
the approved build outside the sandbox successfully compiled the bundled TTF.
The separate `bin/UiVerification/` solution build passed with zero warnings/errors,
its regression runner passed 60/60, and `git diff --check` passed. Checks include
the new font role, compiled XNB and copied license plus updated overflow geometry.
No game was launched. Final readability, label wrapping and alignment remain for
the user's in-game check.

Inner Options font follow-up (2026-11-08): OptionsBody, OptionsHelp and
OptionsHeading now use the already bundled Geist Pixel font at their existing
24/22/36 logical line heights. Header tabs and footer actions retain Jersey 25.
The existing SpriteFont, MGCB registration, OFL license and catalog rendering path
are reused. Font-content checks now require Geist Pixel for those inner roles.
In-game readability and wrapping remain for the user to inspect.

Footer typography follow-up (2026-11-08): OptionsAction shares the header tabs'
Jersey 25 Regular asset at logical line height 16, reduced from 18; OptionsTab
remains at 24. The existing catalog, measurement and rendering path is retained,
including the footer's three-pixel downward offset. Appearance remains for the user.

Date: 2026-10-31. Existing uncommitted work was preserved. No application launch,
Computer Use, screenshots or visual checks were performed. Manual appearance and
readability checks are left to the user.

## Implementation

- Retained the eight-project structure. Framework owns FontCatalog, a small
  FontText extension (`"text".Font("Role")`), and a SpriteFont adapter; Game owns
  the catalog/content and bootstrap composition. Domain projects remain unchanged.
- Game's `Content/Fonts/fonts.json` maps named roles to font assets and logical
  pixel line heights. Title menu, disabled badge/help and all Options text use
  Pixelify Sans Bold. The title logo remains the supplied image. Existing room
  and pause text keeps its placeholder font.
- Uses MonoGame SpriteFont, ContentManager, MeasureString and DrawString, with
  one shared content scope and unique asset cache. No font rasterizer, UI framework
  or runtime third-party font library was added. Unknown/invalid definitions fail
  clearly. Content is parsed/loaded once at startup; shared fonts are disposed
  after screens/renderers on application unload.
- Downloaded Google Fonts' static weight-700 TTF and original OFL license. Font
  bytes, weight and SHA-256 are recorded in Content/Fonts/README.md. The font is
  bundled without modification or system installation.
- Added standard `.spritefont`/MGCB font compilation and the official
  MonoGame.Content.Builder.Task plus local dotnet-mgcb tool, both pinned to the
  existing 3.8.4.1 runtime version. Existing PNG loading remains unchanged.

## Verification

- Baseline solution build: passed with zero warnings/errors; 20/20 regression cases.
- Initial network access and a tool restore were blocked by the sandbox. Approved
  elevated requests downloaded the font/license and restored/built the pinned
  MonoGame content tool. No SDK, framework version or system font was installed.
- MGCB compiled `Fonts/PixelifySans-Bold.spritefont` successfully using the bundled
  TTF. Subsequent sandbox builds use the restored tool and cached content.
- `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/FontVerification/`:
  passed, zero warnings/errors, including the standard content build.
- `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/FontVerification/`:
  22/22 passed. New cases cover named selection, role sizes, schema/malformed JSON,
  duplicate names, invalid asset paths, unknown names/fields and numeric ranges;
  they also verify copied catalog/license and compiled XNB presence in test output.
- Visual layout, raster appearance and readability at each UI scale remain for
  the user to check; headless results do not establish those properties.

## Silkscreen follow-up

Imported unmodified Silkscreen Regular (400) and its OFL license from Google Fonts;
registered its spritefont in the existing MGCB file. TitleMenu now selects that
asset at the existing size 64. Other roles remain Pixelify Sans Bold. Added a
general `Silkscreen` catalog entry and updated the existing content checks to
verify both compiled fonts and licenses. No dependency or renderer changes.

Initial sandbox font compilation failed inside MonoGame's system font lookup;
the approved elevated retry of the solution build passed with zero warnings/errors.
The source TTF is bundled and does not require installation. Manual visual testing
remains with the user.

## Options readability follow-up (2026-11-05)

All three Options roles now use Atkinson Hyperlegible: Bold headings, Regular
body/help. Their existing 36/24/22 logical line heights are retained. These roles
cover every Options category, value, binding column, button, message, footer and
display confirmation prompt. The Options renderer opts into linear sampling for
font downscaling and canvas presentation. Renderer defaults, the title, gameplay
and direct pixel-sprite overlays retain point sampling. Shared FontLibrary
measurement also uses the new glyphs, including selection alignment and wrapping.

Bundled unmodified Regular/Bold static TTFs and their original SIL OFL license
from Google Fonts. Source URLs, byte counts and SHA-256 hashes are recorded in
Content/Fonts/README.md. Both compile with the existing MonoGame 3.8.4.1 pipeline;
the license is copied to build/publish output. No new dependency, system font
installation or runtime download was added.

The sandbox blocked the initial font download; the approved elevated retry
succeeded. Initial MGCB compilation hit the same sandbox font-lookup exception
recorded above. The approved build outside the sandbox then compiled both bundled
TTFs successfully, with zero warnings/errors:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

The runner passed 47/47 checks. Existing font-content checks now require the new
Options role mappings, both compiled XNBs and the copied license. Runtime copies
of the catalog/license were compared with source by SHA-256. Whitespace checks
passed for authored text; the unmodified upstream license retains its original
trailing space on line 20. No application launch or screenshot inspection was performed. Readability,
binding-column fit, message wrapping and display confirmation layout at
100/125/150 percent UI scale remain manual checks for the user.

## Selection indicator alignment

The title selection diamond now centers on the selected label's drawn glyph
bounds using MonoGame's glyph cropping and texture bounds, rather than a fixed
vertical offset. Font size and UI scaling are included; font leading is excluded.
Glyph metadata is cached when the shared font loads. The solution build passed
with zero warnings/errors. Visual alignment remains for the user to verify.

## Jersey 10 tab labels - 2026-11-08

Added the unmodified Jersey 10 Regular TTF and original SIL OFL license from the
official Google Fonts repository. Registered its SpriteFont in Content.mgcb and
added `OptionsTab` at line height 22 in fonts.json. Only the four header-tab labels
select it; both measurement and rendering follow the existing FontLibrary and
`.Font(...)` path. Other font roles, artwork, layout and interactions are retained.
Source links, copyright, bytes and hashes are in Content/Fonts/README.md. The
license is copied to build/publish output. No installation or dependencies.

Initial sandbox font compilation hit the same MGCB FindFont exception recorded
for earlier font imports. The approved build outside the sandbox successfully
compiled Jersey10-Regular.spritefont, with zero warnings/errors. Font-content
checks now require the OptionsTab role, compiled Jersey 10 XNB and copied license.
No game launch, control or screenshots were performed. Appearance and fit are
left to the user.

Jersey 10 verification: solution build passed with zero warnings/errors; rebuilt dependency-free regression runner passed 59/59, including the new role, compiled font and license checks. git diff --check passed with line-ending notices only.

Header-tab typography adjustment: OptionsTab line height increased from 22 to 24 logical pixels. Jersey 10 SpriteFont spacing increased from 0 to 2 atlas pixels; the existing font/UI scale applies to that spacing. Button bounds, positions and other font roles are retained.

## Geist Pixel header-tab substitution - 2026-11-08

Bundled the unmodified Google Fonts Geist Pixel TTF and its original SIL OFL
license. Added a SpriteFont/MGCB entry and changed only the OptionsTab catalog
asset to Fonts/GeistPixel-Regular, retaining line height 24 and Spacing=2.
Existing layout, artwork, input behavior and other font roles are unchanged.
The variable font's default instance compiled successfully through the existing
MGCB pipeline. Font-content checks require its compiled XNB and copied license.
No dependencies, system font installation, game launch or screenshots.

Geist Pixel verification: solution build passed with zero warnings/errors, including MGCB compilation. Rebuilt regression runner passed 59/59; git diff --check passed with line-ending notices only. In-game appearance remains for the user to verify.

## Upheaval Pro tab substitution - 2026-11-08

Downloaded the requested DaFont archive and inspected its original upheaval.txt
freeware terms. Extracted the TTF and license without modifying their bytes.
Registered UpheavalPro.spritefont in MGCB and switched only OptionsTab's asset
to Fonts/UpheavalPro. Line height 24, Spacing=2, centering, button artwork, layout
and behavior remain as before. The license is copied to build/publish output.
Font-content checks require the compiled Upheaval XNB and its license.
Source links, attribution and hashes are recorded in Content/Fonts/README.md.
The solution build passed with zero warnings/errors including font compilation.
No game launch, control or screenshots were performed; visual fit remains for
the user.

Upheaval Pro verification: rebuilt regression runner passed 59/59, including catalog, compiled font and bundled-license checks. git diff --check passed with line-ending notices only.

## Phatone header-tab trial - 2026-11-08

Downloaded the requested DaFont ZIP and read the included freeware license PDF.
Recorded its font-software redistribution/conversion restrictions and local-trial
status in Content/Fonts/README.md. Original OTF and PDF bytes are preserved.
Registered Phatone-Regular.spritefont in MGCB; OptionsTab now selects it at line
height 22 (previously 24), retaining Spacing=2. No font-file format conversion,
font installation, rendering change, button/layout change or other role change.
The PDF license is copied to local output and the font-content regression checks
the new compiled font and copied license.

A temporary pypdf reader was installed only under ignored .cache to read the PDF;
no project/package references changed. The reader needed approved execution
outside the sandbox because its downloaded package was inaccessible within it.
The solution build passed with zero warnings/errors, including direct OTF
compilation through the existing MGCB workflow. No game launch, control or
screenshots were performed; appearance remains for the user to review.

Phatone verification: rebuilt regression runner passed 59/59; git diff --check passed with line-ending notices only.

Phatone typography follow-up: increased OptionsTab line height from 22 to 24 and
SpriteFont spacing from 2 to 4 atlas pixels, following existing UI scaling.
Only header-tab text is affected; button artwork, bounds and behavior are retained.
No game launch, control or screenshots.

## Jersey 25 header tabs - 2026-11-08

Bundled the unmodified Jersey 25 Regular TTF and original OFL license from Google
Fonts. Registered its SpriteFont with MGCB and switched only OptionsTab's asset
to Fonts/Jersey25-Regular. Logical line height 24, Spacing=4, the existing font
selection/rendering path, button artwork, positions, hit bounds and behavior are
retained. The original license is copied to build/publish output. Font-content
checks now require the new compiled font and copied license. Sources/hashes are
recorded in Content/Fonts/README.md. No system installation, new dependency,
game launch, control or screenshots. Visual appearance remains for user review.

Jersey 25 verification: solution build passed with zero warnings/errors, including MGCB font compilation; rebuilt regression runner passed 59/59; git diff --check passed with line-ending notices only.
