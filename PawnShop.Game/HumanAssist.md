# Working on Game
Game is the MonoGame executable and composition boundary. New feature behavior belongs in the owning library; Game is not the default home just because it references all libraries.

## Find the right files
- Program/PawnShopGame: host startup, update/draw delegation, display/device integration.
- Bootstrap/ApplicationFlow and GameSession: navigation, focused input, session lifetime, and feature wiring.
- Bootstrap/AppSettings and OptionsModel: preferences and Options interaction rules.
- Screens/GameScreens and the concrete screens: composition and drawing; shared geometry must also drive pointer input.
- Characters/Player/Create: character-selection presentation, outside this project.

Read [placement/lifecycle rules](../docs/ENGINEERING_CONTRACTS.md#application-placement-and-composition) when changing ownership or lifetime. Existing non-menu code in Game can be moved through a separately scoped refactor.

## Content and authoring
Use the existing [NPC](Content/Npcs/README.md), [title artwork](Content/TitleScreen/README.md), and [font](Content/Fonts/README.md) guides. Audio sources/build are owned by PawnShop.Audio; common-NPC source art/catalog by Characters. Game packages their runtime content.

Source Ink compiles through build/Ink.targets; fonts/audio use pinned MGCB pipelines. Generated obj/bin content is not authoritative. No project/package/SDK upgrade is needed to add ordinary content.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Only launch manually when ready to inspect the change. Check title/selection/Options/pause/return transitions, focus loss, model/overlay alignment, and audible playback as relevant. Automated rules tests do not establish desktop acceptance.
