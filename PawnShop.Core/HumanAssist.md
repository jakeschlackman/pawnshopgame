# Working on Core
Core is the shared, headless domain layer. It is intended to own items, inventory, interactions, conditions/effects, economy, atomic operations, world state, and neutral save storage.

## What exists
`World/RoomState.cs` contains the current room clock/bell fixture and immutable RoomView. The other feature folders are scaffolding. There is no implemented shared inventory, economy, or gameplay save system to configure yet.

## Add a domain slice
Choose a small requested operation, identify its authoritative state, and keep its rules independent of Game, textures, devices, and ContentManager. Read the relevant [engineering contract](../docs/ENGINEERING_CONTRACTS.md) before implementing transactions, ownership, conditions, or persistence.

An operation involving items and money must validate and commit them together. Views should not provide a mutation path. Shared operations belong here; Shop/Narrative workflows use them rather than keeping competing item or wallet models. Feature-specific save DTOs belong to features; Core storage stays neutral.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Add headless tests for meaningful invariants and refusal paths. Save tests must use isolated temporary directories and preserve corrupt/unsupported inputs. No manual graphics check is needed for domain-only behavior.
