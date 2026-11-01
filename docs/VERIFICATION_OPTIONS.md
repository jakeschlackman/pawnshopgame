# Options menu verification

Date: 2026-10-31.

Implemented application preferences, a four-category options menu on title/pause,
display modes and resolution presets through 3840x2160, display confirmation and
rollback, UI/text scaling, audio preview sliders, focus pause/mute, dialogue speed
preview, and rebinding for the existing Interact/Pause actions. Manual resizing is
disabled. Settings are independent of gameplay saves; no gameplay save format or
dependencies changed. Existing uncommitted agents.md work was retained.

## Automated evidence

- Baseline `dotnet build PawnShopSimulator.sln --no-restore`: passed, zero warnings/errors.
- Baseline `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: 10/10 passed.
- Implementation build: passed, zero warnings/errors.
- Implementation regression runner: 16/16 passed. Added checks cover title/pause
  input capture and session preservation; display confirmation, cancellation,
  timeout and focus rollback; isolated temporary settings round trips, backups,
  malformed/unsupported data preservation; key release and duplicate bindings;
  category audio gain and focus mute; dialogue reveal rates; configurable focus
  pause; and failed-write rollback.
- Final build after retaining the original desktop dimensions across fullscreen
  changes: `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/OptionsVerification/`
  passed with zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/OptionsVerification/`
  passed 16/16. Separate ignored output avoided disturbing the launched executable.
- `git diff --check` passed; Git reported only LF/CRLF conversion notices.
- Normal C# compilation builds the preview tone generator and UI; existing PNG
  content is copied by the existing workflow. No new asset compiler is required.

## Desktop evidence and limits

Launched the rebuilt executable with the computer-use skill. Screenshot inspection
confirmed the title's enabled Options entry. Native accessibility inspection
confirmed the Maximize button is disabled. Automated confirm-key attempts did not
produce an observable transition. The user then stopped Computer Use with physical
Escape, and all desktop automation stopped.

The options screen layout, 100/125/150 percent readability, resolution/mode switching,
physical edge-drag behavior, audible previews, runtime rebinding, and settings reload
remain manual acceptance checks. Actual 4K output requires a suitable display and
has not been verified. Headless preview tests do not establish graphics-driver
rollback behavior. No complete M01 desktop/resource acceptance is claimed.
