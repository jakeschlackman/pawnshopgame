using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void PanoramaTiming()
    {
        foreach (int rate in new[] { 20, 30, 60, 144 })
        {
            var camera = new PanoramaCamera();
            Check(camera.Position == 0 && !camera.IsMoving);
            camera.NavigateToOptions();
            var delta = TimeSpan.FromSeconds(1.0 / rate);
            int frames = (int)Math.Round(.4 * rate);
            for (int i = 0; i < frames; i++)
            {
                camera.Update(delta);
                camera.NavigateToOptions(); // Must not restart repeated navigation.
            }
            double t = delta.TotalSeconds * frames / .8;
            Check(Math.Abs(camera.Position - t * t * (3 - 2 * t)) < 1e-9);
            camera.Update(TimeSpan.FromSeconds(1));
            Check(camera.Position == 1 && !camera.IsMoving);
            camera.NavigateToMainMenu();
            camera.Update(TimeSpan.FromSeconds(.2));
            double before = camera.Position;
            camera.NavigateToOptions();
            Check(camera.Position == before, "Retargeting jumped the camera.");
            camera.Update(TimeSpan.FromSeconds(.1));
            Check(camera.Position > before && camera.Position < 1);
            camera.Update(TimeSpan.FromSeconds(10));
            Check(camera.Position == 1);
            camera.NavigateToMainMenu();
            camera.Update(PanoramaCamera.TransitionDuration);
            Check(camera.Position == 0 && !camera.IsMoving);
        }
        var ease = new PanoramaCamera();
        ease.NavigateToOptions();
        ease.Update(TimeSpan.FromSeconds(.2));
        Check(Math.Abs(ease.Position - .15625) < 1e-9, "Expected SmoothStep rather than linear movement.");
        ease.Update(TimeSpan.FromSeconds(.2));
        Check(Math.Abs(ease.Position - .5) < 1e-9);
    }

    internal static void PanoramaGeometry()
    {
        var camera = new PanoramaCamera();
        var sizes = AppSettings.Resolutions.Concat(new[] { (800, 600), (720, 1280), (5120, 1440), (1, 1) });
        foreach (var (width, height) in sizes)
        {
            var left = camera.GetView(2106, 747, width, height);
            Check(left.X == 0);
            Check(Math.Abs(left.Width / left.Height - (double)width / height) < 1e-8);
            Check(left.X >= 0 && left.Y >= 0 && left.Width <= 2106 && left.Y + left.Height <= 747 + 1e-8);
            Check(Math.Abs(left.Width * left.Scale - width) < 1e-8);
            Check(Math.Abs(left.Height * left.Scale - height) < 1e-8);
            camera.NavigateToOptions();
            camera.Update(PanoramaCamera.TransitionDuration);
            var right = camera.GetView(2106, 747, width, height);
            Check(Math.Abs(right.X + right.Width - 2106) < 1e-8);
            camera.NavigateToMainMenu();
            camera.Update(PanoramaCamera.TransitionDuration);
        }
        camera.NavigateToOptions();
        camera.Update(TimeSpan.FromSeconds(.4));
        var before = camera.Position;
        foreach (var (width, height) in sizes)
        {
            var resized = camera.GetView(2106, 747, width, height);
            Check(camera.Position == before && resized.X >= 0 && resized.X + resized.Width <= 2106 + 1e-8);
        }
        // Very wide windows have no horizontal travel: the full width is already visible.
        Check(camera.GetView(2106, 747, 5120, 1440).X == 0);
        try { camera.GetView(2106, 747, 0, 720); throw new Exception("Expected dimension validation"); }
        catch (ArgumentOutOfRangeException) { }
    }

    internal static void PanoramaNavigation()
    {
        using var app = new ApplicationFlow();
        Check(app.UsesTitleBackground && app.TitleUiVisible && app.TitleCamera.Position == 0);
        OpenOptions(app);
        Check(app.Screen == Screen.Options && !app.TitleUiVisible && app.TitleCamera.Position == 0);
        app.Update(new(Right: true, Confirm: true), TimeSpan.FromSeconds(.2));
        Check(app.Options.Draft == app.Options.Current, "Hidden options accepted input.");
        var before = app.TitleCamera.Position;
        app.Update(new(Cancel: true), TimeSpan.Zero);
        Check(app.Screen == Screen.Title && app.TitleCamera.Position == before && !app.TitleUiVisible);
        app.Update(new(Confirm: true, Interact: true), TimeSpan.FromSeconds(.1));
        Check(app.LiveSessions == 0 && app.TitleCamera.Position < before);
        app.Update(default, TimeSpan.FromSeconds(.7));
        Check(app.TitleUiVisible && app.TitleCamera.Position == 0);
        OpenOptions(app);
        app.Update(default, PanoramaCamera.TransitionDuration);
        Check(app.TitleUiVisible && app.TitleCamera.Position == 1);
        FinishOptionsFlight(app);
        Back(app);
        Check(!app.TitleUiVisible);
        app.Update(default, PanoramaCamera.TransitionDuration);
        Begin(app); Back(app);
        Down(app); Down(app); Down(app); Confirm(app);
        Check(app.Screen == Screen.Options && !app.UsesTitleBackground && !app.TitleCamera.IsMoving);
        var room = app.Room;
        Back(app);
        Check(app.Screen == Screen.Pause && app.Room == room);
    }

    internal static void PanoramaPointer()
    {
        using var app = new ApplicationFlow();
        var options = TitleMenuLayout.Row(2).Center;
        var click = MenuPointerInput.HitTest(app, options.X, options.Y, 1920, 1080);
        Check(click.Confirm && click.SelectionIndex == 2);
        app.Update(click, TimeSpan.Zero);
        FinishTitleInteraction(app);
        Check(app.Screen == Screen.Options && !app.TitleUiVisible);
        Check(MenuPointerInput.HitTest(app, 100, 170, 1280, 720) == default);
        app.Update(default, PanoramaCamera.TransitionDuration);
        FinishOptionsFlight(app);
        var back = OptionsMenuLayout.Footer(2).Center;
        app.Update(MenuPointerInput.HitTest(app, back.X, back.Y, 1280, 720), TimeSpan.Zero);
        Check(app.Screen == Screen.Title && !app.TitleUiVisible);
        app.Update(default, PanoramaCamera.TransitionDuration);
        Check(app.TitleUiVisible && app.TitleCamera.Position == 0);
        Check(MenuPointerInput.HitTest(app, 10, 10, 1280, 1024) == default);
        // Map the same row through a centered 16:9 UI canvas on an ultrawide output.
        double scale = 1440.0 / 1080;
        int offset = (3440 - 2560) / 2;
        click = MenuPointerInput.HitTest(app, offset + (int)(options.X * scale), (int)(options.Y * scale), 3440, 1440);
        Check(click.SelectionIndex == 2);
        var pointer = new MenuPointerInput();
        var held = new MouseState(options.X, options.Y, 0, ButtonState.Pressed,
            ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        Check(pointer.Read(app, held, false, 1920, 1080, 1920, 1080) == default);
        Check(pointer.Read(app, held, true, 1920, 1080, 1920, 1080) == default);
        Check(pointer.Read(app, held, true, 1920, 1080, 1920, 1080) == default);
    }
}
