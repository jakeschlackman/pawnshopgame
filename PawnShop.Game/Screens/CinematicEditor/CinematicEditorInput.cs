#if DEBUG
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap.Cinematics;

namespace PawnShop.Game.Screens.CinematicEditor;

// Device-free input controller, so capture/focus/drag behavior is regression tested.
public sealed class CinematicEditorInput
{
    private KeyboardState previous;
    private MouseState previousMouse;
    private bool wasActive;
    private bool draggingPoint;
    private bool scrubbing;
    private Vector2 grabOffset;
    private bool rightView;
    private int repeatRow = -1;
    private int repeatDirection;
    private bool repeatPointer;
    private TimeSpan repeatElapsed;
    private TimeSpan nextRepeat;
    public bool Open { get; private set; }
    public bool Expanded { get; private set; }
    public int Row { get; private set; }
    public CinematicEditSession Session { get; }
    public PanoramaCamera Camera { get; } = new();
    public static Rectangle Panel => new(8, 8, 350, 434);
    public static Rectangle Timeline => new(380, 674, 870, 24);
    public static Rectangle AdjustmentButton(int row, int direction) => new(direction < 0 ? 286 : 318, 59 + row * 24, 28, 20);

    public CinematicEditorInput(CinematicDefinition? definition = null) => Session = new(definition);

    public CinematicEditorView GetView(int width, int height) =>
        CinematicEditorView.Create(Camera.GetView(2106, 747, width, height), width, height, Expanded);

    public static Vector2 HudPoint(Vector2 screen, int width, int height)
    {
        float scale = Math.Min(width / 1280f, height / 720f);
        return (screen - new Vector2((width - (int)(1280 * scale)) / 2, (height - (int)(720 * scale)) / 2)) / scale;
    }

    public bool Read(KeyboardState keys, MouseState mouse, bool active, bool canOpen,
        Vector2 screenPoint, int width, int height, string path, TimeSpan delta)
    {
        bool eligible = active && wasActive;
        bool Pressed(Keys key) => eligible && keys.IsKeyDown(key) && previous.IsKeyUp(key);
        bool captured = Open;
        if (Pressed(Keys.F8) && (Open || canOpen))
        {
            Open = !Open;
            captured = true; // Closing input belongs to the editor too.
            draggingPoint = scrubbing = false;
            StopAdjustment();
        }
        else if (Open && eligible)
        {
            Camera.Update(delta);
            var hud = HudPoint(screenPoint, width, height);
            bool ctrl = keys.IsKeyDown(Keys.LeftControl) || keys.IsKeyDown(Keys.RightControl);
            bool shift = keys.IsKeyDown(Keys.LeftShift) || keys.IsKeyDown(Keys.RightShift);
            bool left = mouse.LeftButton == ButtonState.Pressed;
            bool clicked = left && previousMouse.LeftButton == ButtonState.Released;
            bool right = mouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released;
            bool edited = false;
            bool adjustmentStarted = false;
            bool command = true;
            if (clicked) StopAdjustment();
            try
            {
                if (ctrl && Pressed(Keys.N))
                {
                    Session.NewDraft();
                    rightView = false;
                    Camera.NavigateToMainMenu();
                    edited = true;
                }
                else if (ctrl && Pressed(Keys.S)) { Session.Player.Definition.Save(path); Session.Status = "Saved and round-trip verified. See documentation for export path."; }
                else if (ctrl && Pressed(Keys.L)) { Session.Load(path); edited = true; }
                else if (ctrl && Pressed(Keys.Z)) { Session.Undo(); edited = true; }
                else if (Pressed(Keys.Space)) Session.Player.Playing = !Session.Player.Playing;
                else if (Pressed(Keys.R)) Session.Player.Restart();
                else if (Pressed(Keys.V))
                {
                    Expanded = !Expanded;
                    draggingPoint = scrubbing = false;
                }
                else if (Pressed(Keys.C))
                {
                    draggingPoint = false;
                    rightView = !rightView;
                    if (rightView) Camera.NavigateToOptions(); else Camera.NavigateToMainMenu();
                }
                else if (Pressed(Keys.Tab)) Session.SelectedPoint = (Session.SelectedPoint + 1) % Session.Player.Definition.Points.Count;
                else if (Pressed(Keys.OemOpenBrackets)) Session.SelectKey(-1);
                else if (Pressed(Keys.OemCloseBrackets)) Session.SelectKey(1);
                else if (Pressed(Keys.K)) { Session.AddKey(); edited = true; }
                else if (Pressed(Keys.Delete))
                {
                    if (shift) Session.DeleteKey(); else Session.DeletePoint(Session.SelectedPoint);
                    edited = true;
                }
                else if (Pressed(Keys.Up)) Row = (Row + CinematicEditSession.FieldCount - 1) % CinematicEditSession.FieldCount;
                else if (Pressed(Keys.Down)) Row = (Row + 1) % CinematicEditSession.FieldCount;
                else if (Pressed(Keys.Left) || Pressed(Keys.Right))
                {
                    if (keys.IsKeyDown(Keys.Left) != keys.IsKeyDown(Keys.Right))
                    {
                        StartAdjustment(keys.IsKeyDown(Keys.Left) ? -1 : 1, shift, pointer: false);
                        adjustmentStarted = true;
                    }
                    else StopAdjustment();
                    edited = true;
                }
                else command = false;
                if (command && !adjustmentStarted) StopAdjustment();

                if (edited) draggingPoint = scrubbing = false;
                if (!left) draggingPoint = scrubbing = false;
                var coordinates = GetView(width, height).Coordinates;
                bool scene = !Expanded || coordinates.Viewport.Contains((int)screenPoint.X, (int)screenPoint.Y);
                var pixel = new Point((int)hud.X, (int)hud.Y);
                if (clicked && Panel.Contains(pixel))
                {
                    int row = ((int)hud.Y - 59) / 24;
                    if (hud.Y >= 59 && row is >= 0 and < CinematicEditSession.FieldCount)
                    {
                        Row = row;
                        int direction = AdjustmentButton(row, -1).Contains(pixel) ? -1
                            : AdjustmentButton(row, 1).Contains(pixel) ? 1 : 0;
                        if (direction != 0)
                        {
                            StartAdjustment(direction, shift, pointer: true);
                            adjustmentStarted = edited = true;
                        }
                    }
                }
                else if (clicked && Timeline.Contains(pixel)) scrubbing = true;
                else if (clicked && scene && hud.Y < 630 && !edited)
                {
                    var p = coordinates.ScreenToImage(screenPoint);
                    int hit = Session.HitPoint(p, (float)(10 / coordinates.View.Scale));
                    if (hit >= 0)
                    {
                        Session.SelectedPoint = hit;
                        var original = Session.Player.Definition.Points[hit];
                        grabOffset = new Vector2(original.X, original.Y) - p;
                        Session.Remember();
                        draggingPoint = true;
                        Session.Player.Playing = false;
                    }
                    else Session.AddPoint(p);
                }
                if (right && scene && !Panel.Contains(pixel) && hud.Y < 630)
                {
                    draggingPoint = false;
                    Session.DeletePoint(Session.HitPoint(coordinates.ScreenToImage(screenPoint), (float)(10 / coordinates.View.Scale)));
                }
                if (!adjustmentStarted && RepeatAdjustment(keys, left, pixel, shift, delta)) edited = true;
                if (draggingPoint) Session.MovePoint(coordinates.ScreenToImage(screenPoint) + grabOffset);
                if (scrubbing)
                {
                    Session.Player.Playing = false;
                    Session.Player.Seek((hud.X - Timeline.X) / Timeline.Width);
                }
                if (!scrubbing && !edited) Session.Player.Update(delta);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                Session.Status = e.Message;
                draggingPoint = scrubbing = false;
                StopAdjustment();
            }
        }
        if (!active || !wasActive)
        {
            draggingPoint = scrubbing = false;
            StopAdjustment();
        }
        previous = keys;
        previousMouse = mouse;
        wasActive = active;
        return captured;
    }

    private void StopAdjustment() => repeatRow = -1;

    private void StartAdjustment(int direction, bool fine, bool pointer)
    {
        draggingPoint = scrubbing = false;
        Session.Adjust(Row, direction, fine);
        repeatRow = Row == CinematicEditSession.VisibilityRow ? -1 : Row; // Visibility is a single toggle, never a repeat.
        repeatDirection = direction;
        repeatPointer = pointer;
        repeatElapsed = TimeSpan.Zero;
        nextRepeat = TimeSpan.FromMilliseconds(350);
    }

    private bool RepeatAdjustment(KeyboardState keys, bool left, Point pixel, bool fine, TimeSpan delta)
    {
        if (repeatRow < 0) return false;
        bool held = repeatPointer ? left && AdjustmentButton(repeatRow, repeatDirection).Contains(pixel)
            : keys.IsKeyDown(repeatDirection < 0 ? Keys.Left : Keys.Right)
                && !(keys.IsKeyDown(Keys.Left) && keys.IsKeyDown(Keys.Right));
        if (!held || Row != repeatRow) { StopAdjustment(); return false; }
        // Avoid a large jump after a stalled frame. Normal repeat timing uses
        // supplied presentation delta, independent of the OS keyboard repeat rate.
        repeatElapsed += delta > TimeSpan.FromMilliseconds(250) ? TimeSpan.FromMilliseconds(250) : delta;
        bool changed = false;
        while (repeatElapsed >= nextRepeat)
        {
            int increments = fine ? 1 : nextRepeat.TotalSeconds >= 3 ? 10 : nextRepeat.TotalSeconds >= 1.5 ? 5 : 1;
            Session.Adjust(repeatRow, repeatDirection * increments, fine, remember: false);
            nextRepeat += TimeSpan.FromMilliseconds(100);
            changed = true;
        }
        return changed;
    }
}
#endif
