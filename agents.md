# AGENTS.md — Pawn Shop Game

## 1. Purpose and scope

Build a cozy, humorous, top-down pixel-art pawn shop and salvage game in C# with MonoGame. Favor reusable, understandable components and small, playable milestones.

This file governs development; it is not an instruction to implement every system mentioned below. Follow the current task and preserve the existing repository. Treat project names and example APIs from earlier discussions as proposals unless implemented or explicitly approved.

### Product requirements

- Core loop: discover or acquire junk, transport it, inspect it, clean/repair/restore or dismantle it, and sell useful or valuable objects.
- Support searchable and improvable junk piles, a pawn shop, and exploration of abandoned commercial/industrial locations.
- The player can pilot a large walking salvage mech. A separate small, expressive robot companion accompanies the player.
- Keep the companion visibly smaller than the player. Its personality is overconfident until genuinely challenged; reactions can include boasting, fear, celebration, and comic dizziness.
- Combat opponents are monsters, not human opponents. Avoid humanoid monsters except skeletons.
- Include authored conversations, quests, and a modest storyline integrated with ordinary gameplay.
- Reuse inventory, interaction, animation, UI, conditions, and narrative infrastructure across features.

Default to local, single-player gameplay. Multiplayer, online services, runtime AI-generated dialogue, telemetry, and modding infrastructure are outside scope unless requested. Specific character names, memory-based mysteries, quests, prices, and schedules from brainstorming are not established canon.

## 2. Working agreement

- For explanation, review, or diagnosis, inspect and report; do not implement unrelated changes.
- For an implementation request, briefly identify the scope, then complete and verify it. Do not repeatedly ask permission for routine edits already authorized.
- Ask before material scope expansion, incompatible save-format changes, framework replacement, major dependency additions, or changes that risk user data.
- Make reasonable, reversible implementation choices and state important assumptions.
- Preserve uncommitted work. Do not discard, overwrite, or reformat unrelated changes.
- Do not create branches, commit, push, merge, publish releases, or change Git remotes without authorization. Never force-push or reset user work.
- Do not add secrets, machine-specific paths, personal data, or production save files.
- Do not change these instructions merely to make an implementation appear compliant.
- If using parallel agents, give them non-overlapping ownership and review their combined changes.

## 3. Inspect the repository before changing it

Read applicable repository instructions, README, solution/project files, SDK configuration, package references, build scripts, and nearby tests. Read relevant design/decision documents if present.

Use the actual repository as evidence for what exists. Do not assume an earlier discussion's class names, folders, dependencies, or milestones have been implemented.

Before substantive changes:

1. Check the working tree and identify files affected by the task.
2. Locate existing implementations that could be reused or extended.
3. Establish the relevant build/test commands and any pre-existing failures.
4. State the intended behavior and acceptance checks.
5. Implement the smallest complete change that satisfies the request.

Use the repository's existing SDK, MonoGame target, content build workflow, test framework, naming, and formatting. Do not upgrade packages or retarget projects incidentally. Pin newly approved dependencies consistently with existing conventions; do not assume the latest version is appropriate.

## 4. Architecture and dependency boundaries

Use composition over deep inheritance. Components describe focused state/capabilities; systems or services implement behavior. Not every class needs an interface, component, factory, or event.

Build a game-specific reusable foundation, not a general-purpose engine. Introduce abstractions for demonstrated reuse, important invariants, or testability. Avoid speculative ECS frameworks, editor platforms, and plugin systems. The explicitly approved eight-project layout below includes scaffolding for future features; do not add speculative classes to fill it or infer feature completion from its folders.

`PawnShopSimulator.sln` and these eight project directories live together at the repository root. This approved structure supersedes earlier project-layout proposals:

| Project | Owns | Allowed PawnShop project references |
| --- | --- | --- |
| PawnShop.Framework | Reusable entities/components/systems, scenes, input, UI, events, rendering, audio | None; no game-specific rules |
| PawnShop.Core | Shared items, inventory, interactions, conditions/effects, transactions, economy, world state, persistence contracts/storage | Neutral Framework contracts only when needed; remain headless-testable |
| PawnShop.Narrative | Dialogue, quests, relationships, story flags, progression | Core |
| PawnShop.Characters | AI, behaviors, movement, combat, player, mech, companion | Core, Framework |
| PawnShop.Shop | Customers, appraisal, restoration, displays, shop workflows | Core |
| PawnShop.Exploration | Dungeons, salvage, monsters, hazards, generation | Core, Characters, Framework |
| PawnShop.Game | MonoGame executable; Bootstrap session/service composition; concrete Screens and Content | All six production libraries |
| PawnShop.Tests | Existing regression runner and future focused tests | Projects exercised by each test |

Keep project references acyclic. Put shared contracts in a dependency-neutral location. Register feature-specific handlers at the composition boundary. Integration tests may reference the layers they exercise.

Session creation, service wiring, and cross-feature coordination belong under `PawnShop.Game/Bootstrap`. Shared atomic operations belong in `Core/Transactions`; `Shop/Transactions` uses them rather than creating another inventory or currency model. Lower production projects never reference Game.

Gameplay rules must run in headless tests without a window, graphics/audio device, or MonoGame ContentManager. Do not make domain projects reference a rendering-heavy Framework project just to obtain IDs or event types. Domain data can refer to asset IDs, not Texture2D, SpriteBatch, or audio instances.

Use the feature folders documented in ARCHITECTURE.md within the approved projects. Retain empty folders with `.gitkeep`; they do not authorize implementation. Further project splits require a concrete need or explicit authorization.

Feature-specific save data stays with its owning feature. `Core/Persistence` owns neutral persistence contracts and reusable storage mechanics; `Game/Bootstrap` coordinates complete snapshots without reverse references for serialization.

## 5. Components and authoritative state

- Give each mutable state value one authoritative owner.
- UI, AI, dialogue, and quests request changes through domain operations; they do not directly edit money, inventory contents, item condition, or objective counters.
- Expose read-only views or snapshots to consumers. A read-only collection of mutable objects is not an immutable snapshot.
- Give persistent entities and unique items stable IDs. Never use display names, array positions, or object references as persistent identity.
- Validate required component combinations when spawning entities. Report missing required capabilities clearly rather than silently inventing defaults.
- Keep definitions separate from runtime state: an item/quest/dialogue definition describes content; an instance/progress/session records this playthrough.
- Use explicit construction or dependency injection at bootstrap. Avoid global mutable singletons, service locators, and passing the entire world into every class.
- Keep public contracts narrow; expected gameplay failures should return a meaningful typed result.

## 6. Application, session, scene, and UI lifecycle

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

## 7. Simulation, input, and time

- Each simulation system updates once per simulation tick under one owner. Draw must not mutate gameplay state.
- Use one deliberate timestep strategy. Do not add a second fixed-step accumulator on top of an existing fixed-step host without an explicit reason.
- Separate simulation time, in-game calendar time, and presentation time. Pausing gameplay can leave menu animations responsive.
- Define time advancement during dialogue, repair jobs, travel, and sleep. Do not use wall-clock time for gameplay progression unless explicitly intended.
- Translate device input into actions. Separate held movement from pressed-once interaction/menu actions.
- Keep movement speed consistent across directions; collision footprints are independent of sprite canvas bounds.
- Keep authoritative gameplay mutations on the simulation thread. Background I/O returns data for application at a safe boundary.
- Inject controllable time and seeded randomness where outcomes need reproducible tests. Avoid hidden random sources.

## 8. Commands, events, and transactions

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

## 9. Shared inventory and economy

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

## 10. Interactions, dialogue, and quests

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

## 11. Player, mech, companion, and exploration

The player, rideable mech, and little companion are distinct entities. Mounting changes a control/attachment relationship, not the player's identity.

Define movement control, collision, damage ownership, cargo access, and valid dismount placement. Attached riders must not also run independent ground movement/collision. If no safe dismount position exists, refuse with a clear result.

Render riders independently through named attachment points, facing/frame-specific offsets, and appropriate layering or masks. Do not bake the companion or player into mech sprites as the canonical asset.

Reuse appropriate movement, pathfinding, state-machine, and interaction capabilities across actors. Begin with simple explicit behaviors; a behavior-tree editor or utility-AI framework is not a prerequisite.

Locations own local entities and changes; the session owns persistent player/world progression. Unloaded locations use explicit dormant/time-skip policies rather than accidentally continuing full simulation.

For generated dungeons, use controlled seeds and a generator/content version. Persist relevant layout/state or a compatible seed plus changes; seed alone must not respawn collected loot. Validate reachability, spawn safety, and required exits, and bound generation retries.

## 12. Pixel art, animation, and resources

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
- Own and dispose textures, render targets, audio, and content scopes deliberately; do not dispose shared assets while another scene uses them.

## 13. Persistence and content validation

Save explicit, versioned data models—not live entities with service references, subscriptions, tasks, or graphics objects.

Persist authoritative ownership, player/mech/companion state, time, world changes, active restoration jobs, quest/reward state, story facts, and necessary dungeon state. Reconstruct runtime wiring after load.

Capture a consistent snapshot at a simulation boundary. Coordinate pending events and transactions so a save cannot record a quest reward without its claim state, or a completed sale without its money transfer.

Before snapshotting, either finish processing committed events that affect persistent state or include their pending processing state in the save. Loading must not lose a committed quest-progress event or apply it twice.

Write through a temporary file and safe replacement appropriate to the platform; retain a recoverable backup. Do not overwrite an unsupported or corrupt save while reporting load failure. Specify migration and missing-content policies; preserve the original during migration.

Tests use isolated temporary save directories, never actual player saves. Add round-trip tests as persistent systems arrive, not after the entire game is built.

Validate content IDs, references, numeric ranges, required assets, graph links, and schema versions. Diagnostics should identify the file and relevant entity/node/field. Do not silently substitute empty content for invalid required definitions.

## 14. Coding, performance, and verification

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

These are command forms, not a claim that the repository already builds. Use matching configurations/frameworks for build and test. Run the content build and manual launch checks required by the existing workflow.

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

Use headless tests for rules and explicit manual checks for rendering, audio, animation, and feel. Never claim visual behavior is verified merely because compilation succeeded.

## 15. Milestones and completion

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

## 16. Keep guidance coherent

This file owns durable development rules. Use the existing root ARCHITECTURE.md, GAME_DESIGN.md, and ROADMAP.md for detailed architecture, gameplay design, and milestone status. Verification records live under docs/. Do not create an art specification or move documentation merely because earlier proposals suggested it.

Distinguish approved decisions from proposals. Update relevant documentation when an authorized change alters a contract. Do not accumulate transcripts, speculative APIs, copied tutorials, or temporary progress logs in this file.

When existing code and these rules conflict, identify the smallest safe alignment needed for the current task. Propose consequential redesigns rather than silently rewriting the repository.
