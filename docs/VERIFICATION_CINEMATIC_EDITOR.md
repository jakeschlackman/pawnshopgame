# Temporary cinematic editor — Phase 1

Implemented 2026-11-09 from the complete supplied cinematic path editor spec.
Pitch keyframes were subsequently added by explicit user request. Other Phase 2/3
work and PixelLab generation are deferred. No dependencies, projects,
PowerShell helpers, art files, gameplay save formats or Sprout behavior changed.

## Use

Build Debug and run the game normally. On the settled title menu, F8 opens/closes
the editor over the actual 2106x747 title panorama, hiding the ordinary menu.
Title input/animation is frozen while editing. Opening and closing frames are
captured; focus loss freezes preview/panning and cancels pointer capture. Held
inputs on focus return are suppressed. Working edits survive closing/reopening
within the application; only explicitly saved JSON survives an application exit.

| Input | Action |
| --- | --- |
| Left-click empty scene | Append an ordered waypoint |
| Drag numbered handle | Move waypoint, preserving the grab offset |
| Right-click handle / Delete | Delete targeted / selected waypoint; retain two |
| Tab | Select next waypoint, including offscreen ones |
| Panel row / Up, Down | Choose an editable value |
| Panel minus/increment buttons / Left, Right | Tap to adjust once; hold to repeat and accelerate |
| Shift while adjusting | Fine steps without acceleration (coordinates: 1 original pixel instead of 10) |
| Space / R | Play-pause / restart at zero, paused |
| Drag bottom track/playhead | Scrub, pausing playback |
| K | Insert a pose key at the playhead; select a nearby existing key |
| `[` / `]` | Select previous/next pose key and seek to it |
| Shift+Delete | Delete selected interior key; endpoint keys are required |
| C | Pan to the other side using the existing panorama camera/easing |
| V | Toggle the expanded authoring canvas / actual player view |
| Ctrl+Z | Undo up to 64 document edits; a drag is one edit |
| Ctrl+N | New onscreen draft, undoable; does not overwrite the saved export |
| Ctrl+S / Ctrl+L | Save with round-trip verification / load exported JSON |

The panel edits selected waypoint X/Y, total duration, speed multiplier, selected
key time/scale/yaw/pitch/visibility, and the car's wheel-ground anchor X/Y. Coordinates
are original background pixels and may lie outside the image. Use Tab and X/Y
to select/move points beyond the viewport. The magenta cross is the path's
ground position. Scale/yaw/pitch interpolate; visibility holds until the next key.
Yaw 0 means rear, 45 rear three-quarter, 90 side, 180 front. It changes the
placeholder's projected view independently of the spline tangent. No pixel sprite
is rotated to guess a vehicle viewing angle. The rear edge is red, front yellow.

KEY PITCH tilts the placeholder uphill/downhill: positive values raise the nose,
negative lower it, and 0 is level. Range is -90 to +90 degrees. Select a key with
brackets, or scrub to the desired moment and press K, then adjust KEY PITCH with
the panel buttons or Left/Right. Each normal increment is 1 degree; Shift gives
.1-degree steps. Held adjustment accelerates and supports one undo per hold.
Pitch blends linearly between keys, independently of yaw and the ground path.
The diagnostic model rotates about its registered ground origin, preserving the
contact placement; it does not add vertical movement or terrain simulation.
Pitch is exported as optional `pitchDegrees` on each version-1 key. Existing
exports without it load at zero and retain their original level appearance.

Fresh drafts start with all five handles onscreen, outside the panel and footer.
If an older export has offscreen points, Ctrl+N starts this new working draft;
Ctrl+Z restores the preceding work. To showcase scaling, press R then Space:
the default keys shrink from scale 2 at the start to .25 at 85% progress. Use
brackets to select a key and adjust KEY SCALE, or scrub and press K to add one.

For an offscreen entrance, press V. The wider canvas shows the background and
empty space beyond its edges; the gold rectangle is the player's visible screen
at the current panorama pan. Select waypoint 1 (Tab cycles points) and drag it
beyond the gold frame's bottom-right, leaving enough clearance for the full car.
Press R, then Space to preview; V returns to actual player framing. This is a
path-waypoint edit; pose keys still control scale, yaw, pitch and visibility.
C retains panorama panning in either view. Expanded framing uses the same image
transform for background, car, spline, handles and pointer input, and is never
saved as part of the cinematic. The view toggle leaves authored coordinates intact.

Numeric adjustments repeat after a 350 ms hold, every 100 ms. Steps grow to 5x
after 1.5 seconds and 10x after 3 seconds; Shift retains fine steps throughout.
Release, leaving a pressed button, focus loss, closing the editor or another
command ends the repeat. One Ctrl+Z undoes the complete hold. Visibility toggles
once per press. Car scale supports .01 through 64, including JSON export/load;
the other field limits are unchanged. A stalled frame cannot cause a large jump.

## Export and runtime boundary

Save destination is `Content/TitleScreen/arrival-cinematic.json`, relative to
`AppContext.BaseDirectory` (the running executable's directory). For the separate
verification output it is
`PawnShop.Game/bin/CinematicVerification/Content/TitleScreen/arrival-cinematic.json`.
No source asset or user preferences file is overwritten. Save verifies a temporary
file by reading it back, then replaces a valid existing export with a `.bak` backup.
Corrupt/unsupported exports are preserved and must be moved aside explicitly
before saving at that name. Failed editor loads leave the current document intact.

The readable version-1 JSON contains original image ID/dimensions, path points,
duration, global speed, pose keys, placeholder asset ID and ground anchor. All
numeric ranges, required fields, sorted keys and supported assets are validated.
Normalized key progress is time through the whole shot, also the fraction of path
length at constant speed. Effective duration is `durationSeconds / speed`.
Interpolation uses component-wise monotone cubic Hermite slopes; each segment
stays within its endpoints' rectangle. A 64-subdivision-per-segment lookup maps
distance fractions to position. No second timestep accumulator is introduced.

`Bootstrap/Cinematics` contains only runtime data/player/coordinate classes;
`Screens/TitleCinematicRenderer` owns/disposes the diagnostic drawing resources.
The editor uses those same runtime classes. The loader/player and optional title
playback also compile in Release, which contains no editor classes or F8 handler.
To retain an approved export across builds, deliberately copy it to
`PawnShop.Game/Content/TitleScreen/arrival-cinematic.json`; the existing JSON copy
rule packages it without an editor. A packaged export plays once at application
startup while on title; preview freezes outside title or when unfocused. It does
not restart on returning from a gameplay session. There is no auto-promotion or
bundled draft: without an export, the normal title is unchanged. Existing optional
runtime content is validated on startup and invalid content produces a diagnostic.

## Verification

Baseline: Debug solution build passed, zero warnings/errors; 68/68 existing tests
passed. Verification uses separate ignored outputs and the existing MGCB/Ink
workflow; no game window, browser, screenshots or desktop automation is used.

Final Debug and Release builds passed with zero warnings/errors, including normal
content compilation. Debug regression runner passed 78/78; Release passed 74/74
(the four editor-only groups are excluded). Assembly inspection passed: no editor
types or `UpdateEditor` entry point in Release. `git diff --check` and individual
whitespace checks for every new file passed. No automated blocker remains.

Onscreen-start follow-up: the default handles now lie inside the title viewport
and outside editor controls at every supported resolution (plus 1280x1024).
Ctrl+N creates that working draft with an undo checkpoint; saved exports are
preserved. Added checks cover handle accessibility, reset/undo and the default
scaling preview. The same Debug build passed with zero warnings/errors, all
75/75 tests passed, and whitespace checks passed. No visual testing was performed.

Expanded-canvas follow-up: V toggles a wider, clipped authoring canvas with the
player viewport outlined in gold. Tests drag waypoint 1 beyond its bottom-right,
verify clearance for the entire car, round-trip the export, switch back to player
framing and undo the edit across every supported output plus 1280x1024. Expanding
and panning preserve image-space coordinates. Both solution builds passed with
zero warnings/errors; Debug passed 76/76 tests and Release passed 73/73. Tracked
and new-file whitespace checks passed. In-game canvas/frame appearance remains
manual acceptance; the game was not launched or controlled.

Held-adjustment/scale follow-up: keyboard and pointer holds repeat with accelerated
increments, a single undo checkpoint and unaccelerated Shift fine control. Checks
cover initial delay, repeat totals at 20/30/60/144 Hz, release, pointer departure,
focus return, visibility's single toggle, both scale bounds and scale-32/64 JSON
round trips/runtime poses. Both solution builds passed with zero warnings/errors;
Debug passed 77/77 tests and Release passed 73/73. Whitespace checks passed. Actual
hold feel and large-car appearance remain manual checks; no game was launched.

Pitch follow-up: added KEY PITCH below KEY YAW, optional pitch on exported keys,
linear runtime interpolation and nose-up/down diagnostic projection. Existing
version-1 exports missing pitch load level. Tests cover both signs, zero-pitch
geometry compatibility, ground-origin stability across yaw/pitch combinations,
unchanged path/scale/yaw/visibility, key insertion, limits, fine/held edits, undo,
legacy JSON and file round trips. Both builds passed with zero warnings/errors;
Debug passed 78/78 tests and Release passed 74/74. Whitespace checks passed.
No game was launched; the tilt's in-game appearance remains manual acceptance.

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/CinematicVerification/
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/CinematicVerification/
dotnet build PawnShopSimulator.sln -c Release --no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/CinematicReleaseVerification/
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj -c Release --no-build -p:OutputPath=bin/CinematicReleaseVerification/
git diff --check
```

Automated coverage: waypoint add/drag/delete and drag undo; coordinate edits
outside the image; key insertion/adjustment/deletion; pause/restart/scrub; focus
and opening/closing input capture; bounded interpolation and unequal waypoint
spacing; arc-length speed; supplied delta times at 20/30/60/144 Hz; independent
yaw/scale/visibility; wheel-ground placement and fully offscreen bottom-right
entry; transforms through crop/pan, letterbox offsets, client/backbuffer scaling
and 1280x720, 1920x1080, 3440x1440, 3840x2160 and 1280x1024 outputs. Storage tests
use isolated temporary directories and verify backups, corrupt-file preservation,
exact JSON/pose round trips and runtime playback without editor classes. Release
tests inspect the built assembly to check that editor types and entry point are absent.

## Limitations and Phase 2 prerequisites

Manual acceptance is pending: launch Debug, align the draft to the actual road,
check handle/arrow/HUD readability, car ground placement, panning, key previews,
scrub feel, save/reload and alignment at several resolutions. No visual result is
claimed from automated geometry tests. At the user's request the default draft
now begins onscreen so every handle is immediately accessible; its road alignment
is unapproved. Offscreen authored points remain supported for the final entry shot.
The wire car is a diagnostic shape, not final vehicle art. There is no road mask,
automatic terrain following or guarantee that arbitrary edited curves fit the road.

Phase 1 has one combined pose-key list and a global speed multiplier. Phase 2
requires the manual Phase 1 review, an accepted path/timing, and a decision on
foreground masks versus authored visibility segments. Then add separate tracks,
variable speed, tangent/curvature turn suggestions with manual overrides, discrete
directional sprite states, optional turning frames, hill occlusion, frame stepping
and missing-asset indications, tire timing and dust intensity. No asset-request
manifest, PixelLab generation, wheel animation or dust system has been started.
