# Primary and alternate controls

## Mouse action alternates - 2026-11-08

Current action labels/defaults: Interact / Attack is Z/Left Click; Back / Block is
X/Right Click. Both primary keys remain keyboard inputs. KeyBinding carries an
optional alternate MouseBinding (None/Left/Right); AppSettings stores separate
keyboard and mouse alternate fields and rejects two inputs in one slot or duplicate
mouse assignments. Selecting an action alternate can capture a key or mouse click
after releasing the activation input. Escape cancels; Delete clears both kinds.
Keyboard/mouse action edges merge before dispatch and synchronize on focus changes.
UI hit testing still owns mouse confirmation, preventing blank clicks from
activating focused buttons. Capturing suppresses normal action dispatch.

Older Enter/Escape defaults migrate in memory to mouse defaults with defaults
marker 2. Custom keys and deliberate clears remain intact; current mouse mappings
round-trip through the existing settings store. Restore Defaults chooses the new
pairs. Actual combat attack/block mechanics remain future work.

Separate-output solution build passed with zero warnings/errors; the existing
regression runner passed 64/64, including new mouse edge/focus, pause/resume,
interaction/dialogue, capture/conflict, draft and persistence checks. No game was
launched; in-game input feel and new label wrapping remain manual checks.

## Earlier keyboard implementation

Date: 2026-11-05. Requested: rename Pause to Pause / Back, permit movement
rebinding and support primary/alternate keys with WASD/arrow movement defaults.

## Behavior and ownership

Controls has six rows: Interact, Pause / Back, Move Up, Move Left, Move Down,
Move Right. All have primary and alternate slots. Default movement pairs are
W/Up, A/Left, S/Down, D/Right. As of 2026-11-08, Interact defaults to Z/Enter and
Pause / Back to X/Escape. Missing alternate fields receive these defaults when
nonconflicting; custom mappings and deliberate clears in current saves are retained.

Follow-up: old files with Z/X primaries and unbound alternates now adopt
Enter/Escape on load too. The additive ActionAlternateDefaultsVersion marker is
written through Apply, allowing subsequent deliberate clears to remain unbound.
Custom mappings are retained; loading does not rewrite the original settings.

Select a row with the vertical direction controls and a slot with the horizontal
direction controls, or click a slot. Interact/Enter begins capture. Release keys,
then press the replacement. Escape cancels capture, and Delete/Backspace clears
an alternate. A primary is required. Conflicts identify the existing action/slot.
Enter/Space confirmation, Escape Back, Tab category navigation and the clearing
shortcuts are reserved. Apply saves; Back discards unapplied changes; Restore
Defaults changes the draft until applied.

Framework's immutable KeyboardBindings/KeyBinding contract merges a pair before
action edge detection or axis calculation. Opposing directions cancel. Holding
both keys for one direction produces the same axis as one key. Movement bindings
also navigate UI, Interact confirms and Pause / Back works across gameplay,
dialogue, selection and menus. Enter/Space and Escape remain recovery shortcuts.
The host refreshes mappings when current settings change; capture input cannot
also activate a screen action.

AppSettings retains the existing action fields and adds movement pairs and action
alternates. Existing files gain missing defaults without being rewritten. Old
Space/Escape defaults upgrade to Z/X only for legacy files. If a custom legacy
action occupies a movement default, it is retained; that direction uses its free
default or an unused I/J/K/L key, clears a conflicting alternate and reports that
Controls needs review. New saved choices round-trip unchanged. No gameplay save
schema, package, SDK or art changes were required.

## Automated evidence

Baseline runner: 43/43 passed. Final commands:

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

Final build: zero warnings/errors. Runner: 47/47 passed. Whitespace checks passed.
The build retains the existing MGCB/Ink content workflow.

Coverage includes all default arrow alternates; remapping all movement directions;
primary/alternate edge merging; equal axis magnitude with both keys held; opposing
directions; focus suppression; release-before-capture; both columns and mouse
cells through a scaled viewport; duplicate and reserved keys; alternate clearing;
required primaries; apply/discard/defaults; Pause / Back in pause, dialogue and
selection; and isolated settings round trips, legacy conflicts and invalid data.

## Manual acceptance pending

No game window was launched or screenshots inspected. Check Controls at
100/125/150 percent UI scale: labels, both columns, selected-cell highlight and
capture/conflict messages. Rebind both slots, apply, walk with either, hold both,
pause/back through menus, restart to check persistence, and restore defaults.
Manual rendering/input feel remains unverified; no automated blocker remains.
