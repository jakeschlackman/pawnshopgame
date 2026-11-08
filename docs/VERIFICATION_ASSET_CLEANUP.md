# Phase 1 human asset tooling cleanup

Date: 2026-11-06. User approved Phase 1 after the read-only architecture audit.
Existing uncommitted walking sources and documentation were preserved. No artwork
generation, game launch, screenshot inspection, visual approval, branch or commit.

## Changes

- `PawnShop.Tests/Assets/PlayerAssets.cs` contains the command entry point and
  base/static-shirt/legacy-walk/attire checks. `PlayerAssetOutputs.cs` contains
  local import, packing and review output. `PlayerAssetTests.cs` exercises isolated
  synthetic fixtures through the existing regression runner. No factory/service
  hierarchy or new project was added.
- The test/authoring executable pins System.Drawing.Common 10.0.0 with
  PrivateAssets=all. This retains the old scripts' Windows PNG requirement without
  a graphics device. Production dependencies, SDK and target frameworks are
  unchanged. Game's resolved package graph contains no System.Drawing.Common.
- Removed `Validate-PlayerBases.ps1`, `Validate-Wearable.ps1`,
  `Validate-PlayerWalk.ps1`, `Test-PlayerWalkValidation.ps1` and
  `Build-PlayerWalkReview.ps1` from tools. Three one-time cached helpers
  (`import-walk-sources.ps1`, `Promote-PlayerArt.ps1`,
  `Finish-PlayerArtCleanup.ps1`) were also retired. All eight originals were
  archived with paths and SHA-256 verified before removal in
  `.cache/Phase1ToolArchive/legacy-scripts.zip`. That local archive is ignored;
  source assets and the replacement commands do not depend on it.
- `tools/player-walk-review.html` remains the simple review template. It is
  copied into test output and explicitly labels historical W-to-E artwork.
- Added the missing displayScale=1 field to the candidate walk-sprite.json.
  The C# validator now uses SpriteSheetDefinition.Load, catching fields that the
  former PowerShell metadata check missed. No production parser was weakened.
- Current guidance now consistently requires new S/N/E art with runtime mirrored
  West, shared composition coordinates, 48x64 human canvases and pivot (24,58).
  Historical eight-facing standing sources and NESW walking sources keep their
  identities, ordering, W-to-E provenance and existing approval scope.

## Command behavior

See [the workflow](WEARABLE_ART_WORKFLOW.md) for exact commands. `--assets help`
lists bases, wearable, walk, pack, review and import. Validation is read-only.
Pack/review/import require a fresh destination outside their inputs and refuse
overwrites. Import consumes local exported files and verifies already recorded
hashes; it does not access PixelLab, download URLs or change trust baselines.
Pack preserves source files/manifests, writes candidate atlases and metadata, and
checks the result. Review writes only contact sheets and HTML. There is no
automatic promotion/approval command, generalized cache or orchestration layer.

The static-shirt reader intentionally retains the approved historical manifest's
filenames and coverage expectations, including its shirt-specific color check.
It is not a generic garment extractor. The walk reader supports version-1 NESW
candidates only. New S/N/E runtime metadata and multiple equipment preview layers
belong to subsequent phases, not an implicit conversion in this cleanup.

## Verification

Baseline: SDK 10.0.401; solution build succeeded with zero warnings/errors;
existing runner passed 47/47.

Final command set:

```text
dotnet restore PawnShopSimulator.sln
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project PawnShop.Tests --no-build
dotnet run --project PawnShop.Tests --no-build -- --assets bases PawnShop.Game/Content/Player/Bases
dotnet run --project PawnShop.Tests --no-build -- --assets wearable PawnShop.Game/Content/Player/Wearables/TealWorkShirt
dotnet run --project PawnShop.Tests --no-build -- --assets walk PawnShop.Game/Content/Player/Bases/Walking
dotnet run --project PawnShop.Tests --no-build -- --assets pack PawnShop.Game/Content/Player/Bases/Walking .cache/phase1-packed
dotnet run --project PawnShop.Tests --no-build -- --assets review .cache/phase1-packed .cache/phase1-review
git diff --check
```

Restore and build passed; build had zero warnings/errors. Runner passed 48/48.
Both canonical bodies, both static shirt fits and all legacy walk frames passed
numeric checks. Pack and review succeeded in separate ignored directories.
Import was exercised with local synthetic exports; no network import was needed.
Whitespace checks passed with Git line-ending notices only.

Regression coverage includes missing displayScale, malformed PNG, stale source
and fitted-body hashes, missing paths/female fit, timing mismatch, coverage and
protected pixels, empty masks/body frames, partial alpha, incorrect composites,
duplicate active sources, mirror mismatch, border/crop loss, escaping paths,
existing/nested output refusal, unchanged import manifest and CLI diagnostics.
Generated fixture directories are isolated and cleaned up. The suite requires
Windows and reports an explicit failure rather than claiming PNG checks ran on
unsupported platforms.

All 246 existing Content PNGs were compared before/after by SHA-256: unchanged.
Sprout's assets, builder and runtime implementation were untouched. Runtime
player/NPC behavior was untouched; no phase-2 mirroring or equipment is claimed.

## Manual acceptance and limits

No visual inspection was performed. The generated contact sheets and HTML can be
reviewed manually, but numerical success does not approve gait, fit or scale.
The static shirt remains approved only for both south-facing standing poses;
walks remain candidates. NPC/dialogue script simplification is deferred. There
is no automated blocker to the Phase 1 handoff.
