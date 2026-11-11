# Options menu — current verification

This replaces superseded appearance experiments and repeated implementation logs.
The previous record and removed review attempts are recoverable in
`.cache/DocsArchive/docs-cleanup-20261111-040437.zip`.

## Current behavior

- Options is available from the title and pause menus. It captures underlying input;
  opening it from play preserves the paused session.
- Display changes use a 15-second Keep/Revert confirmation. Escape, timeout, focus
  loss or device failure reverts the preview. Minimum resolution is 1280x720;
  manual window resizing is disabled.
- Draft edits are saved only through Apply. Back with changes opens Unapplied
  Changes, defaulting to Keep Editing. Restore Defaults affects only the active tab.
- Tabs and footer actions stay fixed while overflowing content scrolls within its
  clip. Drawing and pointer input share OptionsMenuLayout geometry.
- Controls supports primary/alternate keys and action mouse alternates. Capture
  waits for release, validates duplicates and reserved inputs, and permits clearing
  only an alternate. Focus changes do not replay held input.
- Comfort stores pause-on-focus-loss, screen shake and dialogue speed preferences.
  Screen shake has no implemented effect yet; dialogue speed previews live.

## Audio

Sources and the content build live in `PawnShop.Audio/Content`:

| Source | Used by |
| --- | --- |
| `UI/rotary_click_01.wav` through `rotary_click_06.wav` | The six Audio sliders |
| `UI/switch_on.wav` / `UI/switch_off.wav` | Checking/unchecking Mute When Unfocused, Pause When Unfocused and Screen Shake |
| `UI/dropdown_click_01.wav` through `dropdown_click_04.wav` | Opening/selecting Window Mode, Resolution and Dialogue Text Speed |
| `UI/sub_click.wav` | Apply, Restore Defaults, Back, and all Primary/Alternate selectors |
| `UI/tab_click.wav` | The four top tabs, including keyboard Tab activation |
| `Music/Forgotten_Treasures.wav` | Intro, title and title-side Options |

The rotary clicks are six distinct 48 ms mono 44.1 kHz 16-bit PCM extracts of
el_boss's [Radial knob clicks turning dial, Freesound 643563](https://freesound.org/people/el_boss/sounds/643563/),
licensed [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/).
The exact supplied `643563__el_boss__radial-knob-clicks-turning-dial.wav` is preserved
under `PawnShop.Audio/Content/UI/Sources/RotaryKnob` with `SOURCE.txt` provenance and
processing details. Original SHA256:
`990CBD06C541BD06F856244226DAA3A6E19EB1DFB2D13FEBB5099098970CE030`.
Only the derivatives are in MGCB. The original Downloads file is unchanged.

Existing MonoGame-bundled FFmpeg 083443d performed extraction without saved scripts
or new dependencies. The six source windows start at 5.204, 5.382, 5.537, 5.816,
5.972 and 6.371 seconds. Each contains one isolated attack, a short preroll and
natural decay; a mono downmix, 80 Hz high-pass, 0.5 ms entrance fade and 8 ms final
fade reduce handling rumble and edge discontinuities. Attenuation only (0 to
-3.746 dB) matches whole-window RMS to -41.31 dBFS; peaks range from -22.6 to
-18.6 dBFS. No compression, amplification, gating, reverb or artificial clicks.
Cleanliness, warmth and comfort remain listening acceptance, not numerical claims.

Audio's shared ShuffledSoundBag uses every sample once per Fisher-Yates shuffle and
swaps the first entry if it repeats the previous bag's last sample. Independent
cosmetic randomness varies rate by +/-2.5%, using log2(rate) for MonoGame's octave
pitch API. A 50 ms presentation-time cooldown skips requests, never queues them.
Each 2117-frame sample lasts less than 50 ms even at 0.975x rate. SoundEffectPlayer
also stops the previous voice before switching and caches/disposes every instance.
Mouse values retain one-percent precision, with feedback for every changed value;
actual keyboard steps (including Confirm) click. Controller input is not implemented.
Initialization, restoring/discarding settings, stationary/clamped values, focus loss
and modal capture request no clicks. Slider gain is 0.8 × Master × Sound Effects
using the current draft, so SFX changes apply to their own feedback immediately.
Muted requests do not consume the bag or cause later playback.

The button and tab effects retain their existing 150 ms PCM sources and behavior.
UI playback uses Master × Sound Effects and focus mute. Button
sound is an optional callback; disabled buttons neither act nor request audio.
Modals and key capture block underlying controls. Opening Options is not a tab click.
AudioPlayback owns device resources; Game supplies requests and an immutable mix.
Settings JSON remains backward compatible and lives outside the repository.
Music behavior is documented in [menu music](VERIFICATION_MENU_MUSIC.md).

Checkbox switches use the supplied `874829__cb9382__onoff-switch.flac`. Its exact
bytes, full converted WAV and provenance are retained in
`PawnShop.Audio/Content/UI/Sources/OnOffSwitch`. Source FLAC SHA256:
`04CDC7E8E251E575D795A53FD624937830DE28435E87DE051FFE02788188115E`.
The full recording is 1.683333 seconds, converted from stereo 48 kHz 24-bit FLAC
to stereo 48 kHz 16-bit PCM WAV using the existing FFmpeg. The first/on extract
uses 0.272-0.360 seconds (88 ms); the second/off extract uses 1.188-1.288 seconds
(100 ms), preserving their attacks and decays with 1 ms entrance and 8 ms exit
fades. There is no normalization, filtering, pitch variation or added effect.
Both sources peak at -0.3 dBFS; playback uses 0.15 × Master × Sound Effects and
focus mute. The old marimba WAV is preserved but no longer built or referenced.
No source files or scripts outside the repository were changed or added.
The download filename identifies cb9382/Freesound 874829; the license was not
supplied or embedded and its source page was not verifiable, so no license is
asserted. Source details are kept in `SOURCE.txt` alongside the original.

OptionsModel requests the new checked state only when a user toggles a checkbox.
The nullable boolean distinguishes off (false) from no request (null). Mouse and
keyboard use the same path. Open, Apply, Restore Defaults, discard, focus loss,
modals and held inputs do not create switch requests. AudioPlayback uses one
owned SoundEffectPlayer for both samples, stopping the previous voice before
switching. Muted requests are skipped immediately without queued playback.

Dropdown clicks use the four supplied fmaudio wood-panel recordings: take 1 is
Freesound 560336, take 2 is 560335, take 3 is 560334 and take 4 is 560333.
Original mono 96 kHz 24-bit WAVs are preserved byte-for-byte under
`PawnShop.Audio/Content/UI/Sources/DropdownClicks`, with source URLs and hashes in
`SOURCE.txt`. Licenses were not supplied or verifiable, so none is asserted.
Existing FFmpeg converts the first 220 ms of each to mono 48 kHz 16-bit PCM,
preserving the immediate attack without a leading trim/fade. Only the final
10 ms is faded, after the impact decay. No normalization, compression, pitch
variation, synthetic effects, dependencies or permanent conversion scripts.

Dropdown playback owns its own four-entry ShuffledSoundBag and cached
SoundEffectPlayer voice, using 0.15 × Master × Sound Effects plus focus mute.
All samples play once per bag, and boundary swapping prevents immediate repeats.
Opening and confirming any valid option therefore always use different samples,
including across reshuffles and when accepting the existing value. Keyboard
Left/Right changes on a closed dropdown also request feedback. Navigation within
an open list, scrolling, Escape/outside cancellation, invalid selections, initial
state and restore/discard are silent. Dropdown requests have no slider cooldown,
so rapid open/select sequences both sound; the previous dropdown voice stops when
the next starts. Muted requests do not consume the bag or queue later playback.

## Verified and pending

Current verification: build/MGCB passed with zero warnings/errors and
92/92 headless regressions passed (dropdown baseline: 89/89; checkbox baseline: 87/87;
initial slider baseline: 82/82). New dropdown checks exhaust four samples over
64 seeds and 100 bags, compare seeded replay, reject adjacent repeats, and exercise
opening/selection through actual mouse and keyboard input on all three dropdowns.
They cover held input, zero-time open/select, current-value confirmation, invalid
choices, direct keyboard changes, restore, focus loss, modal capture, independent
control cues, original hashes and compiled content reaching consumers. Separate
FFmpeg decode and XNB PCM checks validate all four runtime files.
Checkbox checks cover all three controls, both resulting states, actual mouse and
keyboard translation, held inputs, focus and modal capture, silent defaults/apply/
discard and SFX mute. Asset checks prove first-on/second-off source-window order,
valid PCM format, preserved samples outside the fades, source hashes, compiled
XNBs and consumer copies. FFmpeg decoding and compiled PCM payload checks also pass.
Slider checks exercise shuffled
bags with 1, 2 and 6 samples across 32 seeds and 20 bags, pitch limits, seeded
replay, the exact cooldown boundary, 1 ms rapid requests, dropped requests and no
delayed playback. Input checks cover real mouse translation, real keyboard edges,
whole-percent mouse changes, all six sliders, clamps, focus,
modals, initialization, restore/discard, live SFX gain and mute. Asset checks decode
WAV PCM, verify unique waveforms, attack/edge samples, level consistency and duration,
and parse each real MGCB XNB to confirm its PCM payload matches the WAV and reaches
consumers unchanged. They create no graphics/audio device. A separate FFmpeg decode
check validates all final WAVs. No game launch or listening check was performed.
The initial build encountered a running-game DLL lock; after the user closed it,
the normal build succeeded. No automated blocker remains.

Small-adjustment correction: the original five-point sound gate could silently
change a mouse value from 80 to 81-84. A regression through the actual pointer,
Options and cooldown paths reproduced that failure before the fix. Every actual
whole-percent change now requests feedback; the existing 50 ms cooldown remains
the rate limit. The regression also covers sub-percent pixel motion, rapid skipped
changes, idle after dragging and small reversals. The user confirmed that all six
individual source samples have sound; in-game acceptance of this correction remains
pending. The samples and playback selection are unchanged.

```text
dotnet build PawnShopSimulator.sln --no-restore -m:1 -p:UseSharedCompilation=false -p:CopyRetryCount=0
dotnet run --project PawnShop.Tests/PawnShop.Tests.csproj --no-build
git diff --check
```

Manual acceptance remains with the user: appearance/readability, display switching,
scroll/drag and binding feel, title/pause transitions, audible playback and mix,
mute/focus behavior, and slider/button/tab sound selection. Compilation and headless
tests do not establish these results. No game or desktop test accompanied this cleanup.

For the new clicks, launch normally and check:

1. Drag each slider slowly and rapidly. Hear short, varied single detents without
   per-pixel chatter, overlapping tails, harsh cutoffs or clicks after release.
2. Use Left/Right (and remapped movement keys) to change values. Unchanged values
   at 0/100 and simply grabbing the watch should be silent.
3. Lower SFX and Master, set each to zero, then raise it. Confirm comfortable gain,
   silence at zero and immediate feedback when increasing SFX from zero.
4. Open/reopen Options, Restore Defaults, apply and discard edits. Those operations
   must not add slider or switch clicks; button and tab sounds remain distinct.
5. Check title-side and pause-side Options, confirmation capture and focus mute.
   Listen for natural attack/decay and an unobtrusive mix against menu music.
6. Toggle Mute When Unfocused in Audio, then Pause When Unfocused and Screen Shake
   in Comfort, using mouse and keyboard. Confirm the first/on sound when checking
   and the second/off sound when unchecking, a comfortable level, SFX/Master mute,
   and no switch playback from opening, applying, restoring or discarding settings.
7. Open and select options repeatedly in Window Mode, Resolution and Dialogue Text
   Speed, using mouse and keyboard. Confirm distinct open/select sounds, varied
   non-repeating consecutive samples, comfortable levels and responsive quick
   selection. Navigate/scroll/cancel without selecting and confirm no extra cues.
