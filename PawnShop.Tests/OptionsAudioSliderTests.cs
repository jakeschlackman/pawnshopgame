using PawnShop.Audio;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsAudioSliders()
    {
        var model = new OptionsModel(new());
        model.Open();
        model.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        var original = model.Current;
        Check(model.FieldCount == 7 && model.Fields[4].Label == "COMBAT" && model.Fields[5].Label == "MONSTER");
        for (int row = 0; row < OptionsModel.AudioVolumeCount; row++)
        {
            model.Update(new(SelectionIndex: row, SliderValue: 50), TimeSpan.Zero, true);
            Check(model.Volume(row) == 50 && model.Current == original && model.IndicatorRow == row);
            Check(model.TakeSliderClick());
            model.Update(new(Right: true), TimeSpan.Zero, true);
            Check(model.Volume(row) == 55);
            Check(model.TakeSliderClick());
            model.Update(new(Left: true), TimeSpan.Zero, true);
            Check(model.Volume(row) == 50);
            Check(model.TakeSliderClick());
            model.Update(new(SliderValue: -100), TimeSpan.Zero, true);
            Check(model.Volume(row) == 0);
            var empty = OptionsMenuLayout.AudioThumb(row, model);
            var fill = OptionsMenuLayout.AudioFill(row, model);
            Check(empty.X + 14 == fill.Left);
            model.Update(new(SliderValue: 200), TimeSpan.Zero, true);
            Check(model.Volume(row) == 100 && OptionsMenuLayout.AudioThumb(row, model).X + 14 == fill.Right);
            Check(model.TakeSliderClick() && model.TakeCheckboxSwitch() is null,
                "Every slider uses rotary feedback, including Music, Ambient, Combat and Monster.");
            model.Update(new(SliderValue: 100), TimeSpan.Zero, true);
            Check(!model.TakeSliderClick(), "Holding a stationary watch must not restart its click.");
            model.Update(new(SliderValue: 20), TimeSpan.Zero, false);
            Check(model.Volume(row) == 100);
            Check(OptionsMenuLayout.Field(row, model).Contains(OptionsMenuLayout.AudioTrack(row, model)));
            Check(OptionsMenuLayout.ValueBounds(row, model).Contains(OptionsMenuLayout.AudioThumb(row, model)));
        }
        model.Update(new(SelectionIndex: model.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(model.Current.Master == 100 && model.Current.Music == 100 && model.Current.Ambient == 100 && model.Current.Effects == 100);
        Check(model.Current.Combat == 100 && model.Current.Monster == 100);
        model.Update(new(SelectionIndex: 4, SliderValue: 15), TimeSpan.Zero, true);
        model.Update(new(SelectionIndex: 5, SliderValue: 35), TimeSpan.Zero, true);
        Check(model.Draft.Combat == 15 && model.Draft.Monster == 35 && model.Draft.Effects == 100);
        model.Cancel();
        Check(model.Draft.Combat == 100 && model.Draft.Monster == 100);
        model.Update(new(SelectionIndex: model.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
        Check(model.Draft.Combat == 80 && model.Draft.Monster == 80 && model.Current.Combat == 100);
        model.Cancel();
        model.Update(new(SelectionIndex: 0, SliderValue: 15), TimeSpan.Zero, true);
        model.Cancel();
        Check(model.Draft == model.Current && model.Draft.Master == 100);
        model.Update(new(TabIndex: 0), TimeSpan.Zero, true);
        original = model.Draft;
        model.Update(new(SliderValue: 1), TimeSpan.Zero, true);
        Check(model.Draft == original);

        using var app = new ApplicationFlow();
        OpenOptions(app); FinishOptionsFlight(app);
        app.Update(new(TabIndex: 1), TimeSpan.Zero);
        var pointer = new MenuPointerInput();
        InputFrame Read(int x, int y, bool down = false, bool active = true) => pointer.Read(app,
            new MouseState(x, y, 0, down ? ButtonState.Pressed : ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
            active, 1280, 720, 1280, 720);
        var trackFill = OptionsMenuLayout.AudioFill(0, app.Options);
        Read(trackFill.Left, trackFill.Center.Y);
        app.Update(Read(trackFill.Left, trackFill.Center.Y, true), TimeSpan.Zero);
        Check(app.Options.Draft.Master == 0 && app.Options.TakeSliderClick());
        app.Update(Read(trackFill.Right + 100, trackFill.Center.Y, true), TimeSpan.Zero);
        Check(app.Options.Draft.Master == 100 && app.Options.Current.Master == 80 && app.Options.TakeSliderClick());
        Read(trackFill.Right, trackFill.Center.Y);
        var watch = OptionsMenuLayout.AudioThumb(0, app.Options);
        int grabX = watch.X + 6;
        app.Update(Read(grabX, watch.Center.Y, true), TimeSpan.Zero);
        Check(app.Options.Draft.Master == 100 && !app.Options.TakeSliderClick(), "Grabbing away from the dial center must not jump the volume or click.");
        app.Update(Read(grabX - trackFill.Width / 2, watch.Center.Y, true), TimeSpan.Zero);
        Check(app.Options.Draft.Master == 50 && app.Options.IndicatorRow == 0 && app.Options.TakeSliderClick());
        Read(grabX, watch.Center.Y, true, active: false);
        Check(Read(trackFill.Left, trackFill.Center.Y, true) == default);
        Check(app.Options.Draft.Master == 50);
        Read(trackFill.Left, trackFill.Center.Y);

        app.Options.Scroll.MoveTo(app.Options.Scroll.Maximum);
        var hidden = OptionsMenuLayout.AudioTrack(0, app.Options).Center;
        Check(MenuPointerInput.HitTest(app, hidden.X, OptionsMenuLayout.Viewport.Top - 1, 1280, 720).SliderValue is null);
        var scrolledFill = OptionsMenuLayout.AudioFill(5, app.Options);
        int targetX = scrolledFill.X + scrolledFill.Width * 75 / 100;
        foreach (var size in new[] { (960, 540), (1280, 720), (2560, 1440), (3440, 1440) })
        {
            double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
            int x = (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(targetX * ratio);
            int y = (size.Item2 - (int)(720 * ratio)) / 2 + (int)(scrolledFill.Center.Y * ratio);
            var hit = MenuPointerInput.HitTest(app, x, y, size.Item1, size.Item2);
            Check(hit.SelectionIndex == 5 && Math.Abs((hit.SliderValue ?? -100) - 75) <= 1);
        }
        for (int row = 4; row < 6; row++)
        {
            var fill = OptionsMenuLayout.AudioFill(row, app.Options);
            Read(fill.Left, fill.Center.Y);
            app.Update(Read(fill.Left, fill.Center.Y, true), TimeSpan.Zero);
            Check(app.Options.Volume(row) == 0 && app.Options.IndicatorRow == row);
            app.Update(Read(fill.Right, fill.Center.Y, true), TimeSpan.Zero);
            Check(app.Options.Volume(row) == 100);
            Read(fill.Right, fill.Center.Y);
        }
        var checkbox = OptionsMenuLayout.AudioCheckbox(app.Options);
        Check(OptionsMenuLayout.Viewport.Contains(checkbox));
        Check(OptionsMenuLayout.ValueBounds(6, app.Options).Contains(checkbox));
        var checkboxHit = MenuPointerInput.HitTest(app, checkbox.Center.X, checkbox.Center.Y, 1280, 720);
        Check(checkboxHit.Confirm && checkboxHit.SelectionIndex == 6 && checkboxHit.SliderValue is null);
        app.Update(checkboxHit, TimeSpan.Zero);
        Check(!app.Options.Draft.MuteUnfocused && app.Options.Current.MuteUnfocused && app.Options.IndicatorRow == 6);
        app.Update(new(Confirm: true), TimeSpan.Zero);
        Check(app.Options.Draft.MuteUnfocused);
        app.Update(new(Left: true), TimeSpan.Zero);
        Check(!app.Options.Draft.MuteUnfocused);
        var whitespace = OptionsMenuLayout.ValueBounds(6, app.Options);
        Check(!MenuPointerInput.HitTest(app, whitespace.Right - 8, checkbox.Center.Y, 1280, 720).Confirm);
        app.Update(new(SelectionIndex: app.Options.FieldCount, Confirm: true), TimeSpan.Zero);
        Check(!app.Options.Current.MuteUnfocused);
        app.Update(new(SelectionIndex: 6, Confirm: true), TimeSpan.Zero);
        Check(app.Options.Draft.MuteUnfocused);
        app.Options.Cancel();
        Check(!app.Options.Draft.MuteUnfocused);
    }
}
