using PawnShop.Tests;

if (args.Length > 0 && args[0] == "--dialogue") return DialogueAuthoring.Run(args[1..], Console.In, Console.Out);
if (args.Length > 0 && args[0] == "--assets") return PlayerAssets.Run(args[1..], Console.Out);

// Dependency-free executable regression suite. A failure exits nonzero.
var tests = new (string Name, Action Run)[]
{
    ("Cinematic runtime uses delta time, independent pose keys and bounded playback", CinematicTests.Runtime),
    ("Cinematic pitch interpolates, projects around ground contact and preserves older JSON exports", CinematicTests.Pitch),
    ("Cinematic bounded spline preserves waypoints and approximates constant arc-length speed", CinematicTests.Spline),
    ("Cinematic image coordinates, offscreen entry and ground anchors survive camera/output transforms", CinematicTests.Coordinates),
    ("Cinematic JSON round-trips, retains backups and rejects invalid data without overwriting originals", CinematicTests.Storage),
    ("Cinematic exports play without the editor and Release excludes editing classes", CinematicTests.Shipping),
#if DEBUG
    ("Held cinematic adjustments accelerate, preserve fine steps and undo once, with scales through 64", CinematicTests.HeldAdjustments),
    ("Expanded cinematic canvas supports offscreen entry placement without changing playback coordinates", CinematicTests.ExpandedCanvas),
    ("Cinematic editor adds, moves, deletes, undoes and authors independent pose keys", CinematicTests.Editing),
    ("Cinematic F8, pointer editing, scrub/play and focus transitions capture input", CinematicTests.Input),
#endif
    ("Restore Defaults resets only the active tab and preserves other saved and pending settings", RegressionTests.OptionsTabDefaults),
    ("Title sayings validate JSON, select once per startup and match the Comfort preview", RegressionTests.TitleSayingsContent),
    ("Dirty Options require explicit discard, preserve editing and block underlying input", RegressionTests.OptionsDirtyConfirmation),
    ("Mouse action alternates merge edges, respect focus/UI capture and preserve rebinding drafts", RegressionTests.MouseActionBindings),
    ("Comfort checkbox and dialogue dropdown preserve input capture, previews and settings drafts", RegressionTests.OptionsComfortControls),
    ("Audio sliders and mute checkbox preserve drafts, pointer input and keyboard/focus behavior", RegressionTests.OptionsAudioSliders),
    ("Options dropdowns capture input, retain drafts and keep lists inside the viewport", RegressionTests.OptionsDropdowns),
    ("Compact Options footer clicks restore drafts, apply once and discard from every scrolled tab", RegressionTests.OptionsFooterActions),
    ("Options uses the complete source panel and keeps controls inside its boundaries", RegressionTests.OptionsPanelSource),
    ("Options scroll, clipping hit boundaries and fixed actions share geometry at every UI scale", RegressionTests.OptionsScroll),
    ("Common shopper enters with synchronized walk/settle frames and cannot cross the player", CommonNpcRoomTests.Entrance),
    ("Room visitor replacement preserves dialogue, player state and session lifecycle", CommonNpcRoomTests.RoomLifecycle),
    ("Common shoppers reproduce seeded appearances and preserve independent identity", CommonNpcTests.Generation),
    ("Common NPC migration preserves bytes, pose alignment, occlusion and West mirroring", CommonNpcTests.FramesAndMigration),
    ("New common NPC outfits, hair and hats work through data without code changes", CommonNpcTests.ExtensibleContent),
    ("Common NPC content rejects invalid assets, fits, compatibility and metadata", CommonNpcTests.InvalidContent),
    ("Offline player assets preserve pixels and reject invalid sources, attire and metadata", PlayerAssetTests.Run),
    ("Title has no session; Continue explains and rejects activation", RegressionTests.Title),
    ("Menu wraps, owns its items and separates navigation from activation", RegressionTests.Menus),
    ("Input edges reject held/replayed keys", RegressionTests.Edges),
    ("New game consumes confirm and creates exactly one session", RegressionTests.Start),
    ("Pause and both resume routes consume world input and freeze time", RegressionTests.Pause),
    ("Focus loss pauses and never advances the world", RegressionTests.Focus),
    ("Return cancellation preserves state; discard disposes and restarts fresh", RegressionTests.Return),
    ("Ten cycles keep one live session and dispose all previous sessions", RegressionTests.Cycles),
    ("Exit is distinct, cancellable in play, and disposes on confirmation", RegressionTests.Exit),
    ("Application disposal is idempotent and rejects subsequent updates", RegressionTests.Disposal),
    ("Options capture input and preserve title and paused session lifetimes", RegressionTests.OptionsLifecycle),
    ("Display preview confirms, times out, cancels and reverts on focus loss", RegressionTests.DisplayPreview),
    ("Preferences round-trip, retain backups and preserve malformed files", RegressionTests.SettingsPersistence),
    ("Bindings require key release and reject duplicate actions", RegressionTests.Bindings),
    ("Audio gain, text speed and configurable focus pause obey settings", RegressionTests.Preferences),
    ("Failed settings writes retain current preferences and revert display", RegressionTests.SettingsFailure),
    ("Panorama easing is time based, reaches endpoints and reverses without jumps", RegressionTests.PanoramaTiming),
    ("Panorama cover crops stay inside the image across aspect ratios and resizing", RegressionTests.PanoramaGeometry),
    ("Title navigation gates hidden controls, reverses and leaves paused options alone", RegressionTests.PanoramaNavigation),
    ("Mouse Options and Back preserve keyboard behavior and ignore letterboxes", RegressionTests.PanoramaPointer),
    ("Font catalog resolves named roles and rejects invalid content with diagnostics", RegressionTests.FontCatalogValidation),
    ("Bundled font roles, compiled SpriteFont and license reach application output", RegressionTests.FontContent),
    ("Sprite sheet metadata validates frames and row-major source rectangles", RegressionTests.SproutSheet),
    ("Sprite animator loops Idle at 90ms and completes Interact at 50ms boundaries", RegressionTests.SproutTiming),
    ("Title feedback delays actions once, captures input and respects cancellation/focus", RegressionTests.SproutSelection),
    ("Options Sprout floats to the first setting while continuing the pointing loop", RegressionTests.OptionsSproutSequence),
    ("Options Sprout captures input, freezes on focus loss and cancels cleanly", RegressionTests.OptionsSproutCancellation),
    ("Sprout returns from the selected option to the main-menu Options entry without losing animation or input capture", RegressionTests.OptionsSproutReturn),
    ("Ink source matches build output and every visitor branch runs", RegressionTests.DialogueContent),
    ("Dialogue commands reject stale IDs, revisions and duplicate selections", RegressionTests.DialogueCommands),
    ("Ink compiler and adapter diagnose invalid graphs, tags, speakers and external calls", RegressionTests.DialogueValidation),
    ("Dialogue reveal, choice focus and long-text paging obey settings", RegressionTests.DialoguePresentation),
    ("Dialogue captures input, pauses time and closes with its session", RegressionTests.DialogueLifecycle),
    ("Data-only NPCs route clicks and keyboard Talk by identity", RegressionTests.NpcConversations),
    ("NPC bindings reject malformed, missing, duplicate and stale content", RegressionTests.NpcContentValidation),
    ("NPC assets animate only in simulation and conversation bindings select explicit/default entries", RegressionTests.NpcAssetsAndBindings),
    ("Authoring commands list, check branches and preview text without graphics", RegressionTests.AuthoringCommands),
    ("Character selection creates no session until confirmed and supports keyboard, pointer and back", RegressionTests.CharacterSelection),
    ("Z/X actions are edges while WASD movement stays held and focus-safe", RegressionTests.PlayerInputMapping),
    ("Player movement preserves speed across directions, ticks and both genders", RegressionTests.PlayerMovement),
    ("Player footprint sweeps prevent tunneling through furniture and room boundaries", RegressionTests.PlayerCollision),
    ("Nearby Z interaction revalidates range and captures dialogue/pause movement", RegressionTests.NearbyInteraction),
    ("Player atlas poses reach output and malformed content fails headlessly", RegressionTests.PlayerContent),
    ("Cardinal facing follows the latest dominant axis without diagonal speed changes", RegressionTests.CardinalFacing),
    ("Player animation shares one clock across turns, pauses, dialogue and session restart", RegressionTests.PlayerAnimation),
    ("Both placeholder bodies keep layers, tool anchors and West mirroring in one canvas", RegressionTests.PlayerLayers),
    ("Legacy default controls adopt Z/X without rewriting custom preferences", RegressionTests.DefaultControlsMigration),
    ("Primary and alternate keys share movement speed, edges and focus safety", RegressionTests.DualBindingInput),
    ("Both binding columns edit, reject conflicts, clear alternates, apply and restore", RegressionTests.DualBindingEditing),
    ("Binding cells map pointer input and remapped Pause / Back works across screens", RegressionTests.DualBindingPointerAndFlow),
    ("Dual bindings round-trip, upgrade old settings and reject invalid mappings", RegressionTests.DualBindingPersistence)
};
int failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception e) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {e}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} tests passed.");
return failures == 0 ? 0 : 1;
