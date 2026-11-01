# Pawn Shop Game

C#/MonoGame prototype of a cozy salvage and pawn shop game. M01 implements
title, New Game, a placeholder room, pause/resume, return, and exit. Inventory,
movement, economics, saving, narrative, and robots are not implemented.

## Build and run

Requires .NET SDK 10.0.401 (or a compatible later 10.0.4xx patch) and desktop
OpenGL support. The initial target is Windows x64 using MonoGame DesktopGL
3.8.4.1. Other platforms have not been verified. No additional SDK/workload or
template installation is needed. Font builds use MonoGame's content builder,
pinned to the same 3.8.4.1 version in `.config/dotnet-tools.json` and the Game
project. The standard build task restores that local tool when needed; first
restore requires NuGet access. No system font installation is required.

From the repository root:

```powershell
dotnet restore PawnShopSimulator.sln
dotnet build PawnShopSimulator.sln --no-restore
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
dotnet run --project PawnShop.Game/PawnShop.Game.csproj --no-build
git diff --check
```

The regression project is a dependency-free console test runner, not a VSTest
project. Run it with the command above; `dotnet test` does not execute these
checks. Failed assertions exit nonzero. It references Framework, Core, and Game
and exercises graphics-free input/menu/state/Bootstrap code without constructing
the MonoGame host or initializing graphics/audio. The test framework is unchanged.

## Controls and current behavior

- Up/Down or W/S: move menu focus, wrapping at either end.
- Enter or Space: activate the focused enabled item.
- Escape: pause/resume; cancel a discard prompt back to pause. Escape on title
  does not exit.
- Space in the room: ring a silent test bell. Its count demonstrates session
  reset and that a menu-closing confirm cannot also activate the room.

Continue is visibly unavailable; focus it to read the reason.
Gameplay saves are not implemented. Returning to title or exiting during a session
requires confirmation that unsaved progress will be discarded; the safe default
keeps the session. Window close requests use the same exit confirmation.
Losing application focus pauses the room by default; focus return requires explicit resume.

## Options

Options is available from the title and pause menus. Tab cycles Display, Audio,
Comfort, and Controls; Up/Down selects a row; Left/Right adjusts it; Enter activates
buttons or begins key capture. Escape cancels capture or leaves Options, discarding
unapplied edits. The active session stays paused while Options is open.

- Display: Windowed, Borderless, or Fullscreen; presets from 960x540 through
  3840x2160 (4K), including 16:10 and ultrawide choices; UI/text size at 100, 125,
  or 150 percent. Manual window resizing and maximizing are disabled. Borderless
  uses the desktop size; the chosen resolution applies to windowed/fullscreen.
  Unsupported fullscreen modes and windows larger than the desktop are rejected.
- Audio: Master, Music, Ambient, and Sound Effects in steps of five, with preview
  tones and a Mute When Unfocused toggle. There are no authored gameplay audio
  tracks yet. Audio categories multiply their volume by Master.
- Comfort: Pause When Unfocused and Slow/Normal/Fast/Instant dialogue text speed,
  with a live text preview. Conversations are not implemented yet.
- Controls: rebind the existing Interact and Pause actions. Release the activation
  key before pressing a new key. Duplicate action keys are rejected; Escape cancels.
  Menu navigation stays fixed so it remains accessible after rebinding. Movement
  bindings will arrive with movement; Restore Defaults restores Escape for Pause.

Apply saves preferences. Display changes first require confirmation within 15
seconds; Escape, timeout, or losing focus reverts them. Restore Defaults edits all
categories and still requires Apply. UI size, audio, and text speed preview while
editing; leaving without Apply restores the saved values.

Application preferences live in `%LOCALAPPDATA%/RummageHollow/settings.json`,
independently of game sessions. Writes use a temporary file and replacement with
a `.bak` backup. Invalid/unsupported files load defaults and show a warning in
Options; the original remains untouched until Apply. A failed write retains the
current preferences and reverts any display preview. An unavailable saved display
falls back to a 1280x720 window without overwriting the file.

## Implementation boundaries

`PawnShopSimulator.sln` and all eight project directories sit at the repository
root. Six class libraries support the MonoGame executable and existing console
regression project. The approved dependency graph and full folder list are in
[ARCHITECTURE.md](ARCHITECTURE.md#22-approved-solution-layout).

| Project | Current responsibility |
| --- | --- |
| PawnShop.Framework | Reusable input/edges, generic menus, renderer, font catalog and SpriteFont adapter. No PawnShop project references or game-specific commands. |
| PawnShop.Core | Authoritative room clock/bell fixture and immutable view under World. No graphics/project/package dependencies. |
| PawnShop.Narrative | Requested class library and empty future folders; no narrative implementation. |
| PawnShop.Characters | Requested class library and empty future folders; no actor implementation. |
| PawnShop.Shop | Requested class library and empty future folders; no shop implementation. |
| PawnShop.Exploration | Requested class library and empty future folders; no exploration implementation. |
| PawnShop.Game | Executable; Bootstrap owns ApplicationFlow/GameSession; Screens owns title, menus and the room texture scope. |
| PawnShop.Tests | Existing lifecycle checks plus input/menu checks under Framework, using the same console runner. |

One room texture follows the active session ID and is disposed on return.
Framework's PixelRenderer owns the application-wide font pixel, render target,
and SpriteBatch, disposed by the host on unload/application disposal. Drawing
never changes session state. There are no session event subscriptions in M01.
An application-owned FontLibrary shares a single MonoGame ContentManager/font
texture across the title and Options renderers. Screens do not dispose shared fonts.

| Previous location | Current location |
| --- | --- |
| src/PawnShop.Application input and menu | PawnShop.Framework/Input and UI; game menu commands remain in PawnShop.Game/Screens |
| src/PawnShop.Application GameSession/ApplicationFlow | PawnShop.Game/Bootstrap; room state extracted into PawnShop.Core/World |
| src/PawnShop.Game | Root PawnShop.Game; reusable drawing moved to PawnShop.Framework/Rendering and concrete drawing to Game/Screens |
| tests/PawnShop.Tests | Root PawnShop.Tests |
| PawnShop.slnx | PawnShopSimulator.sln (sole solution) |

The former Application project has been retired. Empty requested folders use
`.gitkeep` and do not imply feature completion. Core/Transactions will own shared
atomic operations; Shop/Transactions must use them. Feature save data stays with
its owner; Core/Persistence provides neutral contracts/storage, and Game/Bootstrap
coordinates complete snapshots. No saving was implemented by this refactor.

MonoGame's fixed 60 Hz host is the single tick owner. Menus capture the entire
transition frame. Simulation time stops in overlays; presentation time continues.
The room is intentionally static until M02.

## Content and title screen

The Picker's Hollow title uses `Content/TitleScreen/title-screen-panorama.png`,
an unmodified 2106x747 copy of the supplied single-pass illustration. The older
Rummage Hollow artwork remains preserved but is no longer loaded. The supplied
`logo-pickers-hollow.png` replaces the title text as an independent transparent
overlay; the main menu uses Silkscreen and other title/Options text uses Pixelify
Sans Bold through the font catalog. See the
[asset record](PawnShop.Game/Content/TitleScreen/README.md) for provenance and hash.
Normal build/publish copies PNGs beside the executable; runtime resolves them
relative to the executable. PNG loading is unchanged; fonts compile to XNB using
the standard MonoGame content pipeline in `Content/Content.mgcb`.

The title starts at the panorama's left edge. Click Options (or use keyboard
confirmation) to pan to the right edge over 0.8 seconds with SmoothStep easing.
Options appears after arrival. Back/Escape hides Options and pans left; Escape
can also reverse an outgoing pan. Repeated navigation does not restart the camera.
Hidden controls cannot activate during travel. Options opened from the pause
menu does not use this title animation and keeps the session paused.

The background fills the actual viewport using a uniform cover scale, without
stretching or revealing outside pixels. Its visible source area and right limit
are recomputed from the image and viewport dimensions, including during resizing.
Very wide viewports that already show the entire image width have no horizontal
travel and crop vertically instead. Point sampling preserves sharp pixel edges.
The title and Options UI retain separate, contained 1920x1080 and 1280x720 canvases;
their layout and mouse coordinates do not pan with the background. Transparent
UI targets composite over the panorama after switching back to the backbuffer.

The default window is 1280x720. Resolution presets reach 3840x2160; users change
window size only through Options. Manual resizing and maximizing are disabled,
including after display-mode changes. Existing preferences stay in the original
`RummageHollow` settings directory so renaming the visible title loses no settings.
TitleScreen owns its panorama texture, batches and UI target for the application
lifetime and releases them on disposal. No gameplay camera behavior changed.

The placeholder room and pause overlays retain the
640x360 logical target, integer output scaling, original code-authored shapes,
bitmap font, and generated floor texture. These UI choices do not establish
actor/tile dimensions; M02 must validate world scale.

Menu rows and Options tabs also accept mouse clicks. Clicking a settings row
performs its existing confirm action; Left/Right still provides fine keyboard
adjustments. Display confirmation has clickable Keep/Revert buttons.

Sprout replaces the title menu's selection diamond. `sprout-title.png` contains
32 row-major 48x48 frames (4 columns, 8 rows). Frames 0–15 loop as Idle at 90 ms
per frame; frames 16–31 play once as Interact at 50 ms per frame on confirmation by
Enter, Space, or mouse. The selected action runs after the 0.8-second Interact
clip finishes, so the reaction is visible before starting a game, panning to
Options, or exiting. Repeated input is captured during the clip; Escape cancels
it. Focus loss holds pending activation until focus returns. Disabled Continue
reacts but cannot start a session. Pause/Options controls keep their normal timing.

`Content/TitleScreen/sprout-title.json` defines the grid, named clips, timing,
stable pivot and presentation scale. Sprout stays vertically anchored to the
selected label's glyph center; the fixed sheet pivot preserves authored bobbing.
Framework's SpriteAnimator advances once in Update, with Draw only reading frames.
Sprout renders directly to the final viewport with point sampling and a snapped
pixel position, avoiding a second scaling pass through the title render target.
Clip timings are independently editable in the metadata.

## Central font catalog

`PawnShop.Game/Content/Fonts/fonts.json` maps names to compiled font assets and
logical pixel line heights. It loads once at application startup. Current roles
are `TitleMenu`, `TitleBadge`, `TitleHelp`, `OptionsHeading`, `OptionsBody`, and
`OptionsHelp`. `TitleMenu` uses bundled Silkscreen Regular; the remaining roles
use Pixelify Sans Bold. `Silkscreen` and `PixelifySansBold` are also available as
general named entries. The title logo remains an image.

```csharp
using PawnShop.Framework.Rendering;

renderer.Text("New Game".Font("TitleMenu"), x, y, color);
renderer.Text("Example".Font("PixelifySansBold"), x, y, color);
```

The helper selects a catalog entry; MonoGame's SpriteFont/DrawString handles
glyphs, kerning, measurement and drawing. FontLibrary loads each unique asset once.
JSON `size` is the line height in logical canvas pixels, multiplied by the existing
UI scale setting. Edit the catalog, build and restart to apply changes. Unknown
names, invalid sizes, unsupported schemas and missing assets produce diagnostics
instead of silently choosing a different font. No runtime download or JSON parsing
occurs during drawing. Plain `renderer.Text(string, ...)` retains the existing
placeholder font for room/pause content outside this change.

To add another font, bundle its licensed TTF next to a `.spritefont`, add the
spritefont to `Content/Content.mgcb`, and reference its content ID (without `.xnb`)
from the catalog. The existing MonoGame content build compiles it automatically.
See [MonoGame's font guide](https://docs.monogame.net/articles/getting_started/content_pipeline/adding_ttf_fonts.html)
and [font provenance/license](PawnShop.Game/Content/Fonts/README.md).

## Guidance and evidence

The existing design documents are at the root, not under `docs/`:
[agents.md](agents.md), [ARCHITECTURE.md](ARCHITECTURE.md),
[GAME_DESIGN.md](GAME_DESIGN.md), and [ROADMAP.md](ROADMAP.md).
Their proposals remain proposals beyond the implemented slice.

See [M00/M01 verification](docs/VERIFICATION_M01.md) for historical baseline
results, structural-refactor verification, and the remaining desktop acceptance
checklist. M00 remains complete; M01's full desktop lifecycle/resource acceptance
remains pending. No new gameplay milestone was started.
