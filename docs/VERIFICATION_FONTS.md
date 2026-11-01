# Central font catalog verification

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

## Selection indicator alignment

The title selection diamond now centers on the selected label's drawn glyph
bounds using MonoGame's glyph cropping and texture bounds, rather than a fixed
vertical offset. Font size and UI scaling are included; font leading is excluded.
Glyph metadata is cached when the shared font loads. The solution build passed
with zero warnings/errors. Visual alignment remains for the user to verify.
