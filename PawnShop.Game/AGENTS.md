# Game

- Thin executable/composition and main-menu UI. Before adding a file, check the owning feature project or Framework; existing non-menu code here is not a precedent to expand it.
- `Program.cs` and `PawnShopGame.cs` host MonoGame. `Bootstrap/ApplicationFlow.cs` owns navigation/input routing; `GameSession.cs` owns a session. Keep host work delegated.
- Read [application placement](../docs/ENGINEERING_CONTRACTS.md#application-placement-and-composition) and [lifecycle](../docs/ENGINEERING_CONTRACTS.md#application-session-scene-and-ui-lifecycle) rules when changing these boundaries.
- Settings: `Bootstrap/AppSettings.cs` and `OptionsModel.cs`. Screens draw using shared layout; pointer input must use the same bounds and semantic commands.
- `Screens/GameScreens.cs` composes title/Options/room/creation. Creation presentation belongs in Characters/Player/Create. Shared controls/rendering belong in Framework.
- Audio owns sources/playback; Game supplies AudioMix, playback policy, and requests. Common-NPC sources belong in Characters; this project packages their runtime subset.
- Authored NPC/Ink changes: read `Content/Npcs/AGENTS.md`. Human player-art changes: read the root artwork routing before touching `Content/Player`. Title/font provenance is in the relevant content README.
- Use root build/runner commands; normal Game builds run MGCB and Ink compilation. Do not hand-edit generated output or launch/control the game unless the user requests it.
