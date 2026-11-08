using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PawnShop.Framework.Rendering;

// Corners keep their authored pixel scale. Straight edges and center repeat;
// the last tile is cropped rather than stretched, so grain stays pixel-sharp.
public static class NineSlice
{
    public static void Draw(PixelRenderer renderer, Texture2D texture, Rectangle source,
        Rectangle destination, int border, int scale = 2, Color? tint = null)
    {
        if (border <= 0 || source.Width <= border * 2 || source.Height <= border * 2 || scale < 1
            || destination.Width < border * scale * 2 || destination.Height < border * scale * 2)
            throw new ArgumentException("Nine-slice source/destination cannot contain its borders.");
        int[] sx = [source.X, source.X + border, source.Right - border, source.Right];
        int[] sy = [source.Y, source.Y + border, source.Bottom - border, source.Bottom];
        int[] dx = [destination.X, destination.X + border * scale, destination.Right - border * scale, destination.Right];
        int[] dy = [destination.Y, destination.Y + border * scale, destination.Bottom - border * scale, destination.Bottom];
        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                Tile(renderer, texture, new(sx[col], sy[row], sx[col + 1] - sx[col], sy[row + 1] - sy[row]),
                    new(dx[col], dy[row], dx[col + 1] - dx[col], dy[row + 1] - dy[row]), scale, tint ?? Color.White);
    }

    public static void Tile(PixelRenderer renderer, Texture2D texture, Rectangle source,
        Rectangle destination, int scale, Color tint)
    {
        for (int y = destination.Y; y < destination.Bottom; y += source.Height * scale)
            for (int x = destination.X; x < destination.Right; x += source.Width * scale)
            {
                int w = Math.Min(source.Width * scale, destination.Right - x);
                int h = Math.Min(source.Height * scale, destination.Bottom - y);
                renderer.DrawTexture(texture, new(x, y, w, h),
                    new(source.X, source.Y, (w + scale - 1) / scale, (h + scale - 1) / scale), tint);
            }
    }
}
