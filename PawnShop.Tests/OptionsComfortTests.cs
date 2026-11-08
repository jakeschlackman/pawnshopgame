using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsComfortControls()
    {
        int writes = 0;
        var model = new OptionsModel(new(), _ => writes++);
        model.Open();
        model.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        Check(model.Fields.Select(field => field.Label).SequenceEqual(new[]
            { "PAUSE WHEN UNFOCUSED", "SCREEN SHAKE", "DIALOGUE TEXT SPEED" }));
        Check(model.Draft.ScreenShake);
        model.Update(new(SelectionIndex: 1, Confirm: true), TimeSpan.Zero, true);
        Check(!model.Draft.ScreenShake && model.Current.ScreenShake);
        model.Update(new(Left: true), TimeSpan.Zero, true);
        Check(model.Draft.ScreenShake);
        model.Update(new(Right: true), TimeSpan.Zero, true);
        Check(!model.Draft.ScreenShake);
        model.Update(new(SelectionIndex: 0), TimeSpan.Zero, true);
        model.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(!model.Draft.PauseUnfocused && model.Current.PauseUnfocused);
        model.Update(new(Left: true), TimeSpan.Zero, true);
        Check(model.Draft.PauseUnfocused);
        model.Update(new(Right: true), TimeSpan.Zero, true);
        Check(!model.Draft.PauseUnfocused);
        foreach (var speed in Enum.GetValues<DialogueSpeed>())
        {
            model.Update(new(SelectionIndex: 2, Confirm: true), TimeSpan.FromSeconds(1), true);
            Check(model.DropdownRow == 2 && model.DropdownCount == 4 && model.DropdownChoice == (int)model.Draft.Dialogue);
            Check(model.ChoiceLabel((int)speed) == speed.ToString());
            var before = model.Draft;
            model.Update(new(Down: true), TimeSpan.Zero, true);
            Check(model.Draft == before && writes == 0);
            model.Update(new(ChoiceIndex: (int)speed, Confirm: true), TimeSpan.Zero, true);
            Check(model.Draft.Dialogue == speed && model.TextElapsed == TimeSpan.Zero && model.DropdownRow is null);
            Check(model.Draft.Display == model.Current.Display && !model.Previewing);
        }
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(writes == 1 && !model.Current.PauseUnfocused && model.Current.Dialogue == DialogueSpeed.Instant);
        Check(!model.Current.ScreenShake);
        model.Update(new(SelectionIndex: 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.Draft.ScreenShake);
        model.Update(new(SelectionIndex: 2, Confirm: true), TimeSpan.Zero, true);
        model.Update(new(ChoiceIndex: 0, Confirm: true), TimeSpan.Zero, true);
        Check(!model.Update(new(Cancel: true), TimeSpan.Zero, true) && model.ConfirmingDiscard);
        Check(model.Update(new(Confirm: true, ChoiceIndex: 1), TimeSpan.Zero, true));
        Check(model.Draft == model.Current);
        model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.Draft.PauseUnfocused && model.Draft.ScreenShake
            && model.Draft.Dialogue == DialogueSpeed.Normal && writes == 1);
        model.Cancel();

        foreach (int scale in new[] { 100, 125, 150 })
        {
            using var app = new ApplicationFlow(new() { UiScale = scale });
            OpenOptions(app); FinishOptionsFlight(app);
            app.Update(new(TabIndex: 2), TimeSpan.Zero);
            var checkbox = OptionsMenuLayout.Checkbox(0, app.Options);
            Check(checkbox.Size == OptionsMenuLayout.AudioCheckbox(app.Options).Size);
            Check(OptionsMenuLayout.ValueBounds(0, app.Options).Contains(checkbox));
            var label = OptionsMenuLayout.Field(0, app.Options);
            var whitespace = OptionsMenuLayout.ValueBounds(0, app.Options);
            Check(!MenuPointerInput.HitTest(app, whitespace.Right - 8, checkbox.Center.Y, 1280, 720).Confirm);
            foreach (var size in new[] { (960, 540), (1280, 720), (2560, 1440), (3440, 1440) })
            {
                PawnShop.Framework.Input.InputFrame Click(int x, int y)
                {
                    double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
                    return MenuPointerInput.HitTest(app,
                        (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(x * ratio),
                        (size.Item2 - (int)(720 * ratio)) / 2 + (int)(y * ratio), size.Item1, size.Item2);
                }
                app.Update(Click(checkbox.Center.X, checkbox.Center.Y), TimeSpan.Zero);
                Check(!app.Options.Draft.PauseUnfocused);
                app.Update(Click(label.X + 20, label.Center.Y), TimeSpan.Zero);
                Check(app.Options.Draft.PauseUnfocused);
                var shake = OptionsMenuLayout.Checkbox(1, app.Options);
                Check(OptionsMenuLayout.Viewport.Contains(shake));
                var shakeWhitespace = OptionsMenuLayout.ValueBounds(1, app.Options);
                Check(!Click(shakeWhitespace.Right - 8, shake.Center.Y).Confirm);
                app.Update(Click(shake.Center.X, shake.Center.Y), TimeSpan.Zero);
                Check(!app.Options.Draft.ScreenShake && app.Options.Current.ScreenShake && app.Options.IndicatorRow == 1);
                var shakeLabel = OptionsMenuLayout.Field(1, app.Options);
                app.Update(Click(shakeLabel.X + 20, shakeLabel.Center.Y), TimeSpan.Zero);
                Check(app.Options.Draft.ScreenShake);
                var button = OptionsMenuLayout.ValueBounds(2, app.Options);
                app.Update(Click(button.Center.X, button.Center.Y), TimeSpan.Zero);
                Check(app.Options.DropdownRow == 2 && OptionsMenuLayout.Viewport.Contains(OptionsMenuLayout.Dropdown(app.Options)));
                app.Update(new(Up: true), TimeSpan.Zero);
                Check(app.Options.DropdownChoice == 0);
                app.Update(new(ScrollDelta: 48), TimeSpan.Zero);
                Check(app.Options.Scroll.Offset == 0);
                var item = OptionsMenuLayout.DropdownItem(0, app.Options);
                int choice = app.Options.DropdownFirst;
                app.Update(Click(item.Center.X, item.Center.Y), TimeSpan.Zero);
                Check(app.Options.Draft.Dialogue == (DialogueSpeed)choice && app.Options.DropdownRow is null);
                app.Update(new(SelectionIndex: 2, Confirm: true), TimeSpan.Zero);
                var before = app.Options.Draft;
                var footer = OptionsMenuLayout.Footer(0);
                app.Update(Click(footer.Center.X, footer.Center.Y), TimeSpan.Zero);
                Check(app.Options.DropdownRow is null && app.Options.Current.Dialogue == DialogueSpeed.Normal);
                app.Update(new(Confirm: true), TimeSpan.Zero);
                Check(!app.Options.Update(new(Cancel: true), TimeSpan.Zero, true) && app.Options.Draft == before);
                app.Update(new(Confirm: true), TimeSpan.Zero);
                app.Options.Update(default, TimeSpan.Zero, false);
                Check(app.Options.DropdownRow is null);
                app.Options.Cancel();
            }
        }
    }
}
