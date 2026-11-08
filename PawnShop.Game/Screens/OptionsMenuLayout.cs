using Microsoft.Xna.Framework;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

public static class OptionsMenuLayout
{
    // Contain the supplied 1586x992 panel, preserving its complete illustration.
    public static Rectangle Panel => new(103, 24, 1074, 672);
    public static Rectangle Viewport => new(224, 208, 816, Footer(0).Top - 8 - 208);
    public static Rectangle Track => new(Viewport.Right + 12, Viewport.Y, 40, Viewport.Height);
    // Keep the compass near the shaft ends, with clearance for the visible caps.
    public static Rectangle ThumbTravel => new(Track.X, Track.Y + 12, Track.Width, Track.Height - 24);
    public static Rectangle Header => new(168, 98, 944, 68);
    public static Rectangle Tab(int index) => new(180 + index * 224, 104, 220, 56);
    public static Rectangle DisplayConfirmation => new(232, 190, 816, 340);
    // Leave a clear gap between the closed confirmation buttons and the gold rail.
    public static int ConfirmationRailTop => DisplayConfirmation.Bottom - 23;
    public static Rectangle Keep => new(DisplayConfirmation.Left + 80, ConfirmationRailTop - 72, 316, 56);
    public static Rectangle Revert => new(DisplayConfirmation.Right - 396, ConfirmationRailTop - 72, 316, 56);
    public static int RowHeight(int uiScale) => (64 + (uiScale - 100) / 2) / 2 * 2;
    public static int HelpTop(OptionsModel model) => 44 + model.FieldCount * RowHeight(model.Draft.UiScale);
    // Reserve the authored help's line budget instead of a large blank scroll tail.
    public static int HelpLines(OptionsModel model) => model.Tab == 2 ? 1 : 0;
    public static int HelpLineHeight(OptionsModel model) => 22 * model.Draft.UiScale / 100;
    public static int ContentHeight(OptionsModel model) => HelpTop(model)
        + (HelpLines(model) + (model.Message is null ? 0 : 3)) * HelpLineHeight(model);
    public static Rectangle Field(int index, OptionsModel model) => new(Viewport.X,
        Viewport.Y + 36 + index * RowHeight(model.Draft.UiScale) - model.Scroll.Offset, Viewport.Width, RowHeight(model.Draft.UiScale) - 8);
    public static Vector2 SelectorAnchor(OptionsModel model, bool first = false)
    {
        var bounds = Field(first ? 0 : model.IndicatorRow, model);
        return new Vector2(bounds.X - 20, Math.Clamp(bounds.Center.Y, 240, 640)) * 1.5f;
    }
    public static Rectangle ValueBounds(int row, OptionsModel model)
    {
        var field = Field(row, model);
        return new(field.Center.X, field.Y, field.Width / 2, field.Height);
    }
    public static Rectangle Cell(int row, int slot, OptionsModel model)
    {
        var value = ValueBounds(row, model);
        int width = (value.Width - 16) / 2;
        return new(value.X + slot * (width + 16), value.Y, width, value.Height);
    }
    public static Rectangle AudioTrack(int row, OptionsModel model)
    {
        var value = ValueBounds(row, model);
        return new(value.X + 18, value.Center.Y - 7, value.Width - 120, 14);
    }
    public static Rectangle AudioFill(int row, OptionsModel model)
    {
        var track = AudioTrack(row, model);
        return new(track.X + 4, track.Y + 4, track.Width - 8, track.Height - 8);
    }
    public static Rectangle AudioThumb(int row, OptionsModel model)
    {
        var fill = AudioFill(row, model);
        int x = fill.X + fill.Width * model.Volume(row) / 100;
        // Clock face pivot is (14,20); the winding crown adds width on the right.
        return new(x - 14, fill.Center.Y - 20, 32, 40);
    }
    public static int AudioValueAt(int row, int x, OptionsModel model)
    {
        var fill = AudioFill(row, model);
        return Math.Clamp((int)MathF.Round((x - fill.X) * 100f / fill.Width), 0, 100);
    }
    public static Rectangle AudioCheckbox(OptionsModel model) => Checkbox(OptionsModel.AudioVolumeCount, model);
    public static Rectangle Checkbox(int row, OptionsModel model)
    {
        var track = AudioTrack(row, model);
        return new(track.Center.X - 15, track.Center.Y - 15, 30, 30);
    }
    public static int DropdownRowHeight(OptionsModel model) => 40 * model.Draft.UiScale / 100;
    public static int DropdownVisibleCount(OptionsModel model)
    {
        if (model.DropdownRow is not { } row) return 0;
        var button = ValueBounds(row, model);
        int space = Math.Max(Viewport.Bottom - button.Bottom - 4, button.Top - Viewport.Top - 4);
        return Math.Clamp((space - 32) / DropdownRowHeight(model), 1, Math.Min(6, model.DropdownCount));
    }
    public static Rectangle Dropdown(OptionsModel model)
    {
        var button = ValueBounds(model.DropdownRow ?? throw new InvalidOperationException("No dropdown is open."), model);
        int height = DropdownVisibleCount(model) * DropdownRowHeight(model) + 32;
        int below = Viewport.Bottom - button.Bottom - 4;
        int y = below >= height ? button.Bottom + 4 : button.Top - height - 4;
        return new(button.X, Math.Clamp(y, Viewport.Top, Viewport.Bottom - height), button.Width, height);
    }
    public static Rectangle DropdownItem(int visibleRow, OptionsModel model)
    {
        var list = Dropdown(model);
        return new(list.X + 16, list.Y + 16 + visibleRow * DropdownRowHeight(model), list.Width - 32, DropdownRowHeight(model));
    }
    public static Rectangle DropdownTrack(OptionsModel model)
    {
        var list = Dropdown(model);
        return new(list.Right - 24, list.Y + 16, 6, list.Height - 32);
    }
    public static Rectangle DropdownThumb(OptionsModel model)
    {
        var track = DropdownTrack(model);
        int visible = DropdownVisibleCount(model);
        int height = Math.Max(16, track.Height * visible / model.DropdownCount);
        int maximum = model.DropdownCount - visible;
        return new(track.X, track.Y + (maximum == 0 ? 0 : model.DropdownFirst * (track.Height - height) / maximum), track.Width, height);
    }

    // Meet the original rail's yellow/gold cap rather than painting over it.
    public static int BottomBarTop => Panel.Y + 879 * Panel.Height / 992;
    public static Rectangle Footer(int action) => action switch
    {
        0 => new(782, BottomBarTop - 36, 128, 36),
        1 => new(244, BottomBarTop - 36, 232, 36),
        _ => new(910, BottomBarTop - 36, 128, 36)
    };
    public static Rectangle Thumb(OptionsModel model)
    {
        // The illustrated knob keeps a fixed shape instead of stretching
        // with the content ratio. Its full travel still maps to the scroll range.
        const int width = 32, height = 42;
        var travel = ThumbTravel;
        int y = travel.Y + (model.Scroll.Maximum == 0 ? 0 : model.Scroll.Offset * (travel.Height - height) / model.Scroll.Maximum);
        return new(travel.Center.X + 2 - width / 2, y, width, height);
    }
}
