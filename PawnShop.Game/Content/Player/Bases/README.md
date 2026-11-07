# Player bases

Canonical bald underwear reference bodies for layered customization. Each
male/female base.png is a 384x64 atlas with eight 48x64 cells ordered
S, SE, E, NE, N, NW, W, SW. The visible body is 50 pixels tall, rows 8-57,
with ground pivot (24,58). Do not trim or independently recenter layers.

base-manifest.json records current PixelLab character IDs, source provenance,
layout and acceptance scope. Sources preserves the original source artwork and
the intermediate AI revision; Recipes preserves native-pixel base normalization.
Rejected shirts and hair are not part of the base set.

The game copies the two `base.png` atlases and `../player-sprite.json` into runtime
content. That shared metadata maps the eight standing facings for character
selection and room movement; no walking cycle is implied.

`Walking` contains separate Skeleton v3 NESW authoring candidates for male and
female. These historical files used W/N/S plus copied E. New art uses S/N/E and
runtime mirrored West. The existing candidate set does not replace
these standing atlases or the runtime metadata. Match every garment to both body
variants and every animation frame; see `Walking/README.md` and the workflow.

Run `dotnet run --project PawnShop.Tests --no-build -- --assets bases PawnShop.Game/Content/Player/Bases` from the repository root. All eight standing
directions have geometry/alpha checks; user visual approval currently covers the
south-facing shirt compositions. No walking, mounted-pose or in-game scale
acceptance is implied. See [the authoring workflow](../../../../docs/WEARABLE_ART_WORKFLOW.md).
