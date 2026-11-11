# Working on offline human assets
Use the existing test/authoring executable. Start with the [wearable workflow](../../docs/WEARABLE_ART_WORKFLOW.md); it gives the exact commands, preserved legacy formats, fitting process, and acceptance limits.

- PlayerAssets.cs handles CLI/validation.
- PlayerAssetOutputs.cs handles local import, packing, and review outputs.
- PlayerAssetTests.cs exercises isolated synthetic inputs.
- Common-NPC runtime catalogs and appearance generation live in Characters/Npcs/Common.

After the normal root build, list supported commands:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets help
```

Validation is read-only. Import needs local exports and recorded hashes. Output commands require a fresh destination outside their input and refuse overwrite. Preserve original source art, manifests, prompts/jobs, rejected candidates, and approved body hashes.

Human art needs separately fitted male/female poses, native coordinates, and visual acceptance. New walks use Skeleton v3 S/N/E with mirrored West; preserved W-to-E sources do not gain approval by passing a checker. Numeric success neither promotes assets nor approves fit. No new PowerShell pipeline, runtime generator, or tooling project is needed.
