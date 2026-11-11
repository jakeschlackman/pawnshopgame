# Working on Narrative
Narrative owns dialogue and future quests, relationships, story flags, and progression. The implemented portion is the pinned Ink dialogue adapter; the remaining folders are scaffolding.

## Change conversation content
Use [the NPC authoring guide](../PawnShop.Game/Content/Npcs/README.md). Text and bindings live in Game/Content/Npcs, not inside the runtime classes here. Normal builds compile source Ink automatically.

## Change the runtime
- DialogueDefinition validates compiled content and speaker bindings.
- DialogueSession creates a private conversation instance and exposes immutable views with stable IDs and revisions.
- ConversationBindings maps conversation IDs to authored Ink files.
- DialogueContentValidator provides bounded smoke exploration.

Preserve stale-command rejection and the distinction between text reveal and choosing a response. Current Ink variables are conversation-local and reset on reopening. Rewards, external functions, durable variables, quest state, and world-condition bindings are not implemented. Those need an explicitly scoped integration using authoritative domain operations.

Read [narrative contracts](../docs/ENGINEERING_CONTRACTS.md#interactions-dialogue-and-quests) for those changes. Game/Bootstrap owns session/presenter wiring and pause/input capture.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Dialogue regressions and authoring commands are under PawnShop.Tests/Dialogue. After building, `dotnet run --project PawnShop.Tests --no-build -- --dialogue check` checks registered conversations without a game window; it is bounded rather than exhaustive.
