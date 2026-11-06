# Teal work shirt — approved south-facing fit

The user approved both cuts on 2026-11-05. This is the reference example for
[the wearable authoring workflow](../../../../../docs/WEARABLE_ART_WORKFLOW.md).
Approval covers south-facing static male and female poses only. Other
directions, animation and gameplay integration remain pending.

The male/female folders contain the independently AI-generated PixelLab shirt
object, transparent native-coordinate shirt.png and composed preview.png.
Each PNG is 48x64. Draw the canonical south base from `../../Bases`, then the
matching shirt at the same ground pivot (24,58). Generated body pixels are never
used. There is no shifting, stretching or insertion of fabric rows in this fit.

wearable.json records approval, base references, object/gallery IDs, prompts,
seeds, masks and coverage checks. Sources preserves the AI-dressed references;
Recipes preserves exact extraction/composition. These are authoring metadata,
not an implemented runtime equipment format. Body-specific PixelLab art objects
do not require separate gameplay/economy items.

Open review-4x.png: object, original base, garment-only layer and composition,
with male above female. review-native.png is the same sheet at native size.
Run `./tools/Validate-Wearable.ps1` from the repository root for base preservation,
dimensions, binary alpha, layer composition and shoulder/underwear coverage.
The approved PNGs and sources are bundled locally; no runtime PixelLab access is
needed. All artwork was created through PixelLab; service terms apply.
