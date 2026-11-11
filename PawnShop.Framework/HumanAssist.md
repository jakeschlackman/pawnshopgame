# Working on Framework
Framework contains reusable input, controls, animation metadata, and rendering utilities. It uses MonoGame but has no references to the game's feature projects.

## Find the right starting point
- Input actions/bindings: `Input/KeyboardBindings.cs`, `KeyboardInput.cs`, and `InputFrame.cs`.
- Focus/activation and scrolling: `UI/Menu.cs`, `SelectionModel.cs`, `Button.cs`, and `ScrollState.cs`.
- Drawing and fonts: `Rendering/PixelRenderer.cs`, `FontCatalog.cs`, and `FontLibrary.cs`.
- Sprite clips/clocks: `Rendering/SpriteSheetDefinition.cs` and `SpriteAnimator.cs`.

## Make a change
Find an existing consumer and extend the smallest shared capability it needs. Keep screen-specific commands, item rules, and character decisions in their owning projects. Preserve held movement versus pressed actions, modal input capture, point sampling, stable pivots, and texture ownership. Frame selection should be testable without creating a graphics device.

Relevant tests live under `PawnShop.Tests/Framework` and in the font, sprite, panorama, and binding test files. Read the [architecture](../ARCHITECTURE.md) only for the boundary you are changing.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Rendering and input feel need manual confirmation; automated math/metadata checks do not establish visual acceptance.
