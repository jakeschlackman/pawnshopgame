using PawnShop.Audio;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsTabDefaults()
    {
        var defaults = new AppSettings();
        var initial = defaults with
        {
            Display = new(1920, 1080, WindowMode.Fullscreen), UiScale = 150,
            Master = 35, Music = 40, Ambient = 45, Effects = 50, Combat = 55, Monster = 60,
            MuteUnfocused = false, PauseUnfocused = false, ScreenShake = false, Dialogue = DialogueSpeed.Fast,
            MoveUp = new(Keys.I, Keys.NumPad8), MoveLeft = new(Keys.J, Keys.NumPad4),
            MoveDown = new(Keys.K, Keys.NumPad2), MoveRight = new(Keys.L, Keys.NumPad6),
            InteractKey = Keys.E, InteractAlternateKey = Keys.Enter, InteractAlternateMouse = MouseBinding.None,
            PauseKey = Keys.P, PauseAlternateKey = Keys.Escape, PauseAlternateMouse = MouseBinding.None
        };
        Check(initial.IsValid);
        for (int tab = 0; tab < 4; tab++)
        {
            int writes = 0;
            AppSettings? saved = null;
            var model = new OptionsModel(initial, settings => { saved = settings; writes++; });
            model.Open();
            // Leave pending edits on every tab before resetting one of them.
            model.Update(new(SelectionIndex: 1, Right: true), TimeSpan.Zero, true);
            model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
            model.Update(new(SelectionIndex: 0, SliderValue: 25), TimeSpan.Zero, true);
            model.Update(new(TabIndex: 2), TimeSpan.Zero, true);
            model.Update(new(SelectionIndex: 2, Right: true), TimeSpan.Zero, true);
            model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
            EditBinding(model, 0, 0, Keys.O);
            var before = model.Draft;
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            model.Update(new(SelectionIndex: 1), TimeSpan.Zero, true);
            model.Scroll.MoveTo(model.Scroll.Maximum);
            int offset = model.Scroll.Offset;
            model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
            var after = model.Draft;
            var display = tab == 0 ? defaults : before;
            var audio = tab == 1 ? defaults : before;
            var comfort = tab == 2 ? defaults : before;
            var controls = tab == 3 ? defaults : before;
            Check(after.Display == display.Display);
            Check((after.Master, after.Music, after.Ambient, after.Effects, after.Combat, after.Monster, after.MuteUnfocused)
                == (audio.Master, audio.Music, audio.Ambient, audio.Effects, audio.Combat, audio.Monster, audio.MuteUnfocused));
            Check((after.PauseUnfocused, after.ScreenShake, after.Dialogue)
                == (comfort.PauseUnfocused, comfort.ScreenShake, comfort.Dialogue));
            foreach (var action in OptionsModel.ControlActions)
                Check(after.Bindings.Get(action) == controls.Bindings.Get(action));
            Check(after.UiScale == 150 && after.Version == initial.Version
                && after.ActionAlternateDefaultsVersion == initial.ActionAlternateDefaultsVersion && after.IsValid);
            Check(model.Current == initial && writes == 0 && model.IsDirty && !model.Previewing);
            Check(model.Tab == tab && model.IndicatorRow == 1 && model.Scroll.Offset == offset);
            model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
            Check(model.Previewing && writes == 0 && model.Current == initial);
            model.Update(new(Confirm: true), TimeSpan.Zero, true);
            Check(writes == 1 && saved == after && model.Current == after && !model.IsDirty);
        }
    }

    private static void OpenOptions(ApplicationFlow app)
    {
        if (app.OptionsFlight.IsBusy && app.OptionsFlight.IsReturning) FinishOptionsFlight(app);
        else if (app.Screen == Screen.Title && app.TitleCamera.IsMoving)
            app.Update(default, PawnShop.Framework.Rendering.PanoramaCamera.TransitionDuration);
        app.Update(new(SelectionIndex: 2), TimeSpan.Zero);
        Confirm(app);
    }
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
        Check(AppSettings.Resolutions[0] == (1280, 720));
        Check(AppSettings.Resolutions.All(size => size.Width >= AppSettings.MinimumWidth && size.Height >= AppSettings.MinimumHeight));
        var minimum = new OptionsModel(new());
        minimum.Open();
        minimum.Update(new(SelectionIndex: 1, Left: true), TimeSpan.Zero, true);
        Check(minimum.Draft.Display.Width == 3840 && minimum.Draft.Display.Height == 2160);
        minimum.Update(new(Right: true), TimeSpan.Zero, true);
        Check(minimum.Draft.Display == new DisplaySettings(1280, 720));
        minimum.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(minimum.DropdownChoice == 0 && minimum.ChoiceLabel(0) == "1280 X 720");
        // Removing the scale row moves Apply directly after Resolution, while
        // retaining previously saved scale values for compatibility.
        int scaleWrites = 0;
        var retainedScale = new OptionsModel(new() { UiScale = 125 }, _ => scaleWrites++);
        retainedScale.Open();
        Check(retainedScale.FieldCount == 2 && retainedScale.Fields.Count == 2);
        Check(retainedScale.Fields.Select(field => field.Label).SequenceEqual(new[] { "WINDOW MODE", "RESOLUTION" }));
        retainedScale.Update(new(Down: true), TimeSpan.Zero, true);
        retainedScale.Update(new(Down: true), TimeSpan.Zero, true);
        retainedScale.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(scaleWrites == 1 && retainedScale.Current.UiScale == 125 && !retainedScale.Previewing);

        int writes = 0;
        var model = new OptionsModel(new(), _ => writes++);
        var original = model.Current;
        EditDisplay(model);
        Check(model.Previewing && model.DisplayForHost != original.Display && writes == 0);
        model.Update(default, TimeSpan.FromSeconds(16), true);
        Check(!model.Previewing && model.Current == original && model.DisplayForHost == original.Display);
        Check(model.Message is null);
        EditDisplay(model);
        model.Update(new(Confirm: true), TimeSpan.FromSeconds(1), false);
        Check(!model.Previewing && writes == 0 && model.DisplayForHost == original.Display);
        Check(model.Message is null);
        EditDisplay(model);
        model.Update(new(Cancel: true), TimeSpan.Zero, true);
        Check(!model.Previewing && writes == 0);
        Check(model.Message is null);
        EditDisplay(model);
        var candidate = model.Draft;
        model.Update(new(Confirm: true), TimeSpan.FromSeconds(1), true);
        Check(!model.Previewing && writes == 1 && model.Current == candidate);
        Check(model.Message is null);
        Check(AppSettings.Resolutions.Contains((3840, 2160)));

        var dialog = OptionsMenuLayout.DisplayConfirmation;
        Check(OptionsMenuLayout.Panel.Contains(dialog) && dialog.Center.X == 640);
        Check(dialog.Contains(OptionsMenuLayout.Keep) && dialog.Contains(OptionsMenuLayout.Revert));
        Check(!OptionsMenuLayout.Keep.Intersects(OptionsMenuLayout.Revert));
        Check(OptionsMenuLayout.ConfirmationRailTop - OptionsMenuLayout.Keep.Bottom >= 12
            && OptionsMenuLayout.Revert.Bottom == OptionsMenuLayout.Keep.Bottom);
        Check(OptionsMenuLayout.Keep.Left >= dialog.Left + 64 && OptionsMenuLayout.Revert.Right <= dialog.Right - 64);
        foreach (int scale in new[] { 100, 125, 150 })
        {
            int saved = 0;
            using var app = new ApplicationFlow(new() { UiScale = scale }, _ => saved++);
            OpenOptions(app); FinishOptionsFlight(app);
            foreach (var size in new[] { (960, 540), (1280, 720), (2560, 1440), (3440, 1440) })
            {
                PawnShop.Framework.Input.InputFrame Hit(Microsoft.Xna.Framework.Point point)
                {
                    double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
                    return MenuPointerInput.HitTest(app,
                        (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(point.X * ratio),
                        (size.Item2 - (int)(720 * ratio)) / 2 + (int)(point.Y * ratio), size.Item1, size.Item2);
                }
                EditDisplay(app.Options);
                int beforeSave = saved;
                var pending = app.Options.Draft.Display;
                Check(Hit(OptionsMenuLayout.Tab(1).Center) == default);
                Check(Hit(OptionsMenuLayout.Footer(1).Center) == default);
                app.Update(Hit(OptionsMenuLayout.Keep.Center), TimeSpan.Zero);
                Check(!app.Options.Previewing && app.Options.Current.Display == pending
                    && saved == beforeSave + 1 && app.Options.Message is null);
                var kept = app.Options.Current.Display;
                EditDisplay(app.Options);
                app.Update(Hit(OptionsMenuLayout.Revert.Center), TimeSpan.Zero);
                Check(!app.Options.Previewing && app.Options.Current.Display == kept
                    && app.Options.DisplayForHost == kept && saved == beforeSave + 1 && app.Options.Message is null);
            }
            EditDisplay(app.Options);
            var pointer = new MenuPointerInput();
            var keep = OptionsMenuLayout.Keep.Center;
            var revert = OptionsMenuLayout.Revert.Center;
            pointer.Read(app, new MouseState(keep.X, keep.Y, 0, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720);
            Check(app.Options.HoveredModalAction == 0);
            pointer.Read(app, new MouseState(revert.X, revert.Y, 0, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720);
            Check(app.Options.HoveredModalAction == 1);
            app.Options.RejectDisplay("Display mode unavailable. Previous display restored.");
            Check(app.Options.Message!.Contains("unavailable"));
            app.Update(new(Confirm: true), TimeSpan.Zero);
            Check(app.Options.Message is null, "A successful direct Apply must clear routine status text.");
        }
    }
    internal static void SettingsPersistence()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PawnShopOptionsTests", Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "options.json");
        try
        {
            var store = new SettingsStore(path);
            Check(store.Load(out var warning) == new AppSettings() && warning is null);
            Check(File.Exists(path) && !File.Exists(path + ".bak") && !File.Exists(path + ".tmp"));
            string defaultJson = File.ReadAllText(path);
            Check(store.Load(out warning) == new AppSettings() && warning is null && File.ReadAllText(path) == defaultJson);
            Check(!File.Exists(path + ".bak"), "Loading existing defaults must not rewrite the settings file.");
            string blockedDirectory = Path.Combine(directory, "blocked");
            File.WriteAllText(blockedDirectory, "Not a directory");
            try
            {
                var blocked = new SettingsStore(Path.Combine(blockedDirectory, "settings.json"));
                Check(blocked.Load(out warning) == new AppSettings() && warning is not null);
                Check(File.ReadAllText(blockedDirectory) == "Not a directory");
            }
            finally { File.Delete(blockedDirectory); }
            var settings = new AppSettings { Display = new(3840, 2160, WindowMode.Fullscreen), UiScale = 150,
                Music = 20, Combat = 35, Monster = 65, ScreenShake = false, Dialogue = DialogueSpeed.Instant, InteractKey = Keys.E };
            store.Save(settings);
            Check(store.Load(out warning) == settings && warning is null);
            store.Save(settings with { Master = 0 });
            Check(File.Exists(path + ".bak") && store.Load(out _)!.Master == 0);
            const string oldSmallDisplay = "{\"Display\":{\"Width\":960,\"Height\":540,\"Mode\":\"Fullscreen\"},\"Music\":35,\"ScreenShake\":false}";
            File.WriteAllText(path, oldSmallDisplay);
            var raised = store.Load(out warning);
            Check(warning is null && raised.Display == new DisplaySettings(1280, 720, WindowMode.Fullscreen)
                && raised.Music == 35 && !raised.ScreenShake && File.ReadAllText(path) == oldSmallDisplay);
            store.Save(raised);
            Check(store.Load(out warning) == raised && warning is null && File.ReadAllText(path + ".bak") == oldSmallDisplay);
            // Additive volume fields keep existing version-1 preferences valid and untouched.
            const string oldAudio = "{\"Version\":1,\"Music\":25,\"Effects\":45}";
            File.WriteAllText(path, oldAudio);
            var upgraded = store.Load(out warning);
            Check(warning is null && upgraded.IsValid && upgraded.Music == 25 && upgraded.Effects == 45
                && upgraded.Combat == 80 && upgraded.Monster == 80 && upgraded.ScreenShake && File.ReadAllText(path) == oldAudio);
            const string corrupt = "{broken";
            File.WriteAllText(path, corrupt);
            Check(store.Load(out warning) == new AppSettings() && warning is not null);
            Check(File.ReadAllText(path) == corrupt);
            File.WriteAllText(path, "{\"Version\":999}");
            Check(store.Load(out warning) == new AppSettings() && warning is not null);
            Check(File.ReadAllText(path).Contains("999"));
            Check(!new AppSettings { Master = -1 }.IsValid);
            foreach (int invalid in new[] { -1, 101 })
            {
                Check(!new AppSettings { Combat = invalid }.IsValid);
                Check(!new AppSettings { Monster = invalid }.IsValid);
            }
            Check(!new AppSettings { Display = new(8000, 4000) }.IsValid);
            Check(!new AppSettings { Display = new(960, 540) }.IsValid);
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
        var settings = new AppSettings { Master = 50, Effects = 40, Music = 0, Combat = 60, Monster = 30 };
        Check(Math.Abs(settings.Gain(AudioChannel.Effects, true) - .2f) < .0001);
        Check(settings.Gain(AudioChannel.Music, true) == 0 && settings.Gain(AudioChannel.Effects, false) == 0);
        Check((settings with { MuteUnfocused = false }).Gain(AudioChannel.Effects, false) > 0);
        Check(Math.Abs(settings.Gain(AudioChannel.Combat, true) - .3f) < .0001);
        Check(Math.Abs(settings.Gain(AudioChannel.Monster, true) - .15f) < .0001);
        Check((settings with { Effects = 0 }).Gain(AudioChannel.Combat, true) == settings.Gain(AudioChannel.Combat, true));
        Check((settings with { Effects = 0 }).Gain(AudioChannel.Monster, true) == settings.Gain(AudioChannel.Monster, true));
        foreach (var channel in new[] { AudioChannel.Combat, AudioChannel.Monster })
        {
            Check(settings.Gain(channel, false) == 0);
            Check((settings with { Master = 0 }).Gain(channel, true) == 0);
            Check((settings with { MuteUnfocused = false }).Gain(channel, false) == settings.Gain(channel, true));
        }
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
