using System.Buffers.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap;
using PawnShop.Game.Screens;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    internal static void TitleIntroSequence()
    {
        using var app = new ApplicationFlow(playTitleIntro: true);
        var newGame = TitleMenuLayout.Row(0).Center;
        var halfPan = TimeSpan.FromSeconds(1.6);
        var halfFadeLead = TimeSpan.FromMilliseconds(125);
        Check(app.UsesTitleBackground && app.TitleIntroPlaying && !app.TitleUiVisible);
        Check(app.TitleDescent == 0 && app.TitleMenuOpacity == 0 && app.TitleCamera.Position == 0);
        Check(MenuPointerInput.HitTest(app, newGame.X, newGame.Y, 1920, 1080) == default);

        app.Update(new(Confirm: true), TimeSpan.FromSeconds(.5));
        Check(app.StudioLogoOpacity == .5f && app.TitleDescent == 0 && !app.TitleUiVisible);
        app.Update(new(Down: true), TimeSpan.FromSeconds(.5));
        Check(app.StudioLogoOpacity == 1 && app.TitleDescent == 0);
        app.Update(default, TimeSpan.FromSeconds(2));
        Check(app.StudioLogoOpacity == 1 && app.TitleDescent == 0);
        app.Update(new(Confirm: true), TimeSpan.FromSeconds(.5));
        Check(app.StudioLogoOpacity == .5f && app.TitleDescent == 0 && app.LiveSessions == 0);
        app.Update(default, TimeSpan.FromSeconds(.5));
        Check(app.StudioLogoOpacity == 0 && app.TitleDescent == 0);

        app.Update(new(Down: true), halfPan);
        Check(app.TitleDescent == .5 && app.TitleMenuOpacity == 0 && app.Menu!.SelectedIndex == 0);
        app.Update(new(Confirm: true), halfPan, active: false);
        Check(app.TitleDescent == .5 && app.TitleMenuOpacity == 0);
        app.Update(new(Confirm: true), halfPan - halfFadeLead * 2);
        Check(app.TitleDescent < 1 && app.TitleMenuOpacity == 0);
        app.Update(new(Confirm: true), halfFadeLead);
        Check(app.TitleDescent < 1 && app.TitleMenuOpacity is > .2f and < .4f && app.TitleUiVisible);
        Check(app.TitleIntroPlaying && !app.TitleInteractionPlaying && app.LiveSessions == 0);
        Check(MenuPointerInput.HitTest(app, newGame.X, newGame.Y, 1920, 1080) == default);
        app.Update(new(Confirm: true), halfFadeLead);
        Check(app.TitleDescent == 1 && app.TitleMenuOpacity is > .5f and < .6f && app.TitleIntroPlaying);

        app.Update(new(Confirm: true), TimeSpan.FromSeconds(30));
        Check(!app.TitleIntroPlaying && app.TitleMenuOpacity == 1 && app.TitleUiVisible);
        Check(app.Screen == Screen.Title && !app.TitleInteractionPlaying && app.SessionsCreated == 0);
        Check(MenuPointerInput.HitTest(app, newGame.X, newGame.Y, 1920, 1080).Confirm);
        Begin(app);
        ReturnPrompt(app);
        Discard(app);
        Check(app.Screen == Screen.Title && app.LiveSessions == 0);
        Check(!app.TitleIntroPlaying && app.TitleDescent == 1 && app.TitleMenuOpacity == 1 && app.TitleUiVisible);
    }

    internal static void StudioIntroSkip()
    {
        foreach (double seconds in new[] { 0, .5, 2, 3.5, 5.2, 7.3 })
        {
            using var app = new ApplicationFlow(playTitleIntro: true);
            app.Update(default, TimeSpan.FromSeconds(seconds));
            double descent = app.TitleDescent;
            float studio = app.StudioLogoOpacity;
            float menu = app.TitleMenuOpacity;
            app.Update(new(Cancel: true, Confirm: true), TimeSpan.Zero);
            Check(app.TitleDescent == descent && app.StudioLogoOpacity == studio && app.TitleMenuOpacity == menu);
            app.Update(new(Cancel: true, Down: true), TimeSpan.FromSeconds(.4));
            Check(app.TitleDescent >= descent && app.TitleDescent <= 1 && app.StudioLogoOpacity <= studio);
            Check(app.StudioLogoOpacity == studio * .5f && app.TitleIntroPlaying);
            double frozen = app.TitleDescent;
            float opacity = app.StudioLogoOpacity;
            app.Update(new(Cancel: true), TimeSpan.FromSeconds(10), active: false);
            Check(app.TitleDescent == frozen && app.StudioLogoOpacity == opacity);
            app.Update(new(Cancel: true), TimeSpan.FromSeconds(.4));
            Check(app.StudioLogoOpacity == 0 && app.TitleIntroPlaying);
            app.Update(new(Cancel: true, Confirm: true), TimeSpan.FromSeconds(1.2));
            Check(!app.TitleIntroPlaying && app.TitleDescent == 1 && app.TitleMenuOpacity == 1);
            Check(app.Screen == Screen.Title && app.TitleUiVisible && !app.TitleInteractionPlaying);
            Check(app.Menu!.SelectedIndex == 0 && app.LiveSessions == 0 && !app.ExitRequested);
            Confirm(app);
            Check(app.Screen == Screen.CharacterSelect && app.LiveSessions == 0);
        }

        using var closing = new ApplicationFlow(playTitleIntro: true);
        closing.RequestExit();
        Check(closing.ExitRequested && closing.LiveSessions == 0);
    }

    internal static void TitleIntroFraming()
    {
        using var panorama = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen",
            "title-screen-panorama-day-to-dusk-forest.png"));
        Span<byte> header = stackalloc byte[24];
        panorama.ReadExactly(header);
        Check(header.Slice(12, 4).SequenceEqual("IHDR"u8));
        int imageWidth = BinaryPrimitives.ReadInt32BigEndian(header.Slice(16, 4));
        int imageHeight = BinaryPrimitives.ReadInt32BigEndian(header.Slice(20, 4));
        const int skyHeight = 414, originalHeight = 413, forestWidth = 736, townWidth = 1166;
        Check(imageWidth == forestWidth + townWidth && imageHeight == skyHeight + originalHeight);

        byte[] studio = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen", "logo-torcalev-studios.png"));
        Check(BinaryPrimitives.ReadInt32BigEndian(studio.AsSpan(16, 4)) == 1254);
        Check(BinaryPrimitives.ReadInt32BigEndian(studio.AsSpan(20, 4)) == 1254);
        Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(studio)) ==
            "FDA003D1324FC428EA05068FF4C077C69B4A35598FF6ACE59612B78D9CE9845D");

        var camera = new PanoramaCamera();
        var sizes = AppSettings.Resolutions.Concat(new[] { (800, 600), (720, 1280), (5120, 1440) });
        foreach (var (width, height) in sizes)
        {
            var original = camera.GetView(townWidth, originalHeight, width, height);
            var opening = camera.GetView(imageWidth, imageHeight, width, height, skyHeight, descent: 0, leftExtension: forestWidth);
            var middle = camera.GetView(imageWidth, imageHeight, width, height, skyHeight, descent: .5, leftExtension: forestWidth);
            var settled = camera.GetView(imageWidth, imageHeight, width, height, skyHeight, leftExtension: forestWidth);
            Check(opening == original with { X = original.X + forestWidth, Y = 0 } && opening.Height <= skyHeight);
            Check(middle == original with { X = original.X + forestWidth, Y = (original.Y + skyHeight) / 2 });
            Check(settled == original with { X = original.X + forestWidth, Y = original.Y + skyHeight });

            camera.NavigateToOptions();
            camera.Update(PanoramaCamera.TransitionDuration);
            original = camera.GetView(townWidth, originalHeight, width, height);
            settled = camera.GetView(imageWidth, imageHeight, width, height, skyHeight, leftExtension: forestWidth);
            Check(settled == original with { X = original.X + forestWidth, Y = original.Y + skyHeight });
            camera.NavigateToMainMenu();
            camera.Update(PanoramaCamera.TransitionDuration);
        }
    }

    internal static void PanoramaTiming()
    {
        foreach (int rate in new[] { 20, 30, 60, 144 })
        {
            var camera = new PanoramaCamera();
            Check(camera.Position == 0 && !camera.IsMoving);
            camera.NavigateToOptions();
            var delta = TimeSpan.FromSeconds(1.0 / rate);
            int frames = (int)Math.Round(PanoramaCamera.TransitionDuration.TotalSeconds / 2 * rate);
            for (int i = 0; i < frames; i++)
            {
                camera.Update(delta);
                camera.NavigateToOptions(); // Must not restart repeated navigation.
            }
            double t = delta.TotalSeconds * frames / PanoramaCamera.TransitionDuration.TotalSeconds;
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
        ease.Update(PanoramaCamera.TransitionDuration / 4);
        Check(Math.Abs(ease.Position - .15625) < 1e-9, "Expected SmoothStep rather than linear movement.");
        ease.Update(PanoramaCamera.TransitionDuration / 4);
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

    internal static void ForestPanoramaGeometry()
    {
        const int forest = 736, sky = 414, townWidth = 1166, townHeight = 413;
        var sizes = AppSettings.Resolutions.Concat(new[] { (800, 600), (720, 1280), (5120, 1440), (1, 1) });
        foreach (var (width, height) in sizes)
        {
            var camera = new PanoramaCamera();
            var town = camera.GetView(townWidth, townHeight, width, height);
            var main = camera.GetView(forest + townWidth, sky + townHeight, width, height, sky, leftExtension: forest);
            Check(main == town with { X = forest, Y = town.Y + sky });
            camera.NavigateToCharacterSelection();
            camera.Update(PanoramaCamera.TransitionDuration / 2);
            camera.NavigateToCharacterSelection(); // Repeated target requests preserve progress.
            var halfway = camera.GetView(forest + townWidth, sky + townHeight, width, height, sky, leftExtension: forest);
            Check(camera.Position == -.5 && halfway.X == forest / 2.0 && halfway.Width == main.Width);
            camera.Update(PanoramaCamera.TransitionDuration / 2);
            var selection = camera.GetView(forest + townWidth, sky + townHeight, width, height, sky, leftExtension: forest);
            Check(camera.Position == -1 && selection == main with { X = 0 });
            Check(selection.X >= 0 && selection.X + selection.Width <= forest + townWidth);
            Check(Math.Abs(selection.Width * selection.Scale - width) < 1e-8);
            Check(Math.Abs(selection.Height * selection.Scale - height) < 1e-8);
            if ((double)width / height <= 16.0 / 9)
                Check(selection.Width <= forest, "Standard screens must fit entirely within the forest extension.");
            camera.NavigateToMainMenu();
            camera.Update(PanoramaCamera.TransitionDuration);
            Check(camera.GetView(forest + townWidth, sky + townHeight, width, height, sky, leftExtension: forest) == main);
            camera.NavigateToOptions();
            camera.Update(PanoramaCamera.TransitionDuration);
            var options = camera.GetView(forest + townWidth, sky + townHeight, width, height, sky, leftExtension: forest);
            Check(Math.Abs(options.X + options.Width - (forest + townWidth)) < 1e-8);
        }
        string root = Path.Combine(AppContext.BaseDirectory, "Content", "TitleScreen");
        var active = File.ReadAllBytes(Path.Combine(root, "title-screen-panorama-day-to-dusk-forest.png"));
        Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(active)) ==
            "65A963D4AE159BE57CC444609377F909385D425B2EFE9DEC74DCB6F14A578011");
        var previous = File.ReadAllBytes(Path.Combine(root, "title-screen-panorama-day-to-dusk.png"));
        Check(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(previous)) ==
            "596C4EE3E8BBA47449D00475B9DFBFC3D2D5A5C2D575104B6937E6E97A251ED2");
        try { new PanoramaCamera().GetView(1902, 827, 1280, 720, leftExtension: 1902); throw new Exception("Expected extension validation"); }
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
        app.Update(default, PanoramaCamera.TransitionDuration);
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
