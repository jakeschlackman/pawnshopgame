#if DEBUG
using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;
using PawnShop.Game.Bootstrap.Cinematics;

namespace PawnShop.Game.Screens.CinematicEditor;

// Authoring-only framing. The gold frame is the ordinary player's camera crop;
// zooming out changes this display transform, never the exported path or car scale.
public readonly record struct CinematicEditorView(CinematicCoordinates Coordinates, Rectangle PlayerFrame)
{
    public static CinematicEditorView Create(PanoramaView playerView, int width, int height, bool expanded)
    {
        var output = new Rectangle(0, 0, width, height);
        if (!expanded) return new(new(playerView, output), output);

        float hudScale = Math.Min(width / 1280f, height / 720f);
        var offset = new Vector2((width - (int)(1280 * hudScale)) / 2, (height - (int)(720 * hudScale)) / 2);
        var canvas = new Rectangle((int)(offset.X + 380 * hudScale), (int)(offset.Y + 36 * hudScale),
            (int)(870 * hudScale), (int)(570 * hudScale));
        double fit = Math.Min((double)canvas.Width / width, (double)canvas.Height / height);
        double scale = playerView.Scale * fit * .4;
        double imageWidth = canvas.Width / scale, imageHeight = canvas.Height / scale;
        var view = new PanoramaView(playerView.X + (playerView.Width - imageWidth) / 2,
            playerView.Y + (playerView.Height - imageHeight) / 2, imageWidth, imageHeight, scale);
        var coordinates = new CinematicCoordinates(view, canvas);
        var topLeft = coordinates.ImageToScreen(new((float)playerView.X, (float)playerView.Y));
        var bottomRight = coordinates.ImageToScreen(new((float)(playerView.X + playerView.Width),
            (float)(playerView.Y + playerView.Height)));
        var frame = new Rectangle((int)MathF.Round(topLeft.X), (int)MathF.Round(topLeft.Y),
            (int)MathF.Round(bottomRight.X - topLeft.X), (int)MathF.Round(bottomRight.Y - topLeft.Y));
        return new(coordinates, frame);
    }
}
#endif
