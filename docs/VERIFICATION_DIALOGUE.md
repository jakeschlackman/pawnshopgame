# Ink conversation draft verification

The first-draft evidence below is historical. The authoring follow-up at the end
records the current VS Code workflow and automatic content build, which replace
the original manual export process.

Date: 2026-11-04. User explicitly requested the first draft of the Inky/Ink
conversation engine. Working tree was clean before implementation. No branch,
commit, push, game launch, screenshots or desktop automation were performed.

## Implemented scope

- Official pinned Ink 1.2.1 runtime in Narrative; compiler used only by the
  existing test/authoring runner. Unmodified upstream assemblies, license and
  hashes are recorded under third_party/ink. Eight-project layout, SDK, MonoGame
  version and test framework remain unchanged.
- Editable Ink sample and checked-in compiled export. A visitor has an initial
  question, multiple topics, a conditional follow-up, and a farewell. This is
  prototype content without a quest, item exchange, reward or canon commitment.
- Immutable dialogue views, stable choice IDs, session IDs and view revisions.
  Rejected stale/unknown commands preserve the displayed state. Runtime metadata
  checks diagnose missing IDs, unknown speakers/emotions/tags, duplicate choices,
  unsupported externals and excessive evaluation work.
- GameSession owns transient runtime state. Tab opens Talk in the room, retaining
  the existing Interact/bell fixture. Dialogue captures input and pauses room time.
  Application focus loss retains the modal; close, cancellation and session end
  release it. A window-close request ends dialogue before exit confirmation;
  cancelling that confirmation returns to Pause.
- Shared selection behavior and fixed-width text wrapping. Separate presenter
  uses the existing text-speed setting and UI scale, distinguishes reveal from
  select, pages long text, and scrolls response groups. Keyboard and pointer
  selections use the same stable-ID command path. Presentation uses placeholder
  pixel text and speaker/emotion labels; no portrait assets were introduced.
- Authoring instructions and architecture/design/roadmap status updated. No
  gameplay saves, world-condition bindings, durable Ink variables, quest/economy
  effects or ambient bark system are claimed. Full M06 remains incomplete.

## Automated evidence

| Command/check | Result |
| --- | --- |
| Baseline `dotnet build PawnShopSimulator.sln --no-restore` | Sandbox attempt failed without a useful error diagnostic. A serial verbose retry eventually passed after compiler-server pipe timeouts and fallback. Subsequent builds explicitly disable shared compilation. |
| Baseline `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false` outside sandbox | Passed, zero warnings/errors. |
| Baseline `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build` outside sandbox | 25/25 passed. The sandbox denied the existing isolated temporary-file persistence test; its approved retry passed. |
| `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -- --compile-dialogue PawnShop.Game/Content/Dialogue/shop_visitor.ink` | Compiled successfully with pinned Ink 1.2.1. |
| Final `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false` | Passed inside sandbox, zero warnings/errors, including the existing MonoGame content workflow. |
| Final `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build` outside sandbox | 30/30 passed. |
| Content/dependency checks | Ink source compiles to the exact checked-in export; every sample branch exercised; license copied to output; game output contains runtime and no Ink compiler. |
| `git diff --check` and `git diff --no-index --check -- /dev/null <new text file>` | No whitespace errors in changed or new text files. Git emitted only LF/CRLF notices for tracked files. |

The five new regression groups cover sample branch/condition behavior and fresh
reopening; stale/cross-session/unknown/repeated commands; malformed JSON, broken
Ink graph links, metadata and external-call diagnostics, and a bounded infinite
loop; reveal/selection ordering and long-text paging at 100/125/150 percent; paused
time, focus loss, mouse selection, cancellation, exit confirmation, disposal and
ten fresh session cycles. Existing lifecycle and menu tests still pass after
extracting shared selection behavior. Source and export mismatches fail the tests.

## Manual checks remaining

Start New Game, press Tab, and read/play each visitor branch. Check panel
composition, text at 100/125/150 percent, all four reveal speeds, mouse targeting,
and Escape cancellation. Confirm that the room clock stays still and that a close
press does not ring the bell. Reopen and verify the opening conversation returns.
Check window-close/cancel behavior and ordinary pause/return/new-game navigation.

These visual/input-feel checks remain unverified and do not block this handoff.
No build or automated-test blocker remains. Existing M01 desktop acceptance is
unchanged; this record does not mark it or the full M06 milestone complete.

## VS Code authoring and NPC binding follow-up

The user requested a discoverable developer workflow, then clarified that all
conversations are text-only and VS Code is the preferred editor. Existing
uncommitted dialogue work was preserved and extended. Inky remains optional.

Changes:

- `dialogue.ps1` offers help, safe NPC/template creation, listing, bounded branch
  checks and a text-only terminal preview. Duplicate creation refuses to overwrite
  content. The source template includes the required stable tags and two branches.
- `.vscode/tasks.json` exposes four named Dialogue tasks; workspace snippets cover
  lines, choices and branches. `npc.schema.json` documents and validates editor
  fields. The root README now links directly to the authoring quickstart.
- `*.npc.json` files bind stable NPC IDs/names, Ink filenames and prototype room
  positions. The catalog registers all speakers from content. `TryTalkTo(npcId)`
  handles NPC clicks and keyboard selection without visitor-specific code.
- A normal build compiles Ink with the pinned compiler using `build/Ink.targets`.
  Syntax errors identify the authored file and line. JSON is generated under obj,
  not stored beside source. A generated index prevents stale output NPC/source
  files from reappearing after deletion. The game still ships only the Ink runtime.
- The authoring checker validates bindings and explores up to 256 distinct visible
  line/choice/global-variable states. This is bounded smoke coverage, not proof of
  every visit-count, random, thread, or local-variable-dependent path.

Verification:

| Check | Result |
| --- | --- |
| Baseline serial build with shared compilation disabled | Passed, zero warnings/errors. |
| Baseline headless suite | 30/30 passed. |
| `./dialogue.ps1 new -Id zz_authoring_probe -Name 'Workflow probe' -X 180 -Y 235` | Created the two authoring files. Repeating creation was refused; the source hash stayed unchanged. |
| Edit probe Ink, then `dotnet build PawnShop.Game/PawnShop.Game.csproj --no-restore -m:1 -p:UseSharedCompilation=false` | Passed; the generated JSON contained the new text without an export command. |
| Temporarily replace probe with a broken divert, build, then restore it | Build failed with `INK001`, the source filename and `(1,1)`; the valid probe was restored in a finally block. |
| `./dialogue.ps1 check` | Passed for visitor plus probe after an approved retry for sandbox-blocked NuGet vulnerability lookup. No audit or security setting was disabled. |
| `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -- --dialogue play zz_authoring_probe` with scripted response input | Showed the edited greeting, selected farewell and ended normally without graphics. |
| Remove only the two probe source files and rebuild | Final registry lists only visitor despite stale probe copies in build output. |
| Final `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false` | Passed, zero warnings/errors, including existing MonoGame content and new Ink compilation. |
| Final `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build` outside sandbox | 33/33 passed, including isolated temporary-file tests. |
| `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -- --dialogue check` | Visitor passed: 10 dialogue states. |
| VS Code tasks/snippets and NPC schema JSON | Parsed successfully; no VS Code UI automation performed. |

New tests cover two data-only NPCs, identity-based pointer and keyboard targeting,
no-session/busy/unknown-target rejections, malformed bindings, missing sources,
duplicates, stale output exclusion, terminal preview responses and alternate-branch
metadata failures. Authoring listings point to editable source, not bin copies.

No game window was launched. Check NPC labels/placement, click targets, selection
markers and text readability manually. VS Code task/snippet interaction is also
unverified visually; the underlying commands were exercised. No voice/audio work,
quest implementation, new project split, package upgrade or save change was added.
