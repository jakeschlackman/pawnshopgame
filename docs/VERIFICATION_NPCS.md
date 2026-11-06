# NPC content refactor verification — 2026-11-04

Implemented per-NPC folders with immutable character JSON, sprite PNG/sheet metadata,
optional conversation bindings, Ink sources, and separate shop placements. The existing
visitor text/IDs and rectangle-placeholder appearance are preserved. No new packages,
projects, quest/preferences/schedule behavior, or save formats were introduced.

## Automated evidence

- `dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false`: passed, zero warnings/errors. Includes ordinary Ink content compilation.
- `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: 34/34 passed. The existing settings-file replacement test required an approved run outside the sandbox; its initial sandbox run was denied filesystem access.
- `./npc.ps1 check`: passed; visitor's 10 dialogue states explored.
- Isolated scaffold smoke check under `.cache`: created all five NPC files, added placement, refused duplicate creation, preserved existing NPC content. Initial smoke check found a PowerShell null-overload issue in file replacement; fixed to the existing .NET move-with-overwrite pattern and rerun successfully.
- `git diff --check`: passed (only existing LF/CRLF normalization notices).

Coverage includes data-only NPC routing, authored click bounds, unknown/unplaced targets,
empty rooms, non-speaking NPCs, explicit/default multi-conversation selection, sprite
header dimensions and metadata, invalid/escaping/missing references, duplicate conversation
IDs, unknown placements, stale output exclusion, and animation freezing during dialogue
and pause. Existing dialogue input, branch, session, settings and lifecycle tests still pass.

## Manual acceptance pending

No game launch or image inspection was performed. Verify the visitor's appearance/foot
anchor, click targeting, keyboard selection, dialogue layout, and authored multi-frame
sprites in the game. Header validation does not fully decode arbitrary PNG files.
No visual result is claimed by the automated checks.
