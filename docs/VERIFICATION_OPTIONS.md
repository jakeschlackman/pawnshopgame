# Options menu verification

### Restore Defaults applies to the current tab - 2026-11-08

Restore Defaults now changes only the active tab's draft: Display resets window
mode/resolution; Audio resets all six volumes and mute when unfocused; Comfort
resets pause when unfocused, screen shake and dialogue speed; Controls resets all
primary/alternate keyboard and mouse bindings. Other tabs' saved values and pending
edits stay intact. Hidden legacy UI scale and version markers are preserved.
Nothing is saved until Apply; display changes still require confirmation. The
dialogue preview restarts only when Comfort is reset. Tab, scroll and Sprout's
setting anchor remain in place. Headless checks cover every tab with custom saved
settings and pending edits on all tabs, plus footer clicks, Apply/preview behavior
and selector stability. Manual interaction checks remain for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 68/68.
`git diff --check` passed. No game was launched.

### Unapplied changes confirmation - 2026-11-08

OptionsModel exposes value-based dirty state (Draft != Current). Footer Back and
the cancel binding now request a close: clean drafts leave immediately, while
changed drafts open Unapplied Changes. The existing resolution-confirmation
frame, gradient, fonts, closed buttons and rectangles are reused. Keep Editing
is selected by default; Enter selects the focused choice, Escape/right-click keeps
editing, and keyboard directions or mouse can choose Discard Changes. Only an
explicit discard restores Current and allows leaving. There are no writes or
automatic applies. Failed saves remain dirty; edits changed back to their current
values count as clean. Restore Defaults follows the same actual-value comparison.

The modal captures tabs, selections, sliders, scrollbars, wheel and background
clicks. Focus loss keeps it open without losing the draft. Dropdown/key-capture
cancel remains local, and display-preview cancel/timeout still reverts the preview
before a later close can open the dirty confirmation. Sprout is hidden behind
either modal and starts his main-menu return only once discard/clean close succeeds.
New headless checks cover all tabs, pointer mapping at three saved UI scales and
four outputs, safe defaults, focus/input capture, save failures, clean/reverted
values, preview interaction and flight ordering. Older discard tests now confirm
the prompt explicitly. In-game appearance/input feel remain for the user; no
game was launched or controlled.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 66/66.
`git diff --check` passed.

### Comfort option order - 2026-11-08

Comfort now lists Pause When Unfocused, Screen Shake, then Dialogue Text Speed.
Checkbox rendering/hit testing and keyboard adjustment follow the reordered rows;
the dialogue dropdown is keyed to the last row. Preview, defaults and persistence
are unchanged. Existing Comfort checks cover the new order, toggles, dropdown
selection/capture and popup containment at each saved UI scale/output size.
No game was launched; in-game placement remains for the user's check.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 65/65.
`git diff --check` passed.

### Minimum display resolution - 2026-11-08

Removed 960x540 from the resolution presets; 1280x720 is the minimum. Keyboard
cycling and dropdown choices use the same list. Direct validation rejects lower
settings, and the host checks actual dimensions before switching, including
borderless desktop dimensions. Manual resizing remains disabled. Earlier saved
960x540 preferences load at 1280x720 with their other values and mode retained;
the source file is untouched until Apply. Focused checks cover minimum choices,
wraparound, validation and old-file migration/backup behavior. The existing
smaller-canvas pointer math tests remain as robustness checks, not offered presets.
Manual display-mode/window checks remain with the user; no game was launched.

Verification, including larger modal subtext: `dotnet build PawnShopSimulator.sln
--no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/`
passed with zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.
`git diff --check` passed.

### Larger confirmation subtext - 2026-11-08

The confirmation countdown and keyboard hint now use OptionsLabel at logical
height 26 (previously OptionsHelp at 20). The heading stays at 36 and button
labels at 26. Existing Pixeloid font assets, layout and behavior are retained;
other help text is unchanged. No game was launched; visual sizing remains manual.

### Detached confirmation actions restored - 2026-11-08

Keep/Revert now reuse the original complete four-state walnut/brass binding art,
restoring their own bottom borders. They are raised 16 logical pixels off the
modal's lower rail; widths, horizontal positions, labels and fonts are unchanged.
The new brown gradient and ivy frame remain. Open-bottom source/crops are retained
as earlier artwork but are no longer loaded by OptionsScreen. Shared draw/hit
rectangles follow the raised buttons, and the existing display-preview check now
requires a visible gap above the rail. No new artwork or framework was needed.
In-game appearance remains for the user; no game was launched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.
`git diff --check` passed.

### Faint brown gradient inset - 2026-11-08

Generated a borderless brown gradient/fiber surface through PixelLab UI,
id `8cf12ab6-1d0f-4591-96b6-980037388499`, seed 72801. The source is
Content/UI/Sources/modal-brown-gradient.png (192x336); the runtime asset at
Content/UI/modal-brown-gradient.png is an unscaled [0,21,192,293] crop. It moves
from muted cocoa near the top to darker chocolate below, with faint pixel fibers
and low color variation across each row. No frame, text or controls are baked in.
The 293px native height matches the existing inner surface; only horizontal
repetition is used, avoiding vertical gradient restarts or grain stretching.
Nonvisual checks confirmed full opacity, and static source artwork was inspected.
The ivy frame, typography, new open-bottom action art and confirmation behavior
are retained. In-game texture subtlety/join acceptance remain for the user;
no game was launched. Asset sources, crops and hashes are recorded in provenance.

Verification for the gradient and open-bottom actions: `dotnet build
PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
-p:OutputPath=bin/NotJamVerification/` passed with zero warnings/errors.
`dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
-p:OutputPath=bin/NotJamVerification/` passed 64/64, including copied artwork
dimensions/hashes and modal Keep/Revert hit mapping. `git diff --check` passed.

### Open-bottom Keep/Revert actions - 2026-11-08

Regenerated one shared four-state set through PixelLab UI `create_ui_asset`, id
`0f3f927e-544d-454a-997e-4e203af384f3`, seed 72691, using ivywood-confirmation.png
as reference. Original source: Content/UI/Sources/modal-open-action-states.png
(256x256). Runtime sprites are modal-action-{inactive,selected,hovered,pressed}.png,
193x42 crops [32,16+60*state,193,42]. PixelLab retained a residual lower outline;
the final seven rows were omitted so the face/side posts end cleanly without
lower trim or transparent padding. The original sheet is preserved. A nonvisual
alpha check confirmed fully opaque bottom rows for all four crops.

The new sprites render at native scale with six-pixel nine-slices, matching the
modal frame's finer pixel density. Button bottoms meet the frame's upper gold
edge (23px from its bottom), covering the prior one-pixel shadow gap. The rail
provides their shared lower border. Font, labels, dimensions, horizontal positions,
confirmation/rollback and settings behavior remain intact. Only the hovered
modal button shows a pressed state; selected Keep does not depress with Revert.
Source/crops and their native edges were inspected as artwork. No game was
launched; the updated in-game join remains for the user to review.

### Ivywood confirmation frame and docked actions - 2026-11-08

Generated a new frame through PixelLab UI `create_ui_asset`, id
`feb45b1c-613e-468c-8175-b429da21ed2b`, seed 72581, using the earlier
lanternwood-panel.png as style reference. The artwork has warm reddish wood,
golden brass, small ivy clusters and restrained dark corner scrollwork. Untouched
336x192 source: Content/UI/Sources/ivywood-confirmation.png. Runtime frame:
Content/UI/ivywood-confirmation.png, an exact unscaled crop [41,24,254,143].
Provenance records source/crop dimensions, hashes and rendering choices.

PixelLab removed the requested opaque center along with the background; rather
than inventing generated content, the implementation layers the existing PixelLab
dropdown's quiet dark surface underneath. The frame renders with 64px nine-slice
corners at native 1x, retaining the complete ivy clusters and repeating only plain
rail/inner pixels. All scaling uses the existing point sampling. Static source
and crop were inspected; no game was launched or screenshots captured.

The same Pixeloid family now uses OptionsHeading 36 for the title and OptionsHelp
20 for secondary text. Keep/Revert retain their existing four-state artwork and
OptionsLabel 26 font. They sit against the lower rail (24px from frame bottom),
inset 80px to clear ivy; only their width/position changed. Shared draw/hit bounds
and tests cover docking, corner clearance and existing confirmation behavior.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.
`git diff --check` passed. In-game visual acceptance remains with the user.

### Confirmation uses the label font role - 2026-11-08

All display-confirmation text now uses OptionsLabel, including its title,
countdown, keyboard hint and Keep/Revert labels. Measurement and drawing both
resolve that catalog role (Pixeloid Sans Bold, logical height 26), matching the
inner option labels. Existing dialog art, bounds and behavior are retained.
In-game typography remains for the user's check; no game was launched.

The existing separate-output build command (`bin/NotJamVerification/`, documented
below) passed with zero warnings/errors; its regression runner passed 64/64.
`git diff --check` passed.

### Display confirmation styling and quiet status - 2026-11-08

Replaced the old full-frame redraw/parchment confirmation with a centered 816x340
walnut/brass modal, using the existing PixelLab dropdown-panel nine-slice and
four-state binding buttons. Cream Pixeloid heading/body/help, centered countdown
and title-case Keep/Revert labels reuse existing font roles. The main Options
view is dimmed; existing panel art and all underlying controls retain their layout.
Draw/hit bounds share DisplayConfirmation/Keep/Revert geometry, and pointer
hover/pressed feedback is active. No new artwork/framework/dependency was needed.

Successful Apply/Keep and ordinary cancellation, timeout or focus-loss rollback
no longer produce bottom status text. Display-device/save failures still show
their error messages. Countdown, persistence and rollback semantics are unchanged.
Expanded existing display-preview checks cover both modal buttons at three UI
scales/four output sizes, blocked underlying controls, hover mapping, quiet normal
completion and retained failure messages. Static reused art was inspected; no
game was launched. In-game visual balance remains for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.

### Binding value capitalization - 2026-11-08

Controls' Primary/Alternate values now use title case for multi-character names:
Left Click, Right Click, Enter, Escape, Unbound and Press Input. Single-letter
keys remain uppercase. Compound key enum names separate into words (Left Shift,
Num Pad 8); function keys retain forms such as F12. Action/column labels, font,
layout, bindings and persistence are unchanged. Existing regression checks cover
the displayed labels; in-game appearance remains for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.
`git diff --check` passed; no game was launched.

### Mouse alternates and action labels - 2026-11-08

Controls rows now read Interact / Attack and Back / Block, with Z/Left Click and
X/Right Click defaults. Slot art/layout/fonts are unchanged; alternate labels
display LEFT CLICK or RIGHT CLICK and capture prompts say PRESS INPUT. Existing
KeyboardBindings/KeyBinding carries optional MouseBinding data; the action input
adapter merges keyboard/mouse states before edge detection while UI hit testing
continues to own mouse confirmation. Capture waits for activation input release,
accepts mouse only in action alternates and validates conflicts. Clearing or
assigning a keyboard alternate clears its mouse assignment. Right Click drives
existing Back/pause behavior; Left Click drives existing world interaction.
This request changes labels/bindings, not combat mechanics.

Settings defaults marker 2 upgrades earlier default Enter/Escape alternates in
memory, preserving custom mappings and deliberate clears. Current key/mouse
fields round-trip through the existing settings file. No production settings
were touched and no game was launched. In-game feel/appearance remain manual.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 64/64.

### PickersHollow settings folder - 2026-11-08

The host now reads/writes only `%LOCALAPPDATA%/PickersHollow/settings.json`.
Per user direction there is no fallback, copy or migration from RummageHollow.
SettingsStore creates default AppSettings JSON on launch when the file is absent,
using the existing directory creation and safe-write flow. If creation fails,
defaults are used in memory with a warning; Apply can retry saving. Existing files
are not rewritten on load. No actual
user settings files were read, moved or modified during implementation. Existing
headless persistence checks cover absent-directory/file creation, default JSON
reload, existing-file preservation, creation failure and first-Apply behavior.
Manual launch confirmation remains with the user; no game was launched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 63/63.
`git diff --check` passed.

### Existing unbound alternate defaults - 2026-11-08

Corrected the preceding default change: old saved Z/X bindings with explicit
None alternates now adopt Enter/Escape in memory on load. Missing defaults still
check for conflicts, and custom mappings are retained. The additive
ActionAlternateDefaultsVersion marker distinguishes new saves, so clearing an
alternate after upgrading remains persistent. The original preferences are never
rewritten on load; Apply uses the existing temporary-file/backup flow. Tests use
isolated old-style settings, assert the Controls model shows ENTER/ESCAPE, and
check unrelated values, custom mappings and post-upgrade clears. Restart the
updated game to load the migration; no game or production preferences were touched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 63/63.
`git diff --check` passed. In-game confirmation remains with the user.

### Controls slot artwork and alternate defaults - 2026-11-08

Removed the Controls Left/Right/rebind paragraph and its three-line scroll tail;
capture, conflict, save and error messages remain. Primary and Alternate share
one new four-state walnut/brass slot-button set generated with PixelLab's
`create_ui_asset`, id `30ec32db-4095-45d0-b519-701961e09da1`, seed 72471.
The existing selected header tab supplied the style reference. The untouched
transparent 256x256 sheet is `Content/UI/Sources/walnut-binding-states.png`;
four aligned native 110x28 crops are `binding-{inactive,selected,hovered,pressed}.png`.
They reuse NineSlice with six-pixel corners at integer 2x, keeping slot bounds,
fonts, labels and capture semantics. Per-slot hover and capturing feedback use
the same set for both columns. Provenance records dimensions, crops and hashes.
Static generated artwork was inspected; no game was launched or controlled.

AppSettings and KeyboardBindings now default Interact to Z/Enter and Pause /
Back to X/Escape. Restore Defaults uses these pairs. Loading missing alternate
fields supplies the defaults unless an existing binding conflicts; explicit
custom/unbound alternates and original files remain unchanged. Settings version
and write mechanics are unchanged. Tests cover default action/edge merging,
restore defaults, custom/unbound round trips, missing-field conflicts and art
hash/dimension/output checks. In-game appearance and input feel remain for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 63/63.
The clipping check now targets the hidden portion of the partly visible first
row after removal of the help tail. Source artwork stays outside packaged runtime
assets; only the four crops are copied. `git diff --check` passed.

### Screen Shake preference - 2026-11-08

Replaced Replay Text Preview with Screen Shake on Comfort. It reuses the existing
gold-checked / steel-unchecked checkbox art and row hit geometry; label/checkbox
clicks and keyboard confirmation/Left/Right toggle it. It defaults to true and
persists via the existing AppSettings/Apply flow; missing fields in older files
receive true without rewriting them on load. Back discards drafts and Restore
Defaults enables it again. Dialogue speed still automatically restarts the sample.
There is no implemented screen-shake effect to connect yet; this is the saved
preference for future effects. No camera or rendering framework was introduced.
Regression checks cover row replacement, pointer/keyboard toggles, Apply/Back,
defaults, false-value round trips and default-on old-file compatibility. No game
was launched; in-game checkbox appearance remains for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 63/63.
`git diff --check` passed.

### Comfort checkbox and speed dropdown - 2026-11-08

Pause When Unfocused now reuses Audio's 30x30 gold-checked / steel-unchecked
checkbox states and row-based draw/hit geometry. Clicking its label or checkbox
toggles the draft; empty value-column space only selects it. Keyboard toggling
is retained. Dialogue Text Speed reuses the existing dropdown artwork, bounded
popup and input handling with Slow, Normal, Fast and Instant choices. Confirming
a choice changes only the draft and resets the existing text preview. Direct
Left/Right cycling, Replay Text Preview, Apply/Back/defaults, fonts and persistence
are retained. Popup choices now resolve by tab and row so Display is unchanged.
Focused headless checks cover checkbox/label/whitespace clicks, all speed values,
preview reset, draft commit/cancel/defaults, popup dismissal/focus capture,
scroll routing, and dropdown bounds at saved UI scales and four output sizes.
No game was launched; in-game appearance/input checks remain for the user.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 63/63, including the new
Comfort controls regression check. `git diff --check` passed.

### Combat and Monster volume settings - 2026-11-08

Added Combat Audio and Monster Audio after Sound Effects, reusing the existing
watch-slider artwork and input path. Mute When Unfocused follows all six volume
rows; the Audio body now scrolls at all supported saved UI scales. Both new
volumes default to 80, validate within 0-100, request previews through distinct
channels and persist through the existing settings store. Their gain is Master
times their own volume, independent of Sound Effects, respecting focus muting.
Old version-1 files missing these fields receive defaults without being rewritten
on load. No settings version or save-storage mechanics changed. Authored combat
and monster sounds remain future work. Tests cover slider/keyboard input,
channel previews/gain, scrolling, mute-row mapping, Apply/Back/defaults, validation,
round trips and old-file compatibility. In-game visual/audio checks remain with
the user; no game was launched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed all 62 checks.
`git diff --check` passed.

### Audio help removal - 2026-11-08

Removed the Audio tab's preview-tone/instruction paragraph and its reserved
two-line space. Audio controls, preview tones and status/error messages retain
their behavior. At 100% UI scale the five Audio options now fit without a scrollbar;
larger saved UI scales still scroll as needed. Header/footer layout is unchanged.
In-game appearance remains for the user to check; no game was launched.

Verification: `dotnet build PawnShopSimulator.sln --no-restore -m:1
-p:UseSharedCompilation=false -p:OutputPath=bin/NotJamVerification/` passed with
zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj
--no-build -p:OutputPath=bin/NotJamVerification/` passed 62/62, including updated
Audio overflow expectations. `git diff --check` passed.

### Walnut rail and antique compass — 2026-11-08

Replaced the main scrollbar's metal-heavy surface with rich brown walnut grain,
thin worn brass edging/end caps and restrained ivy. The pocket-watch knob is now
a vintage brass navigation compass with a parchment rose and red/cream needle,
with no winding crown or clock hands. Original artwork was generated through
PixelLab UI using the earlier scrollbar sheet as a style reference: id
`3cd352ab-153f-471e-92a7-15e3a85a4133`, seed 72361. The untouched 256x256 source
is retained at `Content/UI/Sources/walnut-compass-scrollbar.png`.

Native crops are `scroll-walnut-rail.png` (40x128) and four `scroll-compass-*`
states (40x52). Bounds, hashes and active skin metadata are recorded in
provenance.json. Earlier pocket-watch assets remain preserved. The fixed knob
size, track location, scrolling/hit geometry, visibility rules, clipping, audio
wristwatches, fonts, checkbox and dropdown scrollbars are unchanged. Original
pixels render without interpolation; the existing fixed-end-cap/tiled-shaft
renderer is reused. Static source art was inspected; in-game appearance remains
for the user, and no game was launched or controlled.

Alignment follow-up: the compass thumb is offset two logical pixels right of
the rail canvas. Its draw/hover/drag bounds move together; the rail stays fixed.
The exposed right edge still accepts dragging and wheel input. Separate-output
build passed and 62/62 tests passed, including the shifted thumb's right edge.

Size follow-up: the original 40x52 compass source now renders at 32x42 with point
sampling. Its horizontal center retains the two-pixel alignment offset; rail
geometry is unchanged and vertical travel still reaches both scroll endpoints.
Build and all 62 checks passed after resizing. Visual acceptance remains manual.

End-cap clearance follow-up: ThumbTravel reserves 12 logical pixels at each
rail end, reduced from 18 after the user's top/bottom reference shots showed
excessive clearance. The compass travels six pixels closer to each cap.
Compass placement and drag mapping
share that inset area, retaining both scroll endpoints without covering either
cap. User-supplied top/bottom shots were read as references; the updated game was
not launched. Build and all 62 checks passed, including cap clearance, a stable
grab at the bottom position and dragging to both endpoints. Appearance remains
for the user's in-game check.

### Rustic pocket-watch vertical scrollbar — 2026-11-08

The main Options scrollbar now uses original PixelLab UI artwork generated from
the user's slim vertical metal/brass reference: a weathered rail, warm brass end
caps, sparse olive ivy and a rustic round pocket watch with a winding crown and
hanging loop. No wrist straps appear on this thumb. Generation id:
`5f75d09e-784f-48a2-9c90-f5217cd93797`, seed 72251. The complete 256x256 sheet is
preserved as `Content/UI/Sources/rustic-pocketwatch-scrollbar.png`. Native crops:
`scroll-rail.png` (40x120) and four `scroll-watch-*` states (40x52). Inactive,
hovered and pressed states are used; selected artwork is retained for reuse.
Hashes and exact source bounds are in provenance.json.

The 40-pixel track canvas includes the ivy and transparent side margins. A
16-pixel nine-slice border preserves both rail end caps; only shaft pixels repeat
to fit the existing viewport height. The watch renders at native size with point
sampling, retaining its round shape rather than stretching proportionally with
the amount of content. Watch travel maps top/bottom to scroll offsets zero/max.
The wider rail remains clear of the clipped content and footer. Drag, rail click,
wheel, keyboard reveal and conditional visibility remain supported; wheel input
also works directly over the rail. Hover is cleared on focus loss. Fonts, footer,
checkbox, audio sliders and dropdown scrollbar artwork are unchanged.

The source sheet was inspected as static artwork. Separate-output solution build
passed with zero warnings/errors; the regression runner passed 62/62, covering
fixed watch size, endpoints, track containment, dragging, rail wheel input and
focus-safe hover alongside the existing clipping/settings checks. In-game scale,
ivy placement and dragging feel remain for the user; no game was launched.

### Gold/steel mute checkbox — 2026-11-08

Replaced the Audio Mute When Unfocused ON/OFF button with original PixelLab UI
checkbox artwork using the user's example as its style reference. Checked has a
gold/brass frame and cream tick; unchecked has an empty dark inset and cool steel
silver-gray frame. Hover states preserve each material. Generation id:
`78dbcf53-66d7-4094-95ef-350beb9685a8`, seed 72141. The complete 192x192 source is
preserved in `Content/UI/Sources/gold-steel-checkbox-states.png`; four identical
44x44 native crop windows become `checkbox-gold-checked[-hovered].png` and
`checkbox-steel-unchecked[-hovered].png`. No interpolation or corner stretching
is used. Bounds, hashes and state semantics are in UI/provenance.json.

The checkbox renders at 30x30 with point sampling, centered below the audio
tracks at the user's marked position. Original 44x44 source sprites are retained.
Click its box or label to
toggle; blank value-column space selects without toggling. Enter and Left/Right
retain the existing bool toggle behavior. State remains a draft until Apply;
Back discards it. Sprout remains the row indicator; no ON/OFF text is drawn over
the checkbox. Font settings, sliders, panel, tabs, footer and persistence format
are unchanged.

Source artwork was inspected for the gold tick and empty steel states. Headless
checks cover checkbox containment, hit actions, keyboard toggles, draft/apply/
cancel and existing volume behavior. In-game size, hover and placement remain
for the user's check; no game was launched or controlled.

### Wristwatch Audio sliders — 2026-11-08

PixelLab's UI-specific `create_ui_asset` generated a coherent slider sheet using
the user's supplied horizontal green/brass bar as its reference. Generation id:
`db33f839-e067-4564-bcd2-791b0a9764a1`, seed 72031. Original source:
`Content/UI/Sources/wristwatch-audio-slider.png` (256x256). Runtime assets are
`audio-track.png` (240x14), `audio-fill.png` (240x6), and four `audio-watch-*`
states (32x40). The thumb is an analog wristwatch with worn leather straps, a
brass case, ivory face and hands. The unused larger duplicate watches remain in
the source only. Source bounds, hashes, state order and pivot are in provenance.

Master, Music, Ambient and Sound Effects now draw a slim brass-edged dark track,
green fill proportional to draft volume, and the wristwatch at the fill endpoint.
Native pixels render at scale one with point sampling; frame ends are fixed and
the flat center repeats. Watch states share canvas/pivot (14,20). No labels are
baked into art. Both vertical scrollbars, Mute When Unfocused, fonts, tabs, panel
and footer retain their prior visuals and layout.

Track clicks and watch dragging request clamped 0-100 draft values. A grab offset
keeps the watch from jumping when picked up away from its dial center. Pointer
capture ends on release, focus loss, modal state or leaving Audio; clipped rows
cannot start a drag. Left/Right still changes five points. Preview tones only
restart when pointer volume changes; Apply and Back retain commit/discard rules.

Separate-output solution/content build passed with zero warnings/errors. The
runner passed 62/62, including all four channels, endpoints, unchanged values,
keyboard steps, draft/apply/cancel, focus-safe dragging, grab offset, clipped
input and scrolled coordinate mapping at four output sizes. Artwork was inspected
as static source assets only. In-game size, green fill appearance and dragging/
audio feel remain for the user; no game was launched or controlled.

### Display dropdowns — 2026-11-08

Window Mode and Resolution now open dropdown lists using original PixelLab UI
art (`create_ui_asset`, id `da7943f2-ccf3-4c4e-a409-64627188ef1b`, seed 71921).
The existing selected header tab supplied the style reference. The transparent
256x256 source sheet is preserved at `Content/UI/Sources/walnut-dropdown-states.png`.
Seven runtime assets provide inactive/selected/hovered/pressed buttons (112x32),
list surface (176x152), row highlight (56x32), and chevron (15x15). Source bounds,
hashes, native crop/repeat normalization and slice borders are in UI/provenance.json.
All states align; corners retain their authored pixels at scale two, with point
sampling and no embedded text. Static source art and PNG dimensions/alpha were
checked; game rendering was not inspected.

Click or Enter opens a list; arrows move its candidate, Enter/click accepts, and
Escape or outside click dismisses without activating controls underneath. The
resolution list supports wheel scrolling and a draggable scrollbar. Left/Right
still cycles values when closed. Lists fit above or below their buttons within
the existing clipped body, and preserve the surrounding panel/tabs/footer bounds.
Focus loss and tab changes close the list. Selecting only changes the draft;
Apply, display preview/timeout/rollback and persistence follow their existing path.
Sprout stays with the option row, including while selecting a list value.

Separate-output solution build: passed, zero warnings/errors. Dependency-free
regression runner: 61/61 passed. Coverage includes cancellation, explicit choice
validation, draft/preview boundaries, candidate navigation, focus/tab dismissal,
overlay hit priority, popup containment at 100/125/150 percent, wheel/drag behavior,
pointer mapping across four output sizes, and generated asset hashes/dimensions.
In-game appearance, hover/pressed feel and list readability remain for the user.

### Inset body and Sprout focus — 2026-11-08

Inset the inner body by 48 logical pixels on each side, including its scroll track.
Labels, value buttons, both binding columns, sliders, dividers and help now derive
their horizontal positions from the shared viewport. The panel, header tabs,
footer buttons and extended viewport bottom retain their previous geometry.
Removed the row's `>` marker. OptionsModel tracks the last selected option row
separately from focus on footer actions, so Sprout stays beside that option when
Apply / Restore Defaults / Back receives pointer or keyboard focus. Selecting a
new option updates him; changing tabs resets him to the first option as before.

Automated coverage checks inset boundaries, value/binding containment, keyboard
indicator movement and retained indicator rows through all three footer clicks on
every tab. In-game spacing and Sprout placement remain manual checks for the user.
The separate `bin/UiVerification/` build passed with zero warnings/errors, the
regression runner passed 60/60, and `git diff --check` passed. No game was launched.

### Extended viewport and conditional scrollbar — 2026-11-08

The clipped options viewport and scrollbar now end eight logical pixels above the
fixed footer buttons. Both track and thumb render and accept input only when the
current content exceeds that viewport. Replaced the old 180-pixel trailing space
with the help text's line budget (one Comfort line; up to three for other tabs).
Display and Comfort fit at the supported legacy scales; Audio and Controls scroll.
Status messages now follow help inside the clipped content, with reserved space,
so they cannot paint over rows occupying the newly extended viewport.

The separate `bin/UiVerification/` build passed with zero warnings/errors; the
runner passed 60/60, including hidden-track input, lower-viewport wheel input,
per-tab overflow at 100/125/150 percent, footer boundaries and existing scroll/
binding/settings checks. In-game appearance and scrolling feel remain for the user;
no game launch or desktop inspection was performed.

### Options cleanup — 2026-11-08

Removed UI / Text Size from the Display menu and its adjustment path. Display now
contains Window Mode and Resolution; Apply follows those two rows for keyboard
and pointer input. Existing saved UiScale values and persistence remain compatible.
Removed the Preferences and Bindings section captions, keeping the Controls
Primary / Alternate column labels. Footer action labels draw three logical pixels
lower, including their existing pressed offset; button bounds and art are unchanged.
The display regression checks the new field list, keyboard access to Apply, and
retention of a previously saved scale. In-game label alignment remains for the user.
Verification: the normal build was blocked by the running executable's file lock;
the separate `bin/UiVerification/` build passed with zero warnings/errors, its
regression runner passed 60/60, and `git diff --check` passed. No game was launched.

Date: 2026-10-31.

Follow-up: [2026-11-05 dual bindings](VERIFICATION_DUAL_BINDINGS.md) adds movement
rebinding, primary/alternate columns and the Pause / Back label/behavior. The
original evidence below describes the earlier two-action menu.

Implemented application preferences, a four-category options menu on title/pause,
display modes and resolution presets through 3840x2160, display confirmation and
rollback, UI/text scaling, audio preview sliders, focus pause/mute, dialogue speed
preview, and rebinding for the existing Interact/Pause actions. Manual resizing is
disabled. Settings are independent of gameplay saves; no gameplay save format or
dependencies changed. Existing uncommitted agents.md work was retained.

## Automated evidence

- Baseline `dotnet build PawnShopSimulator.sln --no-restore`: passed, zero warnings/errors.
- Baseline `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build`: 10/10 passed.
- Implementation build: passed, zero warnings/errors.
- Implementation regression runner: 16/16 passed. Added checks cover title/pause
  input capture and session preservation; display confirmation, cancellation,
  timeout and focus rollback; isolated temporary settings round trips, backups,
  malformed/unsupported data preservation; key release and duplicate bindings;
  category audio gain and focus mute; dialogue reveal rates; configurable focus
  pause; and failed-write rollback.
- Final build after retaining the original desktop dimensions across fullscreen
  changes: `dotnet build PawnShopSimulator.sln --no-restore -p:OutputPath=bin/OptionsVerification/`
  passed with zero warnings/errors. `dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/OptionsVerification/`
  passed 16/16. Separate ignored output avoided disturbing the launched executable.
- `git diff --check` passed; Git reported only LF/CRLF conversion notices.
- Normal C# compilation builds the preview tone generator and UI; existing PNG
  content is copied by the existing workflow. No new asset compiler is required.

## Desktop evidence and limits

Launched the rebuilt executable with the computer-use skill. Screenshot inspection
confirmed the title's enabled Options entry. Native accessibility inspection
confirmed the Maximize button is disabled. Automated confirm-key attempts did not
produce an observable transition. The user then stopped Computer Use with physical
Escape, and all desktop automation stopped.

The options screen layout, 100/125/150 percent readability, resolution/mode switching,
physical edge-drag behavior, audible previews, runtime rebinding, and settings reload
remain manual acceptance checks. Actual 4K output requires a suitable display and
has not been verified. Headless preview tests do not establish graphics-driver
rollback behavior. No complete M01 desktop/resource acceptance is claimed.

## Pixel-art frame and scrolling foundation ? 2026-11-07

The outer frame now uses PixelLab's dedicated `create_ui_asset` output, generated
with the user's decorative reference as a style image: amber wood, brass curls,
ivy, and a quiet dark inset. Source PNGs and generation IDs/hashes/slice bounds
are recorded in `PawnShop.Game/Content/UI/provenance.json`. The aesthetic applies
only to the outside box. Earlier button/tab/track/surface artwork is provisional;
the user will design controls separately. No asset is visually approved.

Fixed tabs, clipped independently scrolling fields/help, and fixed Apply / Restore
Defaults / Back use shared layout geometry. Wheel input and draggable scrollbar
change a bounded offset. Keyboard navigation reveals rows; tabs reset scroll.
The renderer flushes batches before/after a GPU scissor on the 1280x720 target.
Corners use fixed pixel scale and edges repeat; all Options sampling is point.
Settings values, previews, confirmation, binding capture and persistence retain
the existing command paths. No dependencies or save formats changed.

Baseline build: zero warnings/errors; 57/57 tests passed. The expanded headless
suite passed 58/58, including all tabs at 100/125/150%, scroll clamps and tab reset,
keyboard reveal, clipped hit boundaries, fixed footer, wheel focus suppression,
scrollbar dragging and scrolled pointer mapping at 960x540, 1280x720, 2560x1440
and 3440x1440. Existing settings and lifecycle checks also pass.

Commands (separate ignored output avoids touching a running executable):

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:OutputPath=bin/UiVerification/
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build -p:OutputPath=bin/UiVerification/
git diff --check
```

An earlier draft was launched and one Options screenshot was inspected before
the user instructed the agent to stop. All desktop interaction stopped. The final
reference-inspired frame has not been inspected in-game. Final composition,
readability, input feel, display changes and resolution/scale acceptance remain
for the user. Headless checks establish geometry and behavior, not appearance.
Fractional presentation scales use nearest-neighbor sampling but cannot guarantee
a uniform physical pixel grid. No further game launches or screenshots are needed
for this handoff.

### Panel opacity follow-up

The frame's three authored dark inset colors now render at 75 percent opacity.
Wood, brass, foliage, controls and text remain opaque. The source PNG is preserved;
opacity is applied to the loaded texture once. Removed the opaque parchment and
socket backgrounds from the scrolling viewport so stacked surfaces do not hide
the scenery. Content text now uses cream with a one-pixel dark shadow for contrast.
The display-confirmation prompt retains its opaque paper surface for readability.
Build and headless checks were rerun; in-game appearance remains for the user.

### Background composition correction

Pause-side Options previously had no room drawing callback and used an opaque
canvas clear. It now composites over a freshly drawn, frozen room; title-side
Options continues to composite over the panorama. Options always clears its own
canvas transparently. Frame opacity and premultiplication now run on decoded
RGBA bytes before GPU upload rather than through texture GetData/SetData.
All PNG controls are also premultiplied before AlphaBlend rendering.

A headless check decodes the actual frame and verifies every pixel in its central
160x90 area uploads with alpha 191/255, with premultiplied RGB; opaque decoration
is unchanged and transparent pixels contain zero RGB. It does not assert GPU or
in-game appearance. Standard Debug build (without an alternate OutputPath) passed
with zero warnings/errors, and the rebuilt regression runner passed 59/59.
No game launch, control or screenshot inspection was performed for this fix.

### User screenshot opacity correction

The user-supplied screenshot showed that the 10% fill was too faint against the
sunset. Set the inset to 75% opacity (25% see-through) to retain the dark panel
surface. The pixel regression now checks alpha 191/255 and corresponding
premultiplied RGB. Borders and controls retain their opacity. The screenshot was
supplied by the user; no game interaction or screenshot capture was performed.

### Separate tab header

Added the reference's distinct upper wooden section, using grain and a straight
amber/brass rail sampled from the existing generated PixelLab artwork. Display,
Audio, Comfort and Controls now sit centered in that fixed section. Shared tab
hit bounds moved with the artwork. The Options heading is omitted while the user
decides its placement. Existing button designs remain provisional; content and
footer geometry, 75% inset opacity and settings behavior are retained. Geometry
checks require each tab inside the header and the divider above the viewport.
No game launch or desktop inspection was performed.

Header verification: normal Debug output was locked by the running game. Separate bin/UiVerification build passed with zero warnings/errors; its rebuilt regression runner passed 59/59. Close the game and rebuild normal output before reviewing there. No running process was controlled or stopped.

### Complete source panel replacement

At the user's request, the active panel is now a byte-identical copy of their
1586x992 source image: `Content/UI/options-panel-source.png`. SHA-256:
`77d11e44f365b1998822110ccb5c2ac17ffe3e0ed39a7cbfcb33a73f4c0ad94c`.
It draws as one complete image with point sampling and aspect ratio preserved to
integer rounding. Removed assembled wood/header/divider rendering and the custom
opacity processor. The source's original fill and alpha now apply. Prior source
artwork is preserved and marked superseded in provenance. Controls remain
provisional, not regenerated or redesigned.

Tabs are fitted to the source header; scrolling content, scrollbar, messages and
fixed actions are inset into the narrower source body. Shared mouse geometry
matches. The Options heading remains omitted. The display-confirmation overlay
uses the same complete image and preserves existing confirmation semantics.
The obsolete palette-specific opacity test is replaced by source dimensions,
aspect ratio and contained layout checks; provenance tests verify packaged bytes.
No game launch, control or screenshots were used. Visual acceptance is pending.

Source-panel verification: standard Debug solution build passed with zero warnings/errors; rebuilt runner passed 59/59; git diff --check passed. Source copy hash matches the supplied image. In-game appearance remains unverified.

### Walnut category-tab artwork only

Replaced only Display / Audio / Comfort / Controls artwork with four states
generated by PixelLab's UI-specific `create_ui_asset` tool, ID
`e9814ebf-f1b1-42c7-a0e9-07d1bd7ac23b`, seed 71601. The existing panel image
was provided as the material/style reference. A four-button shape template
produced inactive dark walnut/muted brass, selected honey/brighter brass, warm
hover and recessed pressed states without text or extra ornaments.

The unmodified 256x256 generator output is retained in
`Content/UI/Sources/walnut-tab-states.png`. PixelLab varied the four generated
bounds slightly. Native-pixel nine-slice preparation preserved six-pixel corners
and repeated/cropped edge and center pixels into four aligned 110x28 RGBA PNGs:
`tab-inactive.png`, `tab-selected.png`, `tab-hovered.png`, `tab-pressed.png`.
All four have alpha bounds (0,0)-(109,27), transparent corners and the same
canvas/origin. No interpolation or painted replacement art was used. Original
bounds, source ID and file hashes are recorded in `Content/UI/provenance.json`.

The existing Button drawing method selects these assets only for category tabs,
with pressed > hovered > selected > inactive priority. Each draws at exactly 2x
inside the unchanged 220x56 tab rectangles with point sampling. Existing font,
labels, centering, pressed text offset and semantic input paths are retained.
Footer buttons, values, bindings, sliders, scrollbar, panel and layout are untouched.
No new UI controls, architecture, generation scripts or dependencies were added.
Static generated sprites were inspected; in-game acceptance remains with the user.
No game launch, control or screenshot capture was performed.

Tab-art verification: unchanged hashes for panel source, OptionsMenuLayout, MenuPointerInput, OptionsModel and font catalog. Standard Debug build was locked by the running game; separate bin/UiVerification build passed with zero warnings/errors. Rebuilt regression runner passed 59/59, including packaged asset hashes/dimensions. git diff --check passed. Close the game and rebuild standard output before reviewing there.

Tab artwork size follow-up: reused the existing four sprites with no PixelLab
call. Artwork is now 90% of its prior display size (198x50 after integer rounding),
centered inside unchanged 220x56 hit bounds. Labels, font rendering, panel and
interaction are retained. Point sampling remains enabled. Visual review is for
the user.

Reduced tab artwork by a further 8% of its current size: cumulative scale 82.8%,
182x46 after rounding. Centering, text, hit targets and source sprites are retained.
No PixelLab calls or game interaction.

Tab spacing follow-up: reduced horizontal tab step from 236 to 224 logical pixels, keeping the first tab anchored. This moves Audio 12, Comfort 24 and Controls 36 pixels left. Artwork dimensions, labels and shared drawing/hit bounds are retained.

## Compact footer actions - 2026-11-08

Generated four original text-free walnut/brass states using PixelLab's UI-specific
create_ui_asset tool, ID `e3781e53-c635-4c43-b415-2eb40587340f`, seed 71701, with
the existing selected header tab as the visual reference. The original sheet is
retained in `Content/UI/Sources/walnut-action-states.png`. Source bounds varied
slightly; native-pixel nine-slice preparation preserved four-pixel corners and
repeated/cropped straight edges and center into aligned 64x18 transparent PNGs:
`action-inactive.png`, `action-selected.png`, `action-hovered.png`, and
`action-pressed.png`. All alpha bounds are (0,0)-(63,17); no interpolation.
Generation IDs, hashes and preparation bounds are in UI/provenance.json.

Apply and Back are now 128x36 (previously 180x48); Restore Defaults is 232x36
(previously 352x48). Button centers and the fixed footer position are retained.
Drawing and pointer input share the smaller bounds. The existing Button method
uses the new four states only for footer actions, through the existing nine-slice
renderer at integer 2x source scale. No new controls or framework. Header tabs,
source panel and other controls are unchanged. Footer labels use OptionsAction,
a catalog role sharing the header's Jersey 25 atlas at line height 18, so they
fit the compact controls. Existing font loading and UI scaling are retained.

The new headless footer check clicks Restore Defaults, Apply and Back from each
scrolled tab, checking draft-only restoration, one settings commit and unapplied
edit discard. Existing font-content and provenance checks cover the new role and
assets. The solution build passed with zero warnings/errors; the rebuilt runner
passed 60/60. Source-panel and four header-tab image hashes remain unchanged.
Only static generated sprites were inspected; no game launch, control or
screenshots. In-game composition, label readability and mouse feel await the user.

### Docked footer / orange frame trim

At the user's request, all three footer buttons now end at the source panel's
orange bottom rail (source row 880, logical y=620). Apply is immediately left
of Back: their shared edge is x=934, with no horizontal gap. Restore Defaults
keeps its horizontal placement and also docks at the rail. Button sizes and
OptionsAction typography are retained. Shared draw/hit geometry moves together.

PixelLab's UI-specific generator produced four replacement states, asset
`802ad3cc-1e80-4045-b846-a9467f711241`, seed 71801, using a crop of the actual
frame rail as style reference and an explicit palette including #ee700b. The
original sheet is retained in Sources/orange-docked-action-states.png. All four
65x19 source bounds were prepared as aligned 64x18 sprites with four-pixel
corners and native-pixel repetition/cropping, no interpolation. Runtime files
are action-docked-inactive/selected/hovered/pressed.png. Border #ee700b was
verified in every state (75/120/47/137 border pixels respectively). States have
flat lower edges and orange/golden edging rather than the prior label-plate
style. No text is embedded. Prior sources are preserved; provenance records
generation IDs, source bounds, hashes and the docking contract.

Existing footer regression now also checks flush rail alignment, Apply/Back
adjacency and exclusive click ownership at their shared seam. The standard Debug
build passed with zero warnings/errors; the rebuilt regression runner passed
60/60. Original panel and four header image hashes are unchanged.
Only generated sprites were visually inspected. No game launch, control or
screenshot capture; final in-game integration remains for user review.

### Shared lower border and exact rail palette

Removed all four lower trim rows from the footer sprites. Their open-bottom
64x14 source images draw at the existing button sizes with the existing integer
nine-slice renderer; the source panel alone supplies the horizontal lower border.
The docking edge now meets source row 879 (the rail's gold cap, logical y=619),
a one-pixel upward alignment correction so the original cap remains visible.
Apply and Back still touch at x=934. Labels and button sizes are unchanged.

Used PixelLab's exact pixel-art workbench to map trim colors to samples from the
original panel rail, preserving the generated artwork. PixelLab edit ID
`d35b68cf-e81d-47e5-bc38-ff3d61648fc6` changes 4368 source pixels across all
four states. Principal trim samples from x=792 are source rows 878-882:
#dec44e, #fff04d, #fba71a, #f9870c, #e75d03. Raw edited sheet is retained in
Sources/rail-matched-action-states.png; the active sprites are
action-rail-inactive/selected/hovered/pressed.png. Original sources are retained.
Provenance records source hashes, sample locations and bottom-row removal.
No panel, header, font, setting or action-semantic changes. No game launch or
computer control; only generated assets and the user-supplied screenshot were
inspected. In-game appearance awaits the user's review.

Shared-border verification: solution build passed with zero warnings/errors; rebuilt runner passed 60/60 including footer adjacency, rail alignment, click ownership and packaged-asset checks. Panel and header source-image hashes remain unchanged. git diff --check passed with line-ending notices only.

### Restore Defaults center-seam correction

The footer's four-pixel slice left its one-pixel inset shadow (source column 4)
inside the repeated center, duplicating it across the wider Restore Defaults
button. Expanded only footer fixed slices to five pixels, keeping that shadow
at the left edge. Artwork, palette, labels, layout and behavior are unchanged.
No asset regeneration or game interaction was performed.

Ivy-clearance follow-up: shifted Apply and Back together 24 logical pixels left, preserving their sizes, shared edge, and bottom-rail alignment. Drawing and hit testing use the same updated bounds. Restore Defaults and panel artwork are unchanged. Visual clearance remains for the user to verify.

Residual corner-dot correction: source column 5 still contained the inset's dark
corner pixels at rows 4 and 13. Expanded the footer fixed slices from five to six
pixels, keeping those marks at the actual corners rather than repeating them
through Restore Defaults. Image files, colors, labels, sizes and hit bounds are
unchanged. No asset generation or game interaction.
