# Wearable art workflow

Approved from the male/female teal work-shirt south-facing fit on 2026-11-05.
This is an authoring and acceptance process. The game loads the chosen standalone
player base for selection and movement. Layered clothing and equipment/customization
rules remain unimplemented.

## Reference files and contract

- `PawnShop.Game/Content/Player/Bases/base-manifest.json` identifies the canonical
  bald underwear bases, eight-direction atlas layout and PixelLab character IDs.
- `PawnShop.Game/Content/Player/Wearables/TealWorkShirt/wearable.json` is the
  approved example: separate male/female object references, sources, transparent
  garment layers, recipes, previews, prompts, seeds, IDs and acceptance scope.
- Each player frame is 48x64. Bald-body height is 48-52 pixels, currently 50,
  occupying rows 8-57; ground pivot is (24,58). Keep the full transparent canvas.
  Clothing may change the outer silhouette without changing the body's scale.
- Base atlas order is S, SE, E, NE, N, NW, W, SW. The current shirt has only S.
  Its visual approval does not approve the other seven directions or animation.

## Repeatable steps

1. **Use the actual base pose.** Select the body type, facing and animation frame
   from the canonical source. Preserve its pixel coordinates, canvas and pivot.
   Start with south-facing idle on both bodies before producing a larger batch.
2. **Generate body-specific objects in PixelLab.** Create a male-cut and a
   female-cut design reference for clothing that needs different fitting. Share
   material, palette and recognizable details. A standalone object/icon is a
   design reference; its image position is not a wearable attachment position.
   Name each object by garment and body type. Use approved artwork as a style
   reference. Check the tool's current size support and charge before submission.
3. **Fit each object to its own base.** Use PixelLab reference editing with that
   exact base as the edit target and its matching shirt object as the reference.
   Require shoulder coverage, correct neckline/sleeves, a natural waist/hem and
   complete concealment of underlying underwear where intended. Keep identity,
   pose, proportions, head, hands and feet. Do not substitute hand-drawn polygon
   shirts for requested AI-generated garment art.
4. **Extract the garment, not the generated person.** Keep the complete AI result
   as source evidence. Inspect garment boundaries and copy only its actual
   pixels onto a transparent 48x64 layer, then compose it over the original base.
   Generated body pixels may drift; never make them the new authoritative body.
   A simple image difference is not a reliable garment mask. The example's
   skin-color heuristic is specific to its palette, not a universal extraction
   rule; other skin tones/materials need explicit masks and fresh inspection.
5. **Keep native alignment.** Do not recenter a layer by its visible bounds, shift
   it by eye, stretch it, or insert/repeat fabric rows to force a fit. If fitting
   is wrong, correct the mask or regenerate/repair the matching garment. Small
   pixel cleanup must preserve the generated design and record exact changes.
6. **Validate the files.** Decode PNGs; check dimensions, alpha, bounds/pivot,
   unchanged base pixels and pixel-exact composition. Include regression checks
   for the visible failure risks: shoulder tops, straps, sleeves, waist and
   unwanted skin/underwear gaps. Numeric checks supplement visual judgment.
7. **Inspect the actual result.** Make a sheet with columns object, original base,
   garment-only layer and composition, one row per body type. Review native size,
   a nearest-neighbor 4x full-body view and 12x joint crops. Compare the collar,
   shoulder seams, sleeve openings, arm position, waist/hem and design details.
   Correct visible defects before presenting the candidate. Obtain user visual
   approval before marking the pose approved or bulk-producing its variants.
8. **Promote and record scope.** Save stable asset names under `Player/Wearables`,
   preserve raw sources and editable recipes, record IDs/prompts/seeds and the
   exact approved body types/directions/poses in the manifest. A seed alone does
   not guarantee reproducibility; keep source image IDs and actual PNGs. Later
   direction/animation batches require their own alignment and visual review.

The same reference-fit/extract/review principles apply to armor, hair and facial
hair, with head-specific masks and appropriate front/rear pieces. Helmet hiding,
per-facing order and shared animation pose selection require explicit contracts
when implemented. They are not proven by this shirt example. PixelLab is an
authoring service only; shipped gameplay must not depend on it.

## Validation commands

From the repository root on Windows with PowerShell/System.Drawing:

```powershell
./tools/Validate-PlayerBases.ps1
./tools/Validate-Wearable.ps1
```

The wearable command defaults to the approved teal shirt and accepts `-AssetRoot`.
It currently checks the two south-facing static body variants. Extend its
pose/coverage checks when another asset needs more; do not report unimplemented
animation validation. A successful command never grants visual approval.

## Cleanup and provenance

Keep one canonical base set and one folder per approved garment design, with
body-specific variants inside it. Rejected iterations are not active content.
Before removing them, archive their source artwork and provenance and verify
the approved copies by hash. Remove only explicitly identified obsolete records
from PixelLab; preserve unrelated assets and approved source/character IDs.
The present local recovery archive is recorded in `.cache/PlayerArtArchive/latest.txt`
and is intentionally ignored by Git. Approved source artwork remains in the
canonical folders, so those folders do not depend on that local archive.

PixelLab's current MCP supports deleting characters and objects, but not Creator
gallery images. Obsolete gallery thumbnails may remain until removed through
PixelLab's own UI. Do not silently replace their pixels to disguise old entries.
