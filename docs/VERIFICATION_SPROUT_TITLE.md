# Sprout title selector verification

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
