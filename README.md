# Pawn Shop Game

C#/MonoGame prototype of a cozy salvage and pawn shop game. M01 implements
title, character selection, a movable player in a small room, nearby NPC dialogue,
pause/resume, return, and exit. Inventory, economics, saving, quests, and robots
are not implemented. Character selection retains the existing base art. The room
uses a clearly labeled four-direction placeholder body and synchronized gear demo;
approved walking art and equipment customization remain future work.

## Defining NPCs in VS Code

**Start here: [Add NPCs, sprites and conversations](PawnShop.Game/Content/Npcs/README.md).**
Dialogue is text only. Inky and audio tools are not required.

```powershell
./npc.ps1 new -Id mechanic -Name "Mechanic" -X 180 -Y 235
```

Edit the generated `Content/Npcs/mechanic/` folder in VS Code, build normally, then click the NPC in
the room after walking nearby. `npc.json` defines identity and sprites; `conversations.json` selects Ink files;
`Content/World/shop.npcs.json` places the NPC in the room;
no C# registration or manual JSON export is needed. Use `./npc.ps1 check`
to validate branches or `./npc.ps1 play -Id mechanic` for a terminal preview.
VS Code's **Terminal > Run Task** also lists the four **Dialogue:** authoring tasks.

## Build and run

Requires .NET SDK 10.0.401 (or a compatible later 10.0.4xx patch) and desktop
OpenGL support. The initial target is Windows x64 using MonoGame DesktopGL
3.8.4.1. Other platforms have not been verified. No additional SDK/workload or
template installation is needed. Font builds use MonoGame's content builder,
pinned to the same 3.8.4.1 version in `.config/dotnet-tools.json` and the Game
project. The standard build task restores that local tool when needed; first
restore requires NuGet access. No system font installation is required.

From the repository root:

```powershell
dotnet restore PawnShopSimulator.sln
dotnet build PawnShopSimulator.sln --no-restore
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
dotnet run --project PawnShop.Game/PawnShop.Game.csproj --no-build
git diff --check
```

The regression project is a dependency-free console test runner, not a VSTest
project. Run it with the command above; `dotnet test` does not execute these
checks. Failed assertions exit nonzero. It references Framework, Core, and Game
and exercises graphics-free input/menu/state/Bootstrap code without constructing
the MonoGame host or initializing graphics/audio. The test framework is unchanged.

`Directory.Build.rsp` disables MSBuild node reuse for command-line builds, so
worker processes do not remain as background .NET Hosts after building/running.
This is a development-tool setting; it does not change game shutdown. Use the
`--no-build` launch above after building to avoid another build on each launch.
Visual Studio's own builds do not use this response file.

## Application icon

`PawnShop.Game/Content/Application/pickers-hollow.ico` is the single icon source
for the Windows executable and application window. `ApplicationIcon` embeds it
in Debug, Release and published executables. The same ICO is copied to build and
publish content for the Windows-only `WindowsWindowIcon` bootstrap helper, which
resolves DesktopGL's SDL window to an HWND and sets both native icon sizes during
`Initialize`. The host restores SDL's icons and releases its handles on disposal.
No WinForms, WPF or additional package is needed.

`pickers-hollow-icon.png` in that folder preserves the supplied 1254x1254
transparent source. It is neither loaded nor copied to runtime output. Both
assets are preserved byte-for-byte. The ICO contains 16, 24, 32, 48, 64, 128 and
256 pixel images. Other operating systems retain MonoGame's default window icon;
macOS bundle/Linux desktop branding would require separate platform packaging.
The window title is **Pickers Hollow**. Explorer, taskbar, Alt+Tab and caption
appearance remain manual Windows checks; old shortcuts or Windows icon caches
can retain a previous icon.

## Human asset authoring

Approved modular human art now belongs to common NPC shoppers under
`PawnShop.Characters/Npcs/Common`. Its seeded appearance generator and catalog
support adding outfits, hair and hats through content data. See the
[common NPC authoring guide](PawnShop.Characters/Npcs/Common/AUTHORING.md).
The bundled room now replaces its existing visitor sprite with one generated
common shopper per New Game. It walks in from the right, settles at the original
visitor position and retains the same dialogue. The player stays unchanged.

Build the solution normally, then use the existing C# authoring executable:

```text
dotnet run --project PawnShop.Tests --no-build -- --assets help
```

The Windows-only PNG commands validate bases, static shirts and legacy walking
candidates, pack local source frames, and produce review sheets/HTML. They never
approve art or overwrite an output directory. System.Drawing.Common 10.0.0 is
pinned only in Tests; it is not a game dependency. No game window is opened.
See [the workflow](docs/WEARABLE_ART_WORKFLOW.md) for exact commands and limitations.
New human art follows S/N/E plus runtime-mirrored West. The Phase 2 room demo uses
that convention; historical eight-facing bases and NESW candidates remain intact.

## Controls and current behavior

- New Game opens Male/Female selection. No gameplay session exists until you
  choose a character. WASD/arrows select; Z confirms; X returns to title.
  Clicking a character card also starts a new session with that gender.
- WASD or arrow keys in the room: held movement, with equal cardinal/diagonal speed. The player
  uses four cardinal facings, stays on the floor and collides with the counter
  and NPC footprints. A newly pressed axis wins diagonal ties; otherwise the last
  axis persists. West mirrors East. The room shows a four-phase placeholder walk
  with jacket, helmet, backpack and wrench, sharing one frame clock and feet pivot.
- Z or Left Click near the shop visitor: talk. The prompt appears within 48 logical pixels;
  clicking the NPC also requires proximity. Z reveals text, then advances or
  selects on a later press. W/S or Up/Down selects responses; X leaves dialogue.
- X or Right Click in the room: pause. Either resumes or backs out of the current menu. Movement is
  captured during selection, dialogue, pause, Options and confirmation screens.
- Z activates ordinary menus. Enter/Space remain menu-confirm aliases, Escape
  remains a back/pause alias, and nearby Tab Talk remains a legacy shortcut.
  The old Space-to-ring-bell room shortcut has been removed.

These are the default controls. Both primary and alternate bindings are editable
in Options. Direction bindings also navigate menus; keyboard Interact / Attack confirms and
Back / Block cancels throughout the UI. Enter/Space confirm and Escape back remain
available after rebinding.
Left Click confirms UI targets through hit testing; blank clicks never activate
the focused button. Attack/block mechanics are not implemented by these bindings.

Continue is visibly unavailable; focus it to read the reason.
Gameplay saves are not implemented. Returning to title or exiting during a session
requires confirmation that unsaved progress will be discarded; the safe default
keeps the session. Window close requests use the same exit confirmation.
Losing application focus pauses the room by default; focus return requires explicit resume.

## Options

Options is available from the title and pause menus. Tab cycles Display, Audio,
Comfort, and Controls; Up/Down selects a row; Left/Right adjusts it; Interact or
Enter activates buttons. Back / Block or Escape requests leaving Options.
Unapplied changes open a matching confirmation with Keep Editing (the default)
and Discard Changes; Escape/right-click keeps editing. Clean exits leave immediately.
During key capture, Escape cancels. The active session stays paused
while Options is open. Tabs and Apply / Restore Defaults / Back stay fixed. Scroll
the inner content with the mouse wheel or drag its scrollbar; keyboard navigation
reveals the selected field. Tab changes reset scrolling. The complete decorative
panel uses the supplied source PNG; control designs remain provisional.

- Display: Windowed, Borderless, or Fullscreen; presets from 1280x720 through
  3840x2160 (4K), including 16:10 and ultrawide choices; UI/text size at 100, 125,
  or 150 percent. Manual window resizing and maximizing are disabled. Borderless
  uses the desktop size; the chosen resolution applies to windowed/fullscreen.
  Unsupported fullscreen modes and windows larger than the desktop are rejected.
- Audio: Master, Music, Ambient, Sound Effects, Combat and Monster in keyboard
  steps of five, with mechanical slider clicks and a Mute When Unfocused toggle. There are no authored gameplay audio
  tracks yet. Audio categories multiply their volume by Master.
- Comfort: Pause When Unfocused and Slow/Normal/Fast/Instant dialogue text speed,
  with a live text preview. The visitor conversation uses this preference.
- Controls: **Interact / Attack**, **Back / Block**, Move Up, Move Left, Move Down and Move Right
  each have **Primary** and **Alternate** columns. Select a row and use Left/Right
  to choose a slot, or click a slot, then activate it. Release keys and mouse
  before choosing the replacement. Action alternates accept Left or Right Click
  as well as keys; primaries and movement slots remain keyboard-only.
  Escape cancels capture; Delete/Backspace clears
  an alternate. A primary is required. Duplicate keys across either column are
  rejected with the conflicting action and slot. Holding both bindings performs
  one action and does not double movement speed.
- Restore Defaults chooses Z/Left Click for Interact / Attack, X/Right Click for Back / Block, and W/A/S/D with
  Up/Left/Down/Right arrows as movement alternates. Both binding columns share the
  same walnut-and-brass button style. Enter/Space are reserved for confirmation, Escape for Back,
  Tab for categories, and Delete/Backspace for clearing alternates.
- Older settings gain the movement defaults in memory. Legacy Space/Escape
  defaults adopt Z/X. Custom action keys are retained; if they occupy a new movement
  default, movement uses its remaining default or an unused I/J/K/L key and shows
  a review message. The original file is preserved until Apply. Newly saved
  bindings, including deliberate Space/Escape choices, round-trip unchanged.
  Missing action alternates receive Left/Right Click unless they conflict with an
  existing binding. Old default Enter/Escape alternates also adopt these mouse defaults.
  A saved defaults marker preserves deliberate clears made after upgrading;
  custom primary/alternate mappings remain unchanged.

Apply saves preferences. Display changes first require confirmation within 15
seconds; X/Escape, timeout, or losing focus reverts them. Restore Defaults edits all
categories and still requires Apply. UI size, audio, and text speed preview while
editing; leaving without Apply restores the saved values.

Moving any Audio volume slider plays one of six 48 ms mechanical clicks in
`PawnShop.Audio/Content/UI/rotary_click_01.wav` through `rotary_click_06.wav`.
They are distinct extracts of el_boss's [CC0 rotary-knob recording](https://freesound.org/people/el_boss/sounds/643563/).
The unchanged original, license links and extraction details are retained under
`PawnShop.Audio/Content/UI/Sources/RotaryKnob`, outside the runtime content build.
Mouse dragging requests feedback for each changed whole-percent value; pixel motion
that leaves the value unchanged is silent. Keyboard changes use the same feedback. Controller input is not
currently implemented. Initialization, unchanged/clamped values, Restore Defaults
and discard do not request slider clicks. The shuffled bag uses every sample once,
then reshuffles without repeating the previous sample at the boundary. Each click
varies playback rate by +/-2.5%, converted to MonoGame octave pitch with log2(rate).
A 50 ms presentation-time cooldown drops excess requests without queuing them.
The existing SoundEffectPlayer owns cached instances and stops the previous sample
before switching, so slider samples cannot overlap. Even the slowest variation is
shorter than the cooldown. Gain is 0.8 × Master × Sound Effects with live draft
settings and focus mute, including while adjusting SFX itself; zero SFX is silent.
MGCB compiles all six WAVs as SoundEffects; buttons and tabs retain their own sounds.

All three Options checkboxes (Mute When Unfocused in Audio, and Pause When Unfocused
and Screen Shake in Comfort) play `PawnShop.Audio/Content/UI/switch_on.wav` when
checked and `switch_off.wav` when unchecked, using their new draft state. These
are the first and second sounds from the supplied `874829__cb9382__onoff-switch.flac`.
The unchanged FLAC, complete converted stereo 48 kHz 16-bit WAV and source record
are preserved under `Content/UI/Sources/OnOffSwitch`. The on/off extracts are 88/100 ms
with short edge fades, fixed pitch and 0.15 × Master × Sound Effects gain, respecting
focus mute. The existing SoundEffectPlayer owns one switch voice, so rapid toggles
replace the prior sample. Mouse and keyboard share the request path; opening Options,
Apply, Restore Defaults and discard do not play switch sounds. The previous marimba
WAV is preserved but no longer registered in MGCB or used by the menu.

Apply, Restore Defaults, Back, and all Primary/Alternate binding selectors use
`PawnShop.Audio/Content/UI/sub_click.wav` for button activation. This sound also uses Master
× Sound Effects and focus mute, with its own reusable playback instance. Framework's
`Button(Action onClick, Action? onClickSound = null)` defaults to silent; pass the
optional sound-request callback to opt in. Disabled buttons neither act nor request
audio. The four top tabs opt in separately to `PawnShop.Audio/Content/UI/tab_click.wav`, including
keyboard Tab activation. Confirmation actions remain silent; slider and dropdown
feedback are separate. Tab playback also uses Master × Sound Effects and focus mute.

Window Mode, Resolution and Dialogue Text Speed dropdowns use four supplied fmaudio
wood-panel impacts, `PawnShop.Audio/Content/UI/dropdown_click_01.wav` through `_04.wav`.
A separate shuffled bag plays each once before reshuffling and prevents consecutive
repeats across bag boundaries. Opening and confirming an option always use different
samples, including confirming the already selected option. Closed-dropdown Left/Right
changes also click. Highlight navigation, scrolling, cancel, invalid selections and
settings restoration are silent. Explicit open/select actions have no cooldown, so
quick selections retain their feedback. One cached playback voice prevents stacking.
The 220 ms mono 48 kHz 16-bit derivatives preserve each initial impact, trim excess
tail silence and fade only the last 10 ms. Fixed-pitch gain is 0.15 × Master × Sound
Effects with focus mute. Originals and source details remain under
`Content/UI/Sources/DropdownClicks`, outside the runtime content build.

Application preferences live in `%LOCALAPPDATA%/PickersHollow/settings.json`,
independently of game sessions. Only this folder is used; the older RummageHollow
folder is not read or migrated. A missing settings file is created with defaults
at launch. If creation fails, defaults are used in memory and Options shows a warning.
Writes use a temporary file and replacement with
a `.bak` backup. Invalid/unsupported files load defaults and show a warning in
Options; the original remains untouched until Apply. A failed write retains the
current preferences and reverts any display preview. An unavailable saved display
falls back to a 1280x720 window without overwriting the file.

## Implementation boundaries

`PawnShopSimulator.sln` and all nine project directories sit at the repository
root. Seven class libraries support the MonoGame executable and existing console
regression project. The approved dependency graph and full folder list are in
[ARCHITECTURE.md](ARCHITECTURE.md#22-approved-solution-layout).

| Project | Current responsibility |
| --- | --- |
| PawnShop.Framework | Reusable input/edges, generic menus, renderer, font catalog and SpriteFont adapter. No PawnShop project references or game-specific commands. |
| PawnShop.Audio | Audio playback, music loop/fades, gain snapshots, asset IDs and categorized audio sources/build pipeline. No PawnShop project references. |
| PawnShop.Core | Authoritative room clock/bell fixture and immutable view under World. No graphics/project/package dependencies. |
| PawnShop.Narrative | Headless Ink definition/session adapter, immutable dialogue views, stable choice IDs and content diagnostics. Quest/relationship folders remain empty. |
| PawnShop.Characters | NPC definitions plus headless player identity, gender, facing, held movement and swept ground-footprint collision. Other actor features remain future work. |
| PawnShop.Shop | Requested class library and empty future folders; no shop implementation. |
| PawnShop.Exploration | Requested class library and empty future folders; no exploration implementation. |
| PawnShop.Game | Thin executable/composition and main-menu UI. Existing non-menu presentation remains until a separately scoped extraction. Check for a better library home before placing new files here. |
| PawnShop.Tests | Existing lifecycle checks plus input/menu checks under Framework, using the same console runner. |

Audio source files live under `PawnShop.Audio/Content`: `Music`, `UI`,
`Environmental`, `Combat`, `Monster`, and `Speech`. Empty categories retain
`.gitkeep`. Add audio to the appropriate category and to that project's
`Content/Content.mgcb`; Game's MGCB now builds fonts only. The pinned pipeline
ships compiled assets under `Content/Audio/Music`, `Content/Audio/UI`, etc.
`AudioAssets` owns their runtime IDs. `AudioPlayback` owns the device resources;
Game constructs it, supplies an `AudioMix` snapshot and consumes presentation
requests. Existing application settings and their JSON format are preserved.
Asset categories are distinct from mix channels: UI uses Sound Effects, and
Environmental maps to Ambient. Additional playback channels are not implied by
creating a source folder.

The floor and NPC textures follow the active session ID and are disposed on return.
Two application-owned player base textures are shared by character selection and
room rendering; returning to title does not dispose textures still used by selection.
Framework's PixelRenderer owns the application-wide font pixel, render target,
and SpriteBatch, disposed by the host on unload/application disposal. Drawing
never changes session state. There are no session event subscriptions in M01.
An application-owned FontLibrary shares a single MonoGame ContentManager/font
texture across the title and Options renderers. Screens do not dispose shared fonts.

| Previous location | Current location |
| --- | --- |
| src/PawnShop.Application input and menu | PawnShop.Framework/Input and UI; game menu commands remain in PawnShop.Game/Screens |
| src/PawnShop.Application GameSession/ApplicationFlow | PawnShop.Game/Bootstrap; room state extracted into PawnShop.Core/World |
| src/PawnShop.Game | Root PawnShop.Game; reusable drawing moved to PawnShop.Framework/Rendering and concrete drawing to Game/Screens |
| tests/PawnShop.Tests | Root PawnShop.Tests |
| PawnShop.slnx | PawnShopSimulator.sln (sole solution) |

The former Application project has been retired. Empty requested folders use
`.gitkeep` and do not imply feature completion. Core/Transactions will own shared
atomic operations; Shop/Transactions must use them. Feature save data stays with
its owner; Core/Persistence provides neutral contracts/storage, and Game/Bootstrap
coordinates complete snapshots. No saving was implemented by this refactor.

MonoGame's fixed 60 Hz host is the single tick owner. Menus capture the entire
transition frame. Simulation time stops in overlays; presentation time continues.
The room now supports the requested character-selection/movement/Talk slice.
The NPC and furniture remain static, and M02's scale lab is still pending.

## Content and title screen

The Picker's Hollow title uses `Content/TitleScreen/title-screen-panorama-day-to-dusk.png`,
which adds 747 rows of daytime sky above the supplied 2106x747 illustration. The supplied
`logo-pickers-hollow.png` replaces the title text as an independent transparent
overlay; the main menu uses Silkscreen, title badge/help uses Pixelify Sans Bold,
and Options uses Jersey 25 for header tabs and Atkinson Hyperlegible for other
text through the font catalog. See the
[asset record](PawnShop.Game/Content/TitleScreen/README.md) for provenance and hash.
Normal build/publish copies PNGs beside the executable; runtime resolves them
relative to the executable. PNG loading is unchanged; fonts compile to XNB using
the standard MonoGame content pipeline in `Content/Content.mgcb`.

At launch the camera stays in the daytime sky while the centered Torcalev Studios
logo fades in over one second, holds for two seconds, and fades out over one second.
It then descends for 3.2 seconds to the dusk framing. The game logo and menu begin
fading in 0.25 seconds before arrival. The menu accepts input after the 0.8-second
fade. Escape during the intro fades the studio logo out over 0.8 seconds and
reaches the ready menu within two seconds. Returning to title during play opens
the settled menu.
Click Options (or use keyboard
confirmation) to pan to the right edge over 0.8 seconds with SmoothStep easing.
Options appears during Sprout's flight and accepts input after arrival. Back/Escape hides Options and pans left; Escape
can also reverse an outgoing pan. Repeated navigation does not restart the camera.
Controls cannot activate during travel. Options opened from the pause
menu does not use this title animation and keeps the session paused.

The background fills the actual viewport using a uniform cover scale, without
stretching or revealing outside pixels. The original 2106x747 band determines the
scale and settled crop, so the descent preserves the title and Options framing.
Very wide viewports that already show the entire image width have no horizontal
travel and crop vertically instead. Point sampling preserves sharp pixel edges.
The title and Options UI retain separate, contained 1920x1080 and 1280x720 canvases;
their layout and mouse coordinates do not pan with the background. Transparent
UI targets composite over the panorama after switching back to the backbuffer.

The default/minimum window is 1280x720. Resolution presets reach 3840x2160; users change
window size only through Options. Manual resizing and maximizing are disabled,
including after display-mode changes. Preferences use only the `PickersHollow`
settings directory, creating a default settings file when it is absent.
TitleScreen owns its panorama texture, batches and UI target for the application
lifetime and releases them on disposal. No gameplay camera behavior changed.

The placeholder room and pause overlays retain the
640x360 logical target, integer output scaling, original code-authored shapes,
bitmap font, and generated floor texture. These UI choices do not establish
actor/tile dimensions; M02 must validate world scale.

Player artwork now has canonical reference bases under `Content/Player/Bases`
and the user-approved south-facing male/female teal shirt under
`Content/Player/Wearables/TealWorkShirt`. Follow the
[wearable art workflow](docs/WEARABLE_ART_WORKFLOW.md) for new equipment artwork.
Use the C# `--assets bases` / `--assets wearable` commands above for nonvisual checks.
The game loads the two canonical base PNGs and `Content/Player/player-sprite.json`
for selection; the catalog resolves South/North/East and mirrors East for West.
Room diagnostic layers are created by `Screens/PlayerPlaceholderSprites.cs`;
`Content/Player/placeholder-sprite.json` defines their Idle/Walk timing. Every layer
uses the same frame, 48x64 canvas and (24,58) pivot. Male/female fits are distinct.
These shapes prove animation and layering only; they are not approved wearable art.
Shirt/source/recipe artifacts are not copied into runtime content. Production
equipment/equipping, approved human walking and full customization remain pending.
See [verification and manual checks](docs/VERIFICATION_CARDINAL_RENDERING.md).

Phase 3 begins with [the local human reference/walk preview](tools/player-base-review.html).
Open it in a browser from this checkout to review both bodies' approved standing
views/South walks, existing North candidates and newly generated East walks with
mirrored West. Foot guides and standing/walking comparisons help review alignment.
The room remains on the accepted Phase 2 placeholders until art is approved.

Menu rows and Options tabs also accept mouse clicks. Clicking a settings row
performs its existing confirm action; Left/Right still provides fine keyboard
adjustments. Display confirmation has clickable Keep/Revert buttons.

Sprout replaces the title menu's selection diamond. The bundled local
`sprout-title-menu.png` uses 68x80 cells: frames 0-12 loop PixelLab's east-facing
Floating Point Right Loop at 90 ms/frame; frames 16-31 preserve the original
selection reaction, padded without resizing, at 50 ms/frame on confirmation by
Enter, Space, or mouse. For entries other than Options, the selected action runs after the 0.8-second Interact
clip finishes, so the reaction is visible before starting a game or exiting. Repeated input is captured during the clip; Escape cancels
it. Focus loss holds pending activation until focus returns. Disabled Continue
reacts but cannot start a session. Pause/Options controls keep their normal timing.

`Content/TitleScreen/sprout-title.json` defines the grid, named clips, timing,
stable pivot and presentation scale. Sprout stays vertically anchored to the
selected label's glyph center; the fixed sheet pivot preserves authored bobbing.
Framework's SpriteAnimator advances once in Update, with Draw only reading frames.
Sprout renders directly to the final viewport with point sampling and a snapped
pixel position, avoiding a second scaling pass through the title render target.
Clip timings are independently editable in the metadata.

Options finishes the current pointing cycle, then Sprout floats upright to the
first Display setting during the 0.8-second panorama pan. The Floating Point
Right Loop continues throughout travel and arrival, without rotation or jet clips.
Input is captured until arrival; Escape cancels and focus loss pauses the float.
Leaving main-menu Options keeps Sprout visible and floats him from the selected
setting back to the main menu's Options entry over the same 0.8-second pan.
His pointing animation continues without restarting, and Options remains selected
on arrival. Return travel captures input and freezes with focus loss.
All frames are bundled locally; the game never calls PixelLab.

## Menu music

The supplied **Forgotten Treasures** theme starts with the launch intro and
loops across the main menu and title-side Options without restarting during travel.
Each loop fades out over its final 1.5 seconds and fades in over the next pass's
first 1.5 seconds. There are no entrance/departure fades. Character selection and
gameplay stop the theme, including Options opened from pause; returning to title
starts it again.

The theme's base gain is 50% before the Master/Music sliders, reducing its playback
level without changing saved preferences. Master and Music sliders preview immediately. Apply keeps their values; discarding
edits restores the saved gain. Mute When Unfocused silences playback while its audio
cursor continues, so focus changes do not restart the theme.

The unmodified 24-bit stereo WAV lives at `PawnShop.Audio/Content/Music/Forgotten_Treasures.wav`. Audio's MGCB
build compiles it with `WavImporter`/`SongProcessor` into an Ogg stream and XNB;
only these compiled files ship. No additional dependencies or runtime conversion
are required. See [source provenance, checks and listening checklist](docs/VERIFICATION_MENU_MUSIC.md).

## Central font catalog

Main-menu footer sayings live in
`PawnShop.Game/Content/TitleScreen/title-sayings.json`. Edit the `sayings` array,
keeping `version` at 1, then rebuild/restart. Each entry must be unique, one line,
and 1-80 printable ASCII characters (the current font atlas's supported range).
A random saying is chosen once on launch and shared with the Comfort tab's
dialogue-speed preview. It stays fixed until the app restarts, including returns
to the main menu and selection of unavailable entries. The saying uses Pixeloid Sans Bold through
`TitleSaying` at 28 and fits to the available footer width when needed.

`PawnShop.Game/Content/Fonts/fonts.json` maps names to compiled font assets and
logical pixel line heights. It loads once at application startup. Current roles
are `TitleMenu`, `TitleBadge`, `TitleHelp`, `OptionsHeading`, `OptionsBody`, and
`OptionsHelp`. `TitleMenu` uses bundled Pixeloid Sans Bold, matching inner Options text; title badge/help uses
Pixelify Sans Bold. Options header tabs/footer labels use Jersey 25; inner Options
text uses Pixeloid Sans Bold, with point sampling for the pixel-art Options canvas. Other renderers
retain point sampling. `Silkscreen` and `PixelifySansBold` are also available as
general named entries. The title logo remains an image.

```csharp
using PawnShop.Framework.Rendering;

renderer.Text("New Game".Font("TitleMenu"), x, y, color);
renderer.Text("Example".Font("PixelifySansBold"), x, y, color);
```

The helper selects a catalog entry; MonoGame's SpriteFont/DrawString handles
glyphs, kerning, measurement and drawing. FontLibrary loads each unique asset once.
JSON `size` is the line height in logical canvas pixels, multiplied by the existing
UI scale setting. Edit the catalog, build and restart to apply changes. Unknown
names, invalid sizes, unsupported schemas and missing assets produce diagnostics
instead of silently choosing a different font. No runtime download or JSON parsing
occurs during drawing. Plain `renderer.Text(string, ...)` retains the existing
placeholder font for room/pause content outside this change.

To add another font, bundle its licensed TTF next to a `.spritefont`, add the
spritefont to `Content/Content.mgcb`, and reference its content ID (without `.xnb`)
from the catalog. The existing MonoGame content build compiles it automatically.
See [MonoGame's font guide](https://docs.monogame.net/articles/getting_started/content_pipeline/adding_ttf_fonts.html)
and [font provenance/license](PawnShop.Game/Content/Fonts/README.md).

## Ink conversations

The conversation engine uses Ink 1.2.1 through its official C# runtime. Editable
NPC folders containing `npc.json`, sprites, `conversations.json` and `.ink` sources live in
[Content/Npcs](PawnShop.Game/Content/Npcs/README.md); that guide covers
VS Code editing, automatic build compilation, supported tags, and limitations.
Compiled JSON and the current content index are generated under `obj/` and copied
to output. Normal builds detect Ink syntax errors; `npc.ps1 check` validates
NPC bindings and performs bounded branch checks.
The official runtime/compiler assemblies and MIT license are recorded in
[third_party/ink](third_party/ink/README.md). Only the runtime ships with the game.
No Unity integration or installed Inky app is required.

GameSession owns transient conversations; ApplicationFlow routes modal input and
pauses the world; Screens renders text and responses using the existing pixel
font. Quests, rewards, gameplay effects, durable Ink variables, and gameplay saves
are deliberately absent from this draft. See [verification](docs/VERIFICATION_DIALOGUE.md).

## Guidance and evidence

The existing design documents are at the root, not under `docs/`:
[agents.md](agents.md), [ARCHITECTURE.md](ARCHITECTURE.md),
[GAME_DESIGN.md](GAME_DESIGN.md), and [ROADMAP.md](ROADMAP.md).
Their proposals remain proposals beyond the implemented slice.

See [M00/M01 verification](docs/VERIFICATION_M01.md) for historical baseline
results, structural-refactor verification, and the remaining desktop acceptance
checklist. M00 remains complete; M01's full desktop lifecycle/resource acceptance
remains pending. The user-requested M06 dialogue foundation is now implemented;
the full M06 quest/economy/persistence integration remains planned.
