# Framework

- Owns reusable input, UI, animation metadata, and rendering infrastructure; no PawnShop project references or game-specific commands.
- Start in `Input/KeyboardBindings.cs`, `KeyboardInput.cs`, and `InputFrame.cs` for device/action changes; keep held axes separate from action edges.
- Reuse `UI/Menu.cs`, `SelectionModel.cs`, `Button.cs`, and `ScrollState.cs`; controls emit commands and read state.
- Rendering entry points: `Rendering/PixelRenderer.cs`, `FontCatalog.cs`, `FontLibrary.cs`, `SpriteSheetDefinition.cs`, and `SpriteAnimator.cs`. Preserve point sampling, origins, shared clocks, and resource ownership.
- Concrete application menus/composition belong in Game; feature presentation belongs in its owning project. Do not move domain rules here for convenience.
- For changed lifecycle or asset contracts, read only the matching [engineering rules](../docs/ENGINEERING_CONTRACTS.md).
- Regressions: `PawnShop.Tests/Framework/FrameworkTests.cs`, font, sprite, panorama, and binding tests as relevant. Use root verification commands once; headless tests must not instantiate a device.
