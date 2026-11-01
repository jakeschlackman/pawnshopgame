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
