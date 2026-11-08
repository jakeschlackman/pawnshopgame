using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsDirtyConfirmation()
    {
        for (int tab = 0; tab < 4; tab++)
        {
            int writes = 0;
            var model = new OptionsModel(new(), _ => writes++);
            model.Open();
            Check(!model.IsDirty && model.Update(new(Cancel: true), TimeSpan.Zero, true));
            model.Open();
            model.Update(new(TabIndex: tab), TimeSpan.Zero, true);
            switch (tab)
            {
                case 0: model.Update(new(Right: true), TimeSpan.Zero, true); break;
                case 1: model.Update(new(SliderValue: 35), TimeSpan.Zero, true); break;
                case 2: model.Update(new(Confirm: true), TimeSpan.Zero, true); break;
                case 3: EditBinding(model, 0, 0, Keys.E); break;
            }
            var draft = model.Draft;
            Check(model.IsDirty && writes == 0);
            model.Scroll.MoveTo(model.Scroll.Maximum);
            int offset = model.Scroll.Offset, indicator = model.IndicatorRow;
            Check(!model.Update(new(Cancel: true), TimeSpan.Zero, true));
            Check(model.ConfirmingDiscard && model.HasModal && model.DiscardChoice == 0 && model.Draft == draft);
            model.Update(new(TabIndex: 0, NextTab: true, SelectionIndex: model.FieldCount + 1,
                SliderValue: 99, ScrollDelta: -10000), TimeSpan.Zero, true);
            Check(model.Tab == tab && model.Scroll.Offset == offset && model.IndicatorRow == indicator && model.Draft == draft);
            model.Update(new(ChoiceIndex: 99, Confirm: true), TimeSpan.Zero, true);
            Check(model.ConfirmingDiscard && model.DiscardChoice == 0);
            model.Update(new(ChoiceIndex: 1, Confirm: true), TimeSpan.FromSeconds(20), false);
            Check(model.ConfirmingDiscard && model.Draft == draft && writes == 0);
            Check(!model.Update(new(Confirm: true), TimeSpan.Zero, true)); // Safe Enter default.
            Check(!model.HasModal && model.Draft == draft && model.IsDirty);
            model.Update(new(SelectionIndex: model.FieldCount + 2, Confirm: true), TimeSpan.Zero, true);
            Check(model.ConfirmingDiscard && !model.Update(new(Cancel: true), TimeSpan.Zero, true));
            Check(model.Draft == draft && !model.HasModal);
            model.Update(new(Cancel: true), TimeSpan.Zero, true);
            model.Update(new(Right: true), TimeSpan.Zero, true);
            Check(model.DiscardChoice == 1 && model.Draft == draft);
            Check(model.Update(new(Confirm: true), TimeSpan.Zero, true));
            Check(model.Draft == model.Current && !model.IsDirty && !model.HasModal && writes == 0);
        }

        var reverted = new OptionsModel(new());
        reverted.Open(); reverted.Update(new(TabIndex: 2), TimeSpan.Zero, true);
        reverted.Update(new(Confirm: true), TimeSpan.Zero, true);
        reverted.Update(new(Confirm: true), TimeSpan.Zero, true);
        Check(!reverted.IsDirty && reverted.Update(new(Cancel: true), TimeSpan.Zero, true));
        reverted.Open();
        reverted.Update(new(SelectionIndex: reverted.FieldCount + 1, Confirm: true), TimeSpan.Zero, true);
        Check(!reverted.IsDirty && reverted.Update(new(Cancel: true), TimeSpan.Zero, true));

        foreach (int scale in new[] { 100, 125, 150 })
        {
            int writes = 0;
            using var app = new ApplicationFlow(new() { UiScale = scale }, _ => writes++);
            OpenOptions(app); FinishOptionsFlight(app);
            app.Update(new(TabIndex: 1), TimeSpan.Zero);
            app.Update(new(SliderValue: 30), TimeSpan.Zero);
            var draft = app.Options.Draft;
            var back = OptionsMenuLayout.Footer(2).Center;
            app.Update(MenuPointerInput.HitTest(app, back.X, back.Y, 1280, 720), TimeSpan.Zero);
            Check(app.Screen == Screen.Options && app.Options.ConfirmingDiscard && !app.OptionsFlight.IsBusy);
            foreach (var size in new[] { (1280, 720), (1920, 1080), (2560, 1440), (3440, 1440) })
            {
                InputFrame Hit(int x, int y)
                {
                    double ratio = Math.Min(size.Item1 / 1280d, size.Item2 / 720d);
                    return MenuPointerInput.HitTest(app,
                        (size.Item1 - (int)(1280 * ratio)) / 2 + (int)(x * ratio),
                        (size.Item2 - (int)(720 * ratio)) / 2 + (int)(y * ratio), size.Item1, size.Item2);
                }
                var stay = OptionsMenuLayout.Keep.Center;
                var discard = OptionsMenuLayout.Revert.Center;
                Check(Hit(stay.X, stay.Y).ChoiceIndex == 0 && Hit(discard.X, discard.Y).ChoiceIndex == 1);
                Check(Hit(OptionsMenuLayout.Tab(0).Center.X, OptionsMenuLayout.Tab(0).Center.Y) == default);
                Check(Hit(back.X, back.Y) == default && Hit(0, 0) == default);
                app.Update(Hit(stay.X, stay.Y), TimeSpan.Zero);
                Check(app.Screen == Screen.Options && app.Options.Draft == draft && !app.Options.HasModal && writes == 0);
                app.Update(new(Cancel: true), TimeSpan.Zero);
            }
            var pointer = new MenuPointerInput();
            var track = OptionsMenuLayout.AudioFill(0, app.Options);
            pointer.Read(app, new MouseState(track.Left, track.Center.Y, 0, ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720);
            Check(pointer.Read(app, new MouseState(track.Left, track.Center.Y, -120, ButtonState.Pressed,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), true, 1280, 720, 1280, 720) == default);
            Check(app.Options.Draft == draft);
            var discardPoint = OptionsMenuLayout.Revert.Center;
            app.Update(MenuPointerInput.HitTest(app, discardPoint.X, discardPoint.Y, 1280, 720), TimeSpan.Zero);
            Check(app.Screen == Screen.Title && app.OptionsFlight.IsReturning && app.Options.Draft == app.Options.Current && writes == 0);
        }

        int saved = 0;
        var applied = new OptionsModel(new(), _ => saved++);
        applied.Open(); applied.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        applied.Update(new(SliderValue: 30), TimeSpan.Zero, true);
        applied.Update(new(SelectionIndex: applied.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(saved == 1 && !applied.IsDirty && applied.Update(new(Cancel: true), TimeSpan.Zero, true));
        var failed = new OptionsModel(new(), _ => throw new IOException("Injected failure"));
        failed.Open(); failed.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        failed.Update(new(SliderValue: 30), TimeSpan.Zero, true);
        failed.Update(new(SelectionIndex: failed.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(failed.IsDirty && !failed.Update(new(Cancel: true), TimeSpan.Zero, true) && failed.ConfirmingDiscard);

        var preview = new OptionsModel(new());
        preview.Open(); preview.Update(new(TabIndex: 1), TimeSpan.Zero, true);
        preview.Update(new(SliderValue: 30), TimeSpan.Zero, true);
        preview.Update(new(TabIndex: 0), TimeSpan.Zero, true);
        preview.Update(new(SelectionIndex: 1, Right: true), TimeSpan.Zero, true);
        preview.Update(new(SelectionIndex: preview.FieldCount, Confirm: true), TimeSpan.Zero, true);
        Check(preview.Previewing && !preview.ConfirmingDiscard);
        Check(!preview.Update(new(Cancel: true), TimeSpan.Zero, true));
        Check(!preview.HasModal && preview.IsDirty && preview.Draft.Master == 30 && preview.Draft.Display == preview.Current.Display);
        Check(!preview.Update(new(Cancel: true), TimeSpan.Zero, true) && preview.ConfirmingDiscard);
    }
}
