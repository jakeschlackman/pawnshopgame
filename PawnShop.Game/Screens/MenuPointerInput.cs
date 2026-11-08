using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PawnShop.Framework.Input;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

public static class TitleMenuLayout
{
    public const float FirstRow = 0.572222f, RowSpacing = 0.076852f;
    public static Rectangle Row(int index) => new(240, (int)MathF.Round((FirstRow + index * RowSpacing) * 1080) - 6, 1340, 78);
}

// Pointer clicks translate to the same menu actions as keyboard input; no settings rules here.
public sealed class MenuPointerInput
{
    private bool wasDown;
    private bool wasActive;
    private int wheel;
    private bool dragging;
    private int grab;

    public InputFrame Read(ApplicationFlow flow, MouseState mouse, bool active, int clientWidth, int clientHeight,
        int backBufferWidth, int backBufferHeight)
    {
        bool down = mouse.LeftButton == ButtonState.Pressed;
        bool clicked = active && wasActive && down && !wasDown;
        int delta = active && wasActive ? mouse.ScrollWheelValue - wheel : 0;
        wheel = mouse.ScrollWheelValue;
        wasDown = down;
        wasActive = active;
        if (!active || clientWidth <= 0 || clientHeight <= 0) { dragging = false; return default; }
        int x = (int)((double)mouse.X * backBufferWidth / clientWidth);
        int y = (int)((double)mouse.Y * backBufferHeight / clientHeight);
        var hit = HitTest(flow, x, y, backBufferWidth, backBufferHeight);
        if (flow.Screen == Screen.Options)
        {
            var model = flow.Options;
            model.HoveredSelection = hit.SelectionIndex;
            model.HoveredTab = hit.TabIndex;
            model.PointerDown = down;
            var point = OptionsPoint(x, y, backBufferWidth, backBufferHeight);
            if (!flow.TitleInteractionPlaying && !model.Previewing && !model.CapturingKey)
            {
                var thumb = OptionsMenuLayout.Thumb(model);
                if (clicked && OptionsMenuLayout.Track.Contains(point))
                {
                    dragging = true;
                    grab = thumb.Contains(point) ? point.Y - thumb.Y : thumb.Height / 2;
                }
                if (!down) dragging = false;
                if (dragging)
                {
                    int travel = OptionsMenuLayout.Track.Height - thumb.Height;
                    return new(ScrollPosition: travel <= 0 ? 0 :
                        (point.Y - OptionsMenuLayout.Track.Y - grab) * model.Scroll.Maximum / travel);
                }
                if (delta != 0 && OptionsMenuLayout.Viewport.Contains(point))
                    return new(ScrollDelta: -delta * 48 / 120);
            }
            else dragging = false;
        }
        else dragging = false;
        return clicked ? hit : default;
    }

    private static Point OptionsPoint(int x, int y, int width, int height)
    {
        double scale = Math.Min(width / 1280d, height / 720d);
        int w = (int)(1280 * scale), h = (int)(720 * scale);
        return w <= 0 || h <= 0 ? new(-1, -1) : new(
            (int)((x - (width - w) / 2) * 1280d / w),
            (int)((y - (height - h) / 2) * 720d / h));
    }

    public static InputFrame HitTest(ApplicationFlow flow, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0 || flow.TitleInteractionPlaying || flow.UsesTitleBackground && !flow.TitleUiVisible) return default;
        bool options = flow.Screen == Screen.Options;
        if (options && flow.Options.CapturingKey) return default;
        int canvasWidth = options ? 1280 : flow.Screen == Screen.Title ? 1920 : 640;
        int canvasHeight = options ? 720 : flow.Screen == Screen.Title ? 1080 : 360;
        double scale = Math.Min((double)width / canvasWidth, (double)height / canvasHeight);
        if (!options && flow.Screen != Screen.Title && scale >= 1) scale = Math.Floor(scale);
        int outputWidth = (int)(canvasWidth * scale), outputHeight = (int)(canvasHeight * scale);
        var output = new Rectangle((width - outputWidth) / 2, (height - outputHeight) / 2, outputWidth, outputHeight);
        if (!output.Contains(x, y)) return default;
        var point = new Point((int)((x - output.X) * (double)canvasWidth / outputWidth),
            (int)((y - output.Y) * (double)canvasHeight / outputHeight));
        if (options)
        {
            if (flow.Options.Previewing)
            {
                if (OptionsMenuLayout.Keep.Contains(point)) return new(Confirm: true);
                if (OptionsMenuLayout.Revert.Contains(point)) return new(Cancel: true);
                return default;
            }
            for (int i = 0; i < OptionsModel.Tabs.Count; i++)
                if (OptionsMenuLayout.Tab(i).Contains(point)) return new(TabIndex: i);
            for (int action = 0; action < 3; action++)
                if (OptionsMenuLayout.Footer(action).Contains(point))
                    return new(Confirm: true, SelectionIndex: flow.Options.FieldCount + action);
            if (OptionsMenuLayout.Viewport.Contains(point))
                for (int i = 0; i < flow.Options.FieldCount; i++)
                    if (OptionsMenuLayout.Field(i, flow.Options).Contains(point))
                    {
                        int? slot = flow.Options.Tab == 3
                            ? OptionsMenuLayout.Cell(i, 1, flow.Options).Contains(point) ? 1 : 0 : null;
                        return new(Confirm: true, SelectionIndex: i, BindingIndex: slot);
                    }
        }
        else if (flow.Screen == Screen.Room)
        {
            foreach (var npc in flow.Npcs.Reverse())
            {
                var bounds = npc.Content.Definition.InteractionBounds;
                if (new Rectangle(npc.X + bounds.X, npc.Y + bounds.Y, bounds.Width, bounds.Height).Contains(point)) return new(TalkTargetId: npc.Id);
            }
        }
        else if (flow.Screen == Screen.Dialogue && flow.Dialogue is { } dialogue)
        {
            if (dialogue.ChoicesVisible)
                for (int row = 0; row < 3; row++)
                    if (dialogue.FirstVisibleChoice + row < dialogue.View.Choices.Count && DialogueScreen.ChoiceRow(row).Contains(point))
                        return new(Confirm: true, SelectionIndex: dialogue.FirstVisibleChoice + row);
            if (new Rectangle(24, 42, 592, 145).Contains(point)) return new(Confirm: true);
        }
        else if (flow.Screen == Screen.CharacterSelect)
        {
            for (int i = 0; i < 2; i++)
                if (CharacterSelectionLayout.Card(i).Contains(point)) return new(Confirm: true, SelectionIndex: i);
            if (CharacterSelectionLayout.Back.Contains(point)) return new(Cancel: true);
        }
        else if (flow.Menu is { } menu)
        {
            for (int i = 0; i < menu.Items.Count; i++)
            {
                var row = flow.Screen == Screen.Title ? TitleMenuLayout.Row(i) : new Rectangle(44, 129 + i * 32, 540, 27);
                if (row.Contains(point)) return new(Confirm: true, SelectionIndex: i);
            }
        }
        return default;
    }
}
