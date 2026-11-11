# Offline human asset commands

- Use this existing C# executable for human authoring; do not add PowerShell pipelines, runtime generation services, tooling projects, or production image-codec dependencies.
- Start with `PlayerAssets.cs` (CLI/validation), `PlayerAssetOutputs.cs` (import/pack/review), and `PlayerAssetTests.cs` (synthetic fixtures). Commands are listed by `dotnet run --project PawnShop.Tests --no-build -- --assets help`.
- Before human art changes read [artwork rules](../../docs/ENGINEERING_CONTRACTS.md#artwork-and-resources) and the [wearable workflow](../../docs/WEARABLE_ART_WORKFLOW.md). Preserve separate male/female fits, exact native coordinates, locked body hashes, and per-pose acceptance.
- Historical NESW/W-to-E inputs remain historical; new player directions are Skeleton v3 S/N/E with runtime-mirrored West. Do not relabel old evidence or grant automatic approval.
- Validation is read-only. Import verifies recorded hashes from local exports; pack/review/import require fresh destinations outside inputs and refuse overwrites. No network retries or automatic promotion.
- PNG operations require Windows/System.Drawing in Tests only. Test with isolated synthetic fixtures; original artwork and production save data remain untouched.
- Common-NPC runtime changes belong in Characters/Npcs/Common; this folder validates assets, not appearance generation or gameplay rendering.
