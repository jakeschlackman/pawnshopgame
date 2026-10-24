# M00 baseline and M01 evidence

Verification date: 2026-10-24. No commit, branch, remote, or publishing operations
were performed. This record describes an uncommitted implementation.

The original M00/M01 commands below are historical evidence from before the
structural refactor. Their `src/`, `tests/`, and `PawnShop.slnx` paths have been
superseded by root-level projects and `PawnShopSimulator.sln`. Use README.md's
current commands. The refactor preserves the milestone status and outstanding
manual checklist; its own results are recorded at the end of this file.

## M00: repository baseline

- `Get-Location` and `git rev-parse --show-toplevel` both resolved to the requested
  `jakeschlackman/pawnshopgame` repository. All implementation stayed there.
- `git status --short` initially listed four untracked user files: `agents.md`,
  `ARCHITECTURE.md`, `GAME_DESIGN.md`, and `ROADMAP.md`. `git ls-files` listed only
  `.gitignore`. The existing `.vs` directory is ignored. All user files were
  retained; only the roadmap's status/evidence entries were updated.
- Read all four guidance documents. The design documents are at the root.
  No nested instruction files were found.
- No README, solution, project, SDK pin, package references, application code,
  assets, content pipeline, build scripts, CI, or tests existed. No M01 behavior
  was implemented. There were no existing project conventions to preserve.
- `dotnet --info`: SDK 10.0.401, Windows x64, .NET runtime 10.0.12, no workloads.
- Baseline `dotnet restore`, `dotnet build --no-restore`, and
  `dotnet test --no-build` initially failed because the sandbox prevented SDK
  first-use writes. An approved elevated `dotnet restore` initialized the SDK
  and then reported MSB1003: no project/solution file. There was therefore no
  existing application build/test baseline to pass or repair.
- Baseline `git diff --check` passed (only `.gitignore` was tracked).

The explicitly selected next slice was M01, excluding M02 movement/interactions
and all later gameplay systems. The implementation uses the installed SDK,
one pinned MonoGame package, and no test/UI/engine package additions.

## Implementation and automated evidence

Commands from repository root:

| Command | Result |
| --- | --- |
| `dotnet restore PawnShop.slnx` | Initial sandbox attempt failed with NU1301/socket permission denial. Approved elevated retry succeeded for all three projects. |
| `dotnet build PawnShop.slnx --no-restore` | Passed, zero warnings and errors. |
| `dotnet run --project tests/PawnShop.Tests/PawnShop.Tests.csproj --no-build` | All 10 regression cases passed. |
| `git diff --check` | Passed for tracked changes. New implementation files were also checked with `git diff --no-index --check -- /dev/null <file>`; no whitespace errors, only Git's LF/CRLF conversion notices. |

The tests exercise: no session on title; disabled controls and explanations;
focus wrapping and simultaneous navigation/confirm; held-key suppression and
focus synchronization; exactly one new session; no starting/closing input bleed;
pause/resume time semantics; inactive-window pause; both discard cancellation
paths; fresh identity/time/bell state; ten consecutive start/return cycles;
distinct title/window/menu exit requests; and idempotent application disposal.

The ten-cycle headless test confirms one live session while playing, zero after
return, and equal creation/disposal counts. M01 has no session event handlers to
accumulate. It does not claim to measure GPU resources or process memory.

Content check: the normal C# build compiles all original placeholder content.
There is no separate content compiler, asset manifest, or required external asset
to validate in this slice. No production saves are touched.

## Desktop evidence and limitations

`dotnet run --project src/PawnShop.Game/PawnShop.Game.csproj --no-build` launched
the real MonoGame application. Computer-use screenshot inspection confirmed a
legible title, visible keyboard focus, all four menu items, disabled labels,
instructions, and the no-saving notice. This is actual rendered evidence, not a
compilation inference.

After desktop testing was interrupted, only the launched test process was stopped
by its verified executable path to permit the final rebuild. That forced cleanup
is not evidence for the game's normal exit/disposal behavior. The final rebuild
and all 10 headless cases passed again after the graphics-disposal review.

Keyboard automation was interrupted by intervening desktop input, and a later
capture showed another application. Desktop input was stopped. The title launch
is verified; the following checks remain unverified and must not be inferred
from the headless suite:

- Focus Continue and Settings; verify reasons and that confirming does nothing.
- Start with Space; room appears with zero bell rings. Release and press Space
  again; count increases once. Hold it; count does not repeat.
- Pause; room clock stays fixed, menu marker keeps animating. Resume with Space;
  bell count stays unchanged. Repeat with Escape and check focus-loss pause.
- Cancel return and exit prompts; the same session remains paused.
- Complete ten desktop start/return cycles, changing bell state each time.
  Each new room starts with zero rings and fresh time. Inspect room texture
  disposal and bounded resource use after warm-up; headless counters alone are
  insufficient evidence for this resource check.
- Verify window close/Alt+F4 opens the discard prompt in play, cancellation keeps
  the window alive, and confirmed exit terminates it. Return to title must keep
  the application open. Exit from title must terminate without creating a session.

M00 is complete. M01 implementation and headless verification are complete, but
M01 acceptance remains pending the desktop lifecycle/resource checks above.
The next bounded task is to finish that acceptance pass, then M02's movement,
collision and time slice using the existing room. Do not start inventory or
expand the roadmap automatically.

## Structural refactor verification

The subsequent user request approved the eight-project organization. This was a
structural refactor of the existing implementation, not another milestone.

Mapping:

- The former `src/PawnShop.Application` input and focus models moved into
  `PawnShop.Framework/Input` and `UI`. Menus now accept a generic command enum;
  game commands remain in `PawnShop.Game/Screens`.
- `ApplicationFlow` and `GameSession` moved into `PawnShop.Game/Bootstrap`.
  The existing room state/view moved into `PawnShop.Core/World`; Bootstrap still
  creates exactly one session and owns its disposal.
- The former `src/PawnShop.Game` became root-level `PawnShop.Game`. Input polling
  and shared drawing resources moved into Framework; concrete title/menu/room
  drawing moved into Game/Screens, preserving original content and coordinates.
- The existing console test runner moved from `tests/PawnShop.Tests` into
  root-level `PawnShop.Tests`, with reusable input/menu checks under Framework.
- `PawnShopSimulator.sln` replaced `PawnShop.slnx`. The superseded Application
  project was removed after migration. The old src/tests trees were removed
  only after checking they contained no unmigrated files outside bin/obj.
- Narrative, Characters, Shop, and Exploration are actual class libraries with
  the requested empty feature folders. All empty requested folders have
  `.gitkeep`; no placeholder classes or future gameplay were implemented.

Retained SDK 10.0.401, net10.0, MonoGame DesktopGL 3.8.4.1, code-authored content,
and the dependency-free console test framework. Framework now directly references
the same MonoGame version because it owns rendering; Core has no project/package
dependencies. All four guidance documents and README now describe the approved
organization. Historical evidence above and milestone status are preserved.

Verification results:

| Check | Result |
| --- | --- |
| Pre-refactor `dotnet build PawnShop.slnx --no-restore` | Passed, zero warnings/errors. |
| Pre-refactor console regression runner | 10/10 passed. |
| `dotnet new sln --name PawnShopSimulator --format sln` | Initial template-cache write denied by sandbox; approved elevated retry succeeded. No templates/packages/SDKs were installed. |
| `dotnet restore PawnShopSimulator.sln` | All eight projects restored successfully. |
| `dotnet build PawnShopSimulator.sln --no-restore` | Passed, zero warnings/errors. |
| `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build` | Existing 10/10 cases passed, including ten headless restart cycles. |
| `dotnet build PawnShop.Core/PawnShop.Core.csproj --no-restore` | Passed independently, zero warnings/errors. |
| Project XML/reference checks | Exactly eight root projects; all reference targets exist; graph matches documented dependencies and is acyclic; no obsolete project/solution layout remains. |
| Content | Normal C# build compiles the moved font, shapes, and floor. No external assets or MGCB pipeline exist to rebuild. |

The new MonoGame executable was launched from
`PawnShop.Game/bin/Debug/net10.0/PawnShop.Game.exe`. Screenshot inspection verified
the same title, focus highlight, menu labels, instructions and placeholder content
after extraction into Framework and Screens. Computer Use was stopped by the
user with physical Escape during keyboard/close-window verification. No further
desktop automation was performed. Room rendering, keyboard lifecycle, normal
window-close behavior and ten desktop/resource cycles remain unverified by this
refactor; the earlier M01 manual acceptance checklist remains pending.

The restructuring has no build/test/dependency blocker. M00 remains complete,
M01 remains implemented with desktop acceptance pending, and no later milestone
has been started or marked complete. Nothing was committed or pushed.

## Rummage Hollow title artwork — 2026-10-24

The working directory and Git root matched the requested repository; the working
tree was clean before this change. The supplied town, pawnshop, salvage, and logo
now compose the title screen. The robot is excluded. Existing menu/input/session
logic is reused; only title labels changed. TitleScreen owns the four textures
and a 1920x1080 canvas, while room/overlay rendering remains at 640x360.

| Check | Result |
| --- | --- |
| Baseline and post-change `dotnet build PawnShopSimulator.sln --no-restore` | Both passed, zero warnings/errors. |
| Baseline and post-change `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build` | Both passed 10/10, including ten headless start/return cycles and input capture. |
| Four PNG source/repository/build-output SHA-256 comparisons | All matched; dimensions matched the supplied pack. Background is RGB; three foreground layers are ARGB. No robot copied. |
| Launch `PawnShop.Game/bin/Debug/net10.0/PawnShop.Game.exe` through Computer Use | Launched successfully. Screenshots inspected actual MonoGame rendering: composition, transparent layers, logo, bitmap menu, gold focus indicator, and both disabled-option explanations. |
| `git diff --check` | Passed; Git emitted only LF-to-CRLF conversion notices. |

The UI tool detected user input during attempted menu navigation, so further
automated desktop input was left to the user. Visual inspection does not establish
complete desktop start/pause/return/restart or graphics-resource acceptance. Those
checks and other display aspect ratios remain unverified in this change. The
existing M01 desktop acceptance checklist/status therefore remains pending; the
next bounded task is to complete that checklist before beginning M02.
