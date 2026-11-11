# Working on Tests
This project is an executable regression suite plus offline dialogue/asset tools. It does not use VSTest, and `dotnet test` does not run its checks.

## Add a regression
Find the closest existing test file, use the current assertions/fixtures, and register a new check in Program.cs when needed. Test meaningful behavior: successful commits, refused operations leaving state unchanged, input capture, stable identities, pause/lifetime behavior, and safe save handling.

Keep tests headless: no MonoGame host, graphics/audio device, or real save files. Device adapters have test boundaries. Use isolated temporary folders for file fixtures and clean up only those fixtures. Avoid tests that simply restate implementation.

## Offline tools
Program.cs routes `--assets` to Assets/PlayerAssets.cs and `--dialogue` to Dialogue/DialogueAuthoring.cs. PNG tooling uses System.Drawing.Common here only and requires Windows. See the [asset contributor guide](Assets/HumanAssist.md) and [NPC authoring guide](../PawnShop.Game/Content/Npcs/README.md). There is no built-in test-name filter.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

The runner reports each failure and returns nonzero. Source/content hash expectations should change only for deliberate approved replacements, not merely to silence failures. Visual/listening acceptance is reported separately.
