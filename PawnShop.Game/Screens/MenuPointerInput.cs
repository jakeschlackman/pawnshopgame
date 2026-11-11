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
    private bool draggingDropdown;
    private int grab;
    private int? volumeDragRow;
    private int volumeGrabOffset;

    public InputFrame Read(ApplicationFlow flow, MouseState mouse, bool active, int clientWidth, int clientHeight,
        int backBufferWidth, int backBufferHeight)
    {
        bool down = mouse.LeftButton == ButtonState.Pressed;
        bool clicked = active && wasActive && down && !wasDown;
        int delta = active && wasActive ? mouse.ScrollWheelValue - wheel : 0;
        wheel = mouse.ScrollWheelValue;
        wasDown = down;
        wasActive = active;
        if (!active || clientWidth <= 0 || clientHeight <= 0)
        {
            dragging = draggingDropdown = false;
            volumeDragRow = null;
            flow.Options.HoveredScrollbar = false;
            return default;
        }
        int x = (int)((double)mouse.X * backBufferWidth / clientWidth);
        int y = (int)((double)mouse.Y * backBufferHeight / clientHeight);
        var hit = HitTest(flow, x, y, backBufferWidth, backBufferHeight);
        if (flow.Screen == Screen.Options)
        {
            var model = flow.Options;
            model.HoveredSelection = hit.SelectionIndex;
            model.HoveredTab = hit.TabIndex;
            model.HoveredChoice = hit.ChoiceIndex;
            model.PointerDown = down;
            model.HoveredModalAction = model.ConfirmingDiscard ? hit.ChoiceIndex
                : model.Previewing ? hit.Confirm ? 0 : hit.Cancel ? 1 : null : null;
            var point = OptionsPoint(x, y, backBufferWidth, backBufferHeight);
            model.HoveredBinding = model.Tab == 3 && hit.SelectionIndex is { } rowIndex
                && rowIndex < model.FieldCount && hit.BindingIndex is { } slotIndex
                && OptionsMenuLayout.Cell(rowIndex, slotIndex, model).Contains(point) ? slotIndex : null;
            model.HoveredScrollbar = false;
            if (model.DropdownRow is not null)
            {
                volumeDragRow = null;
                dragging = false;
                int maximum = model.DropdownCount - OptionsMenuLayout.DropdownVisibleCount(model);
                var track = OptionsMenuLayout.DropdownTrack(model);
                var thumb = OptionsMenuLayout.DropdownThumb(model);
                if (maximum > 0 && clicked && track.Contains(point))
                {
                    draggingDropdown = true;
                    grab = thumb.Contains(point) ? point.Y - thumb.Y : thumb.Height / 2;
                }
                if (!down) draggingDropdown = false;
                if (draggingDropdown)
                    return new(ScrollPosition: (point.Y - track.Y - grab) * maximum / Math.Max(1, track.Height - thumb.Height));
                if (delta != 0 && OptionsMenuLayout.Dropdown(model).Contains(point))
                    return new(ScrollDelta: -delta * 48 / 120);
                return clicked ? hit : default;
            }
            draggingDropdown = false;
            if (!flow.TitleInteractionPlaying && !model.HasModal && !model.CapturingKey)
            {
                if (!down || model.Tab != 1) volumeDragRow = null;
                if (clicked && model.Tab == 1 && OptionsMenuLayout.Viewport.Contains(point))
                    for (int row = 0; row < OptionsModel.AudioVolumeCount; row++)
                    {
                        var watch = OptionsMenuLayout.AudioThumb(row, model);
                        if (!OptionsMenuLayout.AudioTrack(row, model).Contains(point) && !watch.Contains(point)) continue;
                        volumeDragRow = row;
                        volumeGrabOffset = watch.Contains(point) ? point.X - (watch.X + 14) : 0;
                        break;
                    }
                if (volumeDragRow is { } audioRow)
                {
                    dragging = false;
                    model.HoveredSelection = audioRow;
                    return new(SelectionIndex: audioRow,
                        SliderValue: OptionsMenuLayout.AudioValueAt(audioRow, point.X - volumeGrabOffset, model));
                }
                var thumb = OptionsMenuLayout.Thumb(model);
                if (model.Scroll.Maximum == 0) dragging = false;
                if (model.Scroll.Maximum > 0 && clicked
                    && (OptionsMenuLayout.Track.Contains(point) || thumb.Contains(point)))
                {
                    dragging = true;
                    grab = thumb.Contains(point) ? point.Y - thumb.Y : thumb.Height / 2;
                }
                if (!down) dragging = false;
                model.HoveredScrollbar = model.Scroll.Maximum > 0
                    && (dragging || OptionsMenuLayout.Track.Contains(point) || thumb.Contains(point));
                if (dragging)
                {
                    var bounds = OptionsMenuLayout.ThumbTravel;
                    int travel = bounds.Height - thumb.Height;
                    return new(ScrollPosition: travel <= 0 ? 0 :
                        (point.Y - bounds.Y - grab) * model.Scroll.Maximum / travel);
                }
                if (model.Scroll.Maximum > 0 && delta != 0
                    && (OptionsMenuLayout.Viewport.Contains(point) || OptionsMenuLayout.Track.Contains(point) || thumb.Contains(point)))
                    return new(ScrollDelta: -delta * 48 / 120);
            }
            else { dragging = false; volumeDragRow = null; }
        }
        else { dragging = draggingDropdown = false; volumeDragRow = null; }
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
        if (width <= 0 || height <= 0 || flow.TitleIntroPlaying || flow.TitleInteractionPlaying
            || flow.UsesTitleBackground && !flow.TitleUiVisible) return default;
        bool options = flow.Screen == Screen.Options;
        if (options && flow.Options.CapturingKey) return default;
        int canvasWidth = options ? 1280 : flow.Screen == Screen.Title ? 1920 : 640;
        int canvasHeight = options ? 720 : flow.Screen == Screen.Title ? 1080 : 360;
        double scale = Math.Min((double)width / canvasWidth, (double)height / canvasHeight);
        if (!options && flow.Screen != Screen.Title && scale >= 1) scale = Math.Floor(scale);
        int outputWidth = (int)(canvasWidth * scale), outputHeight = (int)(canvasHeight * scale);
        var output = new Rectangle((width - outputWidth) / 2, (height - outputHeight) / 2, outputWidth, outputHeight);
        if (!output.Contains(x, y))
            return options && flow.Options.DropdownRow is not null ? new(Cancel: true) : default;
        var point = new Point((int)((x - output.X) * (double)canvasWidth / outputWidth),
            (int)((y - output.Y) * (double)canvasHeight / outputHeight));
        if (options)
        {
            if (flow.Options.ConfirmingDiscard)
            {
                if (OptionsMenuLayout.Keep.Contains(point)) return new(Confirm: true, ChoiceIndex: 0);
                if (OptionsMenuLayout.Revert.Contains(point)) return new(Confirm: true, ChoiceIndex: 1);
                return default;
            }
            if (flow.Options.Previewing)
            {
                if (OptionsMenuLayout.Keep.Contains(point)) return new(Confirm: true);
                if (OptionsMenuLayout.Revert.Contains(point)) return new(Cancel: true);
                return default;
            }
            if (flow.Options.DropdownRow is not null)
            {
                if (flow.Options.DropdownCount > OptionsMenuLayout.DropdownVisibleCount(flow.Options)
                    && OptionsMenuLayout.DropdownTrack(flow.Options).Contains(point)) return default;
                for (int row = 0; row < OptionsMenuLayout.DropdownVisibleCount(flow.Options); row++)
                    if (OptionsMenuLayout.DropdownItem(row, flow.Options).Contains(point))
                        return new(Confirm: true, ChoiceIndex: flow.Options.DropdownFirst + row);
                return new(Cancel: true); // Outside clicks dismiss without activating controls underneath.
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
                        if (flow.Options.Tab == 1 && i < OptionsModel.AudioVolumeCount)
                            return OptionsMenuLayout.AudioTrack(i, flow.Options).Contains(point)
                                || OptionsMenuLayout.AudioThumb(i, flow.Options).Contains(point)
                                ? new(SelectionIndex: i, SliderValue: OptionsMenuLayout.AudioValueAt(i, point.X, flow.Options))
                                : new(SelectionIndex: i);
                        if ((flow.Options.Tab == 1 && i == OptionsModel.AudioVolumeCount) || (flow.Options.Tab == 2 && i is 0 or 1))
                            return new(SelectionIndex: i, Confirm: OptionsMenuLayout.Checkbox(i, flow.Options).Contains(point)
                                || point.X < OptionsMenuLayout.ValueBounds(i, flow.Options).X);
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
