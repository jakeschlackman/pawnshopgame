# Tests and offline authoring

- This is a dependency-free executable regression runner, not VSTest. Register checks in `Program.cs`; use existing assertion/helpers and return nonzero on failure.
- After building, run `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`. `dotnet test` does not execute these checks; there is no test-name filtering option.
- `Program.cs` routes `--assets` and `--dialogue` to existing offline tools. Read `Assets/AGENTS.md` for human PNG tooling; `Dialogue/DialogueAuthoring.cs` and the NPC authoring guide for Ink CLI work.
- Place regressions beside the owning behavior: Framework, Characters, Dialogue, player/lifecycle, Options, audio, or content tests. Add focused invariant/failure checks; avoid tests that mirror implementation.
- Headless tests must not create a MonoGame host or graphics/audio device. Test playback via the existing device boundary; use isolated temporary save/asset directories, never user saves.
- `System.Drawing.Common` is pinned here for Windows-only PNG authoring and must not become a production dependency.
- Preserve content identity expectations when replacing approved assets deliberately; do not refresh hashes merely to silence a failing test.
- Read [verification rules](../docs/ENGINEERING_CONTRACTS.md#coding-performance-and-verification) for relevant failure cases. Reuse the root build command once; no compile/run is needed for docs-only edits.
