# Engineering contracts

These binding development rules were relocated from the root instruction file.
Read the section relevant to a behavior being changed, not this entire file for
every task. Project AGENTS.md files link the appropriate topics. These rules
constrain requested implementations; they do not authorize building future systems.
Current implementation and detailed designs remain in [ARCHITECTURE.md](../ARCHITECTURE.md),
[GAME_DESIGN.md](../GAME_DESIGN.md), and [ROADMAP.md](../ROADMAP.md).

## Application placement and composition

Before creating or moving a file into `PawnShop.Game`, explicitly check whether an
existing feature project or `PawnShop.Framework` is a better owner. Prefer that
home for feature behavior, services, reusable UI, and feature content. Game is not
the default destination simply because it can reference every library.

Keep new Game code limited to the executable host, thin application/session
composition and main-menu UI (including Options). Keep the host mechanically
simple: delegate work to the owning library rather than implementing it there.
If no suitable home is clear, ask the user before placing the file in Game or
introducing a further project split. Do not add graphics/audio dependencies to a
headless domain project just to avoid Game. Existing non-menu code in Game does
not justify adding more; move it through separately scoped refactors, preserving
working behavior.

PawnShop.Audio owns `Content/Music`, `UI`, `Environmental`, `Combat`, `Monster`
and `Speech`. Its MGCB pipeline supplies compiled `Content/Audio` assets to
consumers. Game creates and updates playback; settings storage and main-menu
interaction rules stay at the application boundary.

Session creation, service wiring, and cross-feature coordination belong under `PawnShop.Game/Bootstrap`. Shared atomic operations belong in `Core/Transactions`; `Shop/Transactions` uses them rather than creating another inventory or currency model. Lower production projects never reference Game.

Gameplay rules must run in headless tests without a window, graphics/audio device, or MonoGame ContentManager. Do not make domain projects reference a rendering-heavy Framework project just to obtain IDs or event types. Domain data can refer to asset IDs, not Texture2D, SpriteBatch, or audio instances.

Use the feature folders documented in ARCHITECTURE.md within the approved projects. Retain empty folders with `.gitkeep`; they do not authorize implementation. Further project splits require a concrete need or explicit authorization.

Feature-specific save data stays with its owning feature. `Core/Persistence` owns neutral persistence contracts and reusable storage mechanics; `Game/Bootstrap` coordinates complete snapshots without reverse references for serialization.

## Application, session, scene, and UI lifecycle

Separate application lifetime, game-session lifetime, location lifetime, and overlay lifetime.

- The title screen works without a live gameplay session.
- New Game creates a fresh session. Continue validates and loads an existing one. Only one gameplay session is active at a time; neither action silently overwrites a save.
- Returning to title disposes the session, subscriptions, and owned resources. Starting again must not duplicate handlers or retain old state.
- A location transition preserves persistent world changes, validates the destination/spawn, and handles loading failure without losing the session.
- Scenes/presenters display session state; scene replacement must not reset inventories, quests, or relationships.
- Every overlay explicitly defines input capture, whether simulation pauses, and whether underlying scenes remain visible.
- Route input through the focused UI first. The action closing a modal must not also interact with the world.
- Distinguish application exit from returning to title, with appropriate unsaved-progress handling.

Reuse small UI controls for menus, dialogue, inventories, tooltips, and journals. Inventory views bind to inventory IDs/read models rather than a special player-only implementation. Controls dispatch commands and display errors; they do not contain economy or quest rules.

## Commands, events, and transactions

Use direct commands for requested actions and immutable events for committed facts.

Examples: TryTransferItem is a command; ItemTransferred is an event. An attempted sale must not emit SaleCompleted.

A multi-resource operation follows this rule:

1. Validate ownership, quantities, capacity, money, reservations, and current preconditions.
2. Prepare the complete state change.
3. Commit all related changes together.
4. Publish committed events afterward.

Failure must not partially consume items, money, or rewards. Design a small explicit unit of work where necessary; an event bus is not a transaction manager. Observer failures must not imply a committed transaction was rolled back.

Document event ordering and process events at controlled boundaries. Avoid unbounded synchronous event recursion. Release subscriptions when their owner exits.

Use persistent, stable operation/claim IDs for retryable consequential actions. Repeating a reward claim or restoration completion must not duplicate its effects. Do not add an event-sourcing platform or distributed messaging to achieve local correctness.

## Inventory and economy

Use one inventory model with configurable ownership, capacity, and acceptance rules for the player, mech cargo, storage, shelves, workstations, loot containers, and customers.

- Route transfers through one authoritative service. Do not let callers bypass invariants through public mutable lists or independent remove-then-add calls.
- Each unique item has exactly one authoritative location/owner, including world pickups, equipment, workstations, and inventories.
- Preserve instance identity, condition, modifications, discovery state, and relevant provenance when items move.
- Stack only explicitly fungible items with compatible properties. Two individually damaged antiques are not interchangeable stacks.
- Reject invalid/negative quantities. Define same-container transfers, stack splits/merges, full capacity, weight limits, and reservations.
- Prices use a documented money representation, preferably integer smallest units; avoid floating-point balances.
- A sale coordinates item ownership, listing state, and money atomically. Listings reference authoritative items rather than duplicating them.
- Restoration coordinates inputs, item reservations, job state, and outputs. Completion happens once; blocked output capacity has an explicit policy.
- Quest-item protection and reward overflow are explicit rules, not silent deletion or duplication.
- Keep appraisal uncertainty separate from an item's authoritative properties.

Do not assume collateral loans/redemption mechanics are required merely because the game is a pawn shop. Implement that distinction when the design calls for it.

## Interactions, dialogue, and quests

### Shared interactions

Expose contextual actions such as Talk, Search, Transfer, Mount, Inspect, and Repair through a common interaction contract. Availability checks provide useful failure reasons. Revalidate on execution; a previously enabled UI button is not proof the action remains valid.

### Dialogue

- Author branching content as definitions with stable conversation, node, choice, and speaker IDs.
- Runtime sessions track participants and current progression; UI renders a dialogue view.
- Select by stable choice ID, not visible index or localized text. Recheck conditions when selected.
- Keep text, portraits, emotion keys, and presentation cues separate from game rules.
- Apply gameplay effects through the authoritative services. Do not place irreversible unguarded rewards in repeatable node-entry callbacks.
- Distinguish skipping text animation from choosing/advancing dialogue. Cancellation and unavailable speakers have explicit outcomes.
- Specify whether conversations pause simulation and how saving during a conversation behaves.
- Use reusable text identifiers where practical; do not embed quest/business logic in displayed strings.

### Conditions and effects

Use a small, registered vocabulary of typed conditions and effects. Inventory checks, quest status, relationships, story facts, and location access can be shared by conversations, interactions, and quests.

Validate serialized parameters before runtime use. Unknown handlers, missing IDs, invalid types, and broken graph links must produce actionable diagnostics. Never execute arbitrary C#, reflection-selected methods, or scripts from content files.

A dialogue choice that exchanges items/money or claims a reward uses an atomic domain operation—not an unchecked list of independent effects. Audio/animation cues are presentation outputs, not prerequisites for successful economic state changes.

### Quests

- Separate immutable definitions from persistent runtime progress.
- Define stages, objective activation, and completion explicitly. If a quest supports failure, abandonment, or optional objectives, define those semantics; do not implement unsupported quest modes preemptively.
- Distinguish state objectives ("possess a repaired radio") from event objectives ("sell three radios after acceptance").
- Reconcile state objectives on acceptance/load and relevant state changes; define whether earlier actions qualify for event objectives.
- Count qualifying committed events with appropriate filters and identity rules. Moving the same object repeatedly must not count as discovering new objects.
- Use semantic facts such as ItemRestored or ItemDelivered, not UI clicks, where progression depends on world changes.
- Persist completion/claim state together with reward changes. Reloads, duplicate commands/events, and repeat conversations must not grant rewards twice.
- Handle full inventory during reward delivery explicitly: leave a claim pending or use a defined destination. Never silently lose rewards.
- Keep narrative integration optional for ordinary shop/exploration actions unless an explicit progression gate requires it.

## Actors and exploration

The player, rideable mech, and little companion are distinct entities. Mounting changes a control/attachment relationship, not the player's identity.

Define movement control, collision, damage ownership, cargo access, and valid dismount placement. Attached riders must not also run independent ground movement/collision. If no safe dismount position exists, refuse with a clear result.

Render riders independently through named attachment points, facing/frame-specific offsets, and appropriate layering or masks. Do not bake the companion or player into mech sprites as the canonical asset.

Reuse appropriate movement, pathfinding, state-machine, and interaction capabilities across actors. Begin with simple explicit behaviors; a behavior-tree editor or utility-AI framework is not a prerequisite.

Locations own local entities and changes; the session owns persistent player/world progression. Unloaded locations use explicit dormant/time-skip policies rather than accidentally continuing full simulation.

For generated dungeons, use controlled seeds and a generator/content version. Persist relevant layout/state or a compatible seed plus changes; seed alone must not respawn collected loot. Validate reachability, spawn safety, and required exits, and bound generation retries.

## Artwork and resources

Separate source canvas size, visible body bounds, world size, origin, collision footprint, and final display scale.

- A 48×48 companion source frame is allowed; it does not make the companion larger than the player.
- Earlier numbers for tiles, canvases, render resolution, and frame counts are provisional. Verify the asset files and an in-game scale comparison before standardizing them.
- Use point/nearest-neighbor sampling for crisp pixel art, with a documented camera and scaling policy. Prefer integer output scaling when preserving a uniform pixel grid.
- Downscaling is not lossless. Inspect outlines, eyes, silhouette, and animation consistency at actual gameplay size; use a simplified gameplay asset when needed.
- Validate actual image dimensions, transparency, frame rectangles, padding, origins, animation names, durations, and attachment points. Generated art is not automatically a correctly aligned production sheet.
- Keep sprite origins stable between frames. Animation changes must not cause unintended positional jumps.
- Use data-driven animation metadata and a reusable animator. Gameplay requests animation states, not sprite-sheet row numbers.
- Define arbitration between locomotion, actions, reactions, and dialogue expressions so systems do not overwrite one another every tick.
- Preserve source artwork; keep derivative sheets and generated build output distinct. Record third-party asset provenance/licenses.
- For customizable player clothing, follow [the approved wearable art workflow](WEARABLE_ART_WORKFLOW.md): AI-generate separate body-specific PixelLab object references, fit each to its exact base, and extract garment-only layers over the unchanged body. Do not substitute hand-drawn shapes or force fitting by shifting, stretching or repeating fabric rows.
- Generate every player wearable design for **both male and female models**, with separate fitted art and acceptance records for each body. A shared item identity does not imply interchangeable artwork. Walking attire must match the exact body, direction and animation frame; standing-pose approval never approves walking fit. Validate all north/east/south/west frames for both bodies before describing a wearable as walk-compatible.
- Use PixelLab **Skeleton v3** for player walk generation. Author south, north and east for each body; render west from east with MonoGame `SpriteEffects.FlipHorizontally`. Four movement facings require only three stored visual directions. Apply one character-space mirror to body, clothing, masks, anchors and held-item offsets. Opposite-hand appearance is acceptable; independently authored west requires a concrete visual/gameplay reason. Preserve historical W-to-E sources as evidence, without relabeling them as newly authored east or granting approval.
- Human asset authoring uses the existing C# test/authoring executable (`--assets`; see `docs/WEARABLE_ART_WORKFLOW.md`). Do not add PowerShell scripts, runtime generation services or new tooling projects for this pipeline. Sprout's existing artwork, animations and implementation are frozen.
- Use the canonical `Content/Player/Bases` and `Content/Player/Wearables` assets and their manifests. The current source-frame contract is 48×64 with 48–52 visible body pixels (currently 50) and pivot (24,58); it does not establish collision/world scale. Keep full transparent canvases and native coordinates.
- Check shoulder/underwear coverage and enlarged/native-size composition before requesting visual acceptance. Record approval per body, direction and pose; numeric checks alone cannot approve fit. Archive rejected source work before cleanup, and keep approved files independent of the archive.
- Own and dispose textures, render targets, audio, and content scopes deliberately; do not dispose shared assets while another scene uses them.

## Persistence and content validation

Save explicit, versioned data models—not live entities with service references, subscriptions, tasks, or graphics objects.

Persist authoritative ownership, player/mech/companion state, time, world changes, active restoration jobs, quest/reward state, story facts, and necessary dungeon state. Reconstruct runtime wiring after load.

Capture a consistent snapshot at a simulation boundary. Coordinate pending events and transactions so a save cannot record a quest reward without its claim state, or a completed sale without its money transfer.

Before snapshotting, either finish processing committed events that affect persistent state or include their pending processing state in the save. Loading must not lose a committed quest-progress event or apply it twice.

Write through a temporary file and safe replacement appropriate to the platform; retain a recoverable backup. Do not overwrite an unsupported or corrupt save while reporting load failure. Specify migration and missing-content policies; preserve the original during migration.

Tests use isolated temporary save directories, never actual player saves. Add round-trip tests as persistent systems arrive, not after the entire game is built.

Validate content IDs, references, numeric ranges, required assets, graph links, and schema versions. Diagnostics should identify the file and relevant entity/node/field. Do not silently substitute empty content for invalid required definitions.

## Coding, performance, and verification

Follow repository conventions. Keep methods focused, dependencies explicit, nullable behavior meaningful, and APIs documented where invariants are non-obvious. Avoid catch-and-ignore errors, unexplained null suppression, and generic "manager" classes with unrelated responsibilities.

Use asynchronous I/O where useful without blocking Update/Draw. Avoid avoidable allocations, repeated content parsing, and large searches in frame loops. Measure before introducing pooling, parallel simulation, or complex indexing.

### Build and test procedure

Discover exact project paths; do not invent a solution filename. Use existing scripts/CI commands first. Otherwise adapt these command forms to verified paths:

```text
dotnet --info
dotnet restore <actual-solution-or-project>
dotnet build <actual-solution-or-project> --no-restore
dotnet test <actual-test-project-or-solution> --no-build
git diff --check
```

These are command forms, not a claim that the repository already builds. Use matching configurations/frameworks for build and test. Run the content build required by the existing workflow; the user performs manual launch and desktop checks.

The current solution is `PawnShopSimulator.sln`. Its existing dependency-free test runner uses `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`; `dotnet test` does not execute those checks. Build/run details and the current code-authored content workflow are in README.md.

Do not install SDKs, alter security settings, or bypass access restrictions to hide an environment problem. Report missing prerequisites precisely.

Add focused regression tests for changed behavior, especially:

- Failed inventory/economy operations leave all related state unchanged.
- Unique-item ownership is conserved across pickup, transfer, listing, sale, and save/load.
- Stale dialogue choices are rejected and rewards cannot be claimed twice.
- Quest counters distinguish state, historical events, and repeated transfers.
- Title/session/overlay transitions capture input and release owned resources.
- Mount/dismount, pause, time advancement, and seeded generation obey their contracts.
- Save round trips, corrupt/unsupported data, migrations, and interrupted writes have defined outcomes.

Use headless tests for rules. The user handles manual testing of rendering, audio, animation, input feel, window behavior, and desktop workflows. Do not launch or control the game, use computer vision, inspect screenshots, or use Computer Use to verify changes unless the user explicitly requests that testing. Such agent-driven testing is never required to finish an implementation task, even when other repository documents call for visual or desktop acceptance checks.

Continue running relevant builds, automated tests, and nonvisual content checks. Briefly identify any manual checks left for the user, without treating their pending testing as a blocker to the implementation handoff. Record visual acceptance separately and only mark it verified after the user reports results or explicitly requested testing provides evidence. Never claim visual behavior is verified merely because compilation succeeded.

## Milestones and completion

If there is no implemented application lifecycle, the first playable milestone is:

1. Launch to the title screen without creating a gameplay session.
2. Navigate reusable menu controls.
3. Start a new session and enter a minimal room.
4. Open pause, resume, and return to title cleanly.
5. Start again without leaked state, resources, or subscriptions.

Continue is disabled with a clear reason until a valid load implementation/save exists. Settings must affect real settings or be clearly unavailable. Do not present nonfunctional buttons as finished features.

Use placeholders where they prove behavior. Add inventory, interactions, conversations, quests, robots, and dungeons in subsequent requested slices. Do not build the whole roadmap automatically.

A change is complete when:

- The requested acceptance criteria are met.
- Related invariants have tests where practical.
- Relevant builds/tests/content checks were run, or blockers are accurately reported.
- The final diff contains no unrelated edits or generated clutter.
- Relevant documentation reflects changed contracts or workflows.

Final handoff: summarize behavior and files changed, exact verification performed with results, unverified/manual checks, and remaining blockers. Do not claim commits, tests, launches, or visual inspections that did not occur.
