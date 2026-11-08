using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using KeyboardInput = PawnShop.Framework.Input.KeyboardInput;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void DualBindingInput()
    {
        var input = new KeyboardInput();
        input.Read(new KeyboardState(), true);
        foreach (var (primary, alternate, x, y) in new[]
        {
            (Keys.W, Keys.Up, 0f, -1f), (Keys.A, Keys.Left, -1f, 0f),
            (Keys.S, Keys.Down, 0f, 1f), (Keys.D, Keys.Right, 1f, 0f)
        })
        {
            var first = input.Read(new KeyboardState(primary), true);
            var second = input.Read(new KeyboardState(alternate), true);
            var both = input.Read(new KeyboardState(primary, alternate), true);
            Check(first.MoveX == x && first.MoveY == y && second.MoveX == x && second.MoveY == y);
            Check(both.MoveX == x && both.MoveY == y, "Two bindings must not double movement.");
        }
        var opposing = input.Read(new KeyboardState(Keys.W, Keys.Down, Keys.A, Keys.Right), true);
        Check(opposing.MoveX == 0 && opposing.MoveY == 0);
        input.Bindings = new()
        {
            MoveUp = new(Keys.I, Keys.NumPad8), MoveLeft = new(Keys.J, Keys.NumPad4),
            MoveDown = new(Keys.K, Keys.NumPad2), MoveRight = new(Keys.L, Keys.NumPad6),
            Interact = new(Keys.E, Keys.F), PauseBack = new(Keys.P, Keys.O)
        };
        input.Read(new KeyboardState(), true);
        Check(input.Read(new KeyboardState(Keys.W, Keys.A, Keys.S, Keys.D, Keys.Z, Keys.X, Keys.Up, Keys.Right), true) == default);
        var diagonal = input.Read(new KeyboardState(Keys.I, Keys.NumPad6), true);
        Check(diagonal.MoveX == 1 && diagonal.MoveY == -1 && diagonal.Up && diagonal.Right);
        var interact = input.Read(new KeyboardState(Keys.F), true);
        Check(interact.Interact && interact.Confirm);
        Check(!input.Read(new KeyboardState(Keys.E, Keys.F), true).Interact);
        Check(!input.Read(new KeyboardState(Keys.E), true).Interact);
        input.Read(new KeyboardState(), true);
        Check(input.Read(new KeyboardState(Keys.E), true).Interact);
        Check(input.Read(new KeyboardState(Keys.O), true).Cancel);
        Check(!input.Read(new KeyboardState(Keys.P, Keys.O), true).Cancel);
        Check(input.Read(new KeyboardState(Keys.NumPad8, Keys.F), false) == default);
        Check(input.Read(new KeyboardState(Keys.NumPad8, Keys.F), true) == default);
        var held = input.Read(new KeyboardState(Keys.NumPad8, Keys.F), true);
        Check(held.MoveY == -1 && !held.Interact);
        Check(input.Read(new KeyboardState(Keys.Escape), true).Cancel);
        Check(input.Read(new KeyboardState(Keys.Enter), true).Confirm);
    }

    private static void EditBinding(OptionsModel model, int row, int slot, params Keys[] keys)
    {
        model.Update(new(SelectionIndex: row, BindingIndex: slot, Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(keys);
    }

    internal static void DualBindingEditing()
    {
        int writes = 0;
        var model = new OptionsModel(new(), _ => writes++);
        model.Open();
        model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
        Check(model.Fields.Count == 6 && model.Fields[1].Label == "PAUSE / BACK");
        model.Update(new(Right: true), TimeSpan.Zero, true);
        Check(model.SelectedBinding == 1 && model.Selected == 0);
        model.Update(new(Down: true), TimeSpan.Zero, true);
        Check(model.SelectedBinding == 1 && model.Selected == 1);
        model.Update(new(Left: true), TimeSpan.Zero, true);
        Check(model.SelectedBinding == 0);
        EditBinding(model, 2, 0, Keys.I);
        Check(model.Draft.MoveUp == new KeyBinding(Keys.I, Keys.Up) && model.Current.MoveUp.Primary == Keys.W);
        EditBinding(model, 2, 1, Keys.NumPad8);
        Check(model.Draft.MoveUp == new KeyBinding(Keys.I, Keys.NumPad8));
        EditBinding(model, 3, 1, Keys.I);
        Check(model.CapturingKey && model.Message!.Contains("MOVE UP") && model.Draft.MoveLeft.Alternate == Keys.Left);
        model.Capture(new[] { Keys.J, Keys.K });
        Check(model.CapturingKey && model.Message!.Contains("one supported"));
        model.Capture(new[] { Keys.Escape });
        EditBinding(model, 3, 1, Keys.Delete);
        Check(model.Draft.MoveLeft.Alternate == Keys.None && !model.CapturingKey);
        EditBinding(model, 3, 0, Keys.Delete);
        Check(model.CapturingKey && model.Draft.MoveLeft.Primary == Keys.A);
        model.Capture(new[] { Keys.Tab });
        Check(model.CapturingKey && model.Message!.Contains("reserved"));
        model.Capture(new[] { Keys.Escape });
        EditBinding(model, 0, 1, Keys.E);
        EditBinding(model, 1, 1, Keys.P);
        Check(model.Draft.InteractAlternateKey == Keys.E && model.Draft.PauseAlternateKey == Keys.P && model.Draft.IsValid);
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(writes == 1 && model.Current == model.Draft);
        EditBinding(model, 5, 0, Keys.L);
        model.Update(new(Cancel: true), TimeSpan.Zero, true);
        Check(model.Draft == model.Current && model.Current.MoveRight.Primary == Keys.D);
        model.Open(); model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
        model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.Draft == new AppSettings() && model.Current.MoveUp.Primary == Keys.I && writes == 1);
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(model.Current == new AppSettings() && writes == 2);
    }

    internal static void DualBindingPointerAndFlow()
    {
        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        app.Update(new(TabIndex: 3), TimeSpan.Zero);
        foreach (int slot in new[] { 0, 1 })
        {
            app.Options.Scroll.MoveTo(220);
            var bounds = OptionsMenuLayout.Cell(5, slot, app.Options);
            // Pointer mapping through a 2x contained Options canvas.
            var click = MenuPointerInput.HitTest(app, bounds.Center.X * 2, bounds.Center.Y * 2, 2560, 1440);
            Check(click.SelectionIndex == 5 && click.BindingIndex == slot && click.Confirm);
            app.Update(click, TimeSpan.Zero);
            Check(app.Options.CapturingKey && app.Options.SelectedBinding == slot);
            Check(MenuPointerInput.HitTest(app, bounds.Center.X, bounds.Center.Y, 1280, 720) == default);
            app.Options.Capture(Array.Empty<Keys>()); app.Options.Capture(new[] { Keys.Escape });
        }
        var back = OptionsMenuLayout.Footer(2).Center;
        app.Update(MenuPointerInput.HitTest(app, back.X, back.Y, 1280, 720), TimeSpan.Zero);
        Check(app.Screen == Screen.Title && app.LiveSessions == 0);

        var settings = new AppSettings { InteractAlternateKey = Keys.E, PauseKey = Keys.P, PauseAlternateKey = Keys.O };
        using var room = new ApplicationFlow(settings);
        Begin(room);
        var input = new KeyboardInput { Bindings = settings.Bindings };
        input.Read(new KeyboardState(), true);
        var before = room.Player;
        room.Update(input.Read(new KeyboardState(Keys.O, Keys.Right), true), TimeSpan.FromSeconds(1));
        Check(room.Screen == Screen.Pause && room.Player == before);
        room.Update(input.Read(new KeyboardState(), true), TimeSpan.Zero);
        room.Update(input.Read(new KeyboardState(Keys.P), true), TimeSpan.Zero);
        Check(room.Screen == Screen.Room && room.Player == before);
        room.Update(input.Read(new KeyboardState(Keys.Right), true), TimeSpan.FromSeconds(.5));
        Check(room.Player!.Value.Position.X > before!.Value.Position.X);
        Approach(room, "visitor");
        room.Update(input.Read(new KeyboardState(Keys.E), true), TimeSpan.FromSeconds(1));
        Check(room.Screen == Screen.Dialogue);
        room.Update(input.Read(new KeyboardState(Keys.O), true), TimeSpan.FromSeconds(1));
        Check(room.Screen == Screen.Room);
        // The same remapped Back key also leaves sessionless character selection.
        using var selection = new ApplicationFlow(settings);
        Confirm(selection);
        input.Read(new KeyboardState(), true);
        selection.Update(input.Read(new KeyboardState(Keys.P), true), TimeSpan.Zero);
        Check(selection.Screen == Screen.Title && selection.SessionsCreated == 0);
    }

    internal static void DualBindingPersistence()
    {
        string directory = Path.Combine(Path.GetTempPath(), "PawnShopDualBindings-" + Guid.NewGuid());
        string path = Path.Combine(directory, "settings.json");
        Directory.CreateDirectory(directory);
        try
        {
            var store = new SettingsStore(path);
            var settings = new AppSettings
            {
                MoveUp = new(Keys.I, Keys.NumPad8), MoveLeft = new(Keys.J, Keys.NumPad4),
                MoveDown = new(Keys.K, Keys.NumPad2), MoveRight = new(Keys.L, Keys.NumPad6),
                InteractKey = Keys.E, InteractAlternateKey = Keys.F, PauseKey = Keys.P, PauseAlternateKey = Keys.O
            };
            store.Save(settings);
            Check(store.Load(out var warning) == settings && warning is null);
            store.Save(settings with { MoveUp = new(Keys.I) });
            Check(store.Load(out warning).MoveUp.Alternate == Keys.None && warning is null && File.Exists(path + ".bak"));
            // A newly saved Space/Escape choice is no longer mistaken for legacy defaults.
            store.Save(new AppSettings { InteractKey = Keys.Space, PauseKey = Keys.Escape });
            Check(store.Load(out warning).InteractKey == Keys.Space && warning is null);
            const string legacy = "{\"InteractKey\":\"W\",\"PauseKey\":\"Up\",\"Music\":25}";
            File.WriteAllText(path, legacy);
            var migrated = store.Load(out warning);
            Check(migrated.IsValid && migrated.InteractKey == Keys.W && migrated.PauseKey == Keys.Up && migrated.Music == 25);
            Check(warning is not null && File.ReadAllText(path) == legacy);
            Check(!new AppSettings { MoveUp = new(Keys.None) }.IsValid);
            Check(!new AppSettings { MoveUp = new(Keys.W, Keys.W) }.IsValid);
            Check(!new AppSettings { MoveUp = new(Keys.Z) }.IsValid);
            Check(!new AppSettings { PauseAlternateKey = Keys.Enter }.IsValid);
            Check(!new AppSettings { MoveUp = new(Keys.Tab) }.IsValid);
            Check(!new AppSettings { MoveUp = new((Keys)9999) }.IsValid);
            const string duplicate = "{\"MoveUp\":{\"Primary\":\"Z\",\"Alternate\":\"Up\"}}";
            File.WriteAllText(path, duplicate);
            Check(store.Load(out warning) == new AppSettings() && warning is not null && File.ReadAllText(path) == duplicate);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
