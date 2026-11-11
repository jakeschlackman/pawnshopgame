# Phase 4 — Equipment fit pilots

## Resume checkpoint — 2026-11-07

**Current checkpoint: the generated common shopper now replaces the bundled
room visitor and walks onto the screen from the right.** Its appearance is
generated once per New Game. At 80 logical pixels/second it walks from x=672 to
the existing placement (479,210), then uses walk-1, settling and standing. The
player is unchanged. All layers share the West mirror and 100 ms walk clock.
The approved Common layout, asset promotion and seeded generator remain in place. The user approved the folder layout
("I like this") and authorized implementation ("go ahead"). A subsequent request
requires adding many outfits, hairstyles and hats without generator code changes;
the catalog and regression tests now support that workflow.

- Home: `PawnShop.Characters/Npcs/Common`. Fifty-two approved PNGs are promoted as
  byte-identical copies into `Assets/Bases`, `Assets/Hair` and `Assets/Clothing`.
  `Assets/migration.json` records each original path, destination, SHA256 and
  approval record. Original sources, all review art/HTML, player references and
  Sprout remain preserved; no source cleanup or destructive move was performed.
- `Assets/catalog.json` declares native texture layouts, per-body parts, item
  slots, hair/headgear compatibility and body occlusion. New approved parts/items
  are data-only additions. Seeded selection uses actual compatible fits and
  optional hair/headgear; clothing slots stay filled. All four facings resolve
  from the accepted S/N/E poses, with a single West mirror and side settling.
- `Generation/CommonNpcGenerator` returns caller-supplied stable IDs, seed,
  generator/content versions and realized appearances. It is graphics-free and
  has no PixelLab/network dependency. This is an appearance generator, not
  shopper spawning, browsing, trading, dialogue or save-system integration.
- Game packages the catalog and PNGs into `Content/CommonNpcs`. The bundled
  visitor now uses room-owned layered textures; originals are unchanged. The
  existing visitor identity, dialogue and destination are retained. Its live
  placement drives rendering, click bounds, collision and talking range. It waits
  if the player blocks its entrance and resumes when clear. Pause/dialogue freeze
  both position and animation. Returning to title releases the room graphics;
  another New Game creates one new shopper and restarts the entrance. Injected
  authored NPC catalogs keep their own sprites unless common art is supplied.
- Read [the common NPC authoring guide](../PawnShop.Characters/Npcs/Common/AUTHORING.md)
  for actual outfit/hair/hat examples and the existing-runner validation command:
  `dotnet run --project PawnShop.Tests --no-build -- --assets common PawnShop.Characters/Npcs/Common/Assets`.
  The current five item definitions yield eight appearances; no extra outfit
  artwork was generated. Backpacks remain skipped and held items remain optional.
- Art is for ordinary townies and shop visitors. Main-player and notable-NPC art
  remain a separate strategy. The user approved both bodies' South/North boots
  and East/mirrored-West standing, walking and settling; North's optional wrench
  correction still has its independent pending review.
- Verification: baseline build and 51 tests passed; the implementation build has
  zero warnings/errors and 55 tests pass. Tests cover deterministic identities
  and appearances, data-only outfit/hair/hat additions, compatibility filtering,
  malformed content, source/output hashes, native poses, occlusion and mirroring.
  The asset checker passes. Debug publish output was checked separately.
  The later room entrance slice passes 57 tests and is approved by the user
  ("ok we're good, youre good to clean up"). No agent game/browser launch or
  visual inspection was performed.
- All 725 existing non-generated files were hashed before this implementation.
  All 482 existing PNGs and Sprout remain byte-identical; earlier uncommitted work is
  preserved. Nothing was committed or pushed.

**Room example approved:** the user accepted the entrance example with
"ok we're good, youre good to clean up" and authorized the cleanup below.
Build passes with zero warnings/errors; all 57 headless tests pass, including
entrance timing, obstacle sweep, moving-dialogue pause, click coverage, single
placement and session restart. User approval is separate from those checks; no
agent game/browser launch or screenshot inspection was performed.

**Next:** await the next feature or art instruction. No buying/selling AI or
general population/arrival scheduling is implemented. The optional North wrench
correction retains its separate review status.

### Common NPC folder walkthrough

**Approved root:** `PawnShop.Characters/Npcs/Common`, namespace
`PawnShop.Characters.Npcs.Common`. Common means ordinary generated townies and
shop visitors, separate from authored notable NPCs. This records the user's
destination decision and the subsequent layout approval ("I like this").
The user then authorized implementation; the listed folders now contain the
assets and generator described at the top of this checkpoint.

Approved contents for the migration/implementation slices:

| Destination under Common | Purpose | Current evidence/source |
| --- | --- | --- |
| `Assets/Bases` | Approved body poses and animation metadata, separate male/female art | Historical `PawnShop.Game/Content/Player/Bases`; corrected accepted review poses/layouts, including `WorkJacketNorth` and `WorkJacketEast` manifests |
| `Assets/Hair` | Separate body/direction-specific hair layers and mask compatibility | `docs/ReviewAssets/HairMaskPilot` and `HairMaskEast` |
| `Assets/Clothing` | Approved jackets, jeans, boots and optional headgear; body-specific fit metadata | Corresponding `WorkJacket*`, `Jeans*`, `Boots*` and `WeldingHelmet*` review manifests |
| `Generation` | Seeded compatible appearance selection with caller-owned instance IDs | Implemented `CommonNpcGenerator`; regression-tested data-only outfit, hair and hat additions |

These child folders are implemented. Clothing subfolders use item names rather
than review-phase names.
Do not blanket-copy every historical image into the runtime library: accepted
layers, current corrected poses and manifests identify the actual candidates.
The older standalone teal shirt has South-standing-only approval. Generated
shoppers requiring walking cannot choose it as an all-direction outfit.

Original PixelLab sources, prompt/job provenance, rejected revisions, review
composites and the working HTML remain preserved in their existing authoring
locations during migration. Held-item art is optional and remains preserved;
backpacks are excluded. Sprout remains frozen. No notable-NPC folder rename or
main-player redesign is part of this walkthrough.

Current runtime constraints found in the repository:

- `PlayerSpriteCatalog` and the Game project still load/copy the two canonical
  base PNGs from `Content/Player/Bases`. Moving them without updating their
  consumers would break selection. Keep those references working until an
  explicit replacement/migration is implemented.
- `Npcs/NpcDefinition` describes authored NPC identity and a single sprite.
  `Game/Bootstrap/NpcContentCatalog` resolves authored NPC folders and unique
  placements; it is not a procedural population generator. Keep the shared
  definition at `Npcs`, and add common-specific logic under `Common` as needed.
- The approved asset source belongs to Characters. Game packages selected
  files into build/publish content and continues to own
  texture loading and rendering. Do not introduce a Characters-to-Game reference.
- Appearance generation selects approved compatible parts using seeded randomness
  and gives each realized shopper a stable instance identity and appearance.
  It does not invoke PixelLab at runtime. Buying/selling uses Shop/Core domain
  operations; Game composes those with the generated characters.

Completed implementation: promoted the approved runtime library with explicit
source-to-destination records, preserved existing consumers and review references,
and added a bounded, headless-testable common-NPC appearance generator. Arrival
schedules, browsing AI, transactions and a full
town simulation are separate work; none is claimed complete by this walkthrough.

Implementation and scene-integration verification are recorded in the current
checkpoint above.

For new work, follow AGENTS.md, this checkpoint and the linked manifests. The
migration and appearance-generator slice is complete; subsequent scene behavior
and new art are separate slices.

Historical rejected fits and superseded iteration notes were archived during docs
cleanup: `.cache/DocsArchive/docs-cleanup-20261111-040437.zip`. Current approved source artwork,
fit manifests, the review page and the separate pending North-wrench review
are preserved. The rejected orange-helmet record is an archive pointer only.
