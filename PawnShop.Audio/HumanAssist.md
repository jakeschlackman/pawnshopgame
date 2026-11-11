# Working on Audio
Audio owns playback and its source/content pipeline. Game decides when sounds should play and supplies settings as an immutable AudioMix.

## Add a sound
1. Preserve the original supplied recording and its available provenance/license information under the relevant category's `Sources` folder. Do not invent a license.
2. Put the intended runtime derivative under `Content/Music`, `UI`, `Environmental`, `Combat`, `Monster`, or `Speech`.
3. Add its build entry to `Content/Content.mgcb`, following an existing SongProcessor or SoundEffectProcessor entry.
4. Add a stable runtime ID in `AudioAssets.cs` when the playback code needs one. IDs start with `Audio/` and omit file extensions.
5. Reuse the playback components and wire an explicit request through Game. Keep screen/settings decisions outside this project.

Compiled files reach consumers under `Content/Audio`; do not copy WAVs into Game or edit generated XNB/Ogg output. A category is not a new volume channel: UI uses Effects and Environmental uses Ambient.

## Adjust playback
Start with `AudioPlayback.cs` and `AudioMix.cs`. The `Playback` folder contains owned music/effect adapters, shuffled sample selection, and slider cooldown handling. Keep focus mute and live gain changes from restarting music. Stop/dispose voices before releasing their content scope.

See [menu music](../docs/VERIFICATION_MENU_MUSIC.md) and [Options audio](../docs/VERIFICATION_OPTIONS.md) for existing source records and acceptance limits.

## Check it
From the repository root, after a code change:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
```

For documentation-only edits, inspect the diff and run `git diff --check`. The console runner is the regression suite; `dotnet test` does not run it.

Relevant regressions include AudioProjectTests, MenuMusicTests, and the affected sound-control tests. Listen manually for attacks, decay, clipping, repetition, overlap, volume, focus mute, and transitions; headless checks cannot prove audible quality.
