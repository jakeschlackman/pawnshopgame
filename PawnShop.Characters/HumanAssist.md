# Working on Characters
Characters owns actor identity, movement, AI/behavior, combat, player, mech, and companion features. It references Core and Framework, never Game.

## What exists
- PlayerState and MovementBounds implement held movement, facing, and swept footprint collision.
- Player/Create owns character-selection presentation; see its [contributor guide](Player/Create/HumanAssist.md).
- NpcDefinition describes authored NPC identity/sprite references.
- Npcs/Common owns the approved modular appearance catalog, seeded generator, and bounded room entrance. See [AUTHORING.md](Npcs/Common/AUTHORING.md).
- Other actor folders are scaffolding, not completed mech/companion/combat systems.

## Make a change
Keep movement independent of source-canvas size, use one animation clock, and preserve the current cardinal-axis tie policy. Core owns inventory/items; Game/Bootstrap supplies session composition and cross-feature coordination.

For new art, follow the [wearable workflow](../docs/WEARABLE_ART_WORKFLOW.md) and [artwork contracts](../docs/ENGINEERING_CONTRACTS.md#artwork-and-resources). Keep both body fits and per-pose approvals explicit. Sprout is frozen unless the user explicitly requests a change.

For mounting, companion behavior, or combat, read the [actor contract](../docs/ENGINEERING_CONTRACTS.md#actors-and-exploration) and implement only the requested slice.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Player and Characters tests cover the existing headless rules. In-game motion, layering, fit, and scale remain manual checks.
