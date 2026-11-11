# Working on Shop
Shop is the intended home for customers, appraisal, restoration, displays, and business workflows. It currently contains scaffolding folders only and references Core.

## Start a feature
Select one requested playable slice. Read the matching [transaction/inventory contracts](../docs/ENGINEERING_CONTRACTS.md#inventory-and-economy) and inspect Core before adding a new model. Use shared inventory, item identity, and money operations rather than implementing separate shelf or workstation ownership ledgers.

Listings should reference actual items. Appraisal uncertainty must not change true item properties. Restoration must reserve inputs and complete once, with an explicit full-output policy. These are requirements for future implementation, not existing APIs.

Characters owns shoppers as actors and their appearances; Narrative owns conversations and quests. Game/Bootstrap composes those with shop operations. Pawn loans, deadlines, prices, schedules, and haggling are not automatically part of a requested sale feature.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Add headless tests for complete success and unchanged state after refusals, especially item/money exchanges, reservation failures, repeated completion, and output capacity.
