# Player walking candidates

Both **male and female** player bodies have a separate NESW walking set. These
are authoring candidates awaiting visual acceptance; the game still uses the
existing eight standing facings. No walking clothes or equipment renderer is
implemented by these files.

## Generation and layout

These are preserved **legacy** Skeleton v3 sources: W/N/S were generated and E
was copied/mirrored from W. New production art authors S/N/E, with West mirrored
from East at runtime. Do not reinterpret these historical sources as newly
authored East or approved runtime assets. Runtime changes belong to Phase 2.

The current revised motion is an eight-frame small-step walk at 100 ms/frame,
with the built-in walk move at energy 0.6 and hip/chest twist disabled. The previous
six-frame male attempt was rejected for south appearance, north strutting and
janky west movement. Previous source sets remain under `Previous` with provenance;
they are not active sheets or approved fallbacks. Female approval is independent.

`walk-manifest.json` records generation mode, exact groups, source URLs/hashes,
mirror recipes/results, source padding and approval state. Preserve complete raw
80x80 source PNGs. A single fixed transparent-padding crop maps the authored
source ground anchor to (24,58) in 48x64 cells. The builder refuses to discard any
opaque source pixel. No scaling, fabric-row insertion or per-frame recentering is
permitted. The standing 48x64 contract is unchanged; in-game scale is unverified.

- `male/walk.png`, `female/walk.png`: one direction per row, **N, E, S, W**;
  frames left to right. This differs from the eight-direction standing atlas.
- `walk-sprite.json`: shared grid, looping clip ranges and timing for both sheets.
- `review-native.png`, `review-4x.png`: male NESW rows followed by female NESW.
- `review.html`: standalone synchronized playback, frame stepping, native/2x/4x
  display, and separate male/female garment-sheet uploads for fit inspection.
- `attire-template.json`: incomplete authoring template; missing layers/masks
  deliberately fail validation. Copy into a garment folder and adjust paths.

## Attire acceptance

Generate **every wearable item for both bodies**. A shared inventory item can
select different male/female art. Fit each layer to the exact frozen walking
body/frame, preserve the body sheet hash, and author coverage/protected masks
for every cell. The approved teal shirt covers only static south poses; do not
stretch or reuse it as a walking layer. Follow the
[wearable workflow](../../../../../docs/WEARABLE_ART_WORKFLOW.md).

Build the solution, then validate from the repository root:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets walk PawnShop.Game/Content/Player/Bases/Walking
dotnet run --project PawnShop.Tests --no-build -- --assets walk <walk-root> <garment-folder>/walking.json
```

Use `--assets pack <walk-root> <new-directory>` and then
`--assets review <packed-root> <new-directory>` to create separate candidate
outputs. Existing directories are refused. See the wearable workflow for local
import and regression commands. The retained historical previews are not rebuilt
in place. Phase 1 adds the missing displayScale=1 metadata field only; pixels,
sources, timing, direction provenance and pending approval remain unchanged.

The attire command checks both bodies, all NESW frames, timing/anchors, frozen body
hashes, source preservation, garment coverage, exposed-body protection and exact
composites. A mask can be authored incorrectly, so numeric success is separate
from actual visual approval. Inspect native/enlarged frames and loop seams,
shoulders/straps, sleeves/hand ordering and waist/underwear coverage. Record approval
per body/direction/frame only after user review. This preview does not prove game
rendering, collision, foot sliding at movement speed or wearable integration.
