# Menu theme music

Selection follow-up (2026-11-11): the intro, title, title-side Options and character
selection now share continuous playback. New Game's pan, time spent choosing a
model, and Back preserve the same music instance and cursor. Gameplay still stops
the track; returning from play restarts it. Headless playback counters verify no
extra play/stop during travel or selection, including focus mute and return.
Build/content passed with zero warnings/errors; the regression suite passed 95/95.
No game launch or listening test was performed.

Current ownership (2026-11-10): music source is
`PawnShop.Audio/Content/Music/Forgotten_Treasures.wav`; playback types now use
`PawnShop.Audio`, with `AudioAssets.MenuTheme` resolving to
`Audio/Music/Forgotten_Treasures`. The unchanged source hash, loop behavior and
listening limitations below still apply. Earlier Game/Framework paths are historical.

Date: 2026-11-10. User supplied the theme WAV and requested menu/Options playback,
automatic repetition and fades only at the loop boundary. Existing uncommitted
title-intro/panorama work is preserved. No game launch or listening check was performed.

## Asset and behavior

`PawnShop.Game/Content/Audio/Forgotten_Treasures.wav` is a byte-identical copy
of the user-supplied local Audio Studio output `Forgotten_Treasures_2m20.wav`,
renamed at the user's request. It replaces the earlier mistaken theme; that
superseded imported source is preserved in the ignored local cache, outside active
content. Original external files are unchanged. No license beyond the supplied
asset is asserted. RIFF PCM, stereo, 44,100 Hz, 24-bit, 140 seconds,
37,044,044 bytes. SHA-256:

```text
D5AAD4009AC742BF84CDE2697391A8D57E7D78C4D9123AB23AF7A8B61FE6F5EA
```

The existing pinned MonoGame 3.8.4.1 content build uses WavImporter and SongProcessor
at Best quality. DesktopGL output contains a compressed Ogg stream plus a
Song XNB. The source WAV remains in the repository and is not copied to runtime
output. No new packages, external conversion tools or runtime generation were added.

The launch intro, main menu and title-side Options share uninterrupted playback.
The native player repeats the complete track. Its last 1.5 seconds fade down using
SmoothStep gain, and subsequent passes fade up over their first 1.5 seconds. Fades
read elapsed playback time and duration, independent of simulation time. Initial entry
has full configured gain; screen departures stop immediately. This is a fade through
silence at the repeat boundary, not overlapping playback. Musical seam quality and
any silence authored into the recording remain subject to listening acceptance.

Character selection now retains the theme. Room/dialogue/pause and pause-side
Options do not play it. Returning from gameplay starts it from the beginning. The recording's base gain
is now 0.5, halving its playback level before applying Master/Music preferences
and the fade envelope. At the default 80/80 settings, steady gain is 0.32 rather
than 0.64. Saved preferences are unchanged. Master/Music preview gain multiplies
the fade envelope. Focus mute sets gain to zero without stopping or
resetting the audio cursor. Applying/discarding settings keeps/restores gain through
the existing Options workflow. Slider feedback now uses the authored marimba
SoundEffect described in [Options verification](VERIFICATION_OPTIONS.md).

## Ownership and changed files

- `PawnShop.Framework/Audio/LoopingMusic.cs` owns playback transitions, loop gain
  and release. Its narrow IMusicTrack device boundary permits headless tests.
- `PawnShop.Framework/Audio/SongMusicTrack.cs` is the application's sole MediaPlayer
  adapter, lazily loading one Song through an owned ContentManager. Its monotonic
  presentation clock resets at native track-start/repeat notifications; the
  DesktopGL decoder read-ahead cursor would otherwise skip part of the fade-in.
  The notification publishes only an atomic timestamp; the host Update owns
  volume and envelope changes. Release unsubscribes notifications and stops
  playback before disposing content. Unload/disposal is idempotent; expected asset
  or device failures release once, disable further playback attempts and report in
  Options without preventing menu use.
- `PawnShop.Game/PawnShopGame.cs` owns the music adapter and synchronizes it once
  per Update using the existing title-background state, Options settings and focus.
- `PawnShop.Game/Content/Content.mgcb` builds the preserved source WAV.
- `PawnShop.Tests/MenuMusicTests.cs` and `Program.cs` add four focused checks.
- `ARCHITECTURE.md` and `README.md` document the resulting behavior and workflow.

## Automated verification

Baseline: solution build succeeded with zero warnings/errors; 71/71 tests passed.

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
dotnet publish PawnShop.Game/PawnShop.Game.csproj --no-build --no-restore --configuration Debug --output .cache/forgotten-treasures-final-publish
git diff --check
```

Build and MGCB music compilation passed with zero warnings/errors. All 75/75
regressions passed. Whitespace checks passed. New headless checks cover continuous
intro/title/Options playback, character-selection/gameplay/pause-Options stop,
return/restart, exit/disposal, live draft/saved volume, discard/apply, focus mute,
loop envelope endpoints/midpoints, repeated cursor wraps, no screen-transition
fades, volume changes during fades and expected playback failure cleanup.
Content checks verify the original source hash, Ogg header and compiled runtime
files without creating a graphics/audio device. Local publish packaging passed;
the published Ogg and XNB match the build output hashes and the source WAV is
excluded. These checks do not prove audible playback.

## Manual listening acceptance

Pending with the user:

1. Launch and hear Forgotten Treasures during the intro/main menu at the reduced level.
2. Travel to Options and back; confirm playback does not restart or fade for travel.
3. Wait through 2:20; confirm the end fades down and the repeat fades back up at a
   comfortable rate, with no unwanted click, gap or abrupt musical transition.
4. Preview Master/Music, then discard/apply; test focus mute both enabled and disabled.
5. Enter character selection and return; confirm uninterrupted music. Confirm a
   model, open pause-side Options and return to title; check gameplay stop/title
   restart behavior. Exit and confirm audio stops.

No automated blocker remains. Audible loop quality is unverified.
