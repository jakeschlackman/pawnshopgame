using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using KeyboardInput = PawnShop.Framework.Input.KeyboardInput;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void MouseActionBindings()
    {
        MouseState Mouse(bool left = false, bool right = false) => new(0, 0, 0,
            left ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
            right ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released, ButtonState.Released);
        var input = new KeyboardInput();
        InputFrame Read(bool left = false, bool right = false, bool active = true, params Keys[] keys) =>
            input.Read(new KeyboardState(keys), Mouse(left, right), active);
        Check(Read(left: true) == default); // Synchronize first focus frame.
        Check(!Read(left: true).Interact);
        Read();
        var attack = Read(left: true);
        Check(attack.Interact && !attack.Confirm && !attack.Cancel);
        Check(!Read(true, false, true, Keys.Z).Interact);
        Check(!Read(false, false, true, Keys.Z).Interact);
        Read();
        Check(Read(false, false, true, Keys.Z).Interact);
        Check(!Read(left: true).Interact);
        Read();
        Check(Read(right: true).Cancel);
        Check(!Read(false, true, true, Keys.X).Cancel);
        Read(right: true, active: false);
        Check(Read(right: true) == default && !Read(right: true).Cancel);
        Read();
        Check(Read(right: true).Cancel);

        using var app = new ApplicationFlow();
        Begin(app);
        Read();
        var before = app.Player;
        app.Update(Read(right: true), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Pause && app.Player == before);
        app.Update(Read(right: true), TimeSpan.FromSeconds(1));
        Check(app.Screen == Screen.Pause && app.Player == before);
        Read();
        app.Update(Read(right: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Room);
        Approach(app, "visitor");
        Read();
        app.Update(Read(left: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Dialogue);
        Read();
        app.Update(Read(right: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Room);

        var model = new OptionsModel(new());
        model.Open(); model.Update(new(TabIndex: 3), TimeSpan.Zero, true);
        model.Update(new(SelectionIndex: 0, BindingIndex: 1, Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>(), Mouse(left: true));
        Check(model.CapturingKey, "The activation click must be released before capture.");
        model.Capture(Array.Empty<Keys>());
        model.Capture(Array.Empty<Keys>(), Mouse(right: true));
        Check(model.CapturingKey && model.Message!.Contains("BACK / BLOCK"));
        model.Capture(Array.Empty<Keys>(), Mouse(left: true, right: true));
        Check(model.CapturingKey);
        model.Capture(new[] { Keys.Escape });
        Check(!model.CapturingKey && model.Draft == model.Current);
        EditBinding(model, 0, 1, Keys.E);
        Check(model.Draft.InteractAlternateKey == Keys.E && model.Draft.InteractAlternateMouse == MouseBinding.None);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(Array.Empty<Keys>(), Mouse(left: true));
        Check(!model.CapturingKey && model.Draft.InteractAlternateKey == Keys.None
            && model.Draft.InteractAlternateMouse == MouseBinding.Left);
        EditBinding(model, 0, 1, Keys.Delete);
        Check(OptionsModel.AlternateLabel(model.Binding(0)) == "Unbound");
        model.Update(new(SelectionIndex: 1, BindingIndex: 1, Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(Array.Empty<Keys>(), Mouse(left: true));
        Check(!model.CapturingKey && model.Draft.PauseAlternateMouse == MouseBinding.Left && model.Draft.IsValid);
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(model.Current.PauseAlternateMouse == MouseBinding.Left);
        model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.Draft.InteractAlternateMouse == MouseBinding.Left && model.Draft.PauseAlternateMouse == MouseBinding.Right);
        model.Cancel();
        Check(model.Draft == model.Current);
        model.Update(new(SelectionIndex: 2, BindingIndex: 1, Confirm: true), TimeSpan.Zero, true);
        model.Capture(Array.Empty<Keys>());
        model.Capture(Array.Empty<Keys>(), Mouse(right: true));
        Check(model.CapturingKey && model.Message!.Contains("action alternates"));
    }
}
