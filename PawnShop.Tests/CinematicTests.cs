using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap.Cinematics;
#if DEBUG
using PawnShop.Game.Screens.CinematicEditor;
#endif

namespace PawnShop.Tests;

internal static class CinematicTests
{
    private static void Check(bool value, string message = "Cinematic assertion failed")
    {
        if (!value) throw new Exception(message);
    }
    private static void Near(double a, double b, double tolerance = .0001) => Check(Math.Abs(a - b) <= tolerance, $"Expected {b}, got {a}");
    private static void Near(Vector2 a, Vector2 b, float tolerance = .001f) => Check(Vector2.Distance(a, b) <= tolerance, $"Expected {b}, got {a}");
    private static void Invalid(Action action, string field)
    {
        try { action(); }
        catch (InvalidDataException e) { Check(e.Message.Contains(field), e.Message); return; }
        throw new Exception("Expected cinematic validation failure: " + field);
    }

    private static CinematicDefinition Fixture() => new()
    {
        Version = 1, BackgroundAsset = CinematicDefinition.Background, ImageWidth = 2106, ImageHeight = 747,
        CarAsset = "placeholder-wire-car", GroundAnchor = new(32, 40), DurationSeconds = 12, Speed = 1,
        Points = new CinematicPoint[] { new(2220, 850), new(1900, 700), new(1700, 620), new(1800, 530), new(1500, 440) },
        Keys = new CinematicKey[] { new(0, 2, 45, true), new(.5f, 1, 90, true), new(1, .05f, 0, false) }
    };

    internal static void Runtime()
    {
        var data = Fixture();
        var player = new CinematicPlayer(data);
        Near(player.Pose.GroundPosition, new(2220, 850));
        Check(!player.Playing && player.Pose.Visible);
        player.Update(TimeSpan.FromSeconds(2));
        Near(player.Progress, 0);
        player.Playing = true;
        player.Update(TimeSpan.FromSeconds(3));
        Near(player.Progress, .25);
        Near(player.Pose.Scale, 1.5);
        Near(player.Pose.OrientationDegrees, 67.5);
        Check(player.Pose.Visible);
        player.Playing = false;
        player.Update(TimeSpan.FromSeconds(100));
        Near(player.Progress, .25);
        player.Seek(1);
        Check(!player.Pose.Visible);
        Near(player.Pose.GroundPosition, new(1500, 440));
        player.Restart();
        Check(player.Progress == 0 && !player.Playing);
        foreach (int rate in new[] { 20, 30, 60, 144 })
        {
            player = new(data) { Playing = true };
            var delta = TimeSpan.FromSeconds(1.0 / rate);
            for (int i = 0; i < rate * 6; i++) player.Update(delta);
            Near(player.Progress, delta.TotalSeconds * rate * 6 / 12, .00000001);
            player.Update(TimeSpan.FromDays(1));
            Check(player.Progress == 1 && !player.Playing);
        }
        player = new(data with { Speed = 2 }) { Playing = true };
        player.Update(TimeSpan.FromSeconds(3));
        Near(player.Progress, .5);
        var reverse = data with { Points = data.Points.Reverse().ToArray() };
        var other = new CinematicPlayer(reverse);
        Near(player.Evaluate(.25).OrientationDegrees, other.Evaluate(.25).OrientationDegrees);
        Near(player.Evaluate(.25).Scale, other.Evaluate(.25).Scale);
        var wrap = new CinematicPlayer(data with { Keys = new CinematicKey[] { new(0, 1, 350, true), new(1, 1, 10, false) } });
        Near(wrap.Evaluate(.5).OrientationDegrees, 360);
        var large = Fixture() with { Keys = new CinematicKey[] { new(0, 64, 45, true), new(1, .01f, 0, false) } };
        var largePlayer = new CinematicPlayer(CinematicDefinition.Parse(large.ToJson(), "large-car.json"));
        Near(largePlayer.Pose.Scale, 64);
        Near(largePlayer.Evaluate(1).Scale, .01);
        Invalid(() => (large with { Keys = new CinematicKey[] { new(0, 65, 45, true), new(1, .01f, 0, false) } }).Validate("large-car.json"), "scale");
        wrap = new(data with { Keys = new CinematicKey[] { new(0, 1, 360, true), new(1, 1, -360, false) } });
        Near(wrap.Evaluate(.5).OrientationDegrees, 360);
        // The runtime owns a snapshot rather than exposing the input arrays.
        ((CinematicPoint[])data.Points)[0] = new(0, 0);
        Near(player.PositionAt(0), new(2220, 850));
    }

    internal static void Spline()
    {
        var data = Fixture();
        var player = new CinematicPlayer(data);
        for (int segment = 0; segment < data.Points.Count - 1; segment++)
        {
            var a = data.Points[segment];
            var b = data.Points[segment + 1];
            for (int sample = 0; sample <= 64; sample++)
            {
                var p = player.Path[segment * 64 + sample];
                Check(p.X >= Math.Min(a.X, b.X) - .001 && p.X <= Math.Max(a.X, b.X) + .001, "Spline overshot X endpoints");
                Check(p.Y >= Math.Min(a.Y, b.Y) - .001 && p.Y <= Math.Max(a.Y, b.Y) + .001, "Spline overshot Y endpoints");
            }
        }
        // Unequal waypoint spacing must not turn into unequal travel speed.
        var line = new CinematicPlayer(data with { Points = new CinematicPoint[] { new(0, 0), new(1, 0), new(999, 0), new(1000, 0) } });
        for (int i = 0; i <= 100; i++) Near(line.PositionAt(i / 100.0).X, i * 10, .001);
        double target = player.Length / 1000;
        for (int i = 1; i <= 1000; i++)
        {
            double distance = Vector2.Distance(player.PositionAt((i - 1) / 1000.0), player.PositionAt(i / 1000.0));
            Check(distance <= target * 1.001 && distance >= target * .97, "Arc-length travel changed speed unexpectedly");
        }
        var duplicates = new CinematicPlayer(data with { Points = new CinematicPoint[] { new(0, 0), new(0, 0), new(10, 10), new(10, 10) } });
        Near(duplicates.PositionAt(.5), new(5, 5));
    }

    internal static void Pitch()
    {
        var original = Fixture();
        var data = original with { Keys = new CinematicKey[] { new(0, 2, 45, true, 30), new(.5f, 1, 90, true, -10), new(1, .05f, 0, false, 0) } };
        var player = new CinematicPlayer(data);
        var level = new CinematicPlayer(original);
        Near(player.Evaluate(.25).PitchDegrees, 10);
        Near(player.Evaluate(.5).PitchDegrees, -10);
        player.Playing = true;
        player.Update(TimeSpan.FromSeconds(3));
        Near(player.Pose.PitchDegrees, 10);
        var loaded = new CinematicPlayer(CinematicDefinition.Parse(data.ToJson(), "pitch.json"));
        for (int i = 0; i <= 100; i++)
        {
            double progress = i / 100.0;
            Check(player.Evaluate(progress) == loaded.Evaluate(progress));
            Check(player.Evaluate(progress) with { PitchDegrees = 0 } == level.Evaluate(progress), "Pitch changed path, scale, yaw or visibility");
        }
        // Existing exports have no pitch property and must keep their original view.
        var legacy = System.Text.Json.Nodes.JsonNode.Parse(original.ToJson())!;
        foreach (var key in legacy["keys"]!.AsArray()) key!.AsObject().Remove("pitchDegrees");
        var oldPlayer = new CinematicPlayer(CinematicDefinition.Parse(legacy.ToJsonString(), "old-export.json"));
        Check(oldPlayer.Definition.Keys.All(key => key.PitchDegrees == 0));
        Check(oldPlayer.Pose == level.Pose);
        Invalid(() => (data with { Keys = new CinematicKey[] { new(0, 1, 0, true, 91), new(1, 1, 0, true) } }).Validate("pitch.json"), "pitchDegrees");
        Invalid(() => (data with { Keys = new CinematicKey[] { new(0, 1, 0, true, float.NaN), new(1, 1, 0, true) } }).Validate("pitch.json"), "pitchDegrees");
        var coordinates = new CinematicCoordinates(new PanoramaView(0, 0, 2106, 747, 1), new(0, 0, 2106, 747));
        var pose = new CinematicPose(new(500, 400), 2, 0, true);
        var anchor = new CinematicPoint(32, 40);
        var nose = new Vector3(0, 7, -18);
        Check(coordinates.ProjectCarPoint(nose, pose with { PitchDegrees = 20 }, anchor).Y < coordinates.ProjectCarPoint(nose, pose, anchor).Y, "Positive pitch did not raise the nose");
        Check(coordinates.ProjectCarPoint(nose, pose with { PitchDegrees = -20 }, anchor).Y > coordinates.ProjectCarPoint(nose, pose, anchor).Y, "Negative pitch did not lower the nose");
        foreach (float yaw in new[] { 0f, 45f, 90f, 180f, 270f })
            foreach (float pitch in new[] { -90f, -20f, 0f, 20f, 90f })
            {
                var tilted = pose with { OrientationDegrees = yaw, PitchDegrees = pitch };
                Near(coordinates.ProjectCarPoint(Vector3.Zero, tilted, anchor), pose.GroundPosition);
                // Zero pitch must preserve the original diagnostic geometry exactly.
                var point = new Vector3(12, 25, 18);
                float angle = MathHelper.ToRadians(yaw);
                var expected = coordinates.CarTopLeft(tilted, anchor) + new Vector2(32 + point.X * MathF.Cos(angle) + point.Z * MathF.Sin(angle),
                    40 + (point.Z * MathF.Cos(angle) - point.X * MathF.Sin(angle)) * .35f - point.Y) * tilted.Scale;
                Near(coordinates.ProjectCarPoint(point, tilted with { PitchDegrees = 0 }, anchor), expected);
            }
    }

    internal static void Coordinates()
    {
        var player = new CinematicPlayer(Fixture());
        var camera = new PanoramaCamera();
        camera.NavigateToOptions();
        foreach (double seconds in new[] { 0.0, .4, .4 })
        {
            camera.Update(TimeSpan.FromSeconds(seconds));
            foreach (var (width, height) in new[] { (1280, 720), (1920, 1080), (3440, 1440), (3840, 2160), (1280, 1024) })
            {
                var coordinates = new CinematicCoordinates(camera.GetView(2106, 747, width, height), new(0, 0, width, height));
                foreach (var p in new[] { new Vector2(-100, -50), new Vector2(2220, 850), new Vector2(700, 500) })
                    Near(coordinates.ScreenToImage(coordinates.ImageToScreen(p)), p);
                var start = coordinates.ImageToScreen(player.PositionAt(0));
                // Entire 64x48 placeholder is beyond the bottom-right, not just its anchor.
                var topLeft = coordinates.CarTopLeft(player.Pose, player.Definition.GroundAnchor);
                Check(topLeft.X > width && topLeft.Y > height, "Entry sprite was partly on screen");
                Near(topLeft + new Vector2(32, 40) * player.Pose.Scale * (float)coordinates.View.Scale, start);
                var client = CinematicCoordinates.ClientToScreen(start / 2, width / 2, height / 2, width, height);
                Near(client, start);
            }
        }
        var letterbox = new CinematicCoordinates(new PanoramaView(300, 80, 1000, 500, 2), new(100, 50, 2000, 1000));
        Near(letterbox.ImageToScreen(new(300, 80)), new(100, 50));
        Near(letterbox.ScreenToImage(new(100, 50)), new(300, 80));
        var movedAnchor = Fixture() with { GroundAnchor = new(10, 20) };
        var pose = player.Evaluate(.7);
        Near(letterbox.CarTopLeft(pose, movedAnchor.GroundAnchor) + new Vector2(10, 20) * pose.Scale * 2,
            letterbox.ImageToScreen(pose.GroundPosition));
    }

    internal static void Storage()
    {
        var data = Fixture();
        var roundTrip = CinematicDefinition.Parse(data.ToJson(), "fixture.json");
        Check(roundTrip.ToJson() == data.ToJson());
        var a = new CinematicPlayer(data);
        var b = new CinematicPlayer(roundTrip);
        for (int i = 0; i <= 100; i++) Check(a.Evaluate(i / 100.0) == b.Evaluate(i / 100.0));
        Invalid(() => (data with { Version = 2 }).Validate("fixture.json"), "version");
        Invalid(() => (data with { ImageWidth = 1 }).Validate("fixture.json"), "dimensions");
        Invalid(() => (data with { DurationSeconds = double.NaN }).Validate("fixture.json"), "durationSeconds");
        Invalid(() => (data with { Speed = 0 }).Validate("fixture.json"), "speed");
        Invalid(() => (data with { Points = new[] { new CinematicPoint(0, 0), new(0, 0) } }).Validate("fixture.json"), "length");
        Invalid(() => (data with { Points = null! }).Validate("fixture.json"), "points");
        Invalid(() => (data with { Keys = new CinematicKey[] { new(1, 1, 0, true), new(0, 1, 0, true) } }).Validate("fixture.json"), "progress");
        Invalid(() => CinematicDefinition.Parse(data.ToJson().Replace("\"visible\": true", "\"unknown\": true"), "fixture.json"), "fixture.json");
        Invalid(() => CinematicDefinition.Parse("{}", "fixture.json"), "fixture.json");
        string directory = Path.Combine(Path.GetTempPath(), "pawnshop-cinematic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "cinematic.json");
            data.Save(path);
            Check(File.ReadAllText(path) == data.ToJson());
            var changed = data with { Speed = 1.5, Keys = new CinematicKey[] { new(0, 32, 45, true, 17.5f), new(1, .01f, 0, false, -12.25f) } };
            changed.Save(path);
            Check(CinematicDefinition.Load(path).ToJson() == changed.ToJson());
            Check(File.ReadAllText(path + ".bak") == data.ToJson());
            File.WriteAllText(path, "{ corrupt original }");
            Invalid(() => data.Save(path), "invalid cinematic JSON");
            Check(File.ReadAllText(path) == "{ corrupt original }");
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    internal static void Shipping()
    {
        var assembly = typeof(CinematicPlayer).Assembly;
        var editorTypes = assembly.GetTypes().Where(t => t.Namespace == "PawnShop.Game.Screens.CinematicEditor").ToArray();
#if DEBUG
        Check(editorTypes.Length > 0);
#else
        Check(editorTypes.Length == 0, "Release contains temporary editor classes");
        var title = assembly.GetType("PawnShop.Game.Screens.TitleScreen")!;
        Check(title.GetMethod("UpdateEditor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) is null);
#endif
        // An ordinary exported document is playable with runtime classes alone.
        var player = new CinematicPlayer(CinematicDefinition.Parse(Fixture().ToJson(), "export.json")) { Playing = true };
        player.Update(TimeSpan.FromSeconds(12));
        Check(player.Progress == 1 && !player.Playing && !player.Pose.Visible);
    }

#if DEBUG
    internal static void HeldAdjustments()
    {
        var input = new CinematicEditorInput();
        var none = Array.Empty<Keys>();
        bool Read(Keys[] keys, double seconds = 0, bool down = false, bool active = true, Vector2? point = null) =>
            input.Read(new(keys), new MouseState(0, 0, 0, down ? ButtonState.Pressed : ButtonState.Released,
                ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released), active, true,
                point ?? new Vector2(330, 185), 1280, 720, "unused-cinematic-test-path.json", TimeSpan.FromSeconds(seconds));
        void SelectRow(int row)
        {
            Read(none, point: new Vector2(100, 62 + row * 24));
            Read(none, down: true, point: new Vector2(100, 62 + row * 24));
            Read(none);
        }
        Read(none);
        Read(new[] { Keys.F8 });
        SelectRow(5);
        Read(new[] { Keys.Right });
        Near(input.Session.Player.Pose.Scale, 2.05);
        for (int i = 0; i < 3; i++) Read(new[] { Keys.Right }, .1);
        Near(input.Session.Player.Pose.Scale, 2.05); // Initial delay; a tap remains one increment.
        Read(new[] { Keys.Right }, .05);
        Near(input.Session.Player.Pose.Scale, 2.10);
        for (int i = 0; i < 37; i++) Read(new[] { Keys.Right }, .1);
        Check(input.Session.Player.Pose.Scale > 8, "Held adjustment did not accelerate beyond the previous limit");
        float released = input.Session.Player.Pose.Scale;
        Read(none, .1);
        Read(none, .2);
        Near(input.Session.Player.Pose.Scale, released);
        Read(new[] { Keys.LeftControl, Keys.Z });
        Near(input.Session.Player.Pose.Scale, 2); // Entire hold is one undo action.
        Read(none);
        Read(new[] { Keys.Right, Keys.LeftShift });
        for (int i = 0; i < 40; i++) Read(new[] { Keys.Right, Keys.LeftShift }, .1);
        Check(input.Session.Player.Pose.Scale is > 2 and < 2.25f, "Fine hold accelerated into coarse steps");
        Read(none);
        Read(new[] { Keys.LeftControl, Keys.Z });
        Near(input.Session.Player.Pose.Scale, 2);
        Read(none);
        // Mouse buttons use the same ramp and stop when the pointer leaves.
        Read(none, down: true);
        for (int i = 0; i < 40; i++) Read(none, .1, down: true);
        Check(input.Session.Player.Pose.Scale > 8);
        float stopped = input.Session.Player.Pose.Scale;
        Read(none, .2, down: true, point: new Vector2(100, 185));
        Read(none, .2, down: true);
        Near(input.Session.Player.Pose.Scale, stopped);
        Read(none);
        Read(new[] { Keys.LeftControl, Keys.Z });
        Near(input.Session.Player.Pose.Scale, 2);
        // Focus return with a held control must not resume editing.
        Read(none);
        Read(new[] { Keys.Right });
        float focusScale = input.Session.Player.Pose.Scale;
        Read(new[] { Keys.Right }, .1, active: false);
        Read(new[] { Keys.Right }, .2);
        Read(new[] { Keys.Right }, .2);
        Near(input.Session.Player.Pose.Scale, focusScale);
        Read(none);
        SelectRow(CinematicEditSession.VisibilityRow);
        bool visible = input.Session.Player.Pose.Visible;
        Read(new[] { Keys.Right });
        for (int i = 0; i < 40; i++) Read(new[] { Keys.Right }, .1);
        Check(input.Session.Player.Pose.Visible != visible, "Visibility repeated instead of toggling once");
        Read(none);
        SelectRow(5);
        Read(new[] { Keys.Right });
        for (int i = 0; i < 160; i++) Read(new[] { Keys.Right }, .1);
        Near(input.Session.Player.Pose.Scale, CinematicDefinition.MaximumScale);
        Read(none);
        Read(new[] { Keys.Left });
        for (int i = 0; i < 160; i++) Read(new[] { Keys.Left }, .1);
        Near(input.Session.Player.Pose.Scale, CinematicDefinition.MinimumScale);
        // Repeat totals should agree across presentation rates.
        float? expected = null;
        foreach (int rate in new[] { 20, 30, 60, 144 })
        {
            input = new();
            Read(none);
            Read(new[] { Keys.F8 });
            SelectRow(5);
            Read(new[] { Keys.Right });
            for (int i = 0; i < rate * 4; i++) Read(new[] { Keys.Right }, 1.0 / rate);
            expected ??= input.Session.Player.Pose.Scale;
            Near(input.Session.Player.Pose.Scale, expected.Value);
        }
        Read(none);
        SelectRow(CinematicEditSession.PitchRow);
        Read(new[] { Keys.Right, Keys.LeftShift });
        for (int i = 0; i < 8; i++) Read(new[] { Keys.Right, Keys.LeftShift }, .1);
        Check(input.Session.Player.Pose.PitchDegrees is > 0 and < 1);
        Read(none);
        Read(new[] { Keys.LeftControl, Keys.Z });
        Near(input.Session.Player.Pose.PitchDegrees, 0);
    }

    internal static void ExpandedCanvas()
    {
        foreach (var (width, height) in PawnShop.Game.Bootstrap.AppSettings.Resolutions.Concat(new[] { (1280, 1024) }))
        {
            var input = new CinematicEditorInput();
            var original = input.Session.Player.Definition.ToJson();
            bool Read(Keys[] keys, Vector2 p, bool down = false) => input.Read(new(keys),
                new MouseState(0, 0, 0, down ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                    ButtonState.Released, ButtonState.Released, ButtonState.Released), true, true, p,
                width, height, "unused-cinematic-test-path.json", TimeSpan.Zero);
            var none = Array.Empty<Keys>();
            Read(none, Vector2.Zero);
            Read(new[] { Keys.F8 }, Vector2.Zero);
            Read(none, Vector2.Zero);
            Read(new[] { Keys.V }, Vector2.Zero);
            Check(input.Expanded);
            Check(input.Session.Player.Definition.ToJson() == original, "Expanding the canvas changed cinematic data");
            var view = input.GetView(width, height);
            var frame = view.PlayerFrame;
            var canvas = view.Coordinates.Viewport;
            Check(canvas.Contains(frame), "Player frame escaped expanded canvas");
            Check(frame.Left > canvas.Left + 20 && frame.Top > canvas.Top + 20);
            Check(frame.Right < canvas.Right - 100 && frame.Bottom < canvas.Bottom - 100);
            // The player-frame corners still identify exactly the real camera crop.
            var playerView = input.Camera.GetView(2106, 747, width, height);
            var corner = view.Coordinates.ImageToScreen(new((float)playerView.X, (float)playerView.Y));
            Near(corner, new(frame.Left, frame.Top), 1);
            var point = input.Session.Player.Definition.Points[0];
            var handle = view.Coordinates.ImageToScreen(new(point.X, point.Y));
            var offscreen = new Vector2(frame.Right + 70, frame.Bottom + 70);
            Check(canvas.Contains((int)offscreen.X, (int)offscreen.Y));
            Read(none, handle);
            Read(none, handle, down: true);
            Read(none, offscreen, down: true);
            Read(none, offscreen);
            var moved = input.Session.Player.Definition.Points[0];
            Near(view.Coordinates.ImageToScreen(new(moved.X, moved.Y)), offscreen);
            input.Session.Player.Restart();
            var topLeft = view.Coordinates.CarTopLeft(input.Session.Player.Pose, input.Session.Player.Definition.GroundAnchor);
            Check(topLeft.X > frame.Right && topLeft.Y > frame.Bottom, "Car entry still overlaps the player screen");
            var exported = CinematicDefinition.Parse(input.Session.Player.Definition.ToJson(), "expanded-export.json");
            Check(exported.Points[0] == moved);
            // Return to player view: exported coordinates retain their offscreen placement.
            Read(new[] { Keys.V }, Vector2.Zero);
            Check(!input.Expanded);
            var normal = input.GetView(width, height);
            var normalStart = normal.Coordinates.ImageToScreen(new(moved.X, moved.Y));
            Check(normalStart.X > width && normalStart.Y > height);
            Read(none, Vector2.Zero);
            Read(new[] { Keys.LeftControl, Keys.Z }, Vector2.Zero);
            Check(input.Session.Player.Definition.ToJson() == original);
            // Background camera panning uses the same crop in expanded view.
            input.Camera.NavigateToOptions();
            input.Camera.Update(PanoramaCamera.TransitionDuration);
            var panned = CinematicEditorView.Create(input.Camera.GetView(2106, 747, width, height), width, height, true);
            Near(panned.Coordinates.ScreenToImage(panned.Coordinates.ImageToScreen(new(-100, 950))), new(-100, 950));
        }
    }

    internal static void Editing()
    {
        // A fresh draft must be immediately grabbable, clear of the authoring UI.
        var draft = new CinematicEditSession();
        foreach (var (width, height) in PawnShop.Game.Bootstrap.AppSettings.Resolutions.Concat(new[] { (1280, 1024) }))
        {
            var coordinates = new CinematicCoordinates(new PanoramaCamera().GetView(2106, 747, width, height), new(0, 0, width, height));
            foreach (var p in draft.Player.Definition.Points)
            {
                var screen = coordinates.ImageToScreen(new(p.X, p.Y));
                Check(screen.X >= 10 && screen.X < width - 10 && screen.Y >= 10 && screen.Y < height - 10);
                var hud = CinematicEditorInput.HudPoint(screen, width, height);
                Check(hud.Y < 620 && !CinematicEditorInput.Panel.Contains((int)hud.X, (int)hud.Y), "Draft handle is hidden under editor UI");
            }
        }
        Near(draft.Player.Pose.Scale, 2);
        Near(draft.Player.Evaluate(.85).Scale, .25);
        var session = new CinematicEditSession(Fixture());
        var initial = session.Player.Definition.ToJson();
        session.NewDraft();
        Check(session.Player.Definition.ToJson() == draft.Player.Definition.ToJson());
        session.Undo();
        Check(session.Player.Definition.ToJson() == initial, "New draft discarded the previous work");
        session.AddPoint(new(-50, 900));
        Check(session.Player.Definition.Points.Count == 6 && session.SelectedPoint == 5);
        session.Remember();
        session.MovePoint(new(-100, 1200));
        session.MovePoint(new(-200, 1300));
        session.Undo();
        Check(session.Player.Definition.Points[^1] == new CinematicPoint(-50, 900));
        session.Undo();
        Check(session.Player.Definition.ToJson() == initial);
        session.DeletePoint(0);
        Check(session.Player.Definition.Points.Count == 4);
        session.Undo();
        session.SelectedPoint = 0;
        session.Adjust(0, 1, false);
        Check(session.Player.Definition.Points[0].X == 2230);
        session.Adjust(1, 1, true);
        Check(session.Player.Definition.Points[0].Y == 851);
        session.Player.Seek(.25);
        session.AddKey();
        Check(session.SelectedKey == 1 && session.Player.Definition.Keys.Count == 4);
        session.Adjust(5, 1, false);
        session.Adjust(6, -1, false);
        session.Adjust(CinematicEditSession.VisibilityRow, 1, false);
        Near(session.Player.Pose.Scale, 1.55);
        Near(session.Player.Pose.OrientationDegrees, 62.5);
        Check(!session.Player.Pose.Visible);
        session.Adjust(4, 1, false);
        Near(session.Player.Progress, .26);
        session.Adjust(2, 1, false);
        session.Adjust(3, 1, false);
        Near(session.Player.Definition.DurationSeconds, 13);
        Near(session.Player.Definition.Speed, 1.1);
        session.DeleteKey();
        Check(session.Player.Definition.Keys.Count == 3);
        session.SelectKey(-1);
        session.DeleteKey();
        Check(session.Player.Definition.Keys.Count == 3);
        var before = session.Player.Definition.ToJson();
        try { session.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json")); }
        catch (FileNotFoundException) { }
        Check(session.Player.Definition.ToJson() == before);
        session = new(Fixture() with { Points = new CinematicPoint[] { new(0, 0), new(10, 10) } });
        session.DeletePoint(0);
        Check(session.Player.Definition.Points.Count == 2);
        var pitched = new CinematicEditSession(Fixture() with
        {
            Keys = new CinematicKey[] { new(0, 2, 45, true, 60), new(.5f, 1, 90, true, 0), new(1, .05f, 0, false, -20) }
        });
        pitched.Player.Seek(.25);
        pitched.AddKey();
        Near(pitched.Player.Definition.Keys[pitched.SelectedKey].PitchDegrees, 30);
        pitched.Adjust(CinematicEditSession.PitchRow, 1, false);
        Near(pitched.Player.Pose.PitchDegrees, 31);
        pitched.Adjust(CinematicEditSession.PitchRow, 1, true);
        Near(pitched.Player.Pose.PitchDegrees, 31.1);
        pitched.Undo();
        Near(pitched.Player.Pose.PitchDegrees, 31);
        pitched.Adjust(CinematicEditSession.PitchRow, 1000, false);
        Near(pitched.Player.Pose.PitchDegrees, 90);
        pitched.Adjust(CinematicEditSession.PitchRow, -1000, false);
        Near(pitched.Player.Pose.PitchDegrees, -90);
    }

    internal static void Input()
    {
        var input = new CinematicEditorInput(Fixture());
        bool Read(Keys[] keys, Vector2 point, bool left = false, bool right = false, bool active = true, bool canOpen = true, double seconds = 0) =>
            input.Read(new KeyboardState(keys), new MouseState((int)point.X, (int)point.Y, 0,
                left ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released,
                right ? ButtonState.Pressed : ButtonState.Released, ButtonState.Released, ButtonState.Released),
                active, canOpen, point, 1280, 720, "unused-cinematic-test-path.json", TimeSpan.FromSeconds(seconds));
        var none = Array.Empty<Keys>();
        Check(!Read(new[] { Keys.F8 }, Vector2.Zero)); // Focus-return held toggle suppressed.
        Read(none, Vector2.Zero);
        Check(!Read(new[] { Keys.F8 }, Vector2.Zero, canOpen: false));
        Read(none, Vector2.Zero);
        Check(Read(new[] { Keys.F8 }, Vector2.Zero) && input.Open);
        var original = input.Session.Player.Definition.ToJson();
        Read(none, new(900, 400));
        Read(none, new(900, 400), left: true);
        var point = input.Session.Player.Definition.Points[^1];
        Near(new Vector2(point.X, point.Y), new CinematicCoordinates(new PanoramaCamera().GetView(2106, 747, 1280, 720), new(0, 0, 1280, 720)).ScreenToImage(new(900, 400)));
        Read(none, new(900, 400));
        Read(none, new(900, 400), left: true);
        Read(none, new(950, 420), left: true);
        Read(none, new(960, 430), left: true);
        Read(none, new(960, 430));
        Read(new[] { Keys.LeftControl, Keys.Z }, Vector2.Zero);
        Check(input.Session.Player.Definition.Points[^1] == point, "Drag did not undo as one action");
        Read(none, Vector2.Zero);
        Read(new[] { Keys.LeftControl, Keys.Z }, Vector2.Zero);
        Check(input.Session.Player.Definition.ToJson() == original);
        Read(none, new(900, 400));
        Read(none, new(900, 400), left: true);
        Read(none, new(900, 400));
        Read(none, new(900, 400), right: true);
        Check(input.Session.Player.Definition.ToJson() == original);
        Read(none, Vector2.Zero);
        Read(new[] { Keys.Space }, Vector2.Zero, seconds: 1);
        Check(input.Session.Player.Playing);
        double before = input.Session.Player.Progress;
        Read(none, Vector2.Zero, active: false, seconds: 5);
        Read(new[] { Keys.Space }, Vector2.Zero, seconds: 5);
        Near(input.Session.Player.Progress, before);
        Read(none, new(815, 685));
        Read(none, new(815, 685), left: true);
        Near(input.Session.Player.Progress, .5);
        Check(!input.Session.Player.Playing);
        Read(none, new(1250, 685), left: true);
        Near(input.Session.Player.Progress, 1);
        Read(none, Vector2.Zero);
        Read(new[] { Keys.R }, Vector2.Zero);
        Near(input.Session.Player.Progress, 0);
        Read(none, Vector2.Zero);
        Check(Read(new[] { Keys.F8, Keys.Space }, Vector2.Zero) && !input.Open, "Closing frame was not captured");
        Check(!Read(new[] { Keys.F8, Keys.Space }, Vector2.Zero));
        Check(!input.Session.Player.Playing);
        Read(none, Vector2.Zero);
        Read(new[] { Keys.F8 }, Vector2.Zero);
        Read(none, Vector2.Zero);
        Read(new[] { Keys.LeftControl, Keys.N }, Vector2.Zero);
        Check(input.Session.Player.Definition.ToJson() == CinematicEditSession.Draft().ToJson());
        Read(none, Vector2.Zero);
        Read(new[] { Keys.LeftControl, Keys.Z }, Vector2.Zero);
        Check(input.Session.Player.Definition.ToJson() == original);
        // Hit geometry follows the contained HUD through letterboxing/output scale.
        foreach (var (width, height) in new[] { (1280, 720), (1920, 1080), (3440, 1440), (3840, 2160), (1280, 1024) })
        {
            input = new(Fixture());
            bool At(Keys[] keys, Vector2 hud)
            {
                float scale = Math.Min(width / 1280f, height / 720f);
                var p = hud * scale + new Vector2((width - (int)(1280 * scale)) / 2, (height - (int)(720 * scale)) / 2);
                return input.Read(new(keys), new(), true, true, p, width, height, "unused-cinematic-test-path.json", TimeSpan.Zero);
            }
            At(none, Vector2.Zero);
            At(new[] { Keys.F8 }, Vector2.Zero);
            At(none, Vector2.Zero);
            // Test the same panel/playhead mapping without depending on native mouse rounding.
            float outputScale = Math.Min(width / 1280f, height / 720f);
            var outputOffset = new Vector2((width - (int)(1280 * outputScale)) / 2, (height - (int)(720 * outputScale)) / 2);
            var click = new Vector2(332, 68) * outputScale + outputOffset;
            var pressed = new MouseState(0, 0, 0, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            input.Read(new(), pressed, true, true, click, width, height, "unused-cinematic-test-path.json", TimeSpan.Zero);
            Near(input.Session.Player.Definition.Points[0].X, 2230);
            At(none, Vector2.Zero);
            var scrub = new Vector2(597.5f, 685) * outputScale + outputOffset;
            input.Read(new(), pressed, true, true, scrub, width, height, "unused-cinematic-test-path.json", TimeSpan.Zero);
            Near(input.Session.Player.Progress, .25);
        }
    }
#endif
}
