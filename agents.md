# AGENTS.md — Pawn Shop Game

## Reading and scope

- Read this file and the AGENTS.md / AGENTS.override.md files governing the files being changed, including when starting at the repository root.
- Read only relevant code, tests, and docs. Do not read every project's guide, every Markdown file, or all historical verification records.
- **HumanAssist.md is human-only.** Do not read its contents, include it in routine searches, or treat it as agent instructions. Read/create/edit it only when the user explicitly requests creating or updating those files. Exclude it case-insensitively from content searches, for example with `rg --iglob '!HumanAssist.md'`.
- Reuse unchanged session context. Consult ARCHITECTURE.md for ownership/contracts and GAME_DESIGN.md or ROADMAP.md for gameplay/scope.
- Detailed rules remain binding in [engineering contracts](docs/ENGINEERING_CONTRACTS.md); read only the topic affected by the task. Project guides provide entry points and targeted links.
- Report unreadable required guidance. Routine work needs no document inventory.

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

Read applicable instructions, relevant code, and nearby tests. Inspect project/SDK/package/build configuration when needed for the affected build or dependencies. Read supporting docs as described above.

Use the actual repository as evidence for what exists. Do not assume an earlier discussion's class names, folders, dependencies, or milestones have been implemented.

Before substantive changes:

1. Check the working tree and identify files affected by the task.
2. Locate existing implementations that could be reused or extended.
3. Establish the relevant build/test commands and any pre-existing failures.
4. State the intended behavior and acceptance checks.
5. Implement the smallest complete change that satisfies the request.

Use the repository's existing SDK, MonoGame target, content build workflow, test framework, naming, and formatting. Do not upgrade packages or retarget projects incidentally. Pin newly approved dependencies consistently with existing conventions; do not assume the latest version is appropriate.

### Keep small changes focused

- Make the smallest complete change; reuse existing helpers and avoid unrelated refactors.
- Run appropriate build, regression, and content checks after implementation. Run a separate baseline when relevant unknown failures or risk warrant it.
- Repeat passing checks only after relevant changes, failures, or unresolved concerns.
- For docs-only edits, review the text and run `git diff --check`; build/test only if commands, build/content inputs, or affected behavior require verification.
- Update the owning documentation for durable changes; avoid duplicating routine updates across design docs and logs.

## 4. Architecture and dependency boundaries

Use composition over deep inheritance. Components describe focused state/capabilities; systems or services implement behavior. Not every class needs an interface, component, factory, or event.

Build a game-specific reusable foundation, not a general-purpose engine. Introduce abstractions for demonstrated reuse, important invariants, or testability. Avoid speculative ECS frameworks, editor platforms, and plugin systems. The explicitly approved nine-project layout below includes scaffolding for future features; do not add speculative classes to fill it or infer feature completion from its folders. The user approved extracting PawnShop.Audio on 2026-11-10.

`PawnShopSimulator.sln` and these nine project directories live together at the repository root. This approved structure supersedes earlier project-layout proposals:

| Project | Owns | Allowed PawnShop project references |
| --- | --- | --- |
| [PawnShop.Framework](PawnShop.Framework/AGENTS.md) | Reusable entities/components/systems, scenes, input, UI, events, rendering | None; no game-specific rules |
| [PawnShop.Audio](PawnShop.Audio/AGENTS.md) | Playback, mix snapshots, audio asset IDs and categorized audio content/build pipeline | None; presentation audio, no domain rules |
| [PawnShop.Core](PawnShop.Core/AGENTS.md) | Shared items, inventory, interactions, conditions/effects, transactions, economy, world state, persistence contracts/storage | Neutral Framework contracts only when needed; remain headless-testable |
| [PawnShop.Narrative](PawnShop.Narrative/AGENTS.md) | Dialogue, quests, relationships, story flags, progression | Core |
| [PawnShop.Characters](PawnShop.Characters/AGENTS.md) | AI, behaviors, movement, combat, player, mech, companion | Core, Framework |
| [PawnShop.Shop](PawnShop.Shop/AGENTS.md) | Customers, appraisal, restoration, displays, shop workflows | Core |
| [PawnShop.Exploration](PawnShop.Exploration/AGENTS.md) | Dungeons, salvage, monsters, hazards, generation | Core, Characters, Framework |
| [PawnShop.Game](PawnShop.Game/AGENTS.md) | Thin MonoGame executable/composition layer and main-menu UI; feature code/content belongs in its owning project | All seven production libraries |
| [PawnShop.Tests](PawnShop.Tests/AGENTS.md) | Existing regression runner and future focused tests | Projects exercised by each test |

Keep project references acyclic. Put shared contracts in a dependency-neutral location. Register feature-specific handlers at the composition boundary. Integration tests may reference the layers they exercise.

Read the owning project's AGENTS.md before adding feature files. Game remains
limited to the thin host, composition, and main-menu UI; lower projects never
reference it. Keep domain rules headless and references acyclic. Detailed
[placement/composition rules](docs/ENGINEERING_CONTRACTS.md#application-placement-and-composition)
apply when ownership or wiring changes.

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

Preserve application/session/location/overlay lifetimes and focused input capture. For affected behavior read the [lifecycle rules](docs/ENGINEERING_CONTRACTS.md#application-session-scene-and-ui-lifecycle) and Game instructions.

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

Commands request changes; immutable events describe committed facts. Multi-resource changes must be atomic, with events after commit and retry protection. Read the [transaction rules](docs/ENGINEERING_CONTRACTS.md#commands-events-and-transactions) when changing these operations.

## 9. Shared inventory and economy

Use one authoritative inventory/ownership and money model across features. Failures preserve all related state. Before implementing transfers, sales, listings, or restoration, read the [inventory/economy rules](docs/ENGINEERING_CONTRACTS.md#inventory-and-economy) and Core/Shop instructions.

## 10. Interactions, dialogue, and quests

Reuse interaction/condition contracts and authoritative domain operations. For affected dialogue, quest, reward, or interaction behavior read the [narrative rules](docs/ENGINEERING_CONTRACTS.md#interactions-dialogue-and-quests) and Core/Narrative instructions.

## 11. Player, mech, companion, and exploration

The player, rideable mech, and smaller companion remain separate identities. For movement/control, mounting, combat, locations, or generation, read [actor/exploration rules](docs/ENGINEERING_CONTRACTS.md#actors-and-exploration) and Characters/Exploration instructions.

## 12. Pixel art, animation, and resources

Separate source canvas, visible body, world size, origin, footprint, and output
scale. Preserve source/provenance, point sampling, stable origins, shared clocks,
and deliberate resource lifetimes.

For human art work anywhere in the repository, read
[artwork rules](docs/ENGINEERING_CONTRACTS.md#artwork-and-resources),
[the wearable workflow](docs/WEARABLE_ART_WORKFLOW.md), and Tests/Assets/AGENTS.md.
Those retain the exact two-body, native-coordinate, Skeleton v3 S/N/E, mirrored
West, garment extraction, and per-pose approval requirements. Sprout's existing
artwork, animation, and implementation are frozen unless explicitly requested.
Existing UI wiring does not require regenerating assets.

## 13. Persistence and content validation

Persist versioned data snapshots at a consistent boundary, not live services or device objects. Preserve corrupt/unsupported originals and use isolated test saves. For saving/loading or content-schema work read the [persistence rules](docs/ENGINEERING_CONTRACTS.md#persistence-and-content-validation).

## 14. Coding, performance, and verification

Follow repository conventions; keep methods focused, dependencies explicit, and
errors actionable. Avoid speculative abstractions, new frameworks, and premature
optimization. Measure before pooling, parallel simulation, or complex indexing.
For the detailed invariants relevant to a change, read
[verification rules](docs/ENGINEERING_CONTRACTS.md#coding-performance-and-verification).

After a code/content change use the verified root commands as appropriate:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

Restore only when needed; do not install SDKs, upgrade dependencies, alter security
settings, or bypass access restrictions to hide environment problems. Keep
configuration/framework consistent. The normal build runs the existing MGCB/Ink
workflow. `dotnet test` does not execute this console runner. See Tests/AGENTS.md
for focused test/tool entry points.

Use headless rules tests. The user handles rendering, audio, animation, input feel,
window behavior, and desktop workflows. Do not launch/control the game, inspect
screenshots, use computer vision, or use Computer Use for verification unless the
user explicitly requests it. Manual acceptance is not a blocker to a completed
implementation handoff; compilation/numeric checks do not prove visual behavior.

## 15. Milestones and completion

Implement only the requested slice. Empty folders are scaffolding, not feature
completion. ROADMAP.md owns milestone status; the original
[lifecycle acceptance rules](docs/ENGINEERING_CONTRACTS.md#milestones-and-completion)
remain applicable when that work is requested.

A change is complete when:

- The requested acceptance criteria are met.
- Related invariants have tests where practical.
- Relevant builds/tests/content checks were run, or blockers are accurately reported.
- The final diff contains no unrelated edits or generated clutter.
- Relevant documentation reflects changed contracts or workflows.

Final handoff: summarize behavior and files changed, exact verification performed with results, unverified/manual checks, and remaining blockers. Do not claim commits, tests, launches, or visual inspections that did not occur.

## 16. Keep guidance coherent

This file owns shared development rules; scoped guides and engineering contracts retain the detailed rules. Use the existing root ARCHITECTURE.md, GAME_DESIGN.md, and ROADMAP.md for detailed architecture, gameplay design, and milestone status. Verification records live under docs/. Do not create an art specification or move documentation merely because earlier proposals suggested it.

Distinguish decisions from proposals. Update the owning document; change others only to correct misleading guidance. Small UI edits need not update every design doc/log. Keep verification records for milestones or substantial evidence; ordinary results belong in the handoff. No transcripts, speculative APIs, copied tutorials, or progress logs here.

When existing code and these rules conflict, identify the smallest safe alignment needed for the current task. Propose consequential redesigns rather than silently rewriting the repository.
