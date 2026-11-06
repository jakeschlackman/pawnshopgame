# Player art

- `Bases`: canonical eight-direction bald underwear reference atlases.
- `Wearables/TealWorkShirt`: user-approved male/female south-facing shirt cuts,
  garment-only layers, source artwork, editable recipes and comparisons.

Use [the standard wearable workflow](../../../docs/WEARABLE_ART_WORKFLOW.md).
Approval is per body type, direction and pose. The game loads both base atlases
with `player-sprite.json` for selection and directional movement. Wearables and
sources remain authoring assets; runtime layering/equipment is not implemented.

Rejected prototypes were removed from active content after a verified recovery
archive was made under `.cache/PlayerArtArchive` (ignored by Git). Original art
used by the approved assets remains in each canonical Sources folder.
