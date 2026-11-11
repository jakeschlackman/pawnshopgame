# Panoramic title screen verification

Date: 2026-10-31. No branch, commit, push, framework or package changes.
Existing uncommitted options-menu and agents.md edits were preserved.

## Implementation

- Imported the supplied PNG as `PawnShop.Game/Content/TitleScreen/title-screen-panorama.png`.
  Read its PNG dimensions numerically: 2106x747. Original/imported SHA-256 both
  `55D058617655602B5545B581ECB22949B527F55EEC3559113E04233D29C76DDE`.
  The original was preserved, and no image editing or generation was performed.
- Framework `PanoramaCamera` provides headless bounds/easing calculations;
  `PanoramicBackground` owns the image and batch. ApplicationFlow advances the
  presentation-only camera once per update. No gameplay camera was changed.
- Main starts at normalized position zero. Title-side Options targets one;
  Back/Escape targets zero, including reversing an outgoing transition.
  SmoothStep duration is 0.8 seconds. Retargeting preserves current position;
  repeated same-target requests do not reset elapsed time. Hidden UI cannot activate.
- Direct viewport cover rendering preserves aspect ratio and fills the screen.
  Existing contained UI targets render separately; panorama composition happens
  after returning from the UI target so it does not rely on backbuffer preservation.
- Reused Options controls, persistence and keyboard command paths. Mouse clicks
  activate menu rows, tabs and confirmation buttons. Pause-side Options stays
  separate. Visible branding is Picker's Hollow; existing preference storage is
  retained to preserve settings. Original artwork remains preserved but unloaded.
- Follow-up correction: manual window resizing and maximizing are disabled at
  startup and after every display-mode change. Users change resolution only through
  Options. Panorama geometry still adapts to the resulting viewport dimensions.

## Automated verification

Baseline build: `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/PanoramaVerification/`
passed with zero warnings/errors. Baseline regression runner: 16/16 passed.

Implementation build with the same command passed with zero warnings/errors.
`dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/PanoramaVerification/`
passed 20/20. The four new cases cover:

- 20/30/60/144 Hz time partitions, SmoothStep intermediate values, exact endpoints,
  oversized deltas, repeated target requests and interruption without position jumps.
- Cover/crop bounds, uniform scale and aspect ratio for all resolution presets
  through 4K, 4:3, portrait, ultrawide and minimal viewports; resizing mid-pan and
  no horizontal travel when the entire source width is already visible.
- Main/Options navigation, gating hidden controls, Escape reversal, return arrival,
  no accidental session creation and unchanged pause-side Options/session behavior.
- Mouse Options/Back, contained viewport mapping on ultrawide displays, ignored
  letterbox clicks and held-click suppression on focus return.

No application launch, screenshots, Computer Use or computer-vision testing was
performed, following agents.md. Visual pan smoothness, image/text composition,
readability, mouse feel and actual window/display resizing are for the user to test.
These pending manual checks do not block the implementation handoff.

## Day-to-dusk launch intro

The extended panorama starts in daylight, descends for four seconds, and then
fades in the title UI over 0.8 seconds. Menu input is captured until the fade
finishes. The original band sets the settled scale and crop for Main and Options.
Returning to title during play opens the settled menu.

- `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/TitleIntroVerification/` passed with zero warnings and errors.
- `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/TitleIntroVerification/` passed 70/70 checks. The two launch checks cover timing, input capture, focus loss, returning to title, bundled PNG dimensions, and the original crop and scale across aspect ratios.
- SHA-256 matched between the new source PNG and the built game copy. Desktop pan smoothness and the fade remain for manual review.

### Timing and cloud retouch

The descent now takes 3.2 seconds. The 0.8-second fade begins 250 ms before
arrival with an ease-out curve. The launch test checks partial opacity before
arrival and menu input capture through the end of the fade.

An ImageGen retouch reconnects the diagonal cloud banks and smooths the change
from lavender to coral. The installed 2106×1494 PNG was inspected at native size
around the join. Pixel comparison confined changes to rows 537–863 and matched
the original dusk scene from row 864 down. The same build and test commands
passed again with zero warnings or errors and 70/70 checks. In-game playback
remains for manual review.

### Torcalev Studios splash

The supplied transparent logo is bundled byte-for-byte as
`Content/TitleScreen/logo-torcalev-studios.png`. Bootstrap's TitleIntro owns the
studio fade, hold, descent and skip clock. TitleScreen owns the texture and draws
the centered splash over the stationary daytime sky. The normal sequence fades
in over one second, holds for two seconds and fades out over one second before
the existing descent. Escape fades the splash out over 0.8 seconds and reaches
the ready menu within two seconds, including when pressed during descent or the
game-menu fade. Repeated Escape preserves skip progress and focus loss freezes it.

The recorded build and regression commands passed with zero warnings or errors
and 71/71 checks. Tests cover splash timing, hold position, input capture,
completion-frame consumption, six Escape timings, repeated cancellation, focus
loss and application exit. The content check verifies the bundled 1254×1254 PNG
and its SHA-256 against the supplied image. Logo size, visual fade and playback
remain for manual review.

## Picker's Hollow logo replacement — 2026-11-10

The supplied `Pickers Hollow pixel-art title logo.png` replaces the active logo
byte-for-byte under the existing name `Content/TitleScreen/logo-pickers-hollow.png`.
Its 1774×887 RGBA dimensions match the existing loader and 920×460 placement;
the transparent corner is retained. No code, scaling or layout changed. The
previous logo is preserved under `Content/TitleScreen/Sources/Logos` outside
runtime output. Provenance and the current hash are in the title asset README.

`dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0`
passed with zero warnings/errors; `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`
passed 92/92 checks, and `git diff --check` passed. Source/imported/build-output
SHA256 hashes match; the archived logo matches its previous hash. No new tests
or game launch were needed for this asset replacement. In-game appearance remains
pending user review.
