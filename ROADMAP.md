# Pawn Shop Game — Roadmap

**Document status:** proposed implementation plan, 2026-10-24. M00 repository inspection is complete; M01 is implemented with headless checks passing and desktop lifecycle verification pending. See [verification evidence](docs/VERIFICATION_M01.md). Later milestones remain proposals.

## 1. Purpose, sources, and status

This roadmap turns the game's requirements into small, playable, testable increments. It is not authorization to build every milestone at once. Select one bounded slice, inspect the existing repository, implement it, and demonstrate its acceptance criteria before expanding the work.

The primary source is the repository's [agents.md](agents.md). Additional context comes from the available Pawn Shop - Pixel Sim Idea conversations: the original pawn-shop premise, junk-pile and abandoned-mall discussions, robot/sprite concepts, and the recent architecture/start-screen discussion. Available excerpts and retrieved history are not a complete export of every project chat. Current repository/build evidence is recorded in docs/VERIFICATION_M01.md.

Companion documents:

- [ARCHITECTURE.md](ARCHITECTURE.md) defines ownership, interfaces, dependencies, transactions, persistence, and technical invariants.
- [GAME_DESIGN.md](GAME_DESIGN.md) defines player-facing mechanics, characters, progression, and proposed story content.
- [agents.md](agents.md) owns durable development rules and overrides speculative implementation examples here.

These guidance files remain together at the repository root. Verification records live under docs/.

### Decision labels

| Label | Meaning |
| --- | --- |
| Established | Required by the supplied development guidance or explicit project direction: cozy salvage/shop play, title-first lifecycle, reusable systems, distinct robots, monster-only combat, authored narrative. |
| Working baseline | A reversible design proposed to make implementation concrete: modal pause policy, safe-boundary saves, initial quest and room fixtures. |
| Open | A consequential choice needing a decision before its affected slice: final defeat penalty, fixed art dimensions, procedural-generation scope, commercial scope. |

All milestone scopes and test thresholds below are proposed planning targets. None establishes character names, plot twists, prices, exact asset sizes, release dates, or a production budget as canon.

## 2. Delivery strategy

Build the title screen first, then a minimal room, shared item handling, and save/load. Establish a small local salvage/restore/sell loop before adding authored narrative, robots, and an expedition. Validate the complete game loop in one authored mall before multiplying locations or content.

Use placeholder art as soon as it makes movement, scale, layering, or interaction testable. Polished sprite sheets are not a prerequisite for the first playable milestone. The explicitly requested structural refactor establishes `PawnShopSimulator.sln` with eight root-level projects: PawnShop.Framework, Core, Narrative, Characters, Shop, Exploration, Game, and Tests (all prefixed PawnShop.). This supersedes earlier layout proposals. Retain the requested future folders with `.gitkeep` and do not populate them with speculative systems, an engine, or an editor.

The refactor preserves M00 completion and M01's implemented/pending-verification status. It is structural work, not another gameplay milestone. Game/Bootstrap owns lifecycle composition, Core owns shared rules and neutral persistence, and feature-specific save data remains with each feature. README.md documents the current solution build, console regression runner, launch, and code-authored content workflow. Folder/project creation earns no future milestone acceptance credit.

The sequence is a dependency order, not a calendar estimate. M00 maps existing code onto it; proven functionality may satisfy a gate without being rewritten. A compile-only result does not satisfy a visual or gameplay gate.

### Milestone ledger

| ID | Deliverable | Principal prerequisite | Player-visible exit | Current status |
| --- | --- | --- | --- | --- |
| M00 | Repository reality and build baseline | Authorized repository access | Reproducible starting point | Complete; see M00/M01 verification record |
| M01 | Title and session lifecycle | M00 | Launch, start, pause, return, restart | Implemented; headless checks and title launch pass; desktop lifecycle/resource acceptance pending |
| M02 | Movement, time, interactions, scale lab | M01 | Walk around and inspect a small room | Planned; not repository-verified |
| M03 | Shared inventory and ownership | M02 | Move the same objects between containers | Planned; not repository-verified |
| M04 | Minimal robust save/load | M03 | Continue a consistent saved session | Planned; not repository-verified |
| M05 | Local shop/restoration economy loop | M04 | Search, repair, list, sell, save | Planned; not repository-verified |
| M06 | Dialogue and quests | M05 | Accept and fulfill an authored request | Planned; not repository-verified |
| M07 | Mech and companion | M03–M06 | Ride the mech with a distinct expressive follower | Planned; not repository-verified |
| M08 | Authored mall and integrated slice | M07 | Expedition, extraction, restoration, sale, quest | Planned; not repository-verified |
| M09 | Renewal, upgrades, daily cadence, story | M08 | Repeat the loop with meaningful progression | Planned; not repository-verified |
| M10 | Hardening and release candidate | Agreed M09 scope | Verified, recoverable, distributable build | Planned; not repository-verified |

M05 is the first local economic loop. M08 is the first comprehensive vertical slice, not the first moment at which the game becomes playable. Save/load is extended in every milestone after M04 rather than postponed until M10.

## 3. Rules for every work slice

### Ready to implement

- The task names the behavior, affected feature, explicit exclusions, and acceptance checks.
- Applicable instructions, nearby code, project files, content workflow, and tests have been inspected.
- Existing changes and baseline failures are known; no user work will be discarded.
- A mutable-state owner and persistence policy exist for every new persistent value.
- Open choices that would change ownership, save compatibility, or substantial gameplay are resolved or the task is narrowed around them.

### Done

- The requested behavior meets the selected acceptance criteria, including its failure paths.
- Relevant rule tests, integration checks, build, and content validation pass, or exact blockers are recorded without claiming completion.
- Manual rendering/feel checks are performed where needed and distinguished from automated results.
- Save/load coverage grows with the feature; new consequential actions cannot duplicate effects on retries.
- The diff contains no unrelated changes, secrets, production saves, generated clutter, or incidental dependency upgrades.
- Changed contracts are documented. The handoff identifies actual commands/results, manual checks, and remaining limitations.

Do not create branches, commit, push, publish, change remotes, or install major dependencies without the authorization required by AGENTS.md.

### Evidence to retain

Use an issue, pull-request description, or brief task record containing: milestone/slice, relevant revision, files changed, exact verification commands, result, manual scenario, known limitations, and any accepted decision. Keep temporary execution logs out of AGENTS.md. Mark a milestone complete only when its evidence is inspectable.

## 4. Milestones and acceptance criteria

### M00 — Repository reality and build baseline

**Goal:** establish what exists without reorganizing or replacing it.

**Small slices:** inspect instructions and working tree; locate the actual solution/projects; identify SDK and MonoGame/content dependencies; discover build/test/launch commands; map existing features onto this roadmap.

**Acceptance criteria**

- [x] Actual solution/project paths, SDK requirements, target platform, content workflow, and test framework are recorded from repository evidence.
- [x] A baseline restore/build/test is attempted using existing scripts first. Pre-existing failures and unavailable prerequisites are separated from new failures.
- [x] Existing features and user changes are identified; planned work does not duplicate or overwrite them.
- [x] The next slice is selected explicitly, with a small acceptance checklist. If no lifecycle exists, it is M01.

**Exit evidence:** repository inventory and exact verification results. Missing SDKs or unavailable desktop graphics are reported as environmental blockers, not solved through unauthorized installation or fictional test results.

### M01 — Title screen and session lifecycle

**Goal:** a real start screen that works without a gameplay session.

**Small slices:** application bootstrap and title; reusable menu/focus controls; New Game into a minimal room; pause/resume/return; cleanup and restart regression.

**Acceptance criteria**

- [ ] Launch opens the title with no live player inventory, world simulation, or session subscriptions.
- [ ] New Game constructs one fresh session and enters a minimal room. A second session cannot run alongside it.
- [ ] Continue is disabled with an explanation until M04 provides a compatible save/load path. Settings either changes a real setting or is clearly unavailable; no completed-looking dead buttons.
- [ ] Keyboard navigation, confirm, and cancel behave consistently. The action closing pause does not also interact with the room.
- [ ] Pause freezes simulation while menu animation/input and allowed menu commands remain responsive. Do not freeze the command-dispatch mechanism along with gameplay timers.
- [ ] Return to title disposes session resources and subscriptions. Ten consecutive start/return cycles show no growing handler/session count or old-state carryover; resource use remains bounded after warm-up.
- [ ] Exit and return-to-title are separate actions, with appropriate unsaved-progress handling.

**Verification:** lifecycle/focus tests where headless; actual launch and ten-cycle manual smoke check. **Defer:** inventories, functioning Continue, dungeon loading, final title art.

### M02 — Movement, time, shared interactions, and scale lab

**Goal:** make the room a reliable test bed for the reusable foundation.

**Small slices:** action-based input; movement/collision/camera; clocks and pause policy; contextual Inspect/Search interactions; placeholder scale and animation comparison.

**Acceptance criteria**

- [ ] Cardinal and diagonal movement cover equivalent distances for equal elapsed time within a documented test tolerance. Movement is not tied to draw rate.
- [ ] One owner advances simulation systems once per tick. Draw does not mutate authoritative gameplay state.
- [ ] Simulation time, game minutes, and presentation time are distinguishable. Pausing does not accrue hidden gameplay time; restarting the application does not simulate offline progress.
- [ ] Input is handled by focused UI before the world. Held movement and pressed-once actions have different semantics.
- [ ] A contextual interaction provides availability and a useful denial reason, then revalidates on execution.
- [ ] Collision footprints are independent of sprite canvases. Scale comparison shows the little companion smaller than the player and the rideable mech appropriately larger.
- [ ] Point-sampled rendering, stable sprite origins, and actual gameplay-size readability are manually checked. A 48×48 source frame is not treated as a world-size requirement.

**Verification:** deterministic movement/time/interaction tests plus visual scale lab. **Defer:** final sprite dimensions, large animation packs, full calendar schedules, polished environments.

### M03 — Shared inventory and authoritative ownership

**Goal:** prove one item model and one transfer path work across different owners.

**Small slices:** item definitions versus instances; unique-location registry and containers; transfer validation/commit; reusable inventory UI; edge-case fixtures.

**Acceptance criteria**

- [ ] Player bag, shop storage, and a world loot container use the same contracts with configurable capacity/acceptance rules.
- [ ] A unique item preserves its ID, condition, discovery information, and relevant provenance across pickup and transfer. It exists in exactly one authoritative location.
- [ ] Negative/zero invalid quantities, stale source locations, restricted items, full destinations, and reserved items fail without partial state change.
- [ ] Same-container actions and stack split/merge behavior have defined outcomes. Only compatible fungible materials stack; individually damaged valuables do not.
- [ ] Capacity and configured weight limits are enforced. A UI drag cannot bypass rules used by keyboard interaction or NPC commands.
- [ ] Read views cannot mutate stored item/container state indirectly.
- [ ] Property-style or repeated randomized tests preserve ownership and total quantities across successful and rejected transfer sequences.

**Verification:** headless invariant tests and manual two-container transfer checks. Compare complete relevant before/after state for failures, not merely returned error codes. **Defer:** parallel container implementations, complex sorting UX, economy balancing.

### M04 — Minimal robust save/load

**Goal:** resume the small game safely before persistence complexity expands.

**Small slices:** versioned save DTOs and save metadata; safe-boundary snapshot; temporary-write/replacement/backup; validated load; Continue and user-facing errors.

**Working policy:** manual saving at stable boundaries, including the pause menu, but outside dialogue, location transitions, and active combat. Unsupported save timing reports a reason. No mid-dialogue cursor persistence is required in this baseline; future changes must be explicit.

**Acceptance criteria**

- [ ] Position, location, time, item IDs/ownership, inventories, and available world changes round-trip accurately.
- [ ] A snapshot is captured only after relevant committed events and persistent reactions have drained to a stable boundary. No committed progress is lost or replayed twice. Persisting a pending queue is a later separately designed alternative, not this baseline.
- [ ] An authoritative/persistent event-handler failure retains unfinished work and blocks a successful save until safely resolved. A failing cosmetic observer cannot retroactively undo an already committed transaction.
- [ ] Continue validates save metadata and content compatibility, loads one session, and leaves a recoverable title state when loading fails.
- [ ] App settings/save-slot metadata are distinct from gameplay state. Reading slot metadata does not construct a live world.
- [ ] Temporary-write interruption, failed replacement, and backup recovery are tested using isolated test directories. The prior valid save remains recoverable under the documented platform workflow.
- [ ] Corrupt, unsupported-version, or missing-required-content saves report useful errors and are not silently overwritten.
- [ ] Any migration is versioned, preserves the original, and has a test. Unsupported migrations are rejected rather than guessed.
- [ ] New Game does not silently overwrite a save. Return/exit communicates unsaved progress accurately.

**Verification:** automated round-trip/fault-injection checks plus manual Continue. **Ongoing gate:** every following persistent feature supplies its own round-trip and retry tests.

### M05 — Local salvage, restoration, and shop economy

**Goal:** complete a satisfying small loop without needing a dungeon or story prerequisite.

**Small slices:** finite searchable pile; one purchase from a customer; inspection/cleaning; one repair job; one dismantling operation; authoritative listings and a sale to a customer; restoration/economy persistence. Implement these as separate bounded tasks, not one unchecked expansion.

Use provisional fixtures such as `item.radio` and `recipe.radio_repair`; names and tuning values are not final content commitments.

**Acceptance criteria**

- [ ] The player finds an object, transports it, inspects it, cleans or repairs it, lists it, sells it, and can save the result.
- [ ] Search commits a result once. Refreshing UI, transferring objects, or leaving/re-entering the room does not reroll consumed loot.
- [ ] Buying one offered object transfers its unique ownership and payment atomically. Insufficient funds, inaccessible/reserved stock, stale offers, and insufficient receiving capacity leave all related state unchanged; duplicate successful commands do not charge twice.
- [ ] Repair validates materials, target ownership, station access, and reservations before consuming anything. Rejected starts leave all resources unchanged.
- [ ] Completion applies exactly once. A completed item can remain reserved in the station when collection is blocked by capacity; it is neither deleted nor duplicated.
- [ ] Dismantling one eligible object retires its input and creates accounted material outputs atomically. Check post-operation capacity, including space freed by the input. Insufficient output capacity leaves the original and all resources unchanged; retry/save-load tests prevent duplicate outputs.
- [ ] Listings reference authoritative items. A sale atomically updates ownership, listing state, customer/shop money, and its committed event.
- [ ] Prices/balances use documented integer units. Invalid prices, insufficient funds, stale listings, and arithmetic bounds have tests.
- [ ] Inspection/appraisal uncertainty does not overwrite the item's actual properties.
- [ ] Save/reload during an active job and after a sale preserves correct items, balances, remaining work, reservations, and completion state.

**Verification:** failed-operation before/after tests; duplicate completion/sale tests; manual local loop. **Defer:** haggling trees, complex demand simulation, loans/collateral, decorative shop expansion, broad recipe catalogs.

### M06 — Reusable dialogue, conditions, and quests

**Goal:** prove authored narrative uses the same world rules as ordinary play.

**Small slices:** validated dialogue definitions; dialogue presenter and choice IDs; typed conditions/effects; quest progress and journal; one radio-request integration.

**Acceptance criteria**

- [ ] `dialogue.repair_customer` presents authored lines, branches, and condition-filtered choices for `npc.repair_customer`. Choices are executed by stable ID and revalidated.
- [ ] Text-animation skip does not choose an answer or advance two nodes. Cancellation has a defined result; no cancel/confirm input leaks into the world.
- [ ] Modal dialogue freezes simulation/calendar under the working policy; presentation continues. Ambient barks are separate and do not pause play.
- [ ] Unknown conditions/effects, broken node links, missing text/speaker IDs, and malformed parameters produce file/node/field diagnostics without arbitrary code execution.
- [ ] `quest.radio_request` asks for a serviceable radio in the `PlayerBackpack` scope. A qualifying preexisting backpack radio satisfies its possession objective on acceptance/load. Readiness can regress when the item is moved, sold, or damaged; stale readiness cannot bypass turn-in validation.
- [ ] Delivery and reward claim commit together with a persistent claim ID. Evaluate post-exchange capacity, including the slot freed by the delivered radio. A reward that still cannot fit rejects the entire operation, retaining both the requested item and retryable reward; a valid full-backpack one-for-one exchange succeeds.
- [ ] Repeated conversations, duplicate commands/events, and save/reload cannot grant the reward twice.
- [ ] A separate test fixture distinguishes “sell three radios after acceptance” event progress from possession state. Repeated container transfers do not count as fresh discovery or sales.
- [ ] Requesting a save immediately after a qualifying committed event retains its quest progress exactly once at the next permitted stable boundary. Active dialogue still blocks saving; close it, then save to verify the result. Reopening dialogue derives availability from current facts, not a stale presentation cursor.
- [ ] Ordinary buying, selling, restoration, and allowed exploration still work without accepting the sample quest.

**Verification:** headless narrative/transaction tests, content validation, manual branching conversation and journal. **Defer:** runtime AI dialogue, relationship simulation breadth, cutscene editors, mandatory lore about the companion's core.

### M07 — Rideable mech, companion, and actor composition

**Goal:** make the three-entity relationship playable and visually clear.

**Small slices:** mount/control relationship; rider attachments; cargo integration; follower behavior; authored reactions and animation arbitration.

**Acceptance criteria**

- [ ] Player, large mech, and small companion keep distinct stable IDs and ownership. Mounting changes control/attachment state, not player identity.
- [ ] Mounted input controls the mech; the rider does not also move or collide independently on the ground. Mounted damage is assigned to the mech under the prototype policy.
- [ ] Dismount chooses a safe position, or refuses with a clear reason when all valid positions are blocked.
- [ ] Mech cargo uses the M03 inventory path. Mounting, dismounting, and changing location do not duplicate or lose cargo.
- [ ] Rider art is separate, attached by facing/frame-aware offsets, and layered correctly with the cockpit/body. No canonical combined mech+rider sprite is required.
- [ ] Companion follow/rejoin works around room obstacles; stuck recovery preserves ownership and does not teleport into unsafe space.
- [ ] The smaller companion can boast, react fearfully to a real challenge, celebrate, and show a brief harmless dizzy reaction. It is not permanently killed by a comic reaction.
- [ ] Movement, action, reaction, and expression priorities prevent animation systems from fighting every frame. Repeated barks/reactions have sensible cooldowns.
- [ ] Mount state, cargo, and persistent companion facts round-trip. Return-to-title/restart releases actor handlers.

**Verification:** headless control/dismount/cargo tests; manual facing/layering/size/reaction checks. **Defer:** large equipment trees, sophisticated AI editors, finalized robot names, anatomical or literal-memory story explanations.

### M08 — Authored abandoned-mall dungeon and integrated slice

**Goal:** validate exploration, robots, salvage, shop, and narrative together.

**Small slices:** transition and one small authored mall; monster encounter; loot/extraction; location persistence; full vertical-slice playthrough. A few rooms, one readable monster family, and multiple loot opportunities are sufficient prototype scope, not a final content count.

**Acceptance criteria**

- [ ] A known entrance, reachable loot, safe spawn, and usable exit exist in `location.abandoned_mall_01`. A failed destination load preserves the current session.
- [ ] Combat enemies are monsters, not humans; humanoid monsters are excluded except for the permitted skeleton exception. Visual inspection verifies this, not only a metadata tag.
- [ ] The player pilots the mech, the companion accompanies them, and an encounter has readable attack/response cues.
- [ ] Retreat and defeat outcomes are explicitly chosen before implementing them. Cargo, money, repair cost, rescue location, and recovery ownership obey that policy without unrecoverable softlocks.
- [ ] Collected loot, opened containers, encounter state, and the active expedition survive allowed save/reload and re-entry. Seed or location reload alone cannot repopulate consumed rewards.
- [ ] Re-entry returns to the current dungeon instance and its changes. Automatic reset/replenishment is not inferred from departure; a future new-run/reset policy requires an explicit design decision.
- [ ] Extraction transfers or retains cargo through the common ownership system; failed/full-destination transfers leave it available in a defined place.
- [ ] The player can return, restore a recovered item, sell it, and separately fulfill the radio request. One unique object cannot simultaneously be sold and delivered.
- [ ] The end-to-end scenario in section 5 passes, including its capacity and retry branches.
- [ ] The ordinary salvage/shop loop remains available without mandatory story participation unless an explicit, documented access gate applies.

**Verification:** cross-feature integration/save tests, manual full expedition and town loop. **Defer:** procedural generation, large dungeon catalogs, bosses, procedural puzzles, cinematic story scope.

### M09 — Renewable piles, upgrades, daily cadence, and modest story

**Goal:** make repetition change the player's options rather than merely increase quantities.

**Small slices:** maintained/replenished junk-pile states; explicit day advancement; one useful upgrade path; customer variety; short authored story chain through ordinary tasks.

**Acceptance criteria**

- [ ] Searching, sorting/maintaining, and improving a pile produce distinct feedback. Depletion and replenishment have an explicit source and schedule; reloads and room transitions cannot trigger extra replenishment.
- [ ] Sleep/travel advances game time through a defined command. Due jobs, customers, and pile updates are reconciled once; no wall-clock offline progression is accidentally introduced.
- [ ] A selected upgrade changes an observable capability such as capacity, handling, restoration access, or sorting efficiency, with costs committed atomically.
- [ ] At least one alternate acquisition or recovery path prevents ordinary shortages, optional quests, or defeat costs from permanently stopping play.
- [ ] Customer variety uses shared transaction/dialogue systems, not bespoke inventories or direct balance mutation.
- [ ] A short story chain has explicit entry conditions, committed facts, and completion. New NPC names, dialogue, or plot details are reviewed as authored content; the core motif is not silently converted into a human-brain mystery.
- [ ] Content expansion passes reference/ID/asset validation and round-trip tests. Balance changes do not masquerade as schema migrations.
- [ ] Repeated-day play shows meaningful decisions and identifies dominant money loops or tedious actions for tuning.

**Verification:** boundary-time and repeated-day tests; manual multi-day playtest with observations. **Defer:** seasons, romance, farming, huge town schedules, or collateral lending unless explicitly added to scope.

### M10 — Hardening and release candidate

**Goal:** ship the agreed small game reliably, not silently expand it.

**Small slices:** regression sweep; save recovery/compatibility; presentation/accessibility polish; performance profiling; distribution smoke test and credits.

**Acceptance criteria**

- [ ] All agreed M01–M09 gates have evidence; open blockers are resolved or explicitly excluded from release scope.
- [ ] Clean supported-machine setup, content build, launch, input, audio, save directory, and Continue work using documented prerequisites.
- [ ] Critical inventory/economy/quest conservation and idempotency tests pass; malformed content and corrupt/unsupported saves fail clearly without damaging valid data.
- [ ] Session/scene/resource lifetime checks pass across repeated transitions. Frame-time and memory targets are recorded against named test hardware and measured, not assumed from compilation.
- [ ] Text readability, UI scale, feedback not dependent solely on color, volume controls, animation readability, and the supported input devices are manually checked. Unimplemented accessibility settings are not advertised.
- [ ] No placeholder/debug controls, development-only save fixtures, secrets, or unintended content appear in the distributable build.
- [ ] Third-party asset/source licenses and credits are recorded; generated art has been checked as production assets, not trusted as aligned sheets automatically.
- [ ] A backup/recovery procedure and known limitations are documented. Publication or release upload remains a separate authorized action.

**Verification:** full relevant automated suite, content validation, supported-machine manual smoke test, recorded performance sample, and release checklist.

## 5. End-to-end acceptance scenario

Run this at M08 and again after significant cross-system changes. All named content is a placeholder fixture, not finalized narrative.

1. Launch to title. Start a fresh session in `location.shop`; check that Continue availability matches actual save state.
2. Talk to `npc.repair_customer`, accept `quest.radio_request`, then close dialogue. Confirm the closing input does not activate a nearby object.
3. Mount the mech, inspect cargo capacity, and depart for `location.abandoned_mall_01` with the companion.
4. Encounter a non-human monster and recover two distinct salvage objects, including a repairable radio. Re-enter a cleared room; previously collected objects must not respawn.
5. Extract safely and use the common transfer UI. Make the destination full once; verify cargo is retained, free space, then retry.
6. With the fixture's required materials available, start `recipe.radio_repair`. Save at an allowed boundary while the job is active, return to title, reload, and complete it exactly once. Collect the finished radio into the player backpack before delivery.
7. Deliver the serviceable radio through dialogue. First use a fixture whose two unstackable reward items cannot fit even after delivery frees the radio's one slot: no delivery or reward commits. Free enough additional capacity and retry; the successful operation delivers and rewards once. Separately verify that a one-for-one exchange succeeds in a previously full backpack.
8. Restore or otherwise prepare the second object and sell it. Verify listing removal, customer ownership, and money change agree.
9. Save, return to title, Continue, and repeat the turn-in choice or command. No duplicate reward, restored item, sale, or loot is created.
10. Start another session without retaining the former quest, cargo, subscriptions, or scene state. In a separate run, complete ordinary salvage and selling without accepting the quest.

Also test accepting the radio request while already holding a serviceable radio in the player backpack. That state objective must not require the player to discard, reacquire, or repair it again. Move it into storage and back to prove readiness follows the declared scope, not a permanently latched “ready” flag.

## 6. Optional procedural-generation gate

Procedural dungeons are not required for M08 or an otherwise complete first release. Add them only after the authored expedition is enjoyable and expanded generation is explicitly requested.

Before merging that slice, demonstrate seeded reproducibility for a declared generator/content version, valid start/exit reachability, safe placements, bounded retries with a defined failure/fallback result, and persistent layout/state or a compatible seed-plus-deltas model. Collected loot and completed events must remain consumed after save/load. Define what happens when generator/content versions change; never regenerate a saved run silently and lose its state.

A generation soak test records seeds, failures, and retry counts over an agreed batch. This is additional evidence, not a substitute for playtesting room quality.

## 7. Decisions, risks, and scope control

### Decision checkpoints

| Decide before | Question | Safe working position |
| --- | --- | --- |
| M00 implementation | Actual projects, SDK, dependencies, content pipeline? | Inspect; preserve existing compatible choices. |
| M02 art standardization | Tile size, world scale, sprite bounds, animation counts, render resolution? | Use placeholders and a shared scale lab; 48×48 source is allowed, not mandatory. |
| M04 persistence | Save timing, slots, compatibility and missing-content behavior? | Safe-boundary manual saves; versioned validation; preserve unreadable originals. |
| M06 narrative | Quest turn-in/overflow semantics and conversation cancellation? | Atomic delivery plus reward; insufficient post-exchange capacity leaves the whole exchange retryable; reopen from facts. |
| M07 combat integration | Mounted damage and companion danger? | Mech owns mounted damage; companion reactions are temporary, not permadeath. |
| M08 defeat | Retreat/defeat costs and recovery path? | Choose and document before implementation; prevent irrecoverable progression loss. |
| M09 content | Renewal cadence, costs, upgrades, named cast, story beats? | Tune small content sets; explicitly label proposed lore and values. |
| M10 release | Supported machines/input and performance targets? | Declare a modest tested target; do not imply untested platform support. |

### Risk register

| Risk | Early warning | Response / proving gate |
| --- | --- | --- |
| Framework work replaces game progress | Empty abstractions/projects proliferate | Feature folders and playable exits; review at M00/M01. |
| Assets dictate inconsistent scale | Companion larger than player; jittering rider | M02 scale lab and M07 attachment tests before bulk art. |
| Item or money duplication | Remove/add or callbacks mutate separately | M03/M05 transactions, conservation tests, stable operation IDs. |
| Save loses committed quest progress | Save races observers or rewards | M04 snapshot boundary; M06 immediate-save/reload and duplicate-claim tests. |
| UI owns gameplay rules | Dialogue and shop behave differently | Shared command paths; validate stale choices and failed operations. |
| Dungeon scope balloons | Generator/boss pipeline precedes one room loop | Authored M08 first; optional generation has its own gate. |
| Progress becomes a chore or softlock | Only profitable route requires missing resources | M09 multi-day playtest, alternate recovery paths, explicit balance adjustments. |
| “Done” means only “builds” | Visual/audio/input behavior untested | Separate automated results from manual acceptance at every milestone. |

Outside the baseline are multiplayer, online services, telemetry, runtime AI-generated dialogue, modding platforms, general-purpose editors, and full pawn-loan/redemption mechanics. Brainstorming about them does not add them to the schedule.

The next implementation request should name one milestone or sub-slice, its acceptance checks, and its exclusions. Reassess the next slice from evidence after each completed playable increment rather than treating this document as an unattended batch job.
