# Wearable art workflow

**Current ownership decision:** this modular body/hair/clothing library is for
common NPC townies and shoppers, with its approved destination at
`PawnShop.Characters/Npcs/Common`. Main-player and notable-character art is
separate. Existing player-prefixed paths below are still the actual source paths;
they have not been moved or deleted. Approved runtime layers now have byte-identical
copies under `Common/Assets`, with a source mapping in `Assets/migration.json`.
The body-specific fitting and approval rules continue
to apply. Backpacks are skipped; held-item art is optional preserved evidence.
See the [folder walkthrough](VERIFICATION_EQUIPMENT_PILOT.md#common-npc-folder-walkthrough)
for the approved layout and preservation constraints. The seeded appearance
generator and data-driven catalog now exist. Use the
[common NPC authoring guide](../PawnShop.Characters/Npcs/Common/AUTHORING.md)
when adding outfits, hair or hats; existing source recipes below remain historical
authoring evidence, not the destination for new common-NPC runtime content.

Approved from the male/female teal work-shirt south-facing fit on 2026-11-05.
This is an authoring and acceptance process. The game loads the chosen standalone
player base for selection; the accepted Phase 2 room uses diagnostic layered
placeholders. Production clothing and equipment/customization rules remain
unimplemented. The Phase 3 standing/walk/side-settle checkpoint is accepted;
animated idle and runtime promotion remain pending. Phase 4's first male/female
jacket standing-fit pilot is in [VERIFICATION_EQUIPMENT_PILOT.md](VERIFICATION_EQUIPMENT_PILOT.md).

**Required coverage:** generate every player clothing/equipment design for both
male and female models. Use the same recognizable item design with independently
fitted body variants. Neither body is an optional follow-up. This applies to
shirts, trousers, footwear, armor and other worn items; genuinely identical art
may be reused only after both fits are checked. One economy item can resolve to
two body-specific art sets.

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

### Hair and the welding mask

The [hair compatibility pilot](ReviewAssets/HairMaskPilot/review.json) keeps hair
in independent transparent PNG layers. A normal hair sheet remains unchanged;
equipping this mask selects a separately alpha-clipped hair variant, then draws
the unchanged mask above it. Unequipping restores normal hair. Rear strap gaps
show hair instead of indiscriminately hiding the entire hairstyle. Clipping and
hair move with the same body-specific head anchors as the equipment.

The current pilot covers one short brown style on both bodies in South/North
standing and walking, accepted by the user with "looks good, next phase". It is authoring-preview
behavior, not a production hair/equipment system. Other hairstyles, particularly
bulky or long styles, require their own fit and occlusion review; this rule is not
a blanket compatibility claim. Preserve canonical body and equipment artwork.

East/mirrored West fits, including the existing side settling pose, are in
`docs/ReviewAssets/WeldingHelmetEast` and `docs/ReviewAssets/HairMaskEast`, accepted
by the user with "great, next phase". Hair, mask and jacket share the accepted body's pose clock
and character-space mirror. Open straps reveal hair beneath their transparency.

### Held-tool fit pilot

The carried wrench points **upward and slightly inward toward the torso**;
the original fist remains unchanged, as explicitly clarified by the user. At the user's
explicit instruction, all 22 existing wrench derivative PNGs were corrected in
place across `docs/ReviewAssets/WrenchPilot`, `WrenchWalkSouth` and `WrenchNorth`.
There is no separate downward-facing derivative set or new version folder.
The corrected South/North standing and walking fits on both bodies are accepted
(user: "good enough, whats the next phase?"). Earlier orientation approvals
remain historical; the current manifests identify the accepted PNGs.

The manifests select 31 tool pixels from the original PixelLab fitted source.
Each source row receives a whole-pixel inward offset: rows 42–51 use
`[-1,0,0,1,2,2,3,4,4,5]`. South adds that offset to
`gripX + 15 - sourceX`; North subtracts it from `gripX + sourceX - 15`.
Both use `y = gripY + 45 - sourceY`. The resulting lean is approximately 33 degrees
from vertical, about 15 degrees more than the initial inward fit, as requested.
This is a pixel-stepped diagonal fitting,
not a mathematically exact rotation. It retains the source pixels and palette
without interpolation, new paid generation or changes to the body/animation.

The source grip remains (15,45), but follows the dark crease of the original fist:
one pixel inward from the hand anchor, recorded as `toolGrip` per pose.
Original opaque hand pixels clip source rows 43–46. Row 47 stays visible just
above the fingers, connecting the shaft to the jaw instead of disappearing into
the cuff. Fitted layers retain 21–22 pixels; exact hand masks vary by pose.
All 15 jaw pixels and three handle-end pixels stay visible. The fitted layer
reveals the original fingers at `toolGrip`. Do not require the shaft to pass
through the center of the palm or change the fist's pose. The jaw leans inward
from the fixed grip; the short handle end leans outward.
Standing and walking use the same fit rule and existing frame clock.

South walking sheets have eight columns; North sheets contain standing in column
0 and walking in columns 1–8. All layers share the existing pose clock.

`ReviewAssets/WrenchEast` adds separate male/female side fits with standing in
column 0, walking in 1–8 and middle settling in 9. The same accepted tool pixels
use independently measured East hand grips and finger rectangles. Per the user's
side-view correction, the wrench leans about 45 degrees from vertical, roughly
12 degrees lower than the first side fit. Source placement is
`x = gripX + 15 - sourceX + sourceY - 45`, `y = gripY + 45 - sourceY`.
Original hand pixels stay unchanged. West mirrors every layer and crop in the
same 48-pixel character space; no West PNG or independent tool clock is needed.
East/West standing, walking and settling are accepted (user: "good, next phase").
Tool-use actions are separate work.

The user subsequently corrected North visibility: the wrench is held in front of
the character, so its head must be hidden from the rear. North fitted layers now
exclude the 15 jaw pixels in source rows 48–51 per pose, leaving the original
6–7 visible handle pixels and original hand. Unoccluded tool sheets remain fitting
references only. North body/equipment and jeans-equipped composites are refreshed;
South/East/mirrored West are unchanged. This visibility correction awaits review.

### Remaining equipment checkpoints

The South-standing dark blue jeans pilot is in `ReviewAssets/JeansPilot`.
Separate male/female object references were fitted to the exact bases. The user
requested richer denim on both, with shaped folds, clear pockets/seams and subtle
wear. A paired PixelLab Pro detail edit supplies the current shading; original
body-specific garment masks stay fixed. New pixels outside those masks are
discarded, and existing fitted edges are retained where the model removed alpha.
The manifest records all masks, palette maps and retained edge pixels. Pants draw
under the jacket, while original hands and bare feet remain unchanged. This pilot
was accepted for South standing with "perfect, next phase". Separate South-walking
fits in `ReviewAssets/JeansWalkSouth` were accepted after correcting the female
left-thigh mask (user: "goood, next phase"). Each body's
eight exact poses use its approved standing reference through PixelLab outfit
transfer. Extracted denim follows native per-pose masks, with original hands and
feet protected and explicit boundary/gap cleanup recorded. The preview shares
one clock across body and gear and never pastes a standing layer onto walking legs.

North standing and walking fits in `ReviewAssets/JeansNorth` are accepted on both
bodies (user: "looks good, next"). Column 0 is standing, columns 1–8 are the 100 ms walk poses.
Same-body reference transfer supplies rear pockets, seams and folds; original
body pixels remain authoritative. Garment masks protect actual hands without
cutting into thighs. Explicit skin-gap checks supplement native fit checks.

`ReviewAssets/JeansEast` contains accepted side fits on both bodies: standing column 0,
walking 1–8, middle settling 9. Start/stop reuse the existing sequence and exact
endpoint poses. West mirrors all layers, offsets and crops in full 48px character
space; no West artwork is generated. Original side hands/feet remain protected.

The user accepted the repaired side jeans with "fixed in, next phase".
`ReviewAssets/BootsPilot` now contains separate male/female brown leather work
boots for South standing, approved by the user with "it looks good, next phase".
Each has its own PixelLab
object reference and exact-body fitted source. Extract boot pixels on the original
48x64 canvas, retain ground row 57, and draw boots before jeans so the accepted
hems overlap the tops. A shared seven-color leather/sole palette keeps the design
consistent. The extra generated sole row is discarded without moving or resizing
the body. Boots-alone, jeans and full-equipment composites are available in the
existing preview. These static fits do not approve any walking or other direction.

`ReviewAssets/BootsWalkSouth` fits the same-body approved standing boot references
to all eight accepted South walk poses using PixelLab outfit transfer. Only boot
pixels are extracted onto original bodies. Per-leg masks retain each pose's actual
foot bottom (not the standing ground row); shared palette mapping and two recorded
one-pixel edge repairs finish the extraction. Boots remain beneath the jeans hems.
The preview shares the body frame clock and includes contact strips and foot crops.
Both South-walking fits are approved (user: "perfect"). North standing/walking
is the next unstarted checkpoint; East/mirrored West and settling follow.

After the accepted wrench, complete pants, shoes/boots, and backpacks
as separate Phase 4 checkpoints, in that order. The first pants are dark blue jeans,
beginning with both South-standing fits before generating motion. Each needs both body fits,
South/North/East poses, mirrored West, walking, side settling and combination
checks. Pants and footwear need cuff/hem and foot alignment checks; backpacks
need appropriate rear-bag/front-strap layering. Jeans are accepted in all supported
poses; boot motion/directions and backpacks remain planned work.

### Before production integration

The user will walk through folder organization before runtime integration.
Continue art work in review assets and HTML previews until that review; do not
move files or extend production item/character code ahead of it. The agreed
ownership direction is character-specific logic in `PawnShop.Characters` and
shared item definitions/instances in `PawnShop.Core/Items`. Core remains free of
textures and drawing code. Final PNG/catalog/concrete-renderer placement is still
to be settled with the user. Existing project boundaries and Sprout are preserved.

When production equipment is implemented, write concise Markdown authoring guides
for the supported item types: torso clothing, pants, footwear, headgear, held
tools/weapons and backpacks. Base each guide on a working example and the actual
folder layout, definitions and renderer. Cover file locations, male/female art,
frames/timing/pivot, layering or attachments, mirroring and validation. Do not
invent production paths or add these guides ahead of the folder walkthrough.

## Walking and attire

Author **south, north and east** using Skeleton v3 for both bodies. West will use
East's body and equipment frames with MonoGame `SpriteEffects.FlipHorizontally`.
All layers, masks, anchors and held-item offsets share one character-space mirror
and frame clock. Opposite-hand appearance is acceptable; independently authored
West requires a specific justified exception.

Freeze each accepted body animation before fitting garments. Fit the same item
separately to each body's exact poses; do not animate clothes independently.
Record the body-sheet hash. Extract only garment pixels and compose over the
unchanged body, using coverage/protected masks for each frame. Check shoulder,
underwear, sleeve/hand, waist and foreground-arm occlusion. A regeneration
invalidates the affected fit approvals until reviewed again.

The current `Bases/Walking` files are **legacy candidates**: four NESW rows,
eight frames at 100 ms, generated W/N/S with copied E. Their sources, approval
status and provenance remain unchanged. Phase 1 validates that historical format;
it does not relabel the art or implement the new three-direction runtime.
The static shirt's approval remains south-standing only.

The legacy attire template requires both bodies, identical frame order/timing,
locked body hashes, garment layers, coverage/protected masks and composites.
Null paths fail explicitly. Preserve historical asymmetric exceptions as evidence;
new art follows the East-to-West policy above.

## C# authoring commands

Build first, from the repository root:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project PawnShop.Tests --no-build -- --assets bases PawnShop.Game/Content/Player/Bases
dotnet run --project PawnShop.Tests --no-build -- --assets wearable PawnShop.Game/Content/Player/Wearables/TealWorkShirt
dotnet run --project PawnShop.Tests --no-build -- --assets walk PawnShop.Game/Content/Player/Bases/Walking
dotnet run --project PawnShop.Tests --no-build -- --assets walk <walk-root> <garment-folder>/walking.json
```

Validation is read-only and uses the real runtime sprite metadata parser.
PNG decoding uses System.Drawing.Common 10.0.0 in Tests only, retains the previous
Windows authoring requirement and needs no graphics device. Ordinary builds do
not execute these commands or modify artwork. The static-shirt reader retains
the legacy manifest's filenames and shirt-specific skin-color coverage check;
it is not a general automatic extraction algorithm. New items should use
explicit masks and fit expectations, not copy that color heuristic.

Packing and reviewing are separate commands, each requiring a **new directory
outside its input directory**. The following paths are examples; choose fresh
ones when repeating a command:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets pack PawnShop.Game/Content/Player/Bases/Walking .cache/walk-packed-new
dotnet run --project PawnShop.Tests --no-build -- --assets review .cache/walk-packed-new .cache/walk-review-new
```

Pack copies verified sources and their manifest, writes two pixel-identical
legacy body atlases, runtime-readable metadata (including displayScale=1), and
an incomplete attire template. It does not write previews or promote assets.
Review reads a validated packed set and writes only native/4x contact sheets and
a standalone animated HTML preview. That viewer supports frame stepping and one
matching garment sheet per body; it does not yet test multiple equipment items
or the Phase 2 runtime mirror. Neither operation modifies its input or overwrites
an existing directory. If an I/O failure leaves a partial output, inspect it and
choose a fresh destination for retry; no automatic cleanup of user files occurs.

For local PixelLab exports already arranged under the manifest's relative paths:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets import <manifest-folder>/walk-manifest.json <download-directory> <new-directory>
```

Import requires every expected source hash already recorded. It rejects changed,
missing, duplicated or escaping source paths, validates PNGs/poses, and copies
the original manifest unchanged. It has no network access, token handling, paid
job retries or automatic hash rewriting. Record trusted hashes during deliberate
source intake; never use import to bless unexpectedly changed content. Preserve
all original responses, prompts, IDs and rejected sources separately.

Regression checks are part of the existing runner:

```text
dotnet run --project PawnShop.Tests --no-build
```

No command grants approval or copies assets into production. Before manual
promotion, require exact per-body/direction/action approval and matching hashes.
Review every pose and loop at native size and nearest-neighbor enlargement.
Future runtime equipment and production promotion validation belong to later
phases; do not infer production readiness from legacy validation.

## Cleanup and provenance

Five human authoring PowerShell scripts were replaced by `PawnShop.Tests/Assets`.
The three one-time cached import/promotion/cleanup helpers are retired too.
Their exact pre-removal files are preserved in
`.cache/Phase1ToolArchive/legacy-scripts.zip`; this local recovery archive is
ignored by Git. Earlier rejected-art archives under `.cache/PlayerArtArchive`
remain intact. Canonical approved artwork is independent of these archives.

Sprout's builder, assets and implementation are frozen. NPC/dialogue scripts
remain unchanged for a separate authoring cleanup. No new PowerShell files,
projects, generation service, custom image codec or runtime dependency was added.
