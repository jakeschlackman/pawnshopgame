#if DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap.Cinematics;

namespace PawnShop.Game.Screens.CinematicEditor;

internal sealed class CinematicEditorOverlay : IDisposable
{
    private readonly PixelRenderer hud;
    private readonly TitleCinematicRenderer drawing;
    private readonly GraphicsDevice device;
    public CinematicEditorInput Input { get; }

    public CinematicEditorOverlay(GraphicsDevice device, CinematicDefinition? definition)
    {
        this.device = device;
        Input = new(definition);
        hud = new(device, 1280, 720, integerScaling: false);
        try { drawing = new(device); }
        catch { hud.Dispose(); throw; }
    }

    public void Draw(Action<PanoramaView, Rectangle> drawBackground)
    {
        var session = Input.Session;
        var player = session.Player;
        var data = player.Definition;
        var viewport = device.PresentationParameters;
        var view = Input.GetView(viewport.BackBufferWidth, viewport.BackBufferHeight);
        var coordinates = view.Coordinates;
        var key = data.Keys[session.SelectedKey];
        var point = data.Points[session.SelectedPoint];
        hud.Begin(Color.Transparent);
        var panel = CinematicEditorInput.Panel;
        hud.Fill(panel.X, panel.Y, panel.Width, panel.Height, Color.Black * .9f);
        hud.Text("F8 CINEMATIC - TEMPORARY DEBUG EDITOR", 18, 18, Color.Gold);
        hud.Text($"POINT {session.SelectedPoint + 1}/{data.Points.Count}   KEY {session.SelectedKey + 1}/{data.Keys.Count}", 18, 40, Color.White);
        string[] labels =
        {
            $"IMAGE X: {point.X:0.0}", $"IMAGE Y: {point.Y:0.0}",
            $"DURATION: {data.DurationSeconds:0.0} SECONDS", $"SPEED: {data.Speed:0.00}",
            $"KEY TIME: {key.Progress:0.000}", $"KEY SCALE: {key.Scale:0.00}",
            $"KEY YAW: {key.OrientationDegrees:0.0}", $"KEY PITCH: {key.PitchDegrees:0.0}",
            $"KEY VISIBLE: {(key.Visible ? "YES" : "NO")}",
            $"ANCHOR X: {data.GroundAnchor.X:0.0}", $"ANCHOR Y: {data.GroundAnchor.Y:0.0}"
        };
        for (int i = 0; i < labels.Length; i++)
        {
            int y = 62 + i * 24;
            if (Input.Row == i) hud.Fill(14, y - 4, 338, 22, new Color(38, 63, 73));
            hud.Text(labels[i], 22, y, Color.White);
            var minus = CinematicEditorInput.AdjustmentButton(i, -1);
            var plus = CinematicEditorInput.AdjustmentButton(i, 1);
            hud.Fill(minus.X, minus.Y, minus.Width, minus.Height, Color.DarkSlateGray);
            hud.Fill(plus.X, plus.Y, plus.Width, plus.Height, Color.DarkSlateGray);
            hud.Text("-", 297, y + 2, Color.White);
            // PixelFont has no '+' glyph; '>' is the increment button.
            hud.Text(">", 329, y + 2, Color.White);
        }
        hud.Text("LEFT/RIGHT OR BUTTONS: HOLD TO RAMP", 18, 334, Color.Gold);
        hud.Text("SHIFT: FINE - TAB: NEXT POINT", 18, 352, Color.White);
        hud.Text("K: KEY AT PLAYHEAD - BRACKETS: KEY", 18, 370, Color.White);
        hud.Text("DEL: POINT - SHIFT DEL: KEY", 18, 388, Color.White);
        hud.Text("CTRL S: SAVE - CTRL L: LOAD", 18, 406, Color.White);
        hud.Text("CTRL Z: UNDO - CTRL N: NEW - C: PAN", 18, 424, Color.White);
        hud.Fill(380, 8, 870, 20, Color.Black * .9f);
        hud.Text(Input.Expanded ? "V: PLAYER VIEW - GOLD FRAME: PLAYER SCREEN - DRAG POINTS OUTSIDE TO ENTER OFFSCREEN"
            : "V: EXPAND CANVAS TO PLACE POINTS BEYOND THE PLAYER SCREEN", 390, 14, Color.Gold);
        hud.Fill(8, 630, 1256, 82, Color.Black * .9f);
        hud.Text("LEFT CLICK: APPEND / DRAG - RIGHT CLICK: DELETE - SPACE: PLAY/PAUSE - R: RESTART - F8: CLOSE", 18, 640, Color.Gold);
        string status = session.Status.Replace('\r', ' ').Replace('\n', ' ');
        hud.Text(status.Length > 200 ? status[..200] : status, 18, 656, Color.White);
        hud.Text($"{(player.Playing ? "PLAY" : "PAUSE")} {player.Progress * 100:0.0} PERCENT", 18, 683, Color.White);
        hud.Fill(380, 674, 870, 24, Color.DarkSlateGray);
        foreach (var marker in data.Keys)
            hud.Fill(380 + (int)(marker.Progress * 870) - 2, 674, 4, 8, Color.Orange);
        hud.Fill(380 + (int)(player.Progress * 870) - 4, 672, 8, 28, Color.Gold);
        for (int i = 0; i < data.Points.Count; i++)
        {
            var p = coordinates.ImageToScreen(new(data.Points[i].X, data.Points[i].Y));
            var h = CinematicEditorInput.HudPoint(p, viewport.BackBufferWidth, viewport.BackBufferHeight);
            if (h.X >= 0 && h.X < 1260 && h.Y >= 0 && h.Y < 620 && !CinematicEditorInput.Panel.Contains((int)h.X, (int)h.Y)
                && (!Input.Expanded || coordinates.Viewport.Contains((int)p.X, (int)p.Y)))
                hud.Text((i + 1).ToString(), (int)h.X + 8, (int)h.Y - 10, Color.White);
        }
        hud.Present(clearBackBuffer: false, drawBackground: () =>
        {
            drawBackground(coordinates.View, coordinates.Viewport);
            Rectangle? clip = Input.Expanded ? coordinates.Viewport : null;
            drawing.DrawCar(player, coordinates, clip);
            drawing.Begin(clip);
            if (Input.Expanded)
            {
                var frame = view.PlayerFrame;
                drawing.Line(new(frame.Left, frame.Top), new(frame.Right, frame.Top), Color.Gold, 3);
                drawing.Line(new(frame.Right, frame.Top), new(frame.Right, frame.Bottom), Color.Gold, 3);
                drawing.Line(new(frame.Right, frame.Bottom), new(frame.Left, frame.Bottom), Color.Gold, 3);
                drawing.Line(new(frame.Left, frame.Bottom), new(frame.Left, frame.Top), Color.Gold, 3);
            }
            var samples = player.Path;
            for (int i = 1; i < samples.Count; i++)
                drawing.Line(coordinates.ImageToScreen(samples[i - 1]), coordinates.ImageToScreen(samples[i]), Color.LimeGreen);
            for (int i = 1; i < 16; i++)
            {
                var p = coordinates.ImageToScreen(player.PositionAt(i / 16.0));
                var direction = p - coordinates.ImageToScreen(player.PositionAt(i / 16.0 - .002));
                if (direction.LengthSquared() < .001f) continue;
                direction.Normalize();
                var normal = new Vector2(-direction.Y, direction.X);
                drawing.Line(p, p - direction * 10 + normal * 5, Color.Gold);
                drawing.Line(p, p - direction * 10 - normal * 5, Color.Gold);
            }
            for (int i = 0; i < data.Points.Count; i++)
                drawing.Handle(coordinates.ImageToScreen(new(data.Points[i].X, data.Points[i].Y)),
                    i == session.SelectedPoint ? Color.Gold : Color.White);
            var ground = coordinates.ImageToScreen(player.Pose.GroundPosition);
            drawing.Line(ground - new Vector2(8, 0), ground + new Vector2(8, 0), Color.Magenta);
            drawing.Line(ground - new Vector2(0, 8), ground + new Vector2(0, 8), Color.Magenta);
            drawing.End();
        });
    }

    public void Dispose() { drawing.Dispose(); hud.Dispose(); }
}
#endif
