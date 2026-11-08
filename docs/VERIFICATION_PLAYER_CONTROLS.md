# Player selection, movement and nearby Talk

Date: 2026-11-05. Scope: the requested New Game gender selection, WASD room
movement, one NPC interaction, Z confirm/interact and X back/pause.

## Implemented behavior

- New Game opens male/female previews. WASD/arrows navigate, Z confirms and X
  returns to title. Mouse cards also select. No session exists until confirmation;
  confirmation consumes its entire input frame before room simulation begins.
- The selected bald underwear base uses the eight 48x64 standing facings, with
  ground pivot (24,58). Source artwork and approved shirt layers are preserved.
  Walking cycles and runtime clothing/equipment layers are not implemented.
- Held WASD moves at 90 logical pixels/second. Diagonals are normalized. A 12x6
  foot rectangle collides with the floor boundary, counter and stationary NPC.
  Axis sweeps prevent tunneling and allow sliding along obstacles.
- One placed visitor uses the existing authored Ink conversation. Within 48
  logical pixels, Z or a click starts Talk. Both paths revalidate range. The HUD
  indicates when Talk is available. Dialogue pauses movement, room time and NPC
  animation. W/S select responses, Z advances/confirms, and X closes dialogue.
- X pauses room play and backs out of menus. Opening/closing menus and dialogue
  captures the frame; it cannot also move or interact. Focus transitions suppress
  stale key edges; an unfocused application never receives held movement.
- Fresh defaults are Z/X. The old Space/Escape default pair adopts Z/X in memory
  without rewriting preferences on load. The subsequent
  [dual-binding slice](VERIFICATION_DUAL_BINDINGS.md) adds rebinding for movement
  and primary/alternate slots for all six actions, preserving custom action keys.
  Enter/Space confirm, Escape back, and nearby Tab Talk remain aliases.
- Session state owns position/facing; renderers consume immutable snapshots.
  The two player textures are application-owned and shared by previews and room
  rendering. NPC/floor resources retain location ownership.

## Files and automated checks

`Characters/Player/PlayerState` and `Movement/MovementBounds` implement headless
movement. `Game/Bootstrap` composes the room, selection lifecycle, range checks
and player content catalog. `Game/Screens` draws the selection and chosen player.
Framework input separates held movement from action edges. The Game project
copies only the two base PNGs and shared `player-sprite.json` for player runtime
content. No packages or SDK targets changed.

Baseline: solution build passed and the existing runner passed 36/36 checks.
Final verification commands:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

Final result: build passed with zero warnings/errors; all 43 runner checks
passed; whitespace checks passed. The build runs the existing MGCB/Ink content
workflow. PNG-header/metadata validation checks both body atlases and all eight
poses without a graphics device. Runtime copies were compared with their source
files by SHA-256.

New coverage exercises sessionless selection/cancellation, both genders and fresh
session IDs, input capture, actual keyboard edge/held mappings, all eight facings,
cardinal/diagonal distance, tick partitioning (0.02 logical-pixel tolerance over
one second split into 120 ticks), swept collision in both axes, room boundaries,
NPC collision/range, out-of-range clicks, dialogue/pause movement capture, invalid
player metadata/PNG diagnostics and isolated settings-default migration.

## Manual acceptance pending

No game window was launched, no desktop was controlled and no gameplay screenshots
were inspected. Compilation/headless checks do not establish visual acceptance.
The user should check:

1. New Game previews both genders; X returns to title; Z starts the selected body.
2. WASD turns/moves in all eight directions, stops on release and feels consistent
   diagonally. The player stays inside the floor and cannot pass through the
   counter or visitor. Check feet, depth ordering and visible scale.
3. Approach the visitor, press Z, advance/select responses, then X to leave.
   Movement stays paused in dialogue; closing does not also pause/interact.
4. X pauses/resumes; focus loss behaves correctly; returning to title and starting
   the other gender uses a fresh spawn without retaining the prior session.
5. Check mouse selection, distant NPC clicks, character previews and UI text at
   supported display sizes and 100/125/150 percent UI scale.

No automated blocker remains. The broader M02 calendar and actor/mech/companion
scale lab remain outside this slice.
