using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void OptionsSproutSequence()
    {
        using var app = new ApplicationFlow();
        app.Update(new(SelectionIndex: 2, Confirm: true), TimeSpan.FromMilliseconds(270));
        Check(app.TitleSelector.FrameIndex == 3 && app.OptionsFlight.Phase == OptionsSproutPhase.FinishingPoint);
        app.Update(new(Confirm: true, Down: true), TimeSpan.FromMilliseconds(899));
        Check(app.TitleSelector.FrameIndex == 12 && app.Menu!.SelectedIndex == 2 && app.Screen == Screen.Title);
        app.Update(default, TimeSpan.FromMilliseconds(1));
        Check(app.TitleSelector.FrameIndex == 12 && app.TitleSelector.AnimationName == "Idle");
        app.Update(default, TimeSpan.Zero);
        Check(app.Screen == Screen.Options && app.OptionsFlight.Phase == OptionsSproutPhase.Flying);
        Check(app.TitleSelector.AnimationName == "Idle" && app.TitleCamera.Position == 0);
        app.Update(new(Right: true, Confirm: true, NextTab: true), TimeSpan.FromMilliseconds(400));
        Check(app.OptionsFlight.Progress == .5f && app.TitleCamera.Position == .5);
        Check(app.Options.Draft == app.Options.Current && app.Options.Selected == 0 && app.Options.Tab == 0);
        Check(app.TitleSelector.AnimationName == "Idle" && app.TitleSelector.FrameIndex is >= 0 and <= 12);
        Check(MenuPointerInput.HitTest(app, 100, 170, 1280, 720) == default);
        app.Update(new(Confirm: true, Right: true), TimeSpan.FromMilliseconds(400));
        Check(app.OptionsFlight.Progress == 1 && app.TitleCamera.Position == 1);
        Check(app.OptionsFlight.Phase == OptionsSproutPhase.Arrived && !app.TitleInteractionPlaying);
        Check(app.TitleSelector.AnimationName == "Idle");
        Check(app.Options.Draft == app.Options.Current && app.LiveSessions == 0);
        int arrivalFrame = app.TitleSelector.FrameIndex;
        app.Update(default, TimeSpan.FromMilliseconds(1170));
        Check(app.TitleSelector.FrameIndex == arrivalFrame, "Arrival must continue the pointing loop without restarting it.");
        app.Update(new(Right: true), TimeSpan.Zero);
        Check(app.Options.Draft != app.Options.Current, "Options must accept input after arrival.");
    }

    internal static void OptionsSproutCancellation()
    {
        foreach (var phase in new[] { OptionsSproutPhase.FinishingPoint, OptionsSproutPhase.Flying })
        {
            using var app = new ApplicationFlow();
            app.Update(new(SelectionIndex: 2, Confirm: true), TimeSpan.Zero);
            for (int i = 0; i < 8 && app.OptionsFlight.Phase != phase; i++) app.Update(default, TimeSpan.FromSeconds(2));
            Check(app.OptionsFlight.Phase == phase);
            int frame = app.TitleSelector.FrameIndex;
            double camera = app.TitleCamera.Position;
            float progress = app.OptionsFlight.Progress;
            app.Update(new(Cancel: true), TimeSpan.FromSeconds(30), active: false);
            Check(app.TitleSelector.FrameIndex == frame && app.TitleCamera.Position == camera && app.OptionsFlight.Progress == progress);
            app.Update(new(Confirm: true, Down: true, TabIndex: 2), TimeSpan.Zero);
            Check(app.Options.Selected == 0 && app.Options.Tab == 0 && app.Options.Draft == app.Options.Current);
            app.Update(new(Cancel: true), TimeSpan.Zero);
            Check(app.Screen == Screen.Title && !app.TitleInteractionPlaying && app.TitleSelector.AnimationName == "Idle");
            app.Update(default, PanoramaCamera.TransitionDuration);
            Check(app.TitleCamera.Position == 0 && app.LiveSessions == 0);
            OpenOptions(app);
            FinishOptionsFlight(app);
            Check(app.Options.Selected == 0 && app.Options.Tab == 0);
            Back(app);
            app.Update(default, PanoramaCamera.TransitionDuration);
            OpenOptions(app);
            app.RequestExit();
            Check(app.ExitRequested && !app.TitleInteractionPlaying && app.OptionsFlight.Phase == OptionsSproutPhase.None);
        }
        // Invalid clip contracts fail during startup rather than during a flight.
        string json = File.ReadAllText(SproutMetadataPath).Replace("\"loop\": true", "\"loop\": false");
        try { _ = new OptionsSproutFlight(new(SpriteSheetDefinition.Parse(json), "Idle")); throw new Exception("Expected missing clip rejection"); }
        catch (InvalidDataException) { }
    }

    internal static void OptionsSproutReturn()
    {
        var menuAnchor = new Vector2(280, 820);
        var firstOption = new Vector2(306, 408);
        foreach (int scale in new[] { 100, 125, 150 })
        foreach (bool footer in new[] { false, true })
        {
            using var app = new ApplicationFlow(new() { UiScale = scale });
            OpenOptions(app); FinishOptionsFlight(app);
            app.Update(new(TabIndex: 1), TimeSpan.Zero);
            app.Update(new(Right: true), TimeSpan.Zero);
            Check(app.Options.Draft != app.Options.Current);
            if (scale != 100)
            {
                app.Update(new(SelectionIndex: app.Options.FieldCount + 1, Confirm: true), TimeSpan.Zero);
                Check(app.Options.Draft.UiScale == scale);
                app.Update(new(SelectionIndex: 0, Right: true), TimeSpan.Zero);
            }
            app.Update(new(TabIndex: 3), TimeSpan.Zero);
            app.Update(new(SelectionIndex: 5), TimeSpan.Zero);
            app.Options.Scroll.MoveTo(app.Options.Scroll.Maximum);
            var source = OptionsMenuLayout.SelectorAnchor(app.Options);
            int pointingFrame = app.TitleSelector.FrameIndex;
            var back = OptionsMenuLayout.Footer(2).Center;
            var close = footer ? MenuPointerInput.HitTest(app, back.X, back.Y, 1280, 720)
                : new PawnShop.Framework.Input.InputFrame(Cancel: true);
            app.Update(close, TimeSpan.Zero);
            Check(app.Screen == Screen.Options && app.Options.ConfirmingDiscard && !app.OptionsFlight.IsBusy);
            app.Update(new(Confirm: true, ChoiceIndex: 1), TimeSpan.Zero);
            Check(app.Screen == Screen.Title && app.OptionsFlight.IsReturning && app.OptionsFlight.IsBusy);
            Check(app.Menu!.Selected.Command == MenuCommand.Settings && app.Options.Draft == app.Options.Current);
            Check(app.TitleCamera.Position == 1 && app.OptionsFlight.Progress == 0);
            Check(app.OptionsFlight.ReturnAnchor == source && app.OptionsFlight.GetAnchor(menuAnchor, firstOption) == source);
            Check(app.TitleSelector.FrameIndex == pointingFrame && app.TitleSelector.AnimationName == "Idle");
            var titleOptions = TitleMenuLayout.Row(2).Center;
            Check(MenuPointerInput.HitTest(app, titleOptions.X, titleOptions.Y, 1920, 1080) == default);
            app.Update(new(Cancel: true, Confirm: true, Down: true), TimeSpan.FromSeconds(30), active: false);
            Check(app.OptionsFlight.Progress == 0 && app.TitleCamera.Position == 1 && app.TitleSelector.FrameIndex == pointingFrame);
            app.Update(new(Cancel: true, Confirm: true, Down: true, TabIndex: 2), TimeSpan.FromMilliseconds(400));
            Check(app.OptionsFlight.Progress == .5f && app.TitleCamera.Position == .5 && app.LiveSessions == 0);
            Check(app.OptionsFlight.GetAnchor(menuAnchor, firstOption) == Vector2.Lerp(source, menuAnchor, .5f));
            Check(app.Menu.Selected.Command == MenuCommand.Settings && app.Options.Tab == 3);
            app.Update(new(Confirm: true, Down: true), TimeSpan.FromMilliseconds(400));
            Check(app.OptionsFlight.Phase == OptionsSproutPhase.Arrived && !app.TitleInteractionPlaying);
            Check(app.TitleCamera.Position == 0 && app.TitleUiVisible && app.LiveSessions == 0);
            Check(app.OptionsFlight.GetAnchor(menuAnchor, firstOption) == menuAnchor && app.Menu.SelectedIndex == 2);
            int arrivalFrame = app.TitleSelector.FrameIndex;
            app.Update(default, TimeSpan.FromMilliseconds(1170));
            Check(app.TitleSelector.FrameIndex == arrivalFrame, "Return must preserve the pointing animation clock.");
            OpenOptions(app); FinishOptionsFlight(app);
            Check(!app.OptionsFlight.IsReturning && app.Options.Tab == 0 && app.Options.Selected == 0);
            app.Update(new(Cancel: true), TimeSpan.Zero);
            app.RequestExit();
            Check(app.ExitRequested && !app.OptionsFlight.IsBusy && app.OptionsFlight.Phase == OptionsSproutPhase.None);
        }
    }
}
