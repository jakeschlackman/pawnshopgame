# Working on Exploration
Exploration is the intended owner of dungeons, salvage, monsters, hazards, and generation. It references Core, Characters, and Framework. The current folders are scaffolding; no expedition or generator is implemented.

## Start a feature
Choose the smallest requested location/search/encounter slice. Read the relevant [exploration rules](../docs/ENGINEERING_CONTRACTS.md#actors-and-exploration). Reuse Core ownership/transactions and Characters movement/combat capabilities.

Searching and collecting are different actions: a full bag should leave a committed find available, not reroll or delete it. Location reloads must preserve collected loot and world changes. Validate spawns and exits using the actual player/mech footprint.

Opponents are monsters, never people; avoid humanoids except skeletons. Procedural generation is a separate scope decision with controlled seeds, versioning, bounded retries, and reachability checks. A small authored location can prove the loop first.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Use deterministic headless checks for state, movement, generation, and failure paths. Test travel/persistence integration as those systems arrive; manual play checks are separate evidence.
