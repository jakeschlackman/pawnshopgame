using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;

namespace PawnShop.Game.Bootstrap.Cinematics;

// Uses the same source crop and uniform scale as PanoramicBackground.Draw.
// Viewport may be offset (letterboxing); client pixels can differ from backbuffer pixels.
public readonly record struct CinematicCoordinates(PanoramaView View, Rectangle Viewport)
{
    public Vector2 ImageToScreen(Vector2 point) => new(
        (float)(Viewport.X + (point.X - View.X) * View.Scale),
        (float)(Viewport.Y + (point.Y - View.Y) * View.Scale));

    public Vector2 ScreenToImage(Vector2 point) => new(
        (float)((point.X - Viewport.X) / View.Scale + View.X),
        (float)((point.Y - Viewport.Y) / View.Scale + View.Y));

    public static Vector2 ClientToScreen(Vector2 point, int clientWidth, int clientHeight, int outputWidth, int outputHeight) =>
        new(point.X * outputWidth / clientWidth, point.Y * outputHeight / clientHeight);

    // Subtract the configured wheel contact in source-canvas coordinates, after
    // perspective scale and camera scale. No sprite-center placement shortcut.
    public Vector2 CarTopLeft(CinematicPose pose, CinematicPoint anchor) =>
        ImageToScreen(pose.GroundPosition) - new Vector2(anchor.X, anchor.Y) * pose.Scale * (float)View.Scale;

    // Diagnostic model: local X points right, Y up, Z toward the rear. Positive
    // pitch raises the nose. Rotate about the ground origin before applying yaw;
    // its fixed canvas registration (32,40) and configured contact anchor stay stable.
    public Vector2 ProjectCarPoint(Vector3 point, CinematicPose pose, CinematicPoint anchor)
    {
        float pitch = MathHelper.ToRadians(pose.PitchDegrees);
        float yaw = MathHelper.ToRadians(pose.OrientationDegrees);
        float depth = point.Z * MathF.Cos(pitch) + point.Y * MathF.Sin(pitch);
        float height = point.Y * MathF.Cos(pitch) - point.Z * MathF.Sin(pitch);
        var projected = new Vector2(32 + point.X * MathF.Cos(yaw) + depth * MathF.Sin(yaw),
            40 + (depth * MathF.Cos(yaw) - point.X * MathF.Sin(yaw)) * .35f - height);
        return CarTopLeft(pose, anchor) + projected * pose.Scale * (float)View.Scale;
    }
}
