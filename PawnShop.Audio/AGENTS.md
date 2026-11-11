# Audio

- Owns playback, immutable mix snapshots, asset IDs, categorized source audio, and its MGCB pipeline. No PawnShop project references.
- Start with `AudioPlayback.cs` (owned voices/host update), `AudioMix.cs` (gain/focus mute), and `AudioAssets.cs` (runtime IDs).
- Reuse `Playback/LoopingMusic.cs`, `SongMusicTrack.cs`, `SoundEffectPlayer.cs`, `ShuffledSoundBag.cs`, and `SliderClickSequence.cs` rather than adding parallel playback paths.
- Add approved sources to `Content/Music`, `UI`, `Environmental`, `Combat`, `Monster`, or `Speech`, then register runtime derivatives in `Content/Content.mgcb` and `AudioAssets` as needed. Preserve source/provenance records in category `Sources` folders.
- The project packages compiled `Content/Audio` for consumers. Do not put source WAVs into Game or edit generated XNB/Ogg output.
- Game owns settings storage, screen playback policy, and UI requests; pass an AudioMix and requests into Audio once per host update. UI uses Effects; Environmental uses Ambient. Folders do not create extra mix channels.
- Stop/dispose instances before their content scope; report expected device/content failures once. Audio feedback cannot decide whether a domain operation committed.
- For changed resource contracts read [artwork/resources](../docs/ENGINEERING_CONTRACTS.md#artwork-and-resources). Regressions: `AudioProjectTests.cs`, `MenuMusicTests.cs`, and affected sound-control tests in Tests. Use root build/runner commands; listening remains manual.
