using PawnShop.Tests;

if (args.Length > 0 && args[0] == "--dialogue") return DialogueAuthoring.Run(args[1..], Console.In, Console.Out);

// Dependency-free executable regression suite. A failure exits nonzero.
var tests = new (string Name, Action Run)[]
{
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
    ("Ink source matches build output and every visitor branch runs", RegressionTests.DialogueContent),
    ("Dialogue commands reject stale IDs, revisions and duplicate selections", RegressionTests.DialogueCommands),
    ("Ink compiler and adapter diagnose invalid graphs, tags, speakers and external calls", RegressionTests.DialogueValidation),
    ("Dialogue reveal, choice focus and long-text paging obey settings", RegressionTests.DialoguePresentation),
    ("Dialogue captures input, pauses time and closes with its session", RegressionTests.DialogueLifecycle),
    ("Data-only NPCs route clicks and keyboard Talk by identity", RegressionTests.NpcConversations),
    ("NPC bindings reject malformed, missing, duplicate and stale content", RegressionTests.NpcContentValidation),
    ("NPC assets animate only in simulation and conversation bindings select explicit/default entries", RegressionTests.NpcAssetsAndBindings),
    ("Authoring commands list, check branches and preview text without graphics", RegressionTests.AuthoringCommands)
};
int failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception e) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {e}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} tests passed.");
return failures == 0 ? 0 : 1;
