# Phase 2 — Cardinal rendering foundation

Date: 2026-11-06. Implementation complete; user subsequently reported "looks good,
move onto phase 3" after reviewing the Phase 2 placeholders. This records user
visual acceptance of the Phase 2 demo, not approval of production human artwork.

## Scope and behavior

The existing player renderer and session now demonstrate four cardinal facings
with one shared idle/walk clock and synchronized equipment layers. South, North
and East are stored; West selects East with MonoGame `SpriteEffects.FlipHorizontally`.
Normalized diagonal translation, swept collision, nearby interaction, selection
and session/input lifetimes are retained. Diagonal facing uses the dominant axis;
a newly pressed/reversed axis wins ties, otherwise the previous axis persists.

Character selection still uses the original male/female PNGs. The room deliberately
uses temporary code-authored diagnostic bodies and jacket, helmet, backpack and
wrench layers, labeled “PLACEHOLDER BODY + GEAR”. Both body widths have their own
matching shapes. These are engineering fixtures, not production clothing or
approved human animation. No PixelLab jobs, production art promotion, equipment
inventory/UI, NPC renderer migration or mech work are part of this phase.

## Files and responsibilities

| Files | Change and purpose |
| --- | --- |
| `PawnShop.Characters/Player/PlayerState.cs` | Four facings, stable diagonal tie policy; existing movement/collision retained. No save format exists or changes. |
| `PawnShop.Game/Bootstrap/PlayerSpriteCatalog.cs` | Resolve West to East; validate the placeholder contract; calculate shared destination bounds, reflecting the pivot as well as the image. |
| `PawnShop.Game/Content/Player/placeholder-sprite.json` | Plain existing sprite metadata: 48x64, pivot (24,58), four columns, S/N/E rows; static Idle, four-phase Walk at 100ms. |
| `PawnShop.Game/Bootstrap/GameSession.cs`, `ApplicationFlow.cs` | One existing `SpriteAnimator` per player, advanced only by session ticks; expose its frame to drawing. Turns preserve phase, stopping/fully blocked movement selects Idle. |
| `PawnShop.Game/Screens/PlayerPlaceholderSprites.cs` | Small temporary pixel fixture for both bodies; shared full-canvas layers, frame-specific hand anchor and wrench offset, explicit direction-specific ordering. Replace with approved PNG layers in later phases. |
| `PawnShop.Game/Screens/PlayerSpriteRenderer.cs` | Own/load/dispose fixture textures; draw every layer with the exact same source rectangle, destination and flip. Keep original art for selection. |
| `PawnShop.Framework/Rendering/PixelRenderer.cs` | Add a SpriteEffects overload. Existing drawing paths and Sprout methods are unchanged. |
| `PawnShop.Game/Screens/RoomScreen.cs`, `GameScreens.cs`, `PawnShop.Game.csproj` | Pass the shared frame into the room, label fixtures, copy the new JSON into output. |
| `PawnShop.Tests/PlayerTests.cs`, `PlayerRenderingTests.cs`, `Program.cs` | Update cardinal expectations and add focused headless checks. |

Backpack draws behind South/East/West bodies and in front from North. North's
wrench is behind the body. The jacket and helmet overlay the body. Hand and item
offsets live in the same untrimmed source canvas before reflection; items are never
flipped around separate cropped origins. Opposite apparent handedness is intended.
There are no extra projects, dependencies, scripts, interfaces or services.

## Automated verification

Baseline: solution build passed without warnings/errors; 48/48 existing tests passed.
After implementation:

```text
dotnet build PawnShopSimulator.sln --no-restore
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

Build passed with zero warnings/errors; the existing build/content workflow ran.
51/51 tests passed. Whitespace checks passed (Git reports line-ending notices).
Tests exercise the same metadata, frame selection, canvas bounds, flip flags and
layer order as the renderer without creating a graphics device:

- Both genders, all four facings and all four phases; exactly three source rows.
- Dominant-axis/tie/reversal behavior and unchanged diagonal movement speed/collision.
- One clock across direction changes; loop boundaries; stop/blocked idle;
  pause, dialogue, default focus pause and fresh-session reset.
- Both fits differ; opaque/transparent pixel edges; stable bottom foot pixels;
  body and wrench both cover their shared hand anchor in every phase.
- Directional backpack/tool occlusion ordering and full composition reflection.
- West source equals East; one shared reflected destination, including an
  off-center test pivot and scaled hand-position symmetry.
- Missing direction, invalid PNG and incompatible fixture metadata diagnostics.
- Existing Sprout, dialogue, input, lifecycle and offline asset regressions.

All 246 existing Content PNGs match their pre-cleanup SHA-256 hashes. No PNG was
modified or added. Sprout assets, animation metadata, builder and implementation
remain untouched. Legacy walk candidates and approved shirt layers remain unchanged.

## Manual acceptance

The agent did not launch a game window or inspect screenshots. The user subsequently
accepted the demo as noted above; individual checklist results were not separately
reported. The retained manual checklist for regression is:

1. Start New Game with each appearance; selection should retain its original art,
   while the room explicitly shows the diagnostic body and gear.
2. Walk each cardinal direction and hold/reverse diagonal keys. Confirm no facing
   flicker; East/West should reflect the entire body and gear at the same foot point.
3. Watch the wrench follow the hand, jacket sleeves follow arms and backpack change
   front/back ordering. Check both bodies at normal gameplay size.
4. Stop, push against the room boundary, pause/resume, talk and return/restart.
   Idle/frozen/resumed frames should stay synchronized with no detached layers.
5. Confirm title Sprout, selection, NPC dialogue, input and room collision still
   look and behave as before apart from the intentional placeholder player display.

Approved human artwork is Phase 3; fitted production equipment is Phase 4. The
static diagnostic idle and four-phase gait do not establish final production
frame counts, art quality, world scale, rig compatibility or visual acceptance.
