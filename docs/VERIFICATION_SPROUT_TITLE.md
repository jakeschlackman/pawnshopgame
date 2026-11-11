# Sprout title selector verification

## New Game float and slower menu pans — 2026-11-11

New Game now starts its camera pan immediately and keeps the current pointing
loop, bypassing Interact. Sprout floats with the camera's eased progress to Male,
then follows the active Male/Female/Back label. Creation anchors are owned by
`Characters/Player/Create`. The overlay maps between the title and integer-scaled
selection canvases while preserving its screen position and pixel scale. Travel
and animation freeze while unfocused; arrival consumes input. Existing sprites,
metadata, frame durations and artwork remain unchanged.

Horizontal menu pans now take 1.1 seconds rather than 0.8 seconds, including Options
entry/return. Options' existing pointing-cycle departure and loop continuity remain.
Music continues through New Game travel, selection and Back without restarting,
and stops on entry to gameplay. The intro descent and title fade are unchanged.

The solution/content build passed with zero warnings/errors, 95/95 headless
regressions passed, and `git diff --check` passed. Exact commands are in
[panorama verification](VERIFICATION_PANORAMA.md). Tests cover keyboard/pointer
entry, existing animation phase preservation, shared camera progress, landing on
Male, all three selector anchors, cancellation, focus freeze, input capture, reentry
and music continuity. No game launch, screenshots or listening test was performed;
flight appearance, landing alignment, pan feel and audible continuity remain user
acceptance. Historical timing/behavior below is retained as earlier evidence.

## Main-menu return flight - 2026-11-08

Leaving Options opened from the main menu now retains Sprout and floats him from
the current highlighted option back to the main menu's Options entry. Both the
footer Back and cancel binding use the same path. The visible anchor is captured
before draft cancellation restores settings/scale, so scrolling, the current tab
and uncommitted defaults do not move his departure point. Main-menu Options is
selected for the destination; the title menu draws during return without a second
selector. The existing 800ms SmoothStep travel/pan and point-sampled overlay are
reused, keeping Idle and its clock continuous without new clips, rotation or art.

Return input is captured through the arrival frame; repeated Back cannot skip
the flight. Focus loss freezes camera, progress and animation together. Exit and
disposal reset presentation state. Existing entry cancellation and pause-side
Options behavior are preserved. The requested change concerns the main menu;
no future pause menu was designed or implemented.

Added headless return checks for footer/cancel, scrolled-row departure, three
saved UI scales, discarded defaults, exact endpoints/midpoint, focus freeze,
input capture, main-menu destination selection, animation continuity, repeat
entry and exit cleanup. Test setup now selects intended title entries explicitly
because returning keeps Options selected. In-game travel/rendering acceptance
remains with the user; no game was launched or controlled.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 65/65.
`git diff --check` passed.

Date: 2026-10-31. Existing uncommitted work preserved. No GUI launch, screenshots,
Computer Use or computer-vision testing; visual testing remains with the user.

## Content and behavior

- Copied the user's `sprouttitlepage.png` into title content as `sprout-title.png`,
  preserving the original. Source/imported SHA-256 both
  `6D79201AA34A122FD279477EA6C44EE3F10EBFD6AA909AFEC5289CEC0115F72B`.
- Read PNG dimensions/alpha metadata numerically: 192x384 RGBA, 32 nonempty 48x48
  cells in a 4x8 row-major grid. Alpha bounds and the fixed pivot are recorded in
  the title asset README. No image modification was performed.
- Added data-driven sheet/clip metadata and a small reusable Framework animator.
  Idle frames 0–15 loop at 80ms/frame; Interact frames 16–31 run once at the same
  rate. Renderer draws source rectangles without changing animation state.
- Replaced the diamond with Sprout, anchored to the selected text's glyph center.
  A stable pivot preserves animation motion; the full frame renders at 72x72
  logical title pixels. Original PNGs/fonts and settings are otherwise unchanged.
- Title confirmation plays Interact before the enabled action executes. Enter,
  Space and mouse share the path; disabled Continue reacts but cannot execute.
  During feedback, selection/activation is captured, Escape cancels, focus loss
  holds the animation/command, and completing feedback consumes the whole frame.
  Ordinary window-close requests still cancel pending commands and exit immediately
  on title. Pause-side controls are not delayed.

## Verification

- Baseline `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/SproutVerification/`:
  passed, zero warnings/errors. Baseline regression runner passed 22/22.
- Implementation build with the same command: passed, zero warnings/errors.
- `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/SproutVerification/`:
  passed **25/25**. New cases cover metadata/schema/range errors, source rectangles,
  copied PNG dimensions, exact 80ms boundaries, Idle wrap/large deltas, one-shot
  completion, delayed action execution, input capture, disabled activation,
  cancellation, focus loss and close requests. Existing lifecycle and panorama
  cases were adapted to the deliberate title-feedback delay and still pass.
- Actual appearance, frame ordering, alignment and animation feel remain for the
  user to inspect. No visual verification is inferred from these automated checks.

## Smoothness follow-up

Changed both clips from 80 to 50 ms/frame (20 fps, 0.8 seconds per clip), reducing
frame holds and fitting approximately three updates per sprite frame at 60 Hz.
Sprout now draws directly from the sheet to the backbuffer after the UI canvas
is presented. The shared contained viewport transform preserves alignment; point
sampling and output-position snapping retain crisp edges. This removes the prior
fractional scaling into the title target followed by another scaling pass.
Source artwork, frame sequence and stable pivot remain unchanged. Updated timing
boundary/activation tests cover the shorter interaction. Actual perceived
smoothness remains for the user to evaluate.

## Slower playback correction

The user requested a slowdown after the 50 ms change. Both clips now use 120 ms
per frame (about 8.3 fps), with a 1.92-second interaction. The direct-to-backbuffer
rendering improvement is retained. Timing and delayed-activation checks were
updated to the slower duration; manual appearance testing remains with the user.

## Independent clip timing

Per the latest request, Idle now uses 90 ms/frame (1.44-second loop) and Interact
uses 50 ms/frame (0.8-second one-shot). Updated timing boundaries, metadata
validation and delayed-activation checks to cover the two distinct durations.

## PixelLab floating-point loop — 2026-11-05

Downloaded the existing Floating Point Right Loop, east direction, from Sprout's
PixelLab character via MCP-provided URLs. Preserved all 13 original 68x76 PNGs
under TitleScreen/Sources. The base character is 48x48, but these animation exports
use a larger canvas. No resizing or per-frame recentering was applied.

The new local sprout-title-menu.png uses 68x76 cells. Idle is frames 0-12 at
90 ms/frame (1.17 seconds); Interact remains frames 16-31 at 50 ms/frame. Original
interaction pixels are padded by (10,14), with the shared pivot shifted by the
same amount to (36,42), preserving their display size and position. The original
sprout-title.png remains unchanged. No runtime PixelLab/network dependency exists.
Asset IDs and assembly layout are recorded in the title artwork README.

Verification:

- Baseline and final `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false`: passed, zero warnings/errors, including normal content workflow.
- Baseline and final `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: 34/34 passed. Updated sheet bounds and 13-frame loop timing checks; existing selection/cancellation tests still pass.
- Numerical pixel comparison: all 13 imported frames, 16 retained interaction frames and transparent padding match exactly. Original sheet SHA-256 remains the value recorded above.
- Runtime sheet SHA-256: `2C5B6CBEF41C22CF324B3C59499347B357985E11CF6F67FF9D527C7B13D2A6C1`.
- `git diff --check`: passed with line-ending notices only.

No game launch or visual inspection performed. Check the idle loop's appearance,
alignment and transition to the existing selection reaction manually. Existing
uncommitted work was preserved; no dependency or session behavior changed.

## Options flight sequence — 2026-11-05

Main-menu Options now finishes the active pointing cycle (retaining its final
frame), plays PointToFastFlight once, runs MenuFastJetFlight while moving to the
first Display field, plays FastFlightToPoint once, then resumes Idle. Takeoff and
landing each use 13 frames at 50 ms. Flight uses the existing 0.8-second SmoothStep
pan duration. Rotation eases toward the path during takeoff and back to upright
on landing. Phase boundaries consume excess time and retain endpoint poses until
the next update. Other title commands still use the original Interact clip.

OptionsSproutFlight is graphics-free presentation state driven once by
ApplicationFlow. All input is captured through landing, including the completion
frame; focus loss freezes both flight and camera. Escape cancels from every phase
and reverses the panorama if necessary. Window close clears pending presentation.
The Options panel and sprite are visible during outgoing travel. The idle marker
then follows the selected row; pause-side Options retains its existing behavior.

Downloaded 43 additional original east-facing frames, preserved under Sources.
The 68x80 atlas cells accommodate the largest export without resampling. The
existing pointing/selection position, scale, and requested downward adjustment
are preserved by matching padding and pivot changes. The local assembly script
is `tools/Build-SproutTitleSheet.ps1`; the game never invokes it or contacts MCP.
Source IDs, export dimensions and frame ranges are in the title asset README.

Automated verification:

- Baseline build: passed, zero warnings/errors; baseline runner: 34/34 passed.
- `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false`: passed, zero warnings/errors, including normal content compilation.
- `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: **36/36 passed**. New groups cover every phase/frame boundary, one-shot completion, synchronized travel, input capture, focus loss, cancellation in every phase, repeat entry, exit cleanup and missing clip validation. Existing pointer, pause-side Options, title commands and session checks pass.
- Numerical pixel checks: all 56 imported frames and padding match originals, all 16 original selection frames match, and unused cells are transparent. Original selection-sheet SHA-256 remains unchanged.
- Runtime sheet and built output hashes match: `511E6E74D9DBB4DA230C345D9C32EFA94B43CD8314609E3E77E2040FFA9FBC75`.
- `git diff --check`: passed, line-ending notices only.
- Script execution was initially denied in the sandbox; the approved elevated retry assembled the sheet successfully without changing execution policy or installing dependencies.

No game launch, screenshot inspection or desktop automation was performed.
Manual checks: Options flight direction/tilt, perceived timing, sprite continuity
between exports, landing alignment at supported UI scales, and keyboard/mouse
feel. Automated results do not establish visual acceptance. No build/test blocker
remains, and existing unrelated uncommitted work is preserved.

## Pointing-only float correction — 2026-11-05

At the user's request, Options now uses only Floating Point Right Loop while
floating upright to the first setting. Removed takeoff/landing phases and rotation
from this transition; the downloaded flight artwork remains preserved. The current
pointing cycle still finishes before travel, then Idle continues without restarting
on arrival. The 0.8-second pan, input capture, cancellation and focus pause remain.

`dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false`
passed with zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`
passed 36/36 with updated sequence/continuity checks. `git diff --check` passed.
No game launch or visual check was performed; float appearance remains for the user.
