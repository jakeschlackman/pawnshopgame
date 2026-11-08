# Picker's Hollow — PixelLab MCP Character & Modular Equipment Production Standard

**Status:** Implementation specification  
**Game:** Picker's Hollow (C#, MonoGame)  
**Camera/style:** Cozy top-down/three-quarter pixel art, Stardew Valley-inspired readability  
**Direction policy:** Four gameplay facings; author/store **South, North, East** and render **West by horizontally mirroring East**. No independently authored diagonals or West unless a concrete exception is approved.

**Implementation status:** Phase 1 C# authoring cleanup and Phase 2 cardinal/mirrored rendering foundation implemented. The room uses explicitly labeled male/female placeholder bodies and synchronized gear; selection keeps original art. Historical NESW walk candidates retain W-to-E provenance and pending approval. Production human animation and equipment remain Phases 3/4. Sprout is frozen. See docs/VERIFICATION_ASSET_CLEANUP.md and docs/VERIFICATION_CARDINAL_RENDERING.md.  
**Goal:** Game-ready, consistent, reusable characters and independently equipable gear—not merely attractive concept art.

**Team model:** Human developers and AI coding agents work on the same straightforward C# codebase. Favor obvious, inspectable code over generated automation machinery.

## 0. Scope and simplicity mandate (READ FIRST)

**This project already has an architecture. Do not replace it with a new framework, asset platform, build orchestrator, or collection of generated scripts.** The aim is an art production workflow that ordinary C#/MonoGame developers can understand and maintain.

### In scope

1. **Human player, male and female:** both selectable appearances must work with the same four-direction animation contract and a shared *equipment definition system*. Start with two independently approved base sprite sets; do not simply recolor or scale one to simulate the other. Offer different hairstyles and clothing without assuming gender locks any outfit or item. If their body proportions differ, allow explicitly mapped overlay variants under the same item ID rather than stretching sprites at runtime.
2. **Human NPCs:** reuse the same animation conventions and equipment asset catalog wherever proportions allow. Distinctive NPCs may have their own art while sharing runtime code.
3. **Clothing and equipment:** jackets, pants, boots, helmets, hats, gloves, backpacks, scavenging tools and weapons. All wearables must appear during movement, not only in inventory portraits.
4. **Rideable dungeon mech:** large, egg-shaped, two-legged, non-speaking, expressive mechanical companion/vehicle with arms, retro-futuristic industrial silhouette. Four-direction locomotion, idle, enter/exit, and player-riding states. Treat the mech as **one separate animated vehicle entity with its own rider attachment/occlusion points**, not as humanoid armor or a body-equipment overlay. The rider should be a separate sprite/layer drawn into its seating position.
5. **Sprout:** existing companion art is **approved and frozen**. Do not regenerate, edit, recolor, resize, migrate or replace Sprout's assets unless explicitly requested. Existing Sprout code should be left alone except where a genuinely shared rendering bug requires a narrowly reviewed fix.

### Complexity budget / forbidden default behavior

- **No new `.ps1` files.** No script-generating script, PowerShell orchestration layer, custom code generator, service host, plugin framework, message bus, event-driven asset pipeline, database, or sprawling DI changes. If a one-off command is needed, show it in the README or use an existing project command. If a script is unavoidable, ask first and explain why a simple C# solution is insufficient.
- **No mandatory new project by default.** First attempt to fit asset processing into one small, clearly named C# console command/tool within the existing solution (or an existing tooling project). Add one new tooling project only when justified by the actual repository. A simple checked-in `assets/` folder, JSON manifests, and an ordinary `dotnet run -- ...` entry point are enough for the pilot.
- **Keep the feature small:** one renderer, one animation contract, one equipment definition, one lightweight importer/validator. Avoid abstract base classes, factories, interfaces or services without an immediate second implementation/use case.
- **Human-first files:** descriptive names, short functions, comments explaining art-specific pitfalls, small JSON files that can be edited by hand, and no undocumented magic generation settings.
- **No refactoring unrelated gameplay systems.** Never remove user code/assets or alter DI architecture merely to accommodate the asset pipeline. Prefer minimal additive edits and reviewable diffs.
- **Do not silently produce hundreds of files.** Announce the proposed output file count and directory layout, then request approval for large batches. Keep raw generations out of the runtime Content tree.

### Scope-specific sequencing

**Milestone 1:** Inspect current code and demonstrate four-direction base drawing with placeholder overlays, using the existing architecture.  
**Milestone 2:** Two human base characters (male and female) with approved idle/walk, both demonstrating **one shared item ID** (e.g., jacket) through rig-compatible overlays.  
**Milestone 3:** Helmet, boots, backpack and hand tool/weapon on both human bases; validate cross-equipment combinations.  
**Milestone 4:** NPC reuse of apparel and animation system.  
**Milestone 5:** Separate four-direction rideable mech and rider seating/occlusion, including mount/dismount.  
**Milestone 6:** Expand the asset catalog after quality and maintainability review.

Each milestone must be usable, reviewable, and independently mergeable. Do not implement all milestones in one Codex turn.

## 1. Non-negotiable rules

1. **Four supported facings, three authored directions:** S, N, E; W uses E with MonoGame `SpriteEffects.FlipHorizontally`. For diagonal input, choose the most recently dominant cardinal axis.
2. **One canonical rig per compatible body family.** Start with `human_standard_v1`, with a 48×64 transparent logical frame. This is a canvas size, not an instruction to fill every pixel.
3. **Do not generate a fresh character for each outfit.** Preserve the approved base character and its animations. Equipment is a separate runtime asset.
4. **No automatic acceptance.** AI output is a candidate until inspected in all directions, animated, overlaid on the canonical body, and approved.
5. **Never blindly subtract one generated image from another to obtain gear.** AI edits can change anatomy, pose, outlines, and shading. Use masks, pixel inspection, and manual repair where needed.
6. **Mirror West from East by default.** Apply the same character-space reflection to body, all equipment, anchors and offsets. Opposite-hand appearance is acceptable. Author separate West only for a concrete gameplay/visual requirement.
7. **Generate one pilot before batches:** one human; idle and walk; one hat, one jacket and one held tool. Stop for review before expanding.
8. **Keep the shipped game offline.** PixelLab runs in an authoring process, not within the game's runtime. Never ship an MCP token.
9. **Preserve sources:** original PixelLab responses, candidate PNGs, prompts, references, chosen seeds, source job IDs, approvals and export hashes. Never silently overwrite approved assets.
10. **Avoid claiming PixelLab itself produces separate paper-doll layers.** Its outfit/state/editing output is often a fully composited character. The extraction and alignment process is ours.

## 2. Art direction for Picker's Hollow

- Warm, whimsical, readable pixel art; cozy post-industrial salvage setting; lovingly worn jackets, work boots, improvised tools, scavenged protective gear.
- Clear silhouette and deliberate pixel clusters; restrained palette; cohesive outlining and shading. No noisy dithering, subpixel gradients, blurry scaling, painterly shading or excessive microdetail.
- Consistent proportions across cardinal views. Anatomy must not change when rotating. Head, torso, shoes and accessories maintain recognizable proportions.
- The base sprite should remain usable unclothed except for a neutral permanent outfit; this avoids accidental baked-on armor.
- **Do not use Sprout's 48×48 canvas as the human rig.** Sprout is a companion with its own independent proportions and animation rules. Proposed player base is 48×64; the rideable mech can have a larger logical canvas.
- Keep **separate golden reference packs** for male player, female player, and later NPC body rigs: approved south/north/east neutral PNGs and reviewed mirrored West, palette, scale/proportion notes and one approved walking loop. Always supply the applicable reference pack as inputs when tool parameters permit.

## 3. Model/tool selection (verify MCP schemas before use)

PixelLab's tool list can change. Use the currently connected MCP tools and inspect their schemas/help before executing; do not invent arguments or assume a particular `mode` exists.

| Task | Preferred capability | Notes |
|---|---|---|
| Base concept | `create_image_pro` or available Pro character generator | Iterate on **one** south-facing concept; freeze it once approved. |
| Cardinal rotation | `create_character` using 4 directions, V3 with reference if supported | If V3 supports only 8 in the available schema, use a four-direction mode or generate eight and **export only S/N/E**; do not pay for diagonals if a four-direction route is available. |
| Canonical animation | `animate_character` (or matching managed animation capability) | Generate south walk first, validate before other directions. |
| Outfit/armor reference | `create_image_pro`, `create_character_state`, `edit_image` or `inpaint_image` | Try changing only the target region; don't let the model redesign the body. |
| Multi-frame clothing | `transfer_outfit` if available; otherwise multi-frame `edit_image` | Output may be composited. Must extract or manually author overlays. |
| Rigid props | Pro/Pixen image tools and optional directional object creation | Wrenches, lanterns and similar items use hand anchors and pixel offsets. |
| Repair | `inpaint_image` or local pixel editing | Correct defects selectively; do not reroll entire character for one bad pixel. |

Official references: https://www.pixellab.ai/mcp and https://api.pixellab.ai/v2/docs . The REST API may offer features not exposed in the active MCP server; if essential, report that gap and ask before introducing a new API integration.

**Important:** Four gameplay facings do not require four stored visual sets. Prefer reference V3's four-view route over eight when suitable, but retain only S/N/E in new production assets. Animation accepts explicit S/N/E directions. Do not invent a three-direction character count. Pro Flash character currently returns eight views; preserve extra outputs as sources only.

## 4. Canonical coordinate contract

- Human character body: proposed transparent 48×64 RGBA PNG for every direction/frame. This is the shared logical canvas, not a promise that different body shapes accept the same pixel-perfect equipment overlays.
- Same bottom-center **feet/world anchor** (24,58) for every direction and animation frame. Avoid automatic tight-cropping of individual frames.
- New production authored directions: `south`, `north`, `east`, explicitly mapped in metadata. West resolves to East plus a flip; do not rely on export ordering. Legacy NESW and eight-facing sources keep their existing layouts until separately migrated.
- Standard animation names: `idle`, `walk`, `interact` to start. Target authored frame counts: idle 4, walk 8, interact 6. These are pipeline targets, not promises about any one generation call.
- Each animation has a single timing clock shared by body and wearable layers, indexed by direction and frame.
- Canonical frame metadata includes feet, head, back, left hand, right hand, waist anchors. Coordinates are pixel-space relative to the upper-left corner.
- Distinguish anatomical hands from screen sides. Reflect anchors and item offsets with the full composition. Switching East/West may put an item in the opposite apparent hand.
- Full-frame wearables match the same 48×64 image dimensions, frame index and feet anchor. Rigid held props may use smaller images positioned by hand anchors and per-frame offsets.
- No automatic alpha premultiplication surprises. Normalize source PNG transparency and test MonoGame blending.

## 5. Equipment data model and render behavior

**Equipment slots:** Head, Face, Torso, Arms, Hands, Legs, Feet, Back, MainHand, OffHand. Hair is appearance, not equipment, but supports hide/clip rules.

Equipment `id` and inventory logic are separate from `EquipmentVisual`. **An equipment item can reference male/female/body-rig-specific overlay atlases while remaining one gameplay item.** Do not insist every player body use exactly the same texture when silhouettes differ. Multiple visual pieces may belong to one item: backpack body behind torso, straps in front; a helmet back rim and front visor; a long coat with portions behind legs.

Suggested render groups: `BehindBody`, `BehindHead`, `BodyClothing`, `LegWear`, `Footwear`, `TorsoWear`, `BehindArms`, `Arms`, `Headwear`, `Facewear`, `FrontAccessories`, `HeldBack`, `HeldFront`. **These are semantic groups, not a universal fixed draw order:** resolve the order for each direction and, where necessary, each frame.

Each item manifest should specify:

```json
{
  "id": "helmet_rusty_welder",
  "slot": "Head",
  "compatibleRig": "human_standard_v1",
  "canvas": [48, 64],
  "directions": ["south", "north", "east"],
  "hidesAppearance": ["hair_top"],
  "visualParts": [
    {"part": "back_rim", "renderGroup": "BehindHead", "atlas": "helmet_back.png"},
    {"part": "front_mask", "renderGroup": "Headwear", "atlas": "helmet_front.png"}
  ],
  "animations": {
    "idle": {"frames": 4, "millisecondsPerFrame": 200},
    "walk": {"frames": 8, "millisecondsPerFrame": 100}
  },
  "source": {"generator": "pixellab", "approval": "pending"}
}
```

When equipment lacks a full animation, it must have a documented fallback (e.g., static rigid item attached to a hand anchor). **Never silently repeat a mismatched wearables frame** and call it supported.

## 6. Controlled authoring pipeline

### Gate A — Golden human base characters

1. Write one concise fixed character description and supported exclusion guidance covering blurry outlines, 3D, gradients, anatomical drift, asymmetry errors and off-palette pixels.
2. Produce a few **south-facing** candidates, not dozens of complete four-way sheets. Save all candidates and source metadata.
3. Human reviews silhouette, palette, pixel clusters and consistent appearance with Picker's Hollow title/background aesthetic.
4. Lock the chosen south PNG as the source of truth. Generate north/east from that source using a supported rotation tool; discard extra views from production with explicit reference support where possible. Repeat this independently for the male and female base variants. Both must use the same frame convention and world origin.
5. Compare against approved south and reject changes in head size, body proportions, outfit identity, silhouette and perspective.

### Gate B — Master motion

1. Generate south walk only using Skeleton v3. Confirm feet alignment, left/right leg alternation, loop closure and stable appearance.
2. Generate North/East walks using the same reference pack and Skeleton v3, then idle. West is rendered from East. Treat the selected **body frames** as immutable canonical animation.
3. Normalize frames into fixed 48×64 canvases. Align feet, not bounding-box centers. Record anchors per direction/frame.
4. If outputs have four walk frames, use those as the true timing reference unless a controlled interpolation/manual pipeline can make a visually approved eight-frame cycle. **Do not duplicate four frames and pretend they are eight unique frames.**

### Gate C — Single equipment candidate

1. Write an item brief including silhouette, placement, palette, front/back pieces, occlusion, compatible rig and any attachment anchor.
2. Generate the item concept; make one approved reference on a canonical south-facing body.
3. Use character state / outfit transfer / masked edit to generate the **same item** across S/N/E and target animation frames; review mirrored West too. Group frames into calls within current API limits.
4. Produce transparent overlay candidates. For each frame, mask only intended equipment pixels; preserve exact canonical body coordinates and define covered-body occlusion.
5. Composite overlay atop the **original** immutable base frame—not atop an AI-mutated body. Review at native resolution and 4× nearest-neighbor zoom.
6. Reject changes to face, hands, body width, gait or original body palette outside the intended mask. Repaint or selectively regenerate defects.
7. Export `item_id/{animation}/{direction}/{frame}` to deterministic packed atlas and JSON metadata, then mark approved.

### Gate D — Combination testing

Test on **both player body variants** at least: helmet + hair; helmet + face gear; jacket + gloves; jacket + backpack; boots + pants; tool + gloves; and combinations with head/arm occlusion in all four directions. Some items require explicit compatibility rules or clipping masks.

### Gate E — Rideable mech (after human pipeline works)

1. Approve a single mech silhouette: egg-shaped body, two mechanical legs, expressive arms, retro-industrial materials; no humanoid facial redesign or obligatory speaking mouth.
2. Generate its own south/north/east master views, with mirrored West if its design supports it, and test proportions before motion. Its logical canvas is likely larger than the human canvas (initial proposal 96×96, adjust to actual silhouette).
3. Animate idle and walk with a stable **ground/feet anchor**. Do not mix human animations into the mech rig.
4. Create mount/dismount state transitions and explicit seat/rider offset for each direction/frame. Preserve the player's selectable appearance; generate or compose a seated rider sprite/layer for each compatible human body variant as needed.
5. Define front/back mech body pieces so the rider appears *inside* the mech's seating area instead of floating over it. Test occlusion in every direction.
6. Keep gameplay boarding, movement and rendering logic clearly separated; do not build an elaborate generic vehicle framework for one mech.

## 7. Automated QA + mandatory visual review

**Hard checks** (explicit authoring command now; production build gate in a later phase): fixed dimensions, exact direction set, expected frame indices, valid PNG alpha, no accidental full-background fill, stable feet origin, valid atlas rectangles and item IDs, no orphan references, and all required approved assets present.

**Heuristic checks** (flag for review, not automatic proof): frame-to-frame character silhouette drift, palette deviations, discontinuous clothing textures, stray disconnected pixels, limb discontinuities, sudden equipment scale changes, loop seam jumps, and collision/occlusion anomalies.

Generate a contact sheet and preview GIF (or local animated viewer) showing:

- All four cardinal directions at 1× and nearest-neighbor 4×.
- Base character, equipment-only overlay on checkerboard, and composited character side by side.
- Playback of the same animation with equipment toggled on/off, plus a direction-switch test.
- A composite of multiple simultaneously equipped items.

**Stop rule:** If any of the pilot's directions/frames fails visual QA, repair that asset and repeat the preview. Do not generate ten more items hoping consistency improves.

## 8. MonoGame implementation contract

Extend existing `SpriteSheetDefinition`, `SpriteAnimator`, `PlayerSpriteCatalog` and `PlayerSpriteRenderer`. Add only the concrete equipment and anchor data needed by the pilot; the previously suggested rig/service class list is not a required implementation. The renderer receives character state, direction, animation time and loadout, and resolves all body/equipment frames using **one** clock.

Use `SpriteBatch` with source rectangles; render via `SamplerState.PointClamp` at integer pixel scale. Calculate direction-specific layer order before drawing; do not count on `layerDepth` under an incompatible sprite sort mode. Keep visuals separate from gameplay inventory data. Runtime must use locally packed assets only, no PixelLab access.

Suggested solution placement (adapt after examining actual repository and prefer **existing directories/projects**):

```text
PawnShop.Characters/Animation/        # Rig, frame anchors, animation state
PawnShop.Characters/Equipment/        # Slots, loadout, visuals
PawnShop.Framework/Rendering/         # Atlas lookup + layered renderer
PawnShop.Core/Items/                  # Gameplay item definitions
PawnShop.Tools.Assets/                # OPTIONAL only if existing tooling cannot accommodate a small C# command
PawnShop.Game/Content/Characters/     # Approved runtime artifacts
PawnShop.Game/Content/Equipment/      # Approved runtime artifacts
```

These are suggested paths, not permission to overwrite an existing architecture. Keep established project boundaries and avoid unrelated refactors. **Do not generate `.ps1` files or a scaffolding forest merely to recreate this tree.**

## 9. Production workflow and credit discipline

- **Plan → generate one → inspect → approve → batch.** Never issue a high-volume generation request without approved references and a stated maximum spend/job count.
- For every generation job record tool/model, exact arguments excluding secrets, prompt, references, seed when supported, created time, job/result IDs, cost when available, output hashes, and reviewer outcome.
- Use the connected tools to check existing job IDs before any retry. Record status with the source manifest; do not build a custom polling/orchestration framework for the pilot.
- Keep source files and hashes in ordinary folders/manifests. A change to the canonical body invalidates dependent fit approvals until revalidated. A generalized cache platform is not required.
- Record short rejection reasons: `style_drift`, `wrong_direction`, `bad_loop`, `anchor_drift`, `anatomy_mutation`, `gear_inseparable`, `occlusion_error`, `palette_noise`.
- Report failure honestly: some generated armor will be impossible to extract cleanly, and specific items may require a human pixel artist or hand correction.

## 10. Pilot acceptance criteria

- [ ] Approved male and female S/N/E human bases plus reviewed runtime-mirrored West.
- [ ] One approved idle and walk cycle (frame counts documented truthfully).
- [ ] Stable anchors and no noticeable foot sliding or anatomy drift.
- [ ] One modular welding helmet, one jacket, and one wrench, usable on both player bases through explicit compatible overlays.
- [ ] Gear can be equipped/unequipped without regenerating base animations.
- [ ] Helmet, jacket and wrench compose correctly together in all four directions.
- [ ] Good occlusion, loops and alignment confirmed in an interactive MonoGame preview.
- [ ] Contact sheets, animation previews, source manifests and rejected-candidate records exist.
- [ ] No API credentials shipped; game runs offline.
- [ ] Codex stops for user approval before generating additional equipment.
- [ ] Sprout's existing assets remain unchanged.
- [ ] No newly generated `.ps1` files, orchestration frameworks, or unexplained abstraction layers.
- [ ] A human developer can add or replace an equipment PNG and manifest without understanding agent-specific tooling.
- [ ] Mech work is explicitly deferred until human equipment works, then implemented as a separate mounted entity with a properly seated rider.

## 11. Codex CLI execution instructions

Read this entire specification first. Inspect `AGENTS.md`, actual solution/project files, art assets and existing animation/equipment abstractions. **First audit the previously generated `.ps1` files and architectural complexity and propose a minimal simplification, without deleting anything.** Begin with a read-only audit and a small implementation plan. Verify current connected PixelLab MCP tool schemas. Do **not** immediately generate all the art. First implement or confirm a four-direction runtime rendering contract with temporary debug layers, validate it, then produce the smallest PixelLab pilot (male and female bases, cardinal rotations, walking). Review artifacts before creating gear. Ask for approval before paid bulk generation, destructive changes or broad refactors. Preserve all existing source assets and architecture, especially Sprout. Never use diagonals. Treat the mech as its own mounted entity in a later milestone. Provide a clear report of what succeeded, failed, and remains manually reviewable.

## Sources

- PixelLab MCP: https://www.pixellab.ai/mcp
- PixelLab API: https://api.pixellab.ai/v2/docs
- PixelLab outfit transfer: https://www.pixellab.ai/docs/tools/transfer-outfit-pro
- MonoGame SpriteBatch: https://docs.monogame.net/api/Microsoft.Xna.Framework.Graphics.SpriteBatch.html
