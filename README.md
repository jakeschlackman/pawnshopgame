# Pawn Shop Game

C#/MonoGame prototype of a cozy salvage and pawn shop game. M01 implements
title, New Game, a placeholder room, pause/resume, return, and exit. Inventory,
movement, economics, saving, narrative, and robots are not implemented.

## Build and run

Requires .NET SDK 10.0.401 (or a compatible later 10.0.4xx patch) and desktop
OpenGL support. The initial target is Windows x64 using MonoGame DesktopGL
3.8.4.1. Other platforms have not been verified. No additional SDK/workload or
template installation is needed.

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

Continue and Settings are visibly unavailable. Focus them to read the reason.
No save files are read or written. Returning to title or exiting during a session
requires confirmation that unsaved progress will be discarded; the safe default
keeps the session. Window close requests use the same exit confirmation.
Losing application focus pauses the room; focus return requires explicit resume.

## Implementation boundaries

`PawnShopSimulator.sln` and all eight project directories sit at the repository
root. Six class libraries support the MonoGame executable and existing console
regression project. The approved dependency graph and full folder list are in
[ARCHITECTURE.md](ARCHITECTURE.md#22-approved-solution-layout).

| Project | Current responsibility |
| --- | --- |
| PawnShop.Framework | Reusable input/edges, generic menu focus, bitmap font and renderer. No PawnShop project references or game-specific commands. |
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

## Placeholder content workflow

M01 uses original code-authored shapes, a 5x7 bitmap alphabet, and one generated
floor texture. Game/Content is reserved with `.gitkeep`. There are no external assets, `.mgcb` files, XNB outputs, downloaded
fonts, or third-party artwork. Content is compiled as C# with the normal solution
build; no separate MGCB check applies yet. The MonoGame package supplies its
runtime dependencies through NuGet.

Rendering uses a provisional 640x360 logical render target, a fixed 1280x720
window, point sampling, and integer output scaling/letterboxing. These are M01
UI choices, not approved actor/tile/art dimensions. M02 must validate world scale.

## Guidance and evidence

The existing design documents are at the root, not under `docs/`:
[agents.md](agents.md), [ARCHITECTURE.md](ARCHITECTURE.md),
[GAME_DESIGN.md](GAME_DESIGN.md), and [ROADMAP.md](ROADMAP.md).
Their proposals remain proposals beyond the implemented slice.

See [M00/M01 verification](docs/VERIFICATION_M01.md) for historical baseline
results, structural-refactor verification, and the remaining desktop acceptance
checklist. M00 remains complete; M01's full desktop lifecycle/resource acceptance
remains pending. No new gameplay milestone was started.
