# Player creation

- Start with `PlayerModel_SelectionScreen.cs`; its `CharacterSelectionLayout` shares drawing, pointer, and selector geometry.
- Navigation/session creation: `PawnShop.Game/Bootstrap/ApplicationFlow.cs`.
- Composition/pointer input: `PawnShop.Game/Screens/GameScreens.cs` and `MenuPointerInput.cs`.
- Tests: `PawnShop.Tests/PlayerTests.cs`; consult lifecycle/music tests when affected.
- Creation presentation belongs here; session composition stays in Game/Bootstrap. Characters must not reference Game.
- Reuse models, animator, textures, and layout. Keep selection sessionless until confirmation, capture travel/arrival input, and preserve the shared clock. Draw only reads state.

Use the root verification commands; see PlayerTests, LifecycleTests, PanoramaTests, and MenuMusicTests for affected behavior. HumanAssist.md is human-only under the root rule.
