# Player art

- `Bases`: canonical eight-direction bald underwear reference atlases.
- `Wearables/TealWorkShirt`: user-approved male/female south-facing shirt cuts,
  garment-only layers, source artwork, editable recipes and comparisons.
- `Bases/Walking`: Skeleton v3 NESW candidates for both bodies, source provenance,
  fixed-grid sheets, contact sheets and `review.html` for synchronized playback
  and optional matching garment-sheet overlays. These are authoring assets.

Use [the standard wearable workflow](../../../docs/WEARABLE_ART_WORKFLOW.md).
Approval is per body type, direction and pose. The game loads both base atlases
with `player-sprite.json` for selection. The catalog reads South/North/East only,
with West mirrored from East; extra legacy source columns remain untouched.
The room uses temporary code-authored body and equipment layers from
`Screens/PlayerPlaceholderSprites.cs`, timed by `placeholder-sprite.json`.
Rows are South/North/East, columns are four shared phases, and every layer keeps
the full 48x64 canvas and (24,58) pivot. West reuses East's row with a whole-canvas
flip. These diagnostic shapes are not fitted clothing or production art.
Wearables and walking sources remain authoring assets; production equipping and
approved human walks are pending.

Generate every wearable item for both male and female models. New walking art uses
S/N/E with Skeleton v3; West mirrors East at runtime. Existing NESW files are
historical candidates, not the new runtime contract. Clothing
must match its body/direction/frame and timing; inspect asymmetric pockets/gear.
Use the C# `--assets` commands in [the workflow](../../../docs/WEARABLE_ART_WORKFLOW.md) for source and per-frame attire checks.

Rejected prototypes were removed from active content after a verified recovery
archive was made under `.cache/PlayerArtArchive` (ignored by Git). Original art
used by the approved assets remains in each canonical Sources folder.
