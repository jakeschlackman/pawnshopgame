# Central font catalog — current verification

The authoritative roles are in
[fonts.json](../PawnShop.Game/Content/Fonts/fonts.json).
[Font source and license records](../PawnShop.Game/Content/Fonts/README.md)
remain with the assets. Superseded font trials and their old verification log are
recoverable in `.cache/DocsArchive/docs-cleanup-20261111-040437.zip`.

## Current roles

- Main-menu labels, sayings and inner Options text use Pixeloid Sans Bold.
- Options tabs and footer actions use Jersey 25 Regular.
- The title badge/help use Pixelify Sans Bold; the title logo is an image.
- Framework FontCatalog validates roles and FontLibrary owns the shared
  ContentManager/SpriteFont resources. Screens use the shared scope rather than
  disposing fonts individually. The existing pinned MGCB pipeline compiles them.
- Title sayings are loaded once from JSON, selected once per startup and shared
  with the Comfort dialogue-speed preview. Drawing does not reload or reroll them.

The latest related 82-test suite and content build passed, including role validation,
compiled fonts, source/license packaging, sayings validation and stable selection.
Native rendering, readability, spacing and wrapping remain user acceptance checks
at supported resolutions and UI scales. No visual result is inferred from these tests.
