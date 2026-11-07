using Microsoft.Xna.Framework;
using PawnShop.Characters.Player;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Screens;

public enum PlayerPlaceholderLayer { Body, Jacket, Helmet, Backpack, Wrench }

// Temporary diagnostic artwork, not fitted wearables or approved character art.
// Each independent layer occupies the complete 48x64 canvas. Rows are South/North/East;
// columns share a single four-phase clock. No West pixels or per-layer world origins exist.
public static class PlayerPlaceholderSprites
{
    public static Rectangle Frame(SpriteSheetDefinition sheet, Facing facing, int phase)
    {
        if (phase is < 0 or > 3) throw new ArgumentOutOfRangeException(nameof(phase));
        int row = facing switch { Facing.South => 0, Facing.North => 1, Facing.East or Facing.West => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(facing)) };
        return sheet.GetFrame(row * 4 + phase);
    }

    public static ReadOnlySpan<PlayerPlaceholderLayer> DrawOrder(Facing facing) => facing == Facing.North
        ? [PlayerPlaceholderLayer.Wrench, PlayerPlaceholderLayer.Body, PlayerPlaceholderLayer.Jacket,
            PlayerPlaceholderLayer.Backpack, PlayerPlaceholderLayer.Helmet]
        : [PlayerPlaceholderLayer.Backpack, PlayerPlaceholderLayer.Body, PlayerPlaceholderLayer.Jacket,
            PlayerPlaceholderLayer.Helmet, PlayerPlaceholderLayer.Wrench];

    // Native EAST hand coordinates. Used by both the hand and wrench before the whole canvas flips.
    public static Point Hand(PlayerGender gender, Facing facing, int phase)
    {
        int swing = phase switch { 1 => -2, 3 => 2, _ => 0 };
        bool side = facing is Facing.East or Facing.West;
        int halfWidth = side ? (gender == PlayerGender.Male ? 5 : 4) : (gender == PlayerGender.Male ? 8 : 6);
        return new(24 + halfWidth + (side ? swing : 1), 39 + (side ? 0 : swing));
    }

    public static Color[] CreatePixels(PlayerGender gender, PlayerPlaceholderLayer layer, SpriteSheetDefinition sheet)
    {
        if (!Enum.IsDefined(gender)) throw new ArgumentOutOfRangeException(nameof(gender));
        if (!Enum.IsDefined(layer)) throw new ArgumentOutOfRangeException(nameof(layer));
        var pixels = new Color[sheet.ImageWidth * sheet.ImageHeight];
        var skin = new Color(221, 176, 138);
        var body = new Color(112, 128, 142);
        var jacket = new Color(48, 142, 139);
        var dark = new Color(42, 49, 55);
        foreach (var facing in new[] { Facing.South, Facing.North, Facing.East })
        for (int phase = 0; phase < 4; phase++)
        {
            var frame = Frame(sheet, facing, phase);
            bool side = facing == Facing.East;
            int halfWidth = side ? (gender == PlayerGender.Male ? 5 : 4) : (gender == PlayerGender.Male ? 8 : 6);
            int stride = phase switch { 1 => -2, 3 => 2, _ => 0 };
            var hand = Hand(gender, facing, phase);
            // Pixel positions are character-space coordinates, never cropped texture offsets.
            void Fill(int x, int y, int width, int height, Color color)
            {
                for (int py = y; py < y + height; py++)
                for (int px = x; px < x + width; px++)
                {
                    if (px < 0 || px >= sheet.FrameWidth || py < 0 || py >= sheet.FrameHeight)
                        throw new InvalidDataException("Placeholder geometry exceeds its character canvas.");
                    pixels[(frame.Y + py) * sheet.ImageWidth + frame.X + px] = color;
                }
            }
            switch (layer)
            {
                case PlayerPlaceholderLayer.Body:
                    Fill(18, 8, 12, 17, skin);
                    if (facing != Facing.North) Fill(side ? 28 : 20, 16, side ? 2 : 8, 2, dark);
                    Fill(24 - halfWidth, 25, halfWidth * 2, 19, body);
                    if (side && stride > 0) Fill(24 + halfWidth, 30, stride + 1, 3, skin);
                    Fill(hand.X - 1, 31, 3, hand.Y - 30, skin);
                    Fill(22 - halfWidth, 31, 3, 10 - stride, skin);
                    // Feet stay on y=57; stride changes their horizontal placement, not the world anchor.
                    Fill(18 + stride, 44, 5, 12, body);
                    Fill(25 - stride, 44, 5, 12, body);
                    Fill(17 + stride, 55, 6, 3, dark);
                    Fill(25 - stride, 55, 6, 3, dark);
                    break;
                case PlayerPlaceholderLayer.Jacket:
                    Fill(24 - halfWidth, 25, halfWidth * 2, 18, jacket);
                    if (side && stride > 0) Fill(24 + halfWidth, 30, stride + 1, 3, jacket);
                    Fill(hand.X - 1, 30, 3, 6, jacket);
                    Fill(22 - halfWidth, 30, 3, 6, jacket);
                    if (!side && facing == Facing.South) Fill(23, 25, 2, 18, dark);
                    break;
                case PlayerPlaceholderLayer.Helmet:
                    Fill(17, 7, 14, 14, new Color(218, 174, 63));
                    if (facing != Facing.North) Fill(side ? 27 : 19, 12, side ? 4 : 10, 6, dark);
                    break;
                case PlayerPlaceholderLayer.Backpack:
                    Fill(side ? 15 : 17, 28, side ? 7 : 14, 17, new Color(146, 87, 56));
                    Fill(side ? 15 : 18, 31, side ? 2 : 12, 2, dark);
                    break;
                case PlayerPlaceholderLayer.Wrench:
                    // Offset from the same hand used above; both offset and prop mirror with the full frame.
                    Fill(hand.X, hand.Y - 9, 2, 14, new Color(204, 215, 220));
                    Fill(hand.X - 2, hand.Y - 11, 6, 3, new Color(204, 215, 220));
                    Fill(hand.X, hand.Y - 11, 2, 2, Color.Transparent);
                    break;
            }
        }
        return pixels;
    }
}
