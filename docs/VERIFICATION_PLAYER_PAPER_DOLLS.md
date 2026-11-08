# Player paper doll and wearable art verification

Date: 2026-11-05. The user approved the separate male/female south-facing teal
work-shirt fits ("this is perfect"), requested the workflow become standard, and
authorized cleanup of obsolete attempts. No game window or desktop automation
was used. Static asset inspection was explicitly requested and performed.

## Approved files and scope

Canonical assets now live under `PawnShop.Game/Content/Player`:

- `Bases`: two eight-direction 48x64 bald underwear atlases, base metadata,
  original source artwork and normalization recipes.
- `Wearables/TealWorkShirt`: separate male/female PixelLab shirt objects,
  garment-only south layers, composites, raw AI sources, design reference,
  extraction recipes, prompts/IDs/seeds and recorded user visual approval.
- [The standard workflow](WEARABLE_ART_WORKFLOW.md) defines generation, native
  extraction, numeric checks, visual review, pose-specific approval and cleanup.

All base poses are 50 visible pixels tall, rows 8-57, with ground pivot (24,58).
Approval covers both south-facing shirt compositions, not the other seven shirt
directions, animation or in-game scale. No layered renderer, equipment system,
save format or gameplay feature was implemented by this art slice.

## Verification performed

- `./tools/Validate-PlayerBases.ps1`: passed. All 16 base poses obey cell size,
  body height, centering, clear borders, binary alpha and ground-pivot rules;
  all six canonical base/source PNGs decode.
- `./tools/Validate-Wearable.ps1`: passed. All 11 wearable/source/review PNGs
  decode at their documented sizes. Both canonical bases retain their bounds.
  All 6,144 composed pixels equal the original base plus garment layer.
  Required male/female shoulder points and the female bra region are covered.
- Both upper-body composites were inspected at 12x, and the complete comparison
  at 4x. The user subsequently granted visual approval.
- All promoted copies matched their originals by SHA-256. Cleanup compared every
  removed local file with the recovery archive before deletion.
- Both original canonical PixelLab character imports had previously been
  compared with the local base atlases: zero changed pixels for either body.
- Markdown links, JSON, reference paths and whitespace are checked during handoff.

The PowerShell validators ran through approved execution outside the sandbox,
which blocks script execution. No execution policy was changed. No SDK/package
installation, application build/test runner or game launch was needed for these
unloaded art assets and documentation changes.

## Cleanup

The three prototype/iteration folders and their two obsolete validators were
removed from active content after a 61-file verified recovery archive was made:
`.cache/PlayerArtArchive/before-standardization-20261105-212947.zip`.
The ignored `.cache/PlayerArtArchive/latest.txt` points to that archive.
Approved source artwork remains in the canonical folders independently of it.

PixelLab cleanup removed only the two obsolete 64x64 player records
(`812c9296-b8ae-47cd-8f08-4b363237da98`,
`5a13f6ab-c4a8-46cb-bb55-c9e4a409c06d`) and generic shirt object
(`f29be2e3-dcda-4b7a-8f8b-293091a0d907`). Each had no animations.
The approved 48x64 bases and male/female shirt objects were retained and tagged;
Sprout and unrelated assets were untouched. Current IDs are in the manifests.

The MCP has no Creator-gallery deletion operation. Obsolete gallery thumbnails
remain; they must be removed in PixelLab's UI if desired. Their presence does
not make them canonical or approved.

## Lessons retained and remaining work

The first hand-authored clothes/hair failed visual fit. The next extracted shirt
was incorrectly shifted and lengthened, exposing shoulder skin and producing a
poor female cut. Numeric composition checks passed both bad attempts; their
visual acceptance was withdrawn. The approved result instead uses independently
generated male/female shirt cuts, native-coordinate garment extraction and the
unchanged bodies, plus targeted coverage checks and actual visual approval.
No shirt shifting, stretching or insertion of cloth rows is used.

The standard now records those requirements in agents.md, ARCHITECTURE.md,
GAME_DESIGN.md, ROADMAP.md and the dedicated workflow. Future directions/poses
must be reviewed separately. Hair/facial hair, armor, animation, helmet hiding,
mounted poses and in-game scale/readability remain unverified. The subsequent
[player controls slice](VERIFICATION_PLAYER_CONTROLS.md) copies and loads the two
base atlases and shared sprite metadata. Wearables and authoring sources are not
runtime content; the approved shirt fit does not establish in-game acceptance.
