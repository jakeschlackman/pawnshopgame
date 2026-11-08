# Common NPC appearance library

This folder owns the modular art and seeded appearance generator for ordinary
townies and shoppers. The player and notable characters use separate designs.
The bundled room uses one generated appearance for the existing visitor, with
a bounded entrance from the right. It retains that visitor's authored dialogue.
General browsing, trading and save-system integration are not implemented here. PixelLab is an offline authoring tool, never a runtime dependency.

## Files and ownership

| Location | Contents |
| --- | --- |
| `Assets/Bases/<body>/` | Approved native body sheets |
| `Assets/Hair/<style>/<body>/` | Normal hair and explicitly fitted headgear variants |
| `Assets/Clothing/<item>/<body>/` | Body-specific clothing/headgear sheets |
| `Assets/catalog.json` | Runtime texture, pose, item and compatibility definitions |
| `Assets/migration.json` | Repository-relative original sources, approval records and SHA256 hashes; not shipped |
| `Generation/CommonNpcGenerator.cs` | Seeded selection of catalog choices |
| `CommonNpcCatalog.cs` | Headless validation and ordered frame/layer resolution |

Game copies the PNGs and catalog to `Content/CommonNpcs` in build/publish output.
Edit the Characters-owned sources, never `bin`, `obj` or a publish directory.
Review sources and composites remain under `docs/ReviewAssets`; the existing HTML
still works. The player selection screen still uses its existing base references.
Promotion copied approved bytes without deleting those consumers or originals.

## Add an outfit piece

1. Author and visually approve separate male/female fits using the
   [wearable workflow](../../../docs/WEARABLE_ART_WORKFLOW.md).
   Keep the 48x64 full canvas, pivot (24,58), eight 100 ms walk poses, and exact
   body motion. Approve South/North/East and whole-character mirrored West,
   including East/West settling. Standing-only approval cannot enter this library.
2. Place only approved layer PNGs in `Assets/Clothing/<item>/<body>/`. Retain
   original AI sources and review evidence separately. Add provenance entries to
   `migration.json` when promoting existing reviewed files; record new source and
   approval evidence when authoring new designs.
3. Add each PNG to `catalog.json`'s `textures` with its relative path, SHA256 and
   native column count. No image resizing or sheet normalization is required.
4. Under each body's `parts`, add a unique part ID and three `views`: `south`,
   `north`, `east`. Copy the structure of `teal_work_jacket`, replacing paths and
   frame indices with the actual sheet layout. Every view needs a one-frame
   `standing` and eight-frame `walking` clip; East also needs a one-frame
   `settling` clip. North/South settling is `null`. Every clip names `texture`,
   `firstFrame` and `frameCount`.
5. Add an `items` entry with a stable ID, slot and fits. Slots are `torso`, `legs`,
   `feet`, `hair`, `headgear`. Clothing slots are always filled; hair/headgear may
   be absent. The current `teal_work_jacket` entry is a working example:

```json
{
  "id": "teal_work_jacket",
  "slot": "torso",
  "fits": [
    { "bodyId": "male", "partId": "teal_work_jacket", "headgearParts": {}, "bodyOcclusion": [] },
    { "bodyId": "female", "partId": "teal_work_jacket", "headgearParts": {}, "bodyOcclusion": [] }
  ]
}
```

Each body resolves its own part even when both use the same item/part ID. The
generator filters by actual body fits; missing fits never borrow another body's
art. Complete both body approvals before considering a new design finished.
Separate tops/bottoms must be checked together for coverage and overlap. This
version does not model coordinated outfit sets or special full-body garments.

## Add hair and hats

Use the same item/part process for `Hair/<style>` and `Clothing/<hat>`.
Hair's `headgearParts` maps a **headgear item ID** to a fitted **hair part ID**
within that same body's parts. The current short hair declares:

```json
"headgearParts": { "charcoal_welding_mask": "short_chestnut_masked" }
```

For every new hat, explicitly approve each desired hairstyle combination. Map
to a clipped hair variant, or to the normal hair part if that exact combination
fits unchanged. Missing entries mean incompatible, not an implicit generic crop.
The generator chooses headgear first, then compatible hair or no hair. A hat with
no compatible hairstyle can still appear on the bald base. Non-hair items use
an empty `headgearParts` object.

Only headgear fits may declare `bodyOcclusion`. This removes original body pixels
before drawing the other layers. The male welding-mask fit gives a working South
ear example: native `pixels`, one `direction`, and eight `walkOffsets` for vertical
gait bob. Standing/settling use zero offset. East occlusion mirrors with the body
for West. This supports the current masks; more complex per-pose occlusion needs
an explicit schema extension rather than silently shifting art. Non-headgear items
use an empty array.

## Version, check and use

Keep `version: 1` for this schema. Increment `contentVersion` when changing the
available choices or fitted content. IDs must stay stable; renaming an ID changes
existing realized appearances. Adding ordinary outfits, hair or hats requires
JSON/PNG changes, not generator code or C# registration.

From the repository root:

```text
dotnet build PawnShopSimulator.sln --no-restore
dotnet run --project PawnShop.Tests --no-build -- --assets common PawnShop.Characters/Npcs/Common/Assets
dotnet run --project PawnShop.Tests --no-build
```

The read-only checker validates hashes, dimensions, clip bounds, required poses,
IDs, body fits and hair/headgear references. It does not approve appearance.
The regression suite also exercises data-only additions and preserved references.

Example using the packaged catalog (no graphics device required):

```csharp
using PawnShop.Characters.Npcs.Common;
using PawnShop.Characters.Npcs.Common.Generation;

var catalog = CommonNpcCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Content", "CommonNpcs"));
var generator = new CommonNpcGenerator(catalog);
var shopper = generator.Create(Guid.NewGuid(), seed: 123456UL);
var pose = catalog.Resolve(shopper.Appearance, CommonNpcFacing.West, CommonNpcPose.Walking, walkFrame: 0);
```

Allocate an ID once per realized shopper. Reusing a seed reproduces appearance;
it does not allocate identity or enforce population uniqueness. Choices are
uniform among compatible IDs, with absence included for optional slots. The
current five item definitions yield eight combinations, not eight newly drawn
outfits. Adding content can change seeded results, so preserve the realized
appearance plus catalog/generator versions when a future save system integrates
these records; do not save only the seed. No save-file contract is introduced here.

The frame plan returns body, feet, legs, torso, hair, headgear in draw order, with
native texture rectangles, body occlusion and one whole-character mirror flag.
Texture IDs resolve relative to `Content/CommonNpcs`. Apply occlusion to the body
only, then compose all layers and mirror once. Do not reverse walk chronology.
The caller owns the shared 100 ms clock; `Resolve` never advances animation.
Side settling is a separate pose, not a ninth walking frame. The accepted review
uses standing → settling → walk-1 on start and walk-1 → settling → standing on stop;
the generator does not implement locomotion or transition scheduling. The separate
`CommonNpcEntrance` now implements this stop sequence for the bounded room example.
It moves at 80 logical pixels/second, waits when blocked, and is advanced only by
the room simulation. It is not a general navigation or shopper-behavior system.
