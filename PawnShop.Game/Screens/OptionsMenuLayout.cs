using Microsoft.Xna.Framework;
using PawnShop.Game.Bootstrap;

namespace PawnShop.Game.Screens;

public static class OptionsMenuLayout
{
    // Contain the supplied 1586x992 panel, preserving its complete illustration.
    public static Rectangle Panel => new(103, 24, 1074, 672);
    public static Rectangle Viewport => new(176, 208, 912, 272);
    public static Rectangle Track => new(1100, 208, 18, 272);
    public static Rectangle Header => new(168, 98, 944, 68);
    public static Rectangle Tab(int index) => new(180 + index * 224, 104, 220, 56);
    public static Rectangle Keep => new(190, 444, 360, 56);
    public static Rectangle Revert => new(730, 444, 360, 56);
    public static int RowHeight(int uiScale) => (64 + (uiScale - 100) / 2) / 2 * 2;
    public static int ContentHeight(OptionsModel model) => 36 + model.FieldCount * RowHeight(model.Draft.UiScale) + 180;
    public static Rectangle Field(int index, OptionsModel model) => new(Viewport.X,
        Viewport.Y + 36 + index * RowHeight(model.Draft.UiScale) - model.Scroll.Offset, Viewport.Width, RowHeight(model.Draft.UiScale) - 8);
    public static Rectangle Cell(int row, int slot, OptionsModel model)
    {
        var field = Field(row, model);
        return new(slot == 0 ? 642 : 872, field.Y, 216, field.Height);
    }
    public static Rectangle Footer(int action) => action switch
    {
        0 => new(684, 564, 180, 48),
        1 => new(184, 564, 352, 48),
        _ => new(908, 564, 180, 48)
    };
    public static Rectangle Thumb(OptionsModel model)
    {
        int height = Math.Max(36, Track.Height * Track.Height / Math.Max(Track.Height, ContentHeight(model)));
        int y = Track.Y + (model.Scroll.Maximum == 0 ? 0 : model.Scroll.Offset * (Track.Height - height) / model.Scroll.Maximum);
        return new(Track.X, y, Track.Width, height);
    }
}
