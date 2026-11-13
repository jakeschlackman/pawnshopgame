using PawnShop.Characters.Player;
using PawnShop.Characters.Player.Create;
using PawnShop.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;
using static PawnShop.Tests.RegressionTests;

namespace PawnShop.Tests;

internal static class CharacterSelectionTests
{
    private static CharacterSelectionPresentation Create() => new(new CharacterSelectionCatalog(
        Path.Combine(AppContext.BaseDirectory, "Content", "CharacterSelection")));

    internal static void Motion()
    {
        var scene = Create();
        Check(scene.Male.Pose == SelectionPose.LeaningIdle && scene.Female.Pose == SelectionPose.LeaningIdle);
        Check(scene.Highlighted is null && scene.Male.Progress == 0 && scene.Female.Progress == 0);
        scene.Highlight(0);
        scene.Update(TimeSpan.FromSeconds(5));
        Check(scene.Male.Pose == SelectionPose.SelectedIdle && scene.Male.Progress == 1);
        Check(scene.Male.Position == CharacterSelectionLayout.ForegroundFoot(PlayerGender.Male));
        Check(scene.Female.Pose == SelectionPose.LeaningIdle && scene.Female.Progress == 0);
        scene.Update(TimeSpan.FromMinutes(5));
        Check(scene.Male.Progress == 1 && scene.Male.Pose == SelectionPose.SelectedIdle, "Selected idle returned to the tree.");
        scene.Highlight(1);
        scene.Update(TimeSpan.FromMilliseconds(400));
        Check(scene.Male.Progress is > 0 and < 1 && scene.Female.Pose != SelectionPose.LeaningIdle,
            "Outgoing and incoming transitions should overlap.");
        scene.Update(TimeSpan.FromSeconds(5));
        Check(scene.Male.Pose == SelectionPose.LeaningIdle && scene.Male.Progress == 0);
        Check(scene.Female.Pose == SelectionPose.SelectedIdle && scene.Female.Progress == 1);
        scene.Highlight(2);
        scene.Update(TimeSpan.FromSeconds(5));
        Check(scene.Male.Progress == 0 && scene.Female.Progress == 0 && scene.Highlighted is null);
        scene.Reset();
        Check(scene.Male.Pose == SelectionPose.LeaningIdle && !scene.IsConfirming);
    }

    internal static void RapidSwitches()
    {
        var scene = Create();
        scene.Highlight(0);
        scene.Update(TimeSpan.FromMilliseconds(780));
        double moving = scene.Male.Progress;
        Check(moving is > 0 and < 1);
        scene.Highlight(1);
        scene.Update(TimeSpan.Zero);
        Check(scene.Male.Progress == moving && scene.Male.Pose == SelectionPose.TurnToTree,
            "Switching reset the current position.");
        for (int i = 0; i < 600; i++)
        {
            double male = scene.Male.Progress, female = scene.Female.Progress;
            scene.Highlight(i % 3);
            scene.Update(TimeSpan.FromMilliseconds(17));
            foreach (var actor in new[] { scene.Male, scene.Female })
                Check(actor.Progress is >= 0 and <= 1 && float.IsFinite(actor.Position.X));
            double maximumStep = 17d / SelectionActor.WalkDuration.TotalMilliseconds + .00001;
            Check(Math.Abs(scene.Male.Progress - male) <= maximumStep && Math.Abs(scene.Female.Progress - female) <= maximumStep,
                "Rapid input teleported an actor.");
        }
        scene.Highlight(1);
        scene.Confirm(PlayerGender.Female);
        scene.Highlight(0);
        scene.Confirm(PlayerGender.Male);
        Check(scene.Confirmation == PlayerGender.Female && scene.Highlighted == PlayerGender.Female, "Confirmation was not latched.");
        scene.Update(TimeSpan.FromSeconds(10));
        Check(scene.ReadyToConfirm && scene.Female.Progress == 1 && scene.Male.Progress == 0);

        var coarse = Create();
        var fine = Create();
        foreach (int highlight in new[] { 0, 1, 0, 2, 1 })
        {
            coarse.Highlight(highlight); fine.Highlight(highlight);
            coarse.Update(TimeSpan.FromMilliseconds(1300));
            for (int i = 0; i < 130; i++) fine.Update(TimeSpan.FromMilliseconds(10));
            foreach (var gender in Enum.GetValues<PlayerGender>())
            {
                var a = coarse.Actor(gender); var b = fine.Actor(gender);
                Check(a.Pose == b.Pose && Math.Abs(a.Progress - b.Progress) < 1e-6 && a.Frame == b.Frame,
                    "Frame rate changed the selection trajectory or phase.");
            }
        }
    }

    internal static void Flow()
    {
        using var app = new ApplicationFlow();
        app.Update(new(Confirm: true), TimeSpan.Zero);
        app.Update(new(Confirm: true, Right: true), PanoramaCamera.TransitionDuration);
        Check(app.Screen == Screen.CharacterSelect && app.CharacterSelection.Male.Progress == 0 && app.Menu!.SelectedIndex == 0);
        app.Update(new(Right: true), TimeSpan.FromMilliseconds(200));
        var state = (app.CharacterSelection.Female.Frame, app.CharacterSelection.Female.Position);
        app.Update(new(Confirm: true), TimeSpan.FromSeconds(5), active: false);
        Check(state == (app.CharacterSelection.Female.Frame, app.CharacterSelection.Female.Position) && app.LiveSessions == 0);
        app.Update(new(Confirm: true, MoveX: 1, Interact: true), TimeSpan.Zero);
        Check(app.CharacterSelection.IsConfirming && app.LiveSessions == 0 && app.CharacterSelection.Confirmation == PlayerGender.Female);
        app.Update(new(Left: true, SelectionIndex: 0, Confirm: true), TimeSpan.FromMilliseconds(40));
        Check(app.Menu!.SelectedIndex == 1 && app.CharacterSelection.Confirmation == PlayerGender.Female);
        app.Update(new(Confirm: true, MoveX: 1, Interact: true), TimeSpan.FromSeconds(10));
        Check(app.Player is { Gender: PlayerGender.Female } && app.SessionsCreated == 1 && app.Room!.Value.Elapsed == TimeSpan.Zero);
        Check(app.CharacterSelection.Female.Pose == SelectionPose.SelectedIdle && app.CharacterSelection.Male.Pose == SelectionPose.LeaningIdle);

        using var cancelled = new ApplicationFlow();
        Confirm(cancelled);
        cancelled.Update(new(Confirm: true), TimeSpan.Zero);
        Check(cancelled.CharacterSelection.IsConfirming);
        cancelled.Update(new(Cancel: true), TimeSpan.Zero);
        Check(cancelled.Screen == Screen.Title && cancelled.SessionsCreated == 0);
        Confirm(cancelled);
        Check(!cancelled.CharacterSelection.IsConfirming && cancelled.CharacterSelection.Male.Progress == 0);
    }

    internal static void Pointer()
    {
        using var app = new ApplicationFlow();
        Confirm(app);
        var pointer = new MenuPointerInput();
        var location = app.CharacterSelection.Female.Position;
        var mouse = new Microsoft.Xna.Framework.Input.MouseState((int)location.X, (int)location.Y - 60, 0,
            Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released,
            Microsoft.Xna.Framework.Input.ButtonState.Released, Microsoft.Xna.Framework.Input.ButtonState.Released,
            Microsoft.Xna.Framework.Input.ButtonState.Released);
        pointer.Read(app, mouse, true, 640, 360, 640, 360);
        var moved = new Microsoft.Xna.Framework.Input.MouseState(mouse.X + 1, mouse.Y, 0,
            mouse.LeftButton, mouse.MiddleButton, mouse.RightButton, mouse.XButton1, mouse.XButton2);
        var hover = pointer.Read(app, moved, true, 640, 360, 640, 360);
        Check(hover.SelectionIndex == 1 && !hover.Confirm);
        app.Update(hover, TimeSpan.Zero);
        Check(app.Menu!.SelectedIndex == 1 && !app.CharacterSelection.IsConfirming);
        Check(pointer.Read(app, moved, true, 640, 360, 640, 360) == default,
            "A stationary pointer should not steal keyboard selection.");
        Check(MenuPointerInput.HitTest(app, 2, 2, 1280, 1024) == default);
    }
}
