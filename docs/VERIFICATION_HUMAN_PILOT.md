# Phase 3 — Human reference and motion checkpoint

Status: user accepted the current standing, cardinal walk and East/mirrored-West
settling checkpoint with "great, move onto the next phase" after the forward-gait
and head-bob corrections. Frozen current sources are recorded in
PHASE3_HUMAN_REVIEW.json. Animated idle and runtime promotion remain unfinished
Phase 3 work. Phase 4 begins with a separate male/female South-standing jacket
design-and-fit pilot; it does not imply the remaining Phase 3 work is complete.

The user accepted the Phase 2 placeholder demo, then reviewed the male/female
standing and South-walk preview. They reported male South frame 6's extra arm
outline, accepted its correction with "perfect", and requested continuation.
Approval covers the views shown, not newly generated directions or idle loops.
[PHASE3_HUMAN_REVIEW.json](PHASE3_HUMAN_REVIEW.json) records scope and frozen hashes.

## Approved references and correction

Canonical PixelLab characters remain male
`79b48db4-483d-4fe3-94bd-74bcec89eab9` and female
`5cd54c3f-d34a-4f02-83ca-0e9a21966fe5`. Standing views are S/N/E plus mirrored
East for West. South walks have eight frames at 100ms, from the existing
“Walk restrained v2” Skeleton v3 groups. Original generation arguments, source
URLs and frame hashes remain in the base/walking manifests.

Male South display frame 6 (index 5) had a doubled outline on the model's right
forearm / image-left. A free, guarded PixelLab pixel edit changed nine inner-outline
pixels to adjacent skin colors, preserving the silhouette. Full-atlas RGBA
comparison verified exactly those changes. The approved corrected atlas is
`docs/ReviewAssets/male-walk-south-frame-6-arm-fix.png`; raw sources and legacy
atlas remain unchanged. Exact edits, PixelLab result ID and hashes are in the review
JSON. “Show original male walk” allows comparison without changing the active source.

## Native East walk generation

Two jobs extended the existing Skeleton v3 motion groups, East only:

| Body | Motion group | Job |
| --- | --- | --- |
| Male | a6783f36-96da-47ca-98cf-9111a465f555 | f48b9ecf-c388-4cda-b491-f291f64449b2 |
| Female | 427b11f0-b2f3-40fb-aee5-74fb541f4783 | 12e5602f-c31a-4754-a00a-801685f43570 |

Both completed: eight distinct 80x80 frames, 100ms each. Cost: three generation
units per body, six total. No paid retries. The first male submission was rejected
before queuing because the tool requires `action_description` even when extending
a group; the corrected request included the existing motion description.

Exact arguments, receipts, output URLs, hashes and numeric measurements are in
[the generation record](ReviewAssets/east-walk-generation.json). Free PixelLab
`join --as sheet --columns 8` preserved all frames as two raw 640x80 PNG strips.
No scaling, frame duplication or West generation occurred. PixelLab changed the
group display label to “custom-relaxed small-step walk”; original group IDs and
existing frame URLs remained the same.

The new source foot plane is one pixel lower for male and two for female than
the legacy source anchor. One fixed crop per body/clip maps source feet (40,68)
and (40,69), respectively, to (24,58): male crop (16,10,48,64), female
(16,11,48,64). No opaque pixels are discarded and no individual frame is recentered.
All cropped foot bounds end at y=56 or 57. Original generated body height was
51–52 pixels versus standing's 50; the female correction below addresses this.

## Review artifact

### Eye and eyebrow correction

The user reported eye/brow defects in male side-walk frames 2–3 and every female
frame. Inspection also found the same partial-eye defect in male frame 4; its
inclusion was disclosed. Approved standing East eye/brow pixels were restored
at the existing head offsets. Female interior eye bars were cleared while the
outer face contour was retained. This used free guarded PixelLab pixel edits,
not new animation generation.

Separate `male-east-walk-eyes-fixed.png` and `female-east-walk-eyes-fixed.png`
review candidates preserve the originals. Exact changes and reference offsets
are in [east-face-correction.json](ReviewAssets/east-face-correction.json).
Full-strip comparison verified nine changed male pixels across frames 2–4 and
138 female pixels across all eight frames; every other RGBA pixel is unchanged.
Alpha, dimensions, head/body motion, foot alignment and timing are unchanged.
All corrected head crops were inspected at 8x, and all eleven review file hashes
matched. New face corrections remain pending user approval.

### Female standing-to-walk height correction

The user reported that the female grows when starting to walk. The side walk's
feet were already aligned; its body was two pixels too long. Free PixelLab exact
copy operations shorten the midriff by two native rows in each frame separately,
moving the existing upper body down intact without scaling or resampling. The
repaired face is preserved, and source rows 45–79, including lower legs and feet,
remain pixel-identical. Frame order, 100ms timing, crop and pivot are unchanged.

The separate `female-east-walk-height-fixed.png` candidate measures 49–50 visible
pixels, matching the 50-pixel standing reference with one pixel of gait variation.
Full-strip pixel comparison verified the exact transformation in all eight frames,
binary transparency and complete crop coverage. All twelve review file hashes
match. [female-height-correction.json](ReviewAssets/female-height-correction.json)
records the operations, bounds and provenance. Originals remain intact; this
correction awaits user visual acceptance. No paid generation was needed.

### Female head width correction

The user subsequently reported outward head growth. The generated head extended
one pixel farther forward and its silhouette varied by frame. Exact guarded
PixelLab patches restore the approved standing East head (x=16..31, y=8..22)
at each existing gait offset. This preserves the one-pixel head bob and eliminates
head shape drift without scaling. The repaired eye region is pixel-identical;
every pixel outside the head region, including the height repair and feet, is
unchanged. All eight head crops were inspected. Full-strip numeric comparison
verified the reference match, unchanged surrounding pixels and binary alpha.

The separate `female-east-walk-head-fixed.png` is the current review candidate;
[female-head-correction.json](ReviewAssets/female-head-correction.json) records
the exact patches and provenance. Previous candidates remain intact. East and
mirrored West share the correction; visual acceptance remains pending.

### Male side stride and return-to-standing preview

The user clarified that both walk loops are smooth, but returning to standing is
abrupt. The male East/West stride also spreads the legs too far during continuous
walking. These are separate issues; no female regeneration was performed.

The comparison panel previously cut to standing every 1000ms while the 800ms walk
clock continued independently, so transitions could interrupt any gait phase.
Its local timeline now starts at frame 1, plays two complete cycles, returns to
frame 1 for 100ms, and then stands. The continuous walk panels retain their
original eight-frame clock. Pause/step/scrub respect the comparison timeline.
This fixes preview scheduling; it does not implement runtime stop animations or
prove the artwork's final settle looks smooth. User review remains necessary.

One new male East-only Skeleton v3 pilot uses explicit short foot paths, alternating
support and swing, and restrained arm movement. It costs three generations, with
no paid retries. West mirrors East. The canonical head/face was restored at the
generated head offset with 65 guarded pixel changes; every other raw pilot pixel
is unchanged. This retains the established facial design while allowing the new
gait. All eight source frames were inspected. Dimensions, binary alpha, eight
unique frames, crop coverage and exact head restoration passed numeric checks.
The fixed crop remains (16,10,48,64), pivot (24,58), timing 100ms per frame.

[male-compact-walk.json](ReviewAssets/male-compact-walk.json) records the poses,
receipt, sources, head patches and validation. The new candidate is
`male-east-walk-compact.png`; `male-east-walk-compact-source.png` preserves the raw
generation. Both stay outside production Content. The previous male side walk
is available through “Show previous male side stride”. This candidate has not
received visual acceptance. The game, female artwork and Sprout are unchanged.

### Forward gait, head bob and side-view settling correction

The user rejected the timing-only return to standing and reported missing male
East/North head bob and a backward-looking side gait. They confirmed the snapping
concern is East/West. The male compact foot paths had the support/recovery phases
reversed: the grounded foot advanced while the lifted foot moved back. The exact
frame order is now 1,8,7,6,5,4,3,2, preserving each pose rather than regenerating
the walk. A one-pixel head/neck dip occurs on East frames 3,4,7,8 and North frames
1,2,5,6. Lower-body pixels remain unchanged within each source frame; head pixels
are translated intact, without scaling. West mirrors the complete East frame.

Both side transitions now use real intermediate artwork. Two `animate_image`
endpoint-interpolation jobs (one generation each) were constrained by exact walk
frame 1 and standing East references. These are stop clips, not regenerated walk
cycles. Raw generated frames 2/3 added an unwanted step and proportion drift and
are excluded. The three-pose review strip contains the exact walk pose, generated
frame 4 with the canonical head and palette restored, then the exact standing
pose. Original generated strips remain separate. No paid retries were used.

The preview plays standing, the bridge in reverse, two full forward walk cycles,
then the bridge into standing. Both splice endpoints were verified pixel-for-pixel
against the current walk and canonical standing assets. All three poses retain
binary transparency, canonical colors and feet ending at y=57. North/South keep
their existing cycle-aligned transition; only the requested side transition gains
this strip. Comparison checkboxes affect continuous walks/contact sheets, while
transition panels always use current candidates to preserve matching endpoints.

[walk-settle-correction.json](ReviewAssets/walk-settle-correction.json) records
the exact permutation, bob operations, generation inputs, selected poses and
numeric checks. The HTML timeline/source-selection checks pass, including
start/stop splices and West reflection. Static crops were inspected; the game and
browser animation were not launched. Smoothness and forward gait still require
user visual acceptance. These corrections remain outside production Content.

The preview defaults to the female head correction and male forward/bobbing walk for
East and reflected West, with “Show original side walks (before fixes)” for
comparison. The South arm comparison remains independent. No production assets
or Sprout files changed.

Open [tools/player-base-review.html](../tools/player-base-review.html) in a browser
from this checkout. It reads local PNGs without a server or network:

- Approved standing views and corrected South walks for both bodies.
- Existing North candidates, newly generated East, and reflected East for West.
  Historical West-to-East copies are never selected.
- One shared 100ms continuous-walk clock, with a cycle-aligned comparison timeline;
  pause/next-frame/slider, native/2x/4x display and foot guides.
- A direction selector for standing/walking transitions and eight-frame contact strips.
- Explicit distinction between approved South and unapproved North/East/West motion.

Standing remains a single reference pose, not a completed animated idle. Paper-doll
underwear is intentional; clothing requires independently fitted male/female art.
No new C# command, dependency, project, PowerShell script or runtime behavior was
introduced. New sources remain review assets outside production Content.

## Verification and limits

The earlier checkpoint built with zero warnings/errors and passed 51/51 regressions,
plus base/walk validation. The arm repair passed an exact nine-pixel comparison.
This continuation verified both new strips' dimensions, eight unique frame hashes,
binary alpha, nonempty bodies, palette counts, fixed-crop coverage and bounds.
Every old reference/source hash still matches. Sprout is untouched.

The preview script passed Node syntax checking and a nonvisual DOM/canvas stub
check for both-body loading, source rectangles and West reflection. The agent
inspected PixelLab source contact sheets; it did not run the game or visually
test browser animation. Numeric checks and static contact sheets do not approve
loop closure, gait or perceived proportions. User review of both bodies'
North/East/mirrored-West walks is the next checkpoint.

## Remaining Phase 3 work

After motion acceptance, produce small animated idle pilots from the frozen
references, review them, then extend to the other authored directions. Finally
pack approved S/N/E idle/walk assets and connect them to the existing renderer
and shared clock. Keep diagnostic gear off real bodies until Phase 4 fitted
overlays are ready. Approval of this walk checkpoint does not approve future
idle art, clothing fit or in-game motion.
