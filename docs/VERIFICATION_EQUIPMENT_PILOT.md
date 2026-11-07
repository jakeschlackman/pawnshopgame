# Phase 4 — Equipment fit pilots

## Resume checkpoint — 2026-11-07

The user accepted **both South-walking brown leather work boot fits** with
"perfect" and asked to resume in a later chat. No further generation is running.
**Next unstarted step: North standing and eight walking boot poses, separately
for male and female.** Do only that checkpoint, present the updated preview,
and await visual approval. Then fit East/mirrored West with side settling;
backpacks follow footwear. Do not restart the audit or regenerate accepted art.

- Approved: human cardinal walks and side settling; teal jacket, charcoal welding
  mask, separate short hair and wrench fits; richer dark-blue jeans in all four
  facings/settling; brown boots South standing and South walking on both bodies.
- The North wrench handle-only occlusion correction still has its own pending
  visual record. Keep that correction: only North hides the wrench head;
  South/East/West retain it. Do not infer a separate approval from the boot review.
- Work remains in `docs/ReviewAssets` and `tools/player-equipment-review.html`.
  No production C# or folder moves before the user's folder walkthrough.
  Future character logic belongs in `PawnShop.Characters`; shared item data in
  `PawnShop.Core/Items`. Write item-type Markdown guides during production
  implementation, using actual agreed paths and working examples.
- Use PixelLab and each body's own approved reference. Fit to exact existing
  poses using outfit transfer, then extract equipment only over unchanged bodies.
  Do not independently animate footwear or modify the gait. Sprout is frozen.
  Keep 48x64 frames, pivot (24,58), eight walk frames at 100 ms, and each foot's
  actual per-frame contact/lift. West mirrors the whole East composition.
- Boots draw under jeans, then jacket/hair/mask/wrench. Keep hair separate and
  visible through transparent headgear pixels. No new PowerShell scripts,
  projects, orchestration or runtime generation infrastructure.
- Standing boot references, source URLs, job IDs, palette and hashes:
  `ReviewAssets/BootsPilot/review.json`. Accepted South-walk sources, requests,
  masks and exact corrections: `ReviewAssets/BootsWalkSouth/review.json`.
  Reuse North body/equipment layouts from `WorkJacketNorth`, `JeansNorth`,
  `WeldingHelmetNorth` and `WrenchNorth` manifests; verify actual filenames.
  Prior corrections are documented below and in the associated manifests.
- Last checks: 221 PNG hash records matched; all 211 earlier assets unchanged.
  Sixteen boot poses passed coverage/composition checks over 49,152 native
  pixels. Preview: 73 images, 180 views, 64 loadouts; shared frames, layer order,
  crops, contact strips, West mirroring and settling passed. Visual acceptance
  came from the user; the agent did not launch/control the game or browser.
- The repository has extensive pre-existing uncommitted/untracked work. Preserve
  it. Nothing was committed or pushed. Inspect the working tree on resumption.
  Session-only tool stores and inline test commands are not durable; reconstruct
  inputs from checked-in/on-disk PNGs, JSON and HTML, not conversation memory.

Start by following AGENTS.md startup reading, then this checkpoint and the linked
manifests. This section supersedes older pending-review statements below for the
accepted South boot fits. Pause here until the user resumes; this record does
not authorize continuing generation while they are away.

Status: Phase 4 started, not complete. User accepted the corrected male and
existing female South-standing jacket fits with "great, next phase". A separate
South-walking overlay pilot is accepted for both bodies, including the female
shading correction (user: "its perfect, next phase"). North standing/walking fits
are also accepted (user: "perfect, next phase"). East/West and side settling fits
are also accepted (user: "no issues! next phase"). Separate South-standing welding
helmet fits were rejected for orange color and inconsistent shapes. Revised
charcoal masks using one consistent design on both bodies, including the male
ear occlusion correction, are accepted (user: "you fixed it! ok, good for the next
phase."). South-walking mask fits are also accepted (user: "flawless, next phase").
North standing and walking mask fits and short-hair combinations for both bodies
in South/North are accepted (user: "looks good, next phase"). East/mirrored West
mask and short-hair standing, walking and settling fits are also accepted
(user: "great, next phase"). The user subsequently corrected the wrench carry
direction to upright and explicitly requested replacing all existing fits in
place. The vertical and then palm-centered corrections were rejected because
the tool did not look held. The user clarified that the fist must remain unchanged
and the wrench should face inward. Current South/North fits lean the wrench toward
the torso while retaining the original hand and finger occlusion. The final fit
adds about 15 degrees of lean and follows the fist's dark crease, per the user's
follow-up. The user accepted these South/North fits with "good enough, whats the
next phase?". East/mirrored-West fits, including the subsequently lowered side-view
angle, were accepted with "good, next phase". Dark blue jeans on both South-standing
bodies are accepted (user: "perfect, next phase"). Both South-walking jeans fits
are accepted after the left-leg correction (user: "goood, next phase"). North
standing/walking jeans are accepted (user: "looks good, next"). East/West standing,
walking and settling jeans are accepted after the shin and female hand-mask
corrections (user: "fixed in, next phase"). Brown leather work boots are accepted
South standing on both bodies (user: "it looks good, next phase"); South-walking
boots are also accepted (user: "perfect"). The requested North-only wrench
head-occlusion correction still has a separate pending visual record.
The current jeans revision uses the user's selected richer-denim treatment on
both bodies. South and North jeans approvals remain intact.
Earlier approvals and correction checks below describe superseded fits.
No production artwork was overwritten.

The user accepted the current human motion checkpoint with "great, move onto the
next phase". The exact accepted body files and hashes are frozen in
[PHASE3_HUMAN_REVIEW.json](PHASE3_HUMAN_REVIEW.json); this does not approve older
comparison sources. Animated idle and runtime promotion remain Phase 3 work.

## Current deliverable

- One shared work-jacket design identity with two separately generated object
  references and two independently fitted South-standing garment layers.
- Native transparent 48x64 PNGs, pivot (24,58), retained source coordinates.
- Ordinary source/layer/composite files and a readable
  [review manifest](ReviewAssets/WorkJacketPilot/review.json) containing prompts,
  seeds, IDs, masks, color maps, body hashes and separate approval fields.
- [Equipment review page](../tools/player-equipment-review.html): reference,
  approved base, jacket layer and equipped composition for each body, native and
  1x/2x/4x views, 12x collar/cuff/hem crops and an equipped/un-equipped toggle.
- Separate eight-frame South-walking layers and unchanged-body composites for
  each body in [WorkJacketWalkSouth](ReviewAssets/WorkJacketWalkSouth/review.json).
  The same page retains these accepted walks, using one 100 ms frame
  clock to draw the exact body and matching layer. Pause, frame stepping, a frame
  slider and full eight-pose contact sheets support manual review.
- Accepted North standing and walking jacket fits for both bodies in
  [WorkJacketNorth](ReviewAssets/WorkJacketNorth/review.json). Each 432x64 sheet
  contains the standing pose followed by eight walk frames. The preview keeps
  standing separate from the walking clock and retains the accepted South fits.
- East standing, eight walk poses and the middle settling pose for both bodies
  in [WorkJacketEast](ReviewAssets/WorkJacketEast/review.json). West mirrors these
  same layers around the common character pivot; there are no West PNGs.
  The jacket controls offer all four directions plus side-view
  standing/walking transition playback.
- Revised South-standing charcoal welding masks for both bodies in
  [WeldingHelmetCharcoal](ReviewAssets/WeldingHelmetCharcoal/review.json), with a
  shared reference, retained independent fit sources, checked mask-only layers
  and unchanged-body composites. The original orange candidates are retained in
  [WeldingHelmetPilot](ReviewAssets/WeldingHelmetPilot/review.json) as rejected.
  Independent jacket/helmet toggles show all four equipment combinations;
  native and 12x head/collar views support fit review. A side-by-side comparison
  now puts both equipped bodies and mask silhouettes together at the top.
- South-walking rigid masks for both bodies in
  [WeldingHelmetWalkSouth](ReviewAssets/WeldingHelmetWalkSouth/review.json).
  Their accepted panels appear first when South is selected and provide native/full-size
  playback, a head/collar crop and an eight-pose contact sheet. Standing mask
  comparisons and accepted jacket-only motion remain below. No mask is shown in
  an unsupported facing.
- North standing and walking masks for both bodies in
  [WeldingHelmetNorth](ReviewAssets/WeldingHelmetNorth/review.json). The preview
  defaults to North. A separate standing comparison and eight walking poses use
  the same rigid rear headband/hinge design with each body's measured head offsets.
  The exposed scalp is always the original body artwork.
- [HairMaskPilot](ReviewAssets/HairMaskPilot/review.json) contains independent
  transparent normal and helmet-compatible short-hair layers for both bodies and
  South/North facings. Each sheet includes standing followed by eight walks.
  Hair, mask and jacket toggles are independent; normal/clipped hair-only panels
  make the separation inspectable. No base or gear PNG is modified.

PixelLab Pro Flash generated two one-direction object references and two reference
fits, five generation units per operation (20 total by the documented receipts).
Those standing operations used no paid retries. The subsequent South-walking
pilot used one PixelLab `transfer_outfit` request per body, eight exact accepted
body frames and its matching approved standing-jacket reference, at ten generation
units each. No body animation was regenerated. The walking manifest records both
requests, seeds, IDs, original body hashes, masks and eight output PNG hashes.
The sources changed body colors and introduced extra garment shades, so they are
not used as new player bodies. Explicit inspected garment regions extract only
clothing. A recorded RGB-nearest palette map returns those pixels to the shared
eight-color design palette. This is palette cleanup, not geometry resampling;
no garment pixels were shifted, scaled, stretched or duplicated to force fitting.
The North, South and side standing fits, walks and side transitions are accepted.
Walking extraction copies native source teal and adjacent outline
pixels using explicit per-frame masks, with a small recorded dark-color map.
It never copies generated skin or faces, nor moves fabric to force alignment.

## Verification

Inline nonvisual checks decoded both layers/composites and verified:

- Exact 48x64 dimensions, binary alpha, unchanged body files and all 21 frozen
  Phase 3 source hashes.
- Every composite pixel equals its garment pixel where opaque, otherwise the
  exact original body pixel. No generated skin/body pixel is substituted.
- Protected head, exposed hand and lower-leg regions remain unchanged.
- Shoulder coverage samples and central torso coverage pass for both bodies;
  the female sports-bra region is covered. These samples do not grant fit approval.
- Male layer: 416 garment pixels / five used colors after the pocket correction;
  female: 349 / five.

The user reported that the two gold male chest accents resemble nipples. Free
PixelLab guarded edits replace (20,30) and (27,30) from `#a4824d` to `#28565b`,
continuing the surrounding teal pocket shading. New `male-*-teal-pockets.png`
files preserve the original candidates. Full-image comparisons verify exactly
two RGBA pixel changes in each layer/composite and exact composition over the
unchanged body. The preview defaults to these corrections. Female art is unchanged.

PixelLab source and composition images were inspected, including enlarged collar,
shoulder, cuff and hem regions. The review page passes a Node DOM/canvas stub check
for real PNG dimensions, both body fits, scale, crops and equipped toggling.
Manifest hashes and `git diff --check` pass. No C# or runtime content changed, so
no build or regression-runner result is claimed for this art-only slice. The game
and browser were not launched; user visual acceptance remains separate.

## Next checkpoint

Review the new short hairstyle and hair/mask combinations in South and North,
standing and walking, on both bodies. North mask fit also remains pending; the
request to add hair did not grant North approval. Follow the existing wearable
approval workflow before side-view expansion.

After North acceptance, fit East and mirrored West mask views to each
matching body, including accepted side settling poses. Then
add the wrench with both-body fit checks, shared timing, layering and East-to-West mirroring. Promote
only accepted artwork into runtime content and verify the equipped combinations.
This pilot does not implement the rest of Phase 4 or finish Phase 3 runtime work.

## South-walking verification

Decoded-PNG checks pass for all 16 poses: 384x64 strips, eight native 48x64 frames,
binary garment alpha, recorded palette (no gold or skin in the jacket), exact
body-strip copies from the approved South rows, exact original-body-plus-layer
composition, exact selected source pixels with the recorded dark-color map,
explicit mask/count agreement, protected head/hand/lower-leg regions, central
torso coverage and shoulder samples. All eight new PNG hashes, all 21 frozen
Phase 3 hashes and all ten standing-pilot hashes match. Masks are specific to
these images; the extraction rule is not a general garment segmentation system.

PixelLab static source and composition frames were inspected at 4x. Manual
playback should check sleeve/hand gaps, hem flicker and changing pocket shading,
especially female frames 4 and 8. Protected-region and coverage checks cannot
establish the perceived quality of motion or approve a fit.

The updated preview passes a Node DOM/canvas stub check: 12 real PNG loads,
28 views, all eight frame selections, shared body/layer timing, pause/step/slider,
scale, native/enlarged crops, contact sheets and equipped toggling. `git diff
--check` passes. No C# code or runtime assets changed in this slice, so no new
build/test-runner result is claimed. Neither browser nor game was launched.
Both South-walking fits are now accepted, with the corrected female shading.

## Female jacket flash correction

User reported that the male walk looks fine but the female flashes at the end.
The female jacket had inconsistent bright torso shading in frame 8 and also
frame 4. Free PixelLab guarded recolors restore the neighboring same-height
poses' teal shading and remove isolated highlights: 57 pixels in frame 4 and
58 in frame 8. The exact patches and result IDs are recorded in
[female-shading-correction.json](ReviewAssets/WorkJacketWalkSouth/female-shading-correction.json).
New `female-*-shading-fixed.png` files preserve the original candidates.

Decoded comparisons verify exactly those recolors, identical alpha/silhouettes,
unchanged body pixels, unchanged other six frames and unchanged approved male
art. All eight previous walking PNG hashes and 21 frozen Phase 3 hashes match.
The preview uses the corrected female layer and labels the male as accepted.
DOM stub checks pass for loading, all frame bounds, shared timing, 8-to-1 wrap
and equipped toggling. No C# or runtime content changed; no build was needed.
The user subsequently accepted the corrected playback: "its perfect, next phase".

## North standing and walking checkpoint

One PixelLab `transfer_outfit` call per body used the approved matching South
standing jacket as a design reference, with the exact North standing pose and
eight frozen North walk frames as targets. Each call charged ten generation
units; no paid retries were needed. The back has a plain canvas panel and yoke
seam, with no front pockets or gold accents. The original generated sources are
retained; only jacket pixels enter the extracted layers.

Male walking inputs use the accepted head-bob source cropped at (16,9) from each
80x80 cell. Female walking inputs use the accepted legacy North row. Both standing
inputs use North column 4 from the canonical base atlas. Each native 48x64 pose
keeps pivot (24,58). The manifest records hashes, crop contracts, prompts, seeds,
job IDs, explicit masks, per-frame hand protection, palette maps and output hashes.
The female source used darker main fabric; a fixed palette map aligns it to the
male teal material. No pixels were moved, scaled, stretched or repeated to fit.

Decoded-PNG checks pass for all 18 poses: dimensions, binary alpha, shared palette,
mapped source pixels, exact masks/counts, exact original-body composition,
protected regions, torso/bra coverage and shoulder samples. All eight new PNG
hashes and all 41 previously frozen/accepted artwork hashes match. The sampled
back-panel mean brightness is constant across the eight walks for each body;
this diagnostic does not replace perceptual playback review.

All nine source and composed poses per body were inspected as static 4x PixelLab
images. Node DOM/canvas checks pass for 16 PNG loads, 30 views, North/South switching,
separate standing, eight walking frames, wrap excluding standing, contact/crop
offsets, shared frame timing, pause/step/slider, scale and jacket toggling.
`git diff --check` passes. No C# or runtime content changed, so no new build result
is claimed. The browser and game were not launched. Both North standing and walking
fits were subsequently accepted by the user: "perfect, next phase".

## East/West and settling checkpoint

One PixelLab `transfer_outfit` call per body used its approved South-standing
jacket reference and ten exact body poses: canonical East standing, all eight
accepted East walk crops, and the middle accepted settling pose. Each call
charged ten generation units; no paid retries were needed. Full generated
sources are retained, while explicit masks extract only jacket pixels at native
coordinates. Near/far hands and the face have per-pose protection. The bodies,
male head bob, stride order and accepted transitions are not regenerated.

Each 480x64 strip uses column 0 for standing, 1–8 for walking and 9 for the middle
settle. Starts use 0→9→1; stops use 1→9→0, reusing identical fitted endpoint
poses. The review timeline preserves the accepted 800 ms standing hold, 200 ms
start, 1600 ms walking and 200 ms stop. The normal walking loop excludes columns
0 and 9. Source hashes, crops, generation requests, masks, color maps, sequence
and output hashes are recorded in the manifest.

West uses East's body and jacket under the same transform about x=24 within the
48px canvas. Source pixel x maps to 47−x. Cropped detail views reflect the full
character coordinate system; contact sheets flip each pose independently while
preserving chronological order. This is a local canvas review of the composition;
runtime promotion will use the established MonoGame `SpriteEffects.FlipHorizontally`
path. No West artwork is independently generated or stored.

Decoded checks pass for all 20 poses: dimensions, binary alpha, shared palette,
exact mapped source pixels, masks/counts, exact unchanged-body composition,
protected regions, torso/bra coverage, and matching standing/walk-1 settling
endpoints. All eight new PNG hashes and 49 prior frozen/accepted source hashes
match. All ten source and composite poses per body were inspected as static 4x
PixelLab images. These checks do not grant visual motion or fit approval.

Node DOM/canvas tests pass for 20 actual PNG loads, all four directions, eight-step
walks and the complete 28-step transition sequence, wrap/pause/step/slider,
standing separation, contact chronology, matching layer transforms and correct
full-frame/crop mirroring over 265848 sampled pixel positions, scale and jacket
toggling. `git diff --check` passes. No C# or runtime content changed; no new build
result is claimed. The browser/game was not launched. User subsequently accepted
both East/West standing/walking fits and clothed transitions: "no issues! next phase".
The welding helmet South-standing checkpoint follows; the wrench remains pending.

## Rejected orange welding helmet checkpoint

Two PixelLab Pro Flash object references and two reference-based fitted sources
provide separate male/female fits for shared item `helmet_rusty_welder`, head slot.
The four paid operations were estimated at five generation units each; there were
no paid retries. Raw references and sources are preserved. Explicit per-body
row masks select 195 male and 180 female helmet pixels at their native positions.
Recorded color maps give both shells the same restrained rust-orange palette.
No generated anatomy is used, and no helmet pixels are scaled or repositioned.

The opaque shield covers the original face, eyes and scalp. Original ears, neck
and outline outside the fitted shield remain untouched; the male has small visible
ear portions beside the shield. This is recorded for fit review, not a claim of
complete enclosed-head coverage. There is no body-hide mask or new rendering
infrastructure. Compositions draw original body, optional approved jacket, then
optional helmet. Canonical artwork and Sprout are unchanged.

All ten PNGs pass decoded checks for native 48x64 dimensions and binary alpha.
Source masks, palette mappings, pixel counts, central face coverage, absence of
helmet pixels below y24, and both body/helmet and body/jacket/helmet compositions
pass exact pixel comparisons. All ten new and 57 prior frozen/accepted file hashes
match. Source head crops and composed head/collar crops were inspected through
PixelLab during authoring. The user subsequently rejected both masks: the orange
color and rounded female versus elongated male design were unacceptable. The
numeric checks did not establish visual quality. All original PNGs are retained;
their manifest explicitly records rejection and the replacement below.

Node DOM/canvas checks pass for 24 real PNG loads, 42 views, all four jacket/helmet
toggle combinations in each jacket direction, static South helmet crops and scale.
The accepted jacket direction, mirroring, transition sequence, contact chronology,
shared frame transforms, pause/step/wrap and timing checks also pass (5216 checked
draw calls). These checks do not claim browser rendering or visual acceptance.
The browser/game was not launched. No C# or runtime content changed, so no new
build result is claimed.

Review this standing helmet fit before generating its walking or other directional
fits. Helmet motion, North/East/mirrored West, settling, wrench, combined animated
equipment and production runtime promotion remain pending.

## Revised charcoal welding mask checkpoint

The user's correction is one charcoal-gray manufactured mask with a broad flat
shield and a clearly framed rectangular viewing window, with no body-dependent
design styling. One PixelLab Pro Flash master reference anchors both independent
reference-edit fits. A free exact palette edit changes the generated master's
green shell swatch to charcoal (170 pixels); both raw and corrected sources remain.
Three paid operations were estimated at 15 generation units total; the two fits
each reported five charged units. No paid retries or animation batches were made.

The male fit distorted the window and is explicitly rejected and retained. The
female fit preserved the mask design. Its rigid shield fits both canonical heads
at the same native coordinates, so the final male/female mask PNGs intentionally
contain identical pixels. This follows the workflow's allowance to reuse genuinely
identical art after checking both fits. Both body-specific compositions and approval
records remain separate. This does not assume interchangeable artwork for future
walking frames or different facings.

Explicit row masks select 215 source pixels; recorded palette cleanup reduces
them to five neutral colors without changing geometry. Both 48x64 layers use the
same bounds (17,8,15,17) and pivot (24,58). No source icon is pasted onto a body;
the layer comes from the fitted source. There is no resampling, shifting,
stretching, new hand-drawn shape or generated anatomy. All face/scalp skin is
occluded through y21. Five original male dark outline pixels remain outside the
shield/hinges; no female head pixels remain outside it through that row. Neck and
collar remain unchanged outside the mask. User fit approval is pending.

Decoded checks pass: ten PNG dimensions/binary alpha, identical variant layers,
exact source masks/maps/counts, neutral palette, face coverage, no helmet below
y24, and both original-body and original-body-plus-jacket compositions. All ten
new and 67 prior file hashes match, including all rejected orange PNGs. Source
and composed head/collar crops were inspected through PixelLab during authoring.

Node DOM/canvas checks pass for 24 actual PNG loads, 46 views, the shared reference,
side-by-side comparison panels, four equipment-toggle combinations in all jacket
directions, static South mask crops, retained jacket transitions/mirroring/contact
chronology/shared frame clocks, pause/step/wrap, scale and links (5312 draw calls).
No browser/game was launched; these are not browser-rendering or user-acceptance
claims. This correction changes review art, HTML and records only; no C# build or
runtime test result is claimed. Helmet animation and other directions await review
of the revised standing design.

### Male ear occlusion correction

The user identified the five exposed male outline pixels as an unwanted ear bulge.
The standing preview now hides native body pixels (17,13), (17,14), (16,17),
(16,18), (17,19) only while the charcoal mask is equipped. It clears them after
drawing the original body and before drawing the optional jacket and mask,
subtracting each review crop's origin. Unequipping restores the full original
head. Female, mask-only, bare-body and jacket-motion views are unaffected.
This is a five-point authoring occlusion rule, not a change to the canonical body
or mask artwork and not a generic runtime masking system.

Two free PixelLab guarded pixel edits produce `male-composite-ear-fixed.png` and
`male-jacket-composite-ear-fixed.png`; the previous exports remain intact. Each
has exactly five opaque-to-transparent changes and no other pixel differences.
Both equipped head regions are now pixel-identical to their female counterparts
through y21. No paid generation was needed. The manifest records the occlusion
coordinates, new export paths/IDs/hashes, and unchanged pending approval scope.

Checks pass for 79 recorded hashes (77 preserved, two new), native dimensions and
binary alpha. Node DOM/canvas checks load 24 PNGs and exercise all 46 views,
manifest/preview coordinate agreement, full-frame and cropped clearing, equipment
toggles and draw ordering across 128 composition cases. No body clearing occurs
in female, unequipped, layer-only or jacket-motion views. No C# or runtime asset
change; no build, browser launch or visual acceptance is claimed for this repair.

The user subsequently approved both corrected South-standing masks: "you fixed
it! ok, good for the next phase." The standing manifest records that scope.

## South-walking rigid-mask checkpoint

The approved mask is rigid during locomotion. The accepted head positions on both
bodies follow y offsets [0,0,-1,-1,0,0,-1,-1], with no horizontal offset. Matching
the standing head against each actual walk pose over x14..34/y5..21 establishes
these anchors. Minor original shade/jaw differences are fully beneath the mask;
body artwork remains unchanged. The manifest records the search region, best
offset and mismatch counts separately for each body/frame.

Each 215-pixel mask moves as one rigid piece with that measured head attachment.
The approved five-pixel male outline occlusion follows the same offset; the female
needs none. This is attachment animation, with no scaling, warping or refitting by
eye. The object animator and outfit transfer are deliberately unused: no independent
mask action is needed for this South walk. Keeping the approved shield pixels
prevents shape, visor and palette drift. A future independently moving visor would
need its own authored animation and review. No paid generations were used here.

Free PixelLab workbench exports create six ordinary 384x64 PNG sheets: mask-only,
body/mask and body/jacket/mask for each appearance. Eight frames retain the accepted
order and 100 ms timing, with common 48x64 canvases and pivot (24,58). Sources,
anchors, occlusion coordinates, IDs, hashes and separate pending walking approvals
are in the walking-mask manifest. This rigid-mask reuse does not establish that
clothing, other directions or future poses can share artwork without fitting.

Decoded checks pass for all 16 poses: exact translated mask pixels and palette,
215 pixels per pose, head/ear coverage, native dimensions, binary alpha and exact
original-body/approved-jacket compositions. The equipped head is identical to the
accepted standing head after accounting for its bob. All 85 recorded artwork
hashes match (79 preserved, six new). All combined poses were inspected as static
4x PixelLab contact sheets during authoring; user playback approval is pending.

The existing preview clock drives body, jacket, mask and occlusion together.
Node DOM/canvas checks pass for 26 actual PNG loads, 56 views and 320 walking
composition cases: eight frames, all four equipment combinations, exact crop and
contact offsets, chronological ordering, toggle restoration, pause/step/slider/
wrap, scale and links. Selecting another jacket direction hides the South mask
walking panels instead of claiming that facing is fitted. Accepted side transitions
retain their 28-step sequence. No browser/game was launched. Only review art,
HTML and records changed; no C# build or runtime verification is claimed.

The user subsequently approved both South walking previews: "flawless, next phase".
The South walking manifest now records that acceptance.

## North mask standing/walking checkpoint

One PixelLab Pro Flash reference edit used the exact male North-standing body
crop and the accepted South charcoal mask composition to generate the rear gear.
The request specified suspension band, crown strap, side hinges and shield edges;
the visor faces away, leaving the original bald scalp visible between straps.
The call charged five generation units. No paid retries or independently generated
walk animations were used. Raw source, exact request/seed, source crop and job ID
are preserved in the North manifest.

Explicit inspected row masks extract 136 gear pixels, mapping source shades to
the approved charcoal palette. The same rigid rear gear fits both standing heads
at native coordinates after separate composition checks. Every visible scalp pixel
outside the overlay comes from its own canonical body, not the generated source.
North needs no additional body-clear pixels. Nothing in the original bases,
approved jackets or South masks is overwritten or recolored.

The male's North head offsets are [0,0,0,-1,-1,0,0,-1,-1], including standing
first. The female's are [0,-1,-1,-1,-1,-1,-1,-1,-1]. These are measured against
each body's actual North standing/head frames; the female's already accepted walk
has steady head height. Small pre-existing scalp shade differences are preserved.
No new bob is invented and no body pose is changed. Rigid mask layers follow
these measured anchors with unchanged shape and shading.

Each 432x64 sheet contains standing at column 0 and eight walks at columns 1–8,
all native 48x64 frames with pivot (24,58). Walking uses 100 ms per frame and
never includes standing in its loop. Six sheets provide layers, body/mask and
body/jacket/mask compositions for both bodies. North standing and walking each
remain pending user approval; these exports do not approve future side fits.

Decoded checks pass for all 18 poses: exact selected/mapped source pixels,
136 gear pixels per pose, measured anchors, native dimensions/binary alpha and
exact original-body/approved-jacket composition. All 92 file hashes match
(85 preserved, seven new). Both standing head/collar crops were inspected at 10x
and all combined poses in static 3x PixelLab contact sheets during authoring.

The preview now starts on North and reuses its mask playback panels for North or
South, with a separate standing comparison and the existing shared frame clock.
Node DOM/canvas checks pass for 28 actual PNG loads, 58 views and 768 cases across
both facings: four loadouts, standing separation, eight-walk selection, crop and
contact ordering, layer timing, preserved South ear clearing/restoration, no North
body erasure, pause/step/slider/wrap, scale and links. East/West mask panels stay
hidden; the accepted 28-step jacket transitions are retained. No browser/game
was launched; no visual playback acceptance is claimed. No C# or runtime assets
changed, so no new build or runtime test result is claimed.

## Separate hair and mask compatibility pilot

The user requested hair beneath the mask and confirmed that hair must be on its
own layer. Four PixelLab Pro Flash edits fit one short chestnut crop separately
to each body's South and North standing heads. The male South result anchors
the other three style references. Four operations charged five units each (20
total); no paid retries or new body animations were used. Full raw sources,
requests, seeds and IDs are retained, but generated anatomy is never adopted.

Inspected per-fit row masks extract only hair. A recorded five-color brown map
stabilizes the palette. Normal transparent hair sheets contain no body, jacket
or mask artwork. Separate helmet-compatible sheets retain exact normal-hair
pixels only inside the recorded equipment envelope; clipping changes alpha only.
The normal sheets are never destructively trimmed. Each 432x64 sheet contains
standing followed by eight native 48x64 walk poses at the existing head offsets.
The source face, brows, ears, neck, limbs and gameplay motion remain unchanged.

South's equipped variants trim 9 male and 31 female protruding hair pixels at
standing coordinates (and the corresponding translated pixels in each walk).
The remaining 59 male / 48 female hair pixels lie entirely behind the opaque
shield. The resulting equipped compositions are pixel-identical to the prior
approved bald-body mask/jacket compositions. Removing the mask selects the full
68-pixel male or 79-pixel female hair layer again.

North's short cut already fits the rear envelope, so its 146-pixel male and
140-pixel female normal/equipped layers are identical. Drawing the unchanged
rear gear above hair leaves 70 male and 73 female hair pixels visible through
the strap openings per pose. No blanket hide-hair rule is used. Long/bulky styles,
side views, rear hair behind shoulders and independent hair motion are outside
this pilot and require their own fitted layers/occlusion review.

Preview order is original body, existing equipment-only body occlusion, optional
jacket, optional normal/clipped hair, optional mask. Every layer resolves from
the same standing/walk pose; no independent hair clock is added. Hair/mask-only
panels stay separately inspectable. Turning hair off restores the prior preview,
and turning the mask off restores normal hair without changing the body. Side
hair/mask panels remain hidden because those directions have not been authored.

Decoded checks pass for all 36 fits and 72 exported compositions: source
selection/palette mapping, exact anchors, dimensions, binary derivative alpha,
alpha-only clipping, central face/eyebrow protection, pixel-exact layer ordering,
South identity with approved equipped art, and visible North hair beneath straps.
All 112 artwork hashes match (92 preserved, 20 new: four raw sources, eight hair
layer sheets and eight review compositions). Four standing comparisons were
inspected at 4x, and all equipped head poses in static 3x PixelLab contact sheets.

Node DOM/canvas checks pass for 36 actual PNG loads, 62 views, 2048 direction/pose/
loadout cases and 1024 static comparison cases. Checks cover all eight hair/mask/
jacket combinations, normal/clipped sheet and column selection, ordering, shared
clock, crops/contact chronology, ear clearing/restoration, independent layer
panels, pause/step/wrap, scale, links and unsupported-side hiding. No browser/game
was launched. Hair appearance and compatibility require user review; North mask
approval remains pending. This is a review-art/HTML implementation; no C# or
runtime content changed and no new build result is claimed.

## East/West mask, hair and side settling checkpoint

The user accepted the preceding South/North short-hair and mask combinations
with "looks good, next phase". Both pilot manifests now record that limited
standing/walking scope. Earlier pending statements above describe those earlier
checkpoints, not their current acceptance status.

The current review extends the charcoal mask and short chestnut hair to East,
with West mirrored from the entire East composition. New files are confined to
`ReviewAssets/WeldingHelmetEast`, `ReviewAssets/HairMaskEast`, their manifests,
and the existing equipment HTML preview. There are no West source sheets.

- One PixelLab Pro Flash reference edit produced the East mask source; two
  body-specific reference edits produced the hair sources. Cost: 15 generations,
  three paid operations, no paid retries or independent walk generation.
- The same 134-pixel rigid mask fits both native standing heads. Separate male
  and female sheets follow each body's measured offsets. No body occlusion is
  needed beyond the mask's own alpha; no generated anatomy is retained.
- Each 480x64 sheet contains standing, eight walk poses and the existing middle
  settling pose. The head region matches the accepted standing head exactly
  after the recorded integer translations in all 20 poses. Body motion, including
  head bob and the existing start/stop sequence, remains unchanged.
- Hair is independently extracted and mapped to the established chestnut palette.
  Normal/fitted counts are 72/68 male and 85/74 female pixels per pose. A source
  ear-shading pixel was excluded from the female hair mask before handoff; the
  original ear shows beneath it. Removing the helmet restores the normal hair.
  Open straps reveal 27 male and 32 female hair pixels in every pose.
- The preview defaults to East and supports all four facings, all three toggles,
  native/enlarged views, separate standing, eight-pose contacts and the same
  28-step standing/walking transition used for the accepted jacket. West uses
  the same full-character pivot for body, jacket, hair and mask, including crops.

Verification: all 129 final file hashes match (112 prior files preserved; 17 new
PNGs). Decoded checks cover 20 exact head/overlay poses and 80 exact compositions,
native dimensions, binary alpha, source masks/palettes, integer anchors, alpha-only
clipping and visible hair through straps. The final female ear exclusion was
rechecked across all 10 poses and both hair compositions. Static PixelLab authoring
images include 4x standing comparisons and all 20 equipped head poses at 2x.

Node DOM/canvas checks load 42 actual PNGs and exercise 62 views, 704 direction/
motion/loadout cases, 11264 mask/hair views and 2138752 mirror-coordinate checks.
They cover shared column selection, contact chronology, all eight toggle
combinations, static South comparisons, ear clearing/restoration, pause/step/
slider/wrap, scaling and links. These checks do not claim browser/game playback
verification. No C# build was needed or run for this preview-only slice.

**Pending user review:** both bodies in East/West, continuous walking and
standing/walking transitions; mask on/off, hair on/off, jacket on/off; eye/ear
preservation, matching mask shape, strap transparency and mirrored alignment.
East/West approvals remain pending in both new manifests. Wrench, production
promotion, NPC integration and mech work are not part of this checkpoint.

**Organization before integration:** the user will guide folder changes before
production implementation. Character-specific logic belongs in
`PawnShop.Characters`; shared item definitions/instances belong in
`PawnShop.Core/Items`, without graphics dependencies. Final asset/catalog/renderer
placement remains to be settled with the user. Continue in previews until that
review; no files or production code were moved during this checkpoint.

## South-standing wrench checkpoint

The user accepted the preceding East/West mask and short-hair fits with
"great, next phase". Both East manifests now record standing/walking/settling and
mirrored-West approval for both bodies. Earlier pending statements above describe
the original handoffs, not their current acceptance status.

The wrench pilot remains in `ReviewAssets/WrenchPilot` and the existing equipment
HTML preview. It adds nine 48x64 PNGs: one tool design reference, two raw body-fit
sources, two tool-only layers and four comparison composites. All original body,
jacket, mask and hair artwork remains unchanged. No production C# or content-copy
rules changed, and no folders were moved.

PixelLab Pro Flash generated one object reference and two reference-based body
fits (three paid operations, estimated 15 generation units, no paid retries).
The two fits placed the handle at the existing right hand (image-left), but their
jaw details differed. The clearer female-fitted wrench was selected as the shared
rigid tool; the male fitted source remains as provenance. Exact pixel extraction
and exports used PixelLab's workbench. No new hand anatomy, scaling, stretching,
procedural replacement artwork or independent object animation was used.

The shared 31-pixel tool attaches at male (14,41) / female (15,41). Its selected
source grip is (15,41); the male layer translates that rigid object by (-1,0),
and the female layer stays at source coordinates. This is a hand attachment,
not a garment-fitting adjustment. Both layers have identical normalized shape
and steel palette. Draw tool, original body, existing mask-only body occlusion,
optional jacket, optional hair, optional mask. Original fingers conceal six male
or five female tool pixels, leaving 25/26 visible. Unequipping the wrench restores
the accepted composition exactly.

The review begins with fixed South-standing wrench panels for both bodies:
design reference, original body, tool-only layer, selected equipment composition,
native size and a 12x grip/cuff/jaw crop. All four equipment/hair toggles can be
combined. Direction and motion controls still operate the accepted animation
panels below; wrench panels remain South-standing and never imply unfitted motion.

Verification: all 138 locked hashes match (129 previous files preserved, nine
new PNGs). Decoded checks cover dimensions, binary alpha, source masks/palette,
body-specific anchors, normalized rigid shape, original-finger occlusion and four
pixel-exact comparisons. Final equipped compositions were inspected as static
4x PixelLab images; body/grip comparisons were also inspected during authoring.

Node DOM/canvas checks load 45 actual PNGs and exercise 74 views across 1408
direction/motion/loadout cases, including 16896 fixed standing-wrench views and
22528 accepted mask/hair views. Checks cover all 16 toggle combinations, draw
order, crops, fixed standing pose, unchanged mirror/clock/contact behavior,
pause/step/slider/wrap, scale and links. These are nonvisual automated checks;
no browser/game was launched and no C# build was required or run.

**User acceptance:** "looks good" approves wrench size and silhouette, both grips, cuff clearance,
and appearance with/without jacket, mask and hair for South standing on both bodies. South walking is the next
fit checkpoint after this approval; North/East/mirrored West, side transitions,
tool-use animations, production promotion, NPCs and mech remain later work.
The user-guided organization review remains required before production integration.

## South-walking wrench checkpoint

This checkpoint adds eight 384x64 sheets under `ReviewAssets/WrenchWalkSouth`:
full tool, finger-clipped tool, original-body comparison and complete equipment
comparison for each body. Every sheet has eight native 48x64 frames, pivot
(24,58), with the existing 100 ms pose timing. The South-standing tool and all
previously approved artwork remain unchanged.

Reuse the approved 31-pixel rigid wrench from the standing female layer at its
reference grip (15,41). Per-frame grips come from inspecting each original and
jacketed hand, including the changed fist silhouettes during the arm swing:

| Body | Frames 1–8: grip coordinates |
| --- | --- |
| Male | (14,38), (14,38), (14,40), (14,40), (15,42), (15,42), (14,40), (14,40) |
| Female | (15,38), (15,38), (15,40), (16,40), (16,41), (16,41), (15,40), (15,40) |

The manifest records exact source translations and finger stencils for every
pose. Original fingers conceal five or six handle pixels. Visible layers retain
25/26 unchanged tool pixels; no drawing, rotation, scaling, stretching or body
regeneration was used. PixelLab workbench exported exact source-derived pixels;
this slice required zero paid generations or independent object animations.

Walking requires a different overlap rule from the standing pilot: the wrench
crosses the thigh during the arm swing. Draw its finger-clipped layer over the
body/jacket so the jaw remains intact. Do not hide it behind the entire body.
Only explicit hand pixels make the handle transparent; original fingers show
through those pixels. Female frame 4's (17,42) belongs to the thigh/underwear,
not the finger stencil. Tool pixels never overlap the accepted jacket/cuffs.
Existing mask-related body occlusion and hair rules remain unchanged.

The equipment review defaults to South and now starts with the walking wrench
compositions, full/finger-clipped layers, native and 12x grip views, and an
eight-pose contact sheet for each body. All use the body's single frame clock.
Selecting another direction hides these South-only walking panels with an
explanation. Accepted South-standing comparisons stay separate; North/East/West
mask/hair motion and side transitions retain their previous behavior.

Verification: 146 locked file hashes pass (138 previous files unchanged, eight
new sheets). Decoded checks pass for all 16 source-exact tool poses, native
dimensions, binary alpha, normalized rigid shape, recorded grips/finger masks,
intact jaw over the thigh, zero jacket/cuff overlap, and 32 exact body/equipment
compositions. All 16 equipped poses were inspected in static 2x PixelLab sheets,
and grip/cuff/thigh crops at 5x during authoring.

Node DOM/canvas checks load 49 actual PNGs and exercise 86 views across 1408
direction/motion/loadout cases: 1536 new South walking wrench views, 16896
accepted standing wrench views and 22528 accepted mask/hair views. Checks cover
all 16 toggle combinations, shared columns and draw order, contact chronology,
unsupported-facing hiding, existing mirroring, pause/step/slider/wrap, scale
and links. These are nonvisual automated checks, not browser/game playback
verification. No production C# changed, no build was needed or run, and no
folders were moved.

**Pending user review:** both South walking grips throughout the loop, cuff
clearance, tool shape over the thigh, and combined/toggled equipment appearance.
North/East/mirrored West wrench fits, side transitions, tool-use actions, runtime
promotion, NPCs and mech remain later work. The user-guided folder organization
review still precedes production integration.

## North-standing and walking wrench checkpoint

The user accepted the South-walking wrench fits with "good, next phase"; their
manifest now records that scope for both bodies. The older pending statements
above describe the original handoff, not current South acceptance status.

Eight new 432x64 sheets under `ReviewAssets/WrenchNorth` contain the full rigid
tool, finger-clipped tool, original-body comparison and complete equipment
comparison for each body. Column 0 is standing; columns 1–8 are the eight walk
poses at the existing 100 ms timing. Frames remain 48x64 with pivot (24,58).

Reuse the same approved, unmarked steel wrench's pixels for its rear appearance,
reflected about its grip and attached to the anatomical right hand (screen-right
for North). Native source coordinates map to `x = gripX + 15 - sourceX` and
`y = gripY + sourceY - 41`. This does not mirror the human body or reuse South
motion offsets: North body poses remain independently authored, with their own
measured grips and explicit original-finger masks.

| Body | Standing, then walk frames 1–8: grip coordinates |
| --- | --- |
| Male | (33,41), (33,42), (33,42), (33,40), (32,38), (32,36), (32,38), (32,40), (32,41) |
| Female | (33,41), (32,42), (32,42), (33,40), (33,39), (32,35), (33,36), (33,40), (32,40) |

Each full tool has the same 31 native pixels. Five to seven original hand pixels
occlude the handle; male walk6 also preserves the extended finger at (31,41).
The fitted tool draws over the body/equipment so the jaw stays visible at thigh
crossings. It never overlaps the approved jacket/cuff pixels. No generated
anatomy, resampling, scaling, new paid generation or independent object animation
was used. One comparison export timed out without saving; a guarded exact edit
of the unchanged female body PNG recreated its 230 visible tool pixels instead.

The existing preview now defaults to North, with a separate standing comparison
alongside the walking composition, full/fitted tool layers, native view, grip crop
and chronological eight-frame contact. North grips use the right-side crop.
Switching to South shows its accepted standing and walking fits; selecting a side
direction hides unsupported wrench panels with an explanation. Existing South
standing comparison panels and all accepted mask/hair/jacket motion remain intact.

Verification: all 154 locked file hashes pass (146 prior files unchanged, eight
new sheets). Decoded checks cover all 18 exact reflected/attached poses, native
dimensions and binary alpha, recorded finger masks, normalized rigid shape,
extended-finger preservation, zero cuff overlap and 36 exact compositions.
All 18 grip/cuff/thigh crops were inspected in a static 5x PixelLab contact sheet,
and all complete equipped poses in static 2x sheets during authoring.

Node DOM/canvas checks load 53 actual PNGs and exercise 88 views across 1408
direction/motion/loadout cases: 3584 selected-direction wrench views, 16896
accepted South-standing views and 22528 accepted mask/hair views. They cover all
16 toggle combinations, separate standing/eight-frame loop, North right-hand
crops, source columns/order, contact chronology, unsupported-side hiding, existing
mirroring/clock, pause/step/slider/wrap, scale and links. No browser/game playback
is claimed. No production C# changed, no build was needed or run, and no files
were moved between production folders.

**Pending user review:** both North-standing grips and all North walking frames,
tool continuity through arm swings, original fingers/cuffs and combined/toggled
equipment. East/mirrored West wrench fits and side transitions come next after
approval. Tool-use actions, runtime integration, NPCs and mech remain later work;
the user-guided folder organization review still precedes production integration.

## Upright wrench correction — all existing fits replaced in place

The user corrected the carry pose: the wrench should point upward, then explicitly
directed, "do not preserve the approved downward facing ones, fix all of them".
All 22 derived PNGs in WrenchPilot, WrenchWalkSouth and WrenchNorth were replaced
at their existing paths: standing layers/comparisons and all South/North full-tool,
fitted-tool and comparison sheets. No parallel downward derivative folder or
backup set was created. The raw PixelLab source images remain generation inputs;
their selected tool pixels are transformed by the manifests before use.

This corrects 36 poses: standing and eight walking poses for each body in both
directions. Existing hand anchors, body animation, jacket, mask and hair files
are unchanged. The 31-pixel rigid source tool now maps above the grip using
`y = gripY + 41 - sourceY`, with `x = gripX + 15 - sourceX` for South and
`x = gripX + sourceX - 15` for North. There is no scaling or resampling.

All finger masks were recomputed for the upright handle. Each fitted layer hides
six original finger pixels and retains 25 exact tool pixels. Standing and walking
now both draw the fitted wrench above the original body and selected equipment,
allowing the jaw to overlap the forearm/sleeve while the original fingers show
through the handle mask. Previous downward thigh/finger masks are superseded.
No anatomy or new tool design was generated; all replacements used guarded exact
PixelLab workbench edits, with zero paid generation.

The preview uses only the corrected existing files. Both standing rendering paths
now use the same foreground tool rule as walking. Grip crops were raised to show
the full upright jaw. Labels and manifests mark the corrected orientation as
pending review instead of carrying forward old fit approval. Other equipment
panels and their approvals are unchanged.

Verification: all 154 locked file records match, including the 22 authorized
replacements; remaining records are unchanged. Decoded checks cover all 36 upright
fits and 72 exact composites: every full-tool pixel is above its grip, the full
shape retains 31 source pixels, fitted layers have 25 visible pixels and six
original-finger exclusions, native dimensions and binary alpha are correct, and
no downward tool remnants survive in the active layers/composites. All 36 grip/
cuff/jaw crops were inspected in a static 4x PixelLab contact sheet during authoring.

Node DOM/canvas checks load 53 PNGs and exercise 88 views across 1408 direction/
motion/loadout cases, including 3584 selected-direction wrench views and 16896
standing views. Checks cover all 16 toggle combinations, upright foreground
order/crops, shared frames, chronological contacts, separate standing, unaffected
equipment panels, unsupported-side hiding, pause/step/slider/wrap, scale and links.
No browser/game playback is claimed; no production C# or folder organization
changed, and no build was needed or run for this preview correction.

**Pending user review:** upright South/North standing and walking grips on both
bodies, the jaw's appearance above the sleeve, and combined/toggled equipment.
East/mirrored West remains the next wrench fitting work after this review.

## Palm-centered wrench grip correction

The user reported that the upright wrench did not look held. The previous source
grip (15,41) was outside the selected tool, whose pixels occupy rows 42–51.
Flipping around that point put the entire tool above the palm. The previous
"every tool pixel above the grip" check therefore enforced the wrong fit.

The actual handle center (15,45) now meets each original hand anchor. Both
directions use `y = gripY + 45 - sourceY`; the existing South/North X transforms
are unchanged. Original opaque hand pixels clip handle rows 43–47, retaining
18 or 19 fitted tool pixels. All 15 jaw pixels remain above the knuckles and
three handle-end pixels remain below the fingers. The full tool covers the grip;
the fitted layer is transparent at that point so the original palm shows through.

All 22 existing wrench derivative PNGs were replaced in place with guarded exact
PixelLab workbench exports. No paid generation, new derivative folder, body edit,
animation change, production C# change or folder reorganization was introduced.
Original generated sources remain provenance inputs. The manifests record the
new masks and current hashes; superseded export IDs identify only the preceding
upright revision and are explicitly labeled as historical.

Verification passed: decoded all 22 PNGs against the original selected tool,
body and equipped sources, covering 36 poses and 72 exact composites. Grip
coverage, original-hand occlusion, native dimensions and binary transparency
pass. All 154 locked file records match after recording the authorized 22
replacements; the remaining 132 records are unchanged. Static 5x PixelLab grip
contact sheets were inspected during authoring.

The Node preview check loaded 53 PNGs and 88 views, then exercised 1408 direction,
motion and loadout cases: 3584 selected-direction wrench views and 16896 standing
wrench views. All 16 equipment combinations, source-frame bounds, fitted wrench
drawn last, separate standing pose, unsupported-side hiding, stepping, slider,
integer display scales and links passed. No browser or game was launched, and
live animation or visual acceptance is not claimed. No C# build was needed for
this asset/preview/documentation change.

**Pending user review:** the palm grip in South/North standing and walking on
both bodies, with and without the other equipment. East/mirrored West fitting
remains the next step after this correction is accepted.

## Inward hilt alignment with unchanged fists

The user clarified that the fist must remain unchanged and the wrench must face
inward, then requested at least another 10 degrees of lean guided by the dark
areas of the fist, like a hilt. The current fit uses an approximately 33-degree
pixel-stepped inward angle, about 15 degrees more than the first inward revision.
The source grip meets the existing dark fist crease one pixel inward of each
original hand anchor. The active workflow and three wrench manifests record the
signed per-row offsets, per-pose tool grips and finger masks.

All 22 existing wrench PNG derivatives were replaced in place. The final art uses
only the existing tool pixels and original hands; no hand-edit candidate pixels
were applied. Two PixelLab hand-edit attempts made during the earlier mistaken
interpretation completed at five generations each. Cancellation of the second
arrived after completion. Both are excluded from the assets and recorded by job
ID in the pilot manifest. The final tool fitting itself used exact workbench
edits without further paid generation.

Decoded checks passed for all 22 PNGs, 36 poses and 72 composites: original
body/equipment sources plus the inward tool match pixel-for-pixel, full tool
contains 31 source pixels, fitted layers contain 21 or 22 pixels, and the original
hand remains visible through the handle mask at each tool grip. Source canvas,
frame order and timing are unchanged. All 154 locked records match after updating
the 22 authorized derivatives; the other 132 records remain unchanged. Transparent
canvases and binary alpha pass.

The Node check loaded 53 PNGs and 88 views across 1408 direction/motion/loadout
cases, checking all 16 equipment combinations, image dimensions and frame bounds,
unsupported-side hiding, stepping/wrap, slider, integer scales and links. The
preview's rendering logic is unchanged. A static male South-standing hilt crop
was inspected during PixelLab authoring; no browser or game was launched.
No production code, folder organization or original character artwork changed,
and no C# build was needed for this asset/preview/documentation correction.

Visual acceptance of the final inward angle remains pending for both bodies'
South/North standing and walking poses.

## East/West wrench and side settling checkpoint

The user accepted the corrected South/North fits and requested the next slice.
Their PNGs remain unchanged; the three manifests now record that acceptance.
Eight new PNGs under `ReviewAssets/WrenchEast` fit the accepted rigid wrench to
both original side-view bodies: standing, eight walk poses and middle settling.
The user then requested a lower side angle; the final approximately 45-degree
fit lowers it about 12 degrees around the unchanged per-pose grips. No hand art
was regenerated or applied, and no paid generation was needed for this slice.

The preview defaults to East and supports West through the same full-character
mirror used by body, jacket, hair and mask. Tool and finger clipping use the same
pose clock, including column 9 settling. Contact strips keep chronological order.
South-standing panels remain separate and accepted. No production code or folder
organization changed, and Sprout remains untouched.

Automated checks passed for eight 480×64 PNGs, 20 poses and 40 exact composites,
including original-finger occlusion, binary alpha and transparent canvases. All
154 previously locked file records remain unchanged. The new manifest records
eight additional hashes. Node loaded 57 PNGs and 88 views across 1408 direction,
motion and loadout cases: exact sources, frames, draw order and transforms for
19712 wrench views, including 8064 West views and 8448 legacy South-standing
composites. All 16 equipment combinations, grip crops, chronological contacts,
standing/settling endpoints, controls and integer scales passed. Static PixelLab
authoring previews were inspected; no browser/game was launched. No C# build was
needed for this asset/HTML/documentation slice.

**Pending visual review:** both bodies, East and West, walking and standing ↔
walking transitions, with jacket/mask/hair toggled. This does not imply runtime
promotion or approval of tool-use animations.

The remaining Phase 4 order is pants, footwear, then backpacks. Each requires
both-body animation fits and layering checks. Before production work, perform
the user's folder walkthrough; write practical item-type Markdown guides alongside
the implemented equipment code and working examples.

## Dark blue jeans — South-standing pilot

The user accepted the lowered East/West wrench fits with "good, next phase" and
chose dark blue jeans. `ReviewAssets/JeansPilot` contains separate male/female
PixelLab Pro Flash object references and exact-body reference fits. Only garment
pixels are extracted; canonical body, hand and foot pixels remain authoritative.
Four exposed dark male inner-leg outline pixels received a recorded denim-outline
cleanup at their original coordinates. No fabric rows were shifted or stretched.

After noting inconsistent detail, the user clarified that both jeans should look
better and selected richer denim: shaped folds, clear pockets/seams and subtle
wear. A paired PixelLab Pro edit applies that direction to both. The earlier
female-only simplification is superseded; its source is retained as provenance.
Current layers use a shared six-color dark-denim palette and the original fitted
235-pixel male / 254-pixel female masks. The detail model's geometry drift is
discarded: 15 male and 32 female existing edge pixels are retained and palette
normalized where the new source removed alpha, with exact coordinates recorded.
The sources and workbench exports are recorded in `JeansPilot/review.json`.
Generation usage totals 35 estimated/charged generations across design, fitting
and the two shading revisions; there was no large animation batch.

The existing preview now starts with dedicated South-standing jeans panels for
both bodies: object, original base, layer, body composition, selected equipment,
native size, waistband/jacket hem and ankle/feet crops. Jeans draw before the
jacket, with hair/mask/wrench above as appropriate. The jeans checkbox affects
these explicitly static panels only. Accepted movement equipment remains intact.

Automated verification passed: exact paired-detail-source extraction and palette
mapping into fixed masks; six 48×64 derivative PNGs with binary alpha; complete
waist/leg coverage; original hands and feet protected; exact original-body and
layered-equipment composites. All 175 file records match, including 13 jeans files;
the prior 162 recorded assets are unchanged. Node loads 61 PNGs and 104 views.
Jeans checks cover 2816 direction/motion/loadout cases and 45056 views across all
32 toggle combinations, with correct order and static South poses. Existing
equipment checks cover 1408 cases, 19712 wrench views including 8064 West views,
and 8448 legacy South-standing composites. Controls, crops, scale and links pass.

Static PixelLab authoring previews were inspected. No browser/game was launched,
and no C# build was needed for this asset/HTML/documentation slice. Production
code, approved artwork, project boundaries and Sprout remain unchanged.

The user subsequently accepted both richer-denim South-standing fits with
"perfect, next phase". Their original PNGs are frozen; their manifest records approval.

## Dark blue jeans — South walking

`ReviewAssets/JeansWalkSouth` contains separate male/female eight-frame fitted
sources, garment layers, body composites and fully equipped composites. PixelLab
Pro outfit transfer used each body's approved standing jeans reference and exact
accepted South walk frames, costing 10 generations per body. Generated anatomy
is discarded. No body, motion, jacket, mask, hair or wrench artwork was changed.

Each layer stays on the original 48×64 frame grid with pivot (24,58), eight
100 ms poses and the original frame order. Native hand/foot protection and
per-frame garment masks are recorded in the manifest. The 167 explicit native
gap/boundary cleanup pixels across 16 poses use the approved denim palette;
all remaining garment pixels are exact generated source pixels. No cloth panels
were shifted, stretched or assembled from repeated rows.

The preview now begins with South-walking jeans panels: selected equipment,
body plus jeans, garment layer, native size, waist/ankle crops and chronological
contact strips. These panels appear only for South; accepted standing references
remain visible below. All layers use the same pose clock. Pants draw between the
original body and jacket; hair, mask and wrench retain their existing order.

Automated verification passed for 49,152 native pixels: frame dimensions,
binary derivative alpha, exact extraction/recorded cleanup, per-pose masks,
original silhouette, protected hands/feet, original-body composition and exact
equipment occlusion. All 175 pre-existing PNG hash records remain unchanged.
The eight new PNGs are recorded with hashes. Node loads 63 images and 118 views.
South-walk jeans checks cover 2,816 direction/motion/loadout cases and 39,424
views across all 32 toggle combinations, including contact order, shared frames,
crops and South-only visibility. Existing standing jeans checks (45,056 views),
wrench checks (19,712 views, including 8,064 mirrored West views), side-transition
controls and scale checks still pass. `git diff --check` passes.

No browser/game was launched. No C# build was needed for this asset/HTML/docs
checkpoint. **Pending user review:** both South-walking jeans fits, particularly
denim consistency, moving hems, original feet and jacket overlap. North then
East/mirrored West and side settling remain next, followed by footwear/backpacks.
Production folder decisions and item-type Markdown guides remain deferred to
the agreed integration walkthrough.

The user reported skin showing on the female's left leg. The broad hand exclusion
had also clipped her outer thigh. Per-pose thigh boundaries now restore 107 native
garment pixels (82 from the existing generated source, 25 denim edge cleanup),
with 40 changes visible in the fully equipped composite. No new generation was
needed. The three affected PNGs and manifest were archived locally before repair.
Pixel comparison verifies exactly those changes and no others, preserving hands,
feet, source art and every male asset. All 16-pose PNG checks and preview loadout
checks pass again. Visual acceptance of the corrected South walk remains pending.

The user subsequently accepted the corrected South walks with "goood, next phase".
The South-walk manifest now records that approval; all eight PNGs remain unchanged.

## Dark blue jeans — North standing and walking

`ReviewAssets/JeansNorth` adds separate male/female rear fits: raw PixelLab
sources, jeans-only layers, unchanged-body composites and full equipment
composites. Each 432×64 sheet contains standing in column 0, then eight walk
frames at 100 ms each. The standing pose is excluded from the walk loop.
Every frame retains its 48×64 canvas and pivot (24,58).

PixelLab Pro outfit transfer used each body's accepted South-standing denim
reference with nine exact North body poses, costing 10 generations per body.
Only garment pixels are used. Rear pockets, yoke seams and folds come from the
generated source. The extraction maps 974 source outline pixels to the shared
denim outline and records 56 native boundary/gap cleanup pixels across 18 poses.
Two additional female thigh pixels were restored directly from the fitted source
after checking the hand boundary. No fabric panels were shifted or stretched.

The existing preview defaults to North, with standing comparisons, walking
compositions, isolated pants, native size, pocket/ankle crops and eight-pose
contact strips. Body and equipment use the same frame clock. Jeans draw under
the jacket; original hair/mask/wrench layers retain their existing order. North
jeans panels hide when another direction is selected; accepted South panels remain.

Automated checks pass for 55,296 native pixels: dimensions, binary derivative
alpha, source extraction/palette mapping, per-pose masks, original silhouette,
hands/feet, exposed thigh skin, and exact body/equipment composition. All 183
pre-existing PNG records match; the eight new PNG hashes are recorded. Node loads
65 images and 134 views. North checks cover 2,816 direction/motion/loadout cases
and 45,056 views, including all 32 equipment combinations, fixed standing and
shared walking columns, chronological contacts, crops and North-only visibility.
Existing South jeans, West mirroring, side-transition and control checks pass.
`git diff --check` passes. No browser/game was launched or production C# changed;
no C# build was needed for this asset/HTML/docs slice.

**Pending user review:** both North standing/walking fits, rear denim detail,
moving hems and jacket overlap. East/mirrored West with settling is next after
approval, followed by footwear and backpacks. Folder integration and item-type
Markdown guides remain deferred to the agreed production walkthrough.

## Side-view jeans and North wrench occlusion

The user accepted both North jeans fits with "looks good, next". Separate male
and female side fits now live in `ReviewAssets/JeansEast`: raw fitted sources,
garment layers, body composites and fully equipped composites. PixelLab Pro
outfit transfer used each body's approved denim reference with the exact ten
side poses, costing 10 generations per body. The 480×64 sheets retain 48×64
frames and pivot (24,58): standing 0, walking 1–8, middle settling 9. Walking
never inserts the standing or settling frame. Start/stop use the existing
28-step preview sequence. West mirrors the entire East composition and crops;
there are no independently generated West assets.

Native extraction preserves original bodies, accepted hand masks and explicit
per-pose foot regions. The manifest records all garment masks, 589 source-black
pixels normalized to the denim outline and 36 boundary/gap cleanup pixels.
No garment panels were shifted, stretched or made from repeated rows.
Automated PNG checks pass across 61,440 pixels and 20 poses: dimensions, alpha,
palette, exact extraction, protected hands/feet, lower-leg coverage and exact
body/equipment composition. All 191 prior PNG records match, including the
explicitly authorized North wrench corrections; eight new PNG hashes are recorded.

During this work the user clarified that North-facing characters hold the wrench
in front of their bodies, so its head should not show from behind. Only North
visibility changed: 15 jaw pixels per pose are hidden, retaining 6–7 original
visible handle pixels. Original fists, grip and angle are unchanged. Six
`WrenchNorth` layer/composite PNGs and two `JeansNorth` fully equipped composites
were corrected; source/full-tool references and every South/East wrench PNG
remain unchanged. The earlier files/manifests are archived locally under
`.cache/NorthWrenchBeforeOcclusionFix`. Exact comparison checks all 1,080 affected
jaw locations across eight PNGs, zero visible jaw pixels in all 18 North poses,
correct restoration behind the head and no changes to any other pixel.
No paid generation was needed for this occlusion fix.

The HTML preview defaults to East; select West and Standing ↔ walking to check
the mirrored settling fit. Select North to inspect the handle-only correction.
The full-tool panel is explicitly a fitting reference, not the rendered layer.
Node checks load 67 PNGs and 150 views. Side jeans pass 2,816 direction/motion/
loadout cases and 45,056 views, including 18,432 West views, all 32 combinations,
shared pose columns, whole-composition mirroring, cropped views, chronological
contact sheets and standing/settling controls. Existing South/North jeans and
gear checks pass. `git diff --check` passes. No browser/game was launched;
no production C# changed or required a build.

**Pending user review:** both East/West jeans fits through standing, walking and
settling, plus corrected North wrench visibility. Shoes/boots follow acceptance,
then backpacks. Production folder decisions and item-type Markdown guides remain
deferred to the agreed integration walkthrough.

The user found exposed leg skin in both side animations, especially the female.
The raised-foot protection rectangles included parts of the shins. Those masks
now isolate the distal feet; denim covers the remaining leg pixels in each pose.
The correction adds 73 male and 85 female garment pixels: 44 copied from existing
generated denim and 114 native seam/boundary cleanup pixels. It changes only the
six side jeans derivative PNGs and their fit record; the originals are archived
under `.cache/JeansEastBeforeShinCoverageFix`. No paid generation was needed.
Exact comparisons verify only those additions (71 male / 76 female visible in
equipped composites), leaving all other pixels unchanged. Checks pass across all
20 side poses, 61,440 native pixels, all 32 loadouts, West mirroring and settling.
The North handle-only wrench correction remains unchanged and passes its checks.
Visual approval of the repaired side jeans remains pending.

The user then identified remaining female exposure at the hand. The reused jacket
hand rectangles included 37 pixels of fitted denim beside/below the fist. These
are now restored exactly from the existing source, and excluded from the jeans'
per-pose hand masks. Only three female jeans PNGs changed (37 layer/body pixels,
34 visible equipped pixels); every other female pixel, the male PNGs, raw sources
and other equipment remain unchanged. Checks confirm no fitted-source denim is
still rejected by these hand masks, all ten female side poses compose correctly,
and all 32 loadouts, West mirroring and settling still pass. The earlier female
files are archived under `.cache/JeansEastBeforeFemaleHandMaskFix`. No generation
or animation changes were needed. The user subsequently accepted the corrected
side jeans on both bodies with "fixed in, next phase".

## Brown leather work boots — South standing

The user selected brown leather work boots. `ReviewAssets/BootsPilot` contains
separate male/female PixelLab Pro Flash object references and reference-fitted
sources, plus boot-only layers and body, jeans and full-equipment composites.
Two object jobs and two fitting jobs used approximately 20 generation credits;
no animation job was requested. Exact prompts, job IDs, exports, palette mapping
and PNG hashes are recorded in `BootsPilot/review.json`.

Both fits retain the original 48x64 canvas and pivot (24,58). Source extraction
uses male rows 49–57 and female rows 48–57, with a shared seven-color brown
leather/charcoal sole palette. Ten extra generated sole pixels below row 57 are
discarded per body without shifting or resizing. No boundary repair was needed.
Only footwear is extracted; generated anatomy is discarded. Boots draw before
the accepted jeans, allowing their hems to overlap boot tops. Full boot shafts
remain available when jeans are switched off.

The existing HTML preview adds both South-standing fits, isolated boot layers,
original-body comparisons, jeans and full loadouts, native-size views and enlarged
ankle/sole crops. These panels remain static South standing under every motion
or direction control. Existing directional equipment panels are retained.

Automated verification passes: all 12 PNGs are 48x64, eight derivatives have
binary alpha, and source/palette extraction plus all three composites per body
match exactly across 6,144 native pixels. The 112 male and 127 female boot pixels
cover every original foot pixel, preserve the central gap and end at ground row
57. All 211 recorded PNG hashes match, including 199 unchanged earlier assets;
both canonical body hashes remain unchanged.

Node canvas checks load 71 PNGs and 166 views. Boot checks cover 5,632 cases and
90,112 views across all 64 equipment combinations, validating layer order,
unchanged standing bodies, crops and independence from motion/direction controls.
Existing jeans and wrench checks also pass, including West mirroring, shared
frames, side settling, contact strips, controls and scaling. `git diff --check`
passes. No C# code changed in this checkpoint, so no build was needed. No browser
or game launch was performed by the agent.

**Pending user review:** boot design, toe/sole shapes and jeans hem overlap on
both South-standing bodies. This does not approve walking, North, East/West or
settling fits. South walking follows design approval; backpacks follow footwear.
Production code and item-type guides remain after the user's folder walkthrough.

## Brown leather work boots — South walking

The user accepted both South-standing fits with "it looks good, next phase".
`ReviewAssets/BootsWalkSouth` adds separate male/female eight-frame boot fits,
transparent layers and body, jeans and full-equipment composites. PixelLab Pro
outfit transfer used the approved same-body standing reference and the exact
existing walk frames, costing 20 generations total. It did not generate a new
animation. Prompts, job IDs, raw source exports, masks and hashes are in the JSON.

Boot extraction uses per-leg bounds for every pose, retaining each original
foot's lowest opaque row and at most one source-authored horizontal edge pixel
outside the body silhouette. The approved seven-color leather/sole palette is
shared across both bodies. One missing native edge pixel in male frame 4 and
one in female frame 2 receive recorded dark outline coverage. No body pixels,
frame timing or leg placement are changed; generated anatomy is discarded.
Boots draw below the accepted jeans and share their frame clock.

The existing HTML preview adds South-walking boot panels, isolated layers,
body-plus-boots, jeans combinations, full equipment, native-size and foot crops,
plus chronological contact strips. Pause and frame stepping inspect all eight
poses. Walking boot panels hide outside South; standing comparisons remain.

Automated checks pass: eight derivative PNGs are 384x64 with binary alpha;
49,152 native pixels across 16 poses match source/palette extraction and exact
compositions. All original foot pixels are covered, and each foot's ground/lift
row is preserved. All 221 PNG records match, including 211 unchanged earlier
assets and both walking body hashes. Node loads 73 PNGs and 180 views; walking
boots pass 5,632 cases and 78,848 view checks across all 64 equipment combinations.
These cover shared frames, layer order, crops, contacts and South-only visibility.
Standing boot, jeans, wrench, West mirroring and settling regressions also pass.
`git diff --check` passes. No production C# changes or build were needed; no
browser/game launch or agent visual acceptance is claimed.

**Approved by user:** both South-walking boot fits ("perfect"). North
standing/walking is next, then East/mirrored West and side settling.
Backpacks and the production folder walkthrough remain later checkpoints.
