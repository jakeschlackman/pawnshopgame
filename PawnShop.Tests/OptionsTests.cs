using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private static void OpenOptions(ApplicationFlow app) { Down(app); Down(app); Confirm(app); }
    private static void Select(OptionsModel model, int row)
    {
        while (model.Selected != row) model.Update(new(Down: true), TimeSpan.Zero, true);
    }
    private static void EditDisplay(OptionsModel model)
    {
        model.Open();
        Select(model, 1);
        model.Update(new(Right: true), TimeSpan.Zero, true);
        Select(model, model.FieldCount);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
    }
    internal static void OptionsLifecycle()
    {
        using var app = new ApplicationFlow();
        OpenOptions(app);
        Check(app.Screen == Screen.Options && app.LiveSessions == 0);
        Step(app, new(Interact: true));
        Back(app);
        Check(app.Screen == Screen.Title && app.LiveSessions == 0);
        Begin(app); Back(app);
        Down(app); Down(app); Down(app); Confirm(app);
        var room = app.Room;
        Check(app.Screen == Screen.Options);
        Step(app, new(Interact: true));
        Step(app, new(Cancel: true, Interact: true));
        Check(app.Screen == Screen.Pause && app.Room == room);
        Confirm(app);
        Check(app.Screen == Screen.Room && app.Room == room);
    }
    internal static void DisplayPreview()
    {
        int writes = 0;
        var model = new OptionsModel(new(), _ => writes++);
        var original = model.Current;
        EditDisplay(model);
        Check(model.Previewing && model.DisplayForHost != original.Display && writes == 0);
        model.Update(default, TimeSpan.FromSeconds(16), true);
        Check(!model.Previewing && model.Current == original && model.DisplayForHost == original.Display);
        EditDisplay(model);
        model.Update(new(Confirm: true), TimeSpan.FromSeconds(1), false);
        Check(!model.Previewing && writes == 0 && model.DisplayForHost == original.Display);
        EditDisplay(model);
        model.Update(new(Cancel: true), TimeSpan.Zero, true);
        Check(!model.Previewing && writes == 0);
        EditDisplay(model);
        var candidate = model.Draft;
        model.Update(new(Confirm: true), TimeSpan.FromSeconds(1), true);
        Check(!model.Previewing && writes == 1 && model.Current == candidate);
        Check(AppSettings.Resolutions.Contains((3840, 2160)));
    }
    internal static void SettingsPersistence()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PawnShopOptionsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "options.json");
        try
        {
            var store = new SettingsStore(path);
            Check(store.Load(out var warning) == new AppSettings() && warning is null);
            var settings = new AppSettings { Display = new(3840, 2160, WindowMode.Fullscreen), UiScale = 150,
                Music = 20, Dialogue = DialogueSpeed.Instant, InteractKey = Keys.E };
            store.Save(settings);
            Check(store.Load(out warning) == settings && warning is null);
            store.Save(settings with { Master = 0 });
            Check(File.Exists(path + ".bak") && store.Load(out _)!.Master == 0);
            const string corrupt = "{broken";
            File.WriteAllText(path, corrupt);
            Check(store.Load(out warning) == new AppSettings() && warning is not null);
            Check(File.ReadAllText(path) == corrupt);
            File.WriteAllText(path, "{\"Version\":999}");
            Check(store.Load(out warning) == new AppSettings() && warning is not null);
            Check(File.ReadAllText(path).Contains("999"));
            Check(!new AppSettings { Master = -1 }.IsValid);
            Check(!new AppSettings { Display = new(8000, 4000) }.IsValid);
        }
        finally
        {
            foreach (string name in new[] { path, path + ".bak", path + ".tmp" })
                if (File.Exists(name)) File.Delete(name);
            Directory.Delete(directory);
        }
    }
    internal static void Bindings()
    {
        var model = new OptionsModel(new());
        model.Open();
        for (int i = 0; i < 3; i++) model.Update(new(NextTab: true), TimeSpan.Zero, true);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        model.Capture(new[] { Keys.Enter });
        Check(model.CapturingKey && model.Draft.InteractKey == Keys.Z);
        model.Capture(Array.Empty<Keys>());
        model.Capture(new[] { Keys.W });
        Check(model.CapturingKey && model.Draft.InteractKey == Keys.Z && model.Message!.Contains("Already used"));
        model.Capture(new[] { Keys.Escape });
        Check(!model.CapturingKey && model.Draft.InteractKey == Keys.Z);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(new[] { Keys.E });
        Check(!model.CapturingKey && model.Draft.InteractKey == Keys.E);
        Select(model, 1);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(new[] { Keys.E });
        Check(model.CapturingKey && model.Draft.PauseKey == Keys.X);
        model.Capture(new[] { Keys.P });
        Check(!model.CapturingKey && model.Draft.PauseKey == Keys.P);
        Select(model, model.FieldCount);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(model.Current.InteractKey == Keys.E && model.Current.PauseKey == Keys.P);
    }
    internal static void Preferences()
    {
        var settings = new AppSettings { Master = 50, Effects = 40, Music = 0 };
        Check(Math.Abs(settings.Gain(AudioChannel.Effects, true) - .2f) < .0001);
        Check(settings.Gain(AudioChannel.Music, true) == 0 && settings.Gain(AudioChannel.Effects, false) == 0);
        Check((settings with { MuteUnfocused = false }).Gain(AudioChannel.Effects, false) > 0);
        Check(settings.RevealedCharacters(TimeSpan.FromSeconds(1), 100) == 30);
        Check((settings with { Dialogue = DialogueSpeed.Slow }).RevealedCharacters(TimeSpan.FromSeconds(1), 100) == 15);
        Check((settings with { Dialogue = DialogueSpeed.Fast }).RevealedCharacters(TimeSpan.FromSeconds(1), 100) == 60);
        Check((settings with { Dialogue = DialogueSpeed.Instant }).RevealedCharacters(TimeSpan.Zero, 100) == 100);
        using var app = new ApplicationFlow(settings with { PauseUnfocused = false });
        Begin(app);
        app.Update(new(Interact: true), TimeSpan.FromSeconds(5), false);
        Check(app.Screen == Screen.Room && app.Room!.Value.Elapsed.TotalSeconds == 5 && app.Room.Value.BellRings == 0);
    }
    internal static void SettingsFailure()
    {
        var model = new OptionsModel(new(), _ => throw new IOException("Injected failed write"));
        var previous = model.Current;
        EditDisplay(model);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(model.Current == previous && model.DisplayForHost == previous.Display && !model.Previewing);
        Check(model.Message!.Contains("Could not save"));
    }
}
